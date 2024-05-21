using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.LevelPlayer.Input;
using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public interface IPopupManagerOpener
    {
        IPopupManager PopupManager { set; }
    }

    public interface IPopupManager
    {
        T OpenPopup<T>(string popupName, PopupOpenMode openMode = PopupOpenMode.Queue) where T : BasePopup;
        T OpenPopup<T>(PopupOpenMode openMode = PopupOpenMode.Queue) where T : BasePopup;
        void ClearQueue();
    }

    public enum PopupOpenMode
    {
        Queue,
        Immediate
    }

    public class PopupManager : Singleton<PopupManager>,
                                IGameObjectFinderSetter,
                                ILevelPlayerInputDisabler,
                                IPopupManager,
                                ILoadingInfoProvider
    {
        [SerializeField] private GameObject _backgroundObject;
        [SerializeField] private RectTransform _popupRoot;

        private const string PopupLabel = "Popup";

        public event Action requestDisableInput;
        public event Action requestEnableInput;

        private Dictionary<string, BasePopup> _popupDict;
        private LinkedList<BasePopup> _popupLinkedList;
        private BasePopup _currentPopup;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            foreach (var popupManagerOpener in finder.FindGameObjectOfType<IPopupManagerOpener>())
            {
                popupManagerOpener.PopupManager = this;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (_isDuplicated == false)
            {
                _popupLinkedList = new();

                _popupDict = new();

                _backgroundObject.SetActive(false);
            }
        }

        public T OpenPopup<T>(string popupName, PopupOpenMode openMode = PopupOpenMode.Queue) where T : BasePopup
        {
            if (_popupDict.TryGetValue(popupName, out BasePopup popup))
            {
                if (openMode == PopupOpenMode.Queue)
                {
                    if (_popupLinkedList.Count == 0
                        || _popupLinkedList.Last.Value.name != popupName)
                    {
                        _popupLinkedList.AddLast(popup);
                    }
                    else
                    {
                        Debug.LogWarning($"같은 팝업이 큐에 존재합니다 : {popupName}");
                    }
                }
                else if (openMode == PopupOpenMode.Immediate)
                {
                    _popupLinkedList.AddFirst(popup);
                    if (_currentPopup != null)
                    {
                        _popupLinkedList.AddLast(_currentPopup);
                        CloseCurrentPopup();
                    }
                }

                OpenNextPopup();
            }
            else
            {
                Debug.LogWarning($"팝업을 찾을 수 없습니다 : {popupName}");
            }

            return popup != null ?
                   popup.GetComponent<T>() :
                   null;
        }

        public T OpenPopup<T>(PopupOpenMode openMode = PopupOpenMode.Queue) where T : BasePopup
        {
            string popupName = typeof(T).Name;
            return OpenPopup<T>(popupName, openMode);
        }

        public void ClearQueue()
        {
            _popupLinkedList.Clear();
        }

        private void OnClose()
        {
            CloseCurrentPopup();
            OpenNextPopup();
        }

        private void CloseCurrentPopup()
        {
            _currentPopup.onClose.RemoveAllListeners();
            _currentPopup.gameObject.SetActive(false);
            _currentPopup = null;
        }

        private void OpenNextPopup()
        {
            if (_popupLinkedList.Count > 0)
            {
                if (_currentPopup == null)
                {
                    _OpenNextPopup();
                }
                else
                {
                    Debug.LogWarning($"열린 팝업이 있습니다 : {_currentPopup.name}");
                }
            }
            else
            {
                StartCoroutine(EnableInputCoroutine());
            }
        }

        private void _OpenNextPopup()
        {
            DisableInput();

            BasePopup popup = _popupLinkedList.First.Value;
            popup.onClose.AddListener(OnClose);
            popup.gameObject.SetActive(true);
            Debug.Log($"_OpenNextPopup : {popup.name}");

            _popupLinkedList.RemoveFirst();

            _currentPopup = popup;
        }

        private void DisableInput()
        {
            _backgroundObject.SetActive(true);
            requestDisableInput?.Invoke();
        }

        private IEnumerator EnableInputCoroutine()
        {
            yield return null;
            EnableInput();
        }

        private void EnableInput()
        {
            _backgroundObject.SetActive(false);
            requestEnableInput?.Invoke();
        }

        public LoadingInfo GetLoadingInfo()
        {
            return new LoadingInfo($"Load {PopupLabel} Assets", 1f, PreparePopupAssetsCoroutine);
        }

        private IEnumerator PreparePopupAssetsCoroutine(Action<float> onProgress, Action onComplete, Action<string> onError)
        {
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(PopupLabel);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                onError?.Invoke($"Failed to load {PopupLabel} asset locations.");
                yield break;
            }

            float totalToLoad = handle.Result.Count;
            if (totalToLoad == 0)
            {
                Debug.LogWarning($"No {PopupLabel} assets found.");
            }

            float currentLoadProgress = 0;
            foreach (IResourceLocation location in handle.Result)
            {
                AsyncOperationHandle<GameObject> loadAssetHandle = Addressables.InstantiateAsync(location);
                while (!loadAssetHandle.IsDone)
                {
                    //Debug.Log($"{currentLoadProgress}, {loadAssetHandle.PercentComplete}");
                    float totalProgress = (currentLoadProgress + loadAssetHandle.PercentComplete) / totalToLoad;
                    onProgress?.Invoke(totalProgress);
                    yield return null;
                }

                if (loadAssetHandle.Status == AsyncOperationStatus.Failed)
                {
                    onError?.Invoke($"Failed to load {PopupLabel} asset at {location.PrimaryKey}");
                    yield break;
                }

                string key = location.PrimaryKey.Replace("Popup/", "").Replace(".prefab", "");
                BasePopup basePopup = loadAssetHandle.Result.GetComponent<BasePopup>();
                if (basePopup != null)
                {
                    basePopup.gameObject.SetActive(false);
                    basePopup.CachedTransform.SetParent(_popupRoot, false);
                    _popupDict.Add(key, basePopup);
                }

                currentLoadProgress++;

                onProgress?.Invoke(currentLoadProgress / totalToLoad);
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke();
            Addressables.Release(handle);
        }

        public bool HasLoadingInfo(SceneName sceneName)
        {
            return _popupDict.Count == 0;
        }
    }
}
