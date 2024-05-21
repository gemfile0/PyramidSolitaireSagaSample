using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.Input;
using PyramidSolitaireSagaSample.System;
using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameTutorial
{
    public interface IGameTutorialManagerPlayer
    {
        IGameTutorialManager GameTutorialManager { set; }
    }

    public interface IGameTutorialManager
    {
        public bool HasLevelTutorial(LevelDataPath levelDataPath, int level);
        public IEnumerator PlayLevelTutorial(LevelDataPath levelDataPath, int level);
    }

    public enum GameTutorialWaitState
    {
        None,
        TapHighlightedObject,
        TapAnywhere
    }

    public class GameTutorialManager : Singleton<GameTutorialManager>,
                                       IGameObjectFinderSetter,
                                       ILevelPlayerInputDisabler,
                                       IGameTutorialManager,
                                       ILoadingInfoProvider
    {
        [Space]
        [SerializeField] private Canvas _gameTutorialStepPanelCanvas;
        [SerializeField] private GameObject _gameTutorialStepPanelObject;
        [SerializeField] private GameObject _gameTutorialContinuePanelObject;
        [SerializeField] private List<GameTutorialStepUI> _gameTutorialStepUiList;

        [Space]
        [SerializeField] private Transform _cardRendererRoot;
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private GameBoardCardRenderer _cardRendererPrefab;

        private const string LevelTutorialDataLabel = "LevelTutorialData";

        public event Action requestDisableInput;
        public event Action requestEnableInput;

        private Dictionary<LevelDataPath, Dictionary<string, LevelTutorialData>> _levelTutorialDictByLevelPath;
        private Dictionary<GameTutorialStepUiLocation, GameTutorialStepUI> _gameTutorialStepUiDict;
        private GameObjectPool<GameBoardCardRenderer> _cardRendererPool;
        private Dictionary<LevelTutorialCloneType, ILevelTutorialCardCloner> _cardClonerDict;
        private Dictionary<LevelTutorialCloneType, ILevelTutorialUiCloner> _uiClonerDict;
        private int _sortingOrder;
        private EnumState<GameTutorialWaitState> _waitState;
        private StringBuilder _tutorialDataNameBuilder;
        private StringBuilder _assetLoadLogBuilder;
        private List<GameBoardCardRenderer> _cardRendererList;
        private GameObject _uiCloneContainer;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            foreach (var gameTutorialPlayer in finder.FindGameObjectOfType<IGameTutorialManagerPlayer>())
            {
                gameTutorialPlayer.GameTutorialManager = this;
            }

            _cardClonerDict = new();
            foreach (var cardCloner in finder.FindGameObjectOfType<ILevelTutorialCardCloner>())
            {
                _cardClonerDict.Add(cardCloner.CardCloneType, cardCloner);
            }
            _uiClonerDict = new();
            foreach (var uiCloner in finder.FindGameObjectOfType<ILevelTutorialUiCloner>())
            {
                _uiClonerDict.Add(uiCloner.UiCloneType, uiCloner);
            }

            var mainCamera = finder.FindRootGameObject<Camera>("Main Camera");
            _gameTutorialStepPanelCanvas.worldCamera = mainCamera;
        }

        protected override void Awake()
        {
            base.Awake();

            if (_isDuplicated == false)
            {
                _levelTutorialDictByLevelPath = new();

                _gameTutorialStepUiDict = new();
                foreach (GameTutorialStepUI stepUI in _gameTutorialStepUiList)
                {
                    stepUI.gameObject.SetActive(false);
                    _gameTutorialStepUiDict.Add(stepUI.Location, stepUI);
                }
                _cardRendererPool = new(_cardRendererRoot, _cardRendererPrefab.gameObject);

                _sortingOrder = 1;

                _tutorialDataNameBuilder = new();
                _assetLoadLogBuilder = new();
                _cardRendererList = new();

                _waitState = new();
            }
        }

        private void Start()
        {
            _gameTutorialStepPanelObject.SetActive(false);
            _gameTutorialContinuePanelObject.SetActive(false);
        }

        public IEnumerator PlayLevelTutorial(LevelDataPath levelDataPath, int level)
        {
            string tutorialDataName = MakeTutorialDataName(level);
            if (_levelTutorialDictByLevelPath.TryGetValue(levelDataPath, out Dictionary<string, LevelTutorialData> levelTutorialDict)
                && levelTutorialDict.TryGetValue(tutorialDataName, out LevelTutorialData levelTutorialData))
            {
                DisableInput();

                foreach (LevelTutorialStepData stepData in levelTutorialData.StepDataList)
                {
                    GameTutorialWaitState waitState = stepData.triggerDrawCard ?
                                                      GameTutorialWaitState.TapHighlightedObject :
                                                      GameTutorialWaitState.TapAnywhere;
                    _waitState.Set(waitState);

                    CloneCard(stepData.cardCloneType,
                              stepData.CardCloneDataList,
                              onCardClick: () =>
                              {
                                  if (waitState == GameTutorialWaitState.TapHighlightedObject)
                                  {
                                      _waitState.Set(GameTutorialWaitState.None);
                                  }
                              });
                    CloneUi(stepData.uiCloneType, stepData.uiCloneData);

                    string continueMessage = waitState == GameTutorialWaitState.TapHighlightedObject ?
                                             "Tap the highlighted object" :
                                             "Tap anywhere to continue";
                    OpenStepUI(stepData.uiLocation, stepData.message, continueMessage);
                    while (_waitState.CurrState != GameTutorialWaitState.None)
                    {
                        yield return null;
                    }
                    ReleaseCardRenderer();
                    ReleaseUi();
                    if (stepData.triggerDrawCard)
                    {
                        DrawCard(stepData.cardCloneType, stepData.CardCloneDataList);
                    }
                }

                EnableInput();
            }
            else
            {
                Debug.LogError($"튜토리얼을 찾지 못했습니다. {levelDataPath}, {tutorialDataName}");
            }
        }

        private void ReleaseUi()
        {
            Destroy(_uiCloneContainer);
            _uiCloneContainer = null;
        }

        private void OpenStepUI(GameTutorialStepUiLocation uiLocation, string message, string continueMessage)
        {
            foreach (GameTutorialStepUI _stepUI in _gameTutorialStepUiList)
            {
                _stepUI.CachedGameObject.SetActive(false);
            }
            if (_gameTutorialStepUiDict.TryGetValue(uiLocation, out GameTutorialStepUI stepUI))
            {
                stepUI.CachedGameObject.SetActive(true);
                stepUI.UpdateMessageText(message, continueMessage);
            }
            else
            {
                Debug.LogWarning($"StepUI 를 찾지 못했습니다 : {uiLocation}");
            }
        }

        private void ReleaseCardRenderer()
        {
            foreach (GameBoardCardRenderer tutorialCardRenderer in _cardRendererList)
            {
                _cardRendererPool.Release(tutorialCardRenderer);
            }
            _cardRendererList.Clear();
        }

        private void DrawCard(LevelTutorialCloneType cardCloneType, IEnumerable<CardRendererCloneData> cloneDataList)
        {
            if (_cardClonerDict.TryGetValue(cardCloneType, out ILevelTutorialCardCloner cardCloner))
            {
                cardCloner.DrawCard(cloneDataList);
            }
            else
            {
                Debug.LogWarning($"CardRendererCloner 를 찾지 못했습니다 : {cardCloneType}");
            }
        }

        private void CloneCard(
            LevelTutorialCloneType cardCloneType,
            IEnumerable<CardRendererCloneData> cloneDataList,
            Action onCardClick
        )
        {
            if (_cardClonerDict.TryGetValue(cardCloneType, out ILevelTutorialCardCloner cardCloner))
            {
                _cardRendererList.Clear();
                int cloneCount = cardCloner.GetCloneCount(cloneDataList);
                for (int i = 0; i < cloneCount; i++)
                {
                    GameBoardCardRenderer cardRenderer = _cardRendererPool.Get();
                    cardRenderer.Init(Vector2Int.zero, _sortingOrder, null);
                    AddTutorialSortingGroup(cardRenderer.CachedGameObject);

                    cardRenderer.CachedTransform.SetParent(_cardRendererRoot);

                    var mouseClickTrigger = cardRenderer.ColliderObject.AddComponent<MouseClickTrigger>();
                    mouseClickTrigger.Init(onCardClick);

                    _cardRendererList.Add(cardRenderer);
                }

                cardCloner.CloneCardRendererList(_cardRendererList, cloneDataList);
            }
            else
            {
                Debug.LogWarning($"CardRendererCloner 를 찾지 못했습니다 : {cardCloneType}");
            }
        }

        private void AddTutorialSortingGroup(GameObject targetObject)
        {
            if (targetObject.GetComponent<SortingGroup>() == null)
            {
                var sortingGroup = targetObject.AddComponent<SortingGroup>();
                sortingGroup.sortingLayerName = "Tutorial";
                sortingGroup.sortingOrder = _sortingOrder;
                _sortingOrder += 1;
            }
        }

        private void CloneUi(LevelTutorialCloneType uiCloneType, UiCloneData uiCloneData)
        {
            if (_uiClonerDict.TryGetValue(uiCloneType, out ILevelTutorialUiCloner uiCloner))
            {
                _uiCloneContainer = new GameObject("UiCloneContainer");
                if (uiCloneType == LevelTutorialCloneType.Streaks)
                {
                    Transform uiCloneContainerTransform = _uiCloneContainer.transform;
                    uiCloneContainerTransform.SetParent(_cardRendererRoot);
                    uiCloner.CloneUI(uiCloneContainerTransform, uiCloneData);
                    GameObject clonedGameObject = uiCloneContainerTransform.GetChild(0).gameObject;
                    AddTutorialSortingGroup(clonedGameObject);
                }
                else
                {
                    var uiCloneContainerRectTransform = _uiCloneContainer.AddComponent<RectTransform>();
                    uiCloneContainerRectTransform.SetParent(_uiRoot);
                    uiCloneContainerRectTransform.localScale = Vector3.one;
                    uiCloner.CloneUI(uiCloneContainerRectTransform, uiCloneData);
                }
            }
        }

        private void DisableInput()
        {
            _gameTutorialStepPanelObject.SetActive(true);
            _gameTutorialContinuePanelObject.SetActive(true);
            requestDisableInput?.Invoke();
        }

        private void EnableInput()
        {
            _gameTutorialStepPanelObject.SetActive(false);
            _gameTutorialContinuePanelObject.SetActive(false);
            requestEnableInput?.Invoke();
        }

        public void OnContinueButtonClick()
        {
            if (_waitState.CurrState == GameTutorialWaitState.TapAnywhere)
            {
                _waitState.Set(GameTutorialWaitState.None);
            }
        }

        public LoadingInfo GetLoadingInfo()
        {
            return new LoadingInfo(MakeAssetLoadLog("Load ", LevelTutorialDataLabel, " assets"), 1f, PrepareLevelTutorialAssetsCoroutine);
        }

        private IEnumerator PrepareLevelTutorialAssetsCoroutine(Action<float> onProgress, Action onComplete, Action<string> onError)
        {
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(LevelTutorialDataLabel);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                onError?.Invoke(MakeAssetLoadLog("Failed to load asset locations", additionalLog: LevelTutorialDataLabel));
                yield break;
            }

            float totalToLoad = handle.Result.Count;
            if (totalToLoad == 0)
            {
                Debug.LogWarning($"Label 로 지정된 어드레서블 에셋이 없습니다 : {LevelTutorialDataLabel}");
            }

            float currentLoadProgress = 0;
            foreach (IResourceLocation location in handle.Result)
            {
                AsyncOperationHandle<LevelTutorialData> loadAssetHandle = Addressables.LoadAssetAsync<LevelTutorialData>(location);
                while (!loadAssetHandle.IsDone)
                {
                    //Debug.Log($"{currentLoadProgress}, {loadAssetHandle.PercentComplete}");
                    float totalProgress = (currentLoadProgress + loadAssetHandle.PercentComplete) / totalToLoad;
                    onProgress?.Invoke(totalProgress);
                    yield return null;
                }

                if (loadAssetHandle.Status == AsyncOperationStatus.Failed)
                {
                    onError?.Invoke(MakeAssetLoadLog("Failed to load asset at ", location.PrimaryKey, additionalLog: LevelTutorialDataLabel));
                    yield break;
                }

                string[] pathSegments = location.PrimaryKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathSegments.Length > 0)
                {
                    const string tutorialSuffix = "Tutorial";
                    string folderName = pathSegments[0];
                    folderName = folderName.Substring(0, folderName.Length - tutorialSuffix.Length);
                    Enum.TryParse(folderName, true, out LevelDataPath levelDataPath);
                    if (_levelTutorialDictByLevelPath.ContainsKey(levelDataPath) == false)
                    {
                        _levelTutorialDictByLevelPath.Add(levelDataPath, new());
                    }

                    Dictionary<string, LevelTutorialData> levelTutorialDict = _levelTutorialDictByLevelPath[levelDataPath];
                    string assetName = pathSegments[1].Replace(".asset", "");
                    //Debug.Log($"LevelDataPath : {levelDataPath}, assetName : {assetName}");
                    LevelTutorialData levelTutorialData = loadAssetHandle.Result;
                    levelTutorialDict.Add(assetName, levelTutorialData);
                }

                currentLoadProgress++;

                onProgress?.Invoke(currentLoadProgress / totalToLoad);
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke();
            Addressables.Release(handle);
        }

        private string MakeAssetLoadLog(string firstLog, string secondLog = "", string thirdLog = "", string additionalLog = "")
        {
            _assetLoadLogBuilder.Length = 0;
            _assetLoadLogBuilder.Append(firstLog);
            _assetLoadLogBuilder.Append(secondLog);
            _assetLoadLogBuilder.Append(thirdLog);

            if (string.IsNullOrEmpty(additionalLog) == false)
            {
                _assetLoadLogBuilder.Append(" : ");
                _assetLoadLogBuilder.Append(LevelTutorialDataLabel);
            }
            return _assetLoadLogBuilder.ToString();
        }

        public bool HasLoadingInfo(SceneName sceneName)
        {
            return sceneName == SceneName.LevelPlayerScene
                   && _levelTutorialDictByLevelPath.Count == 0;
        }

        public bool HasLevelTutorial(LevelDataPath levelDataPath, int level)
        {
            string tutorialDataName = MakeTutorialDataName(level);
            return _levelTutorialDictByLevelPath.ContainsKey(levelDataPath)
                   && _levelTutorialDictByLevelPath[levelDataPath].ContainsKey(tutorialDataName);
        }

        private string MakeTutorialDataName(int level)
        {
            _tutorialDataNameBuilder.Length = 0;
            _tutorialDataNameBuilder.Append(LevelTutorialDataLabel);
            _tutorialDataNameBuilder.Append(level);
            return _tutorialDataNameBuilder.ToString();
        }
    }
}
