using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PyramidSolitaireSagaSample.System.SceneTransition
{
    public enum SceneName
    {
        None,
        LevelPlayerScene,
        RuntimeLevelEditorScene,
        LoadLevelPlayerScene,
        LevelMapScene,
    }

    public interface ISpecificSceneTrigger
    {
        event Action<SceneName> requestSpecificScene;
    }

    public interface IMainSceneTrigger
    {
        event Action requestMainScene;
    }

    public interface ILoadingInfoProvider
    {
        LoadingInfo GetLoadingInfo();
        bool HasLoadingInfo(SceneName sceneName);
    }

    public class SceneTransitionManager : Singleton<SceneTransitionManager>, IGameObjectFinderSetter
    {
        [SerializeField] private SceneTransitionUI _sceneTransitionUiPrefab;
        [SerializeField] private Transform _sceneTransitionUiRoot;

        private IEnumerable<ISpecificSceneTrigger> _specificSceneTriggers;
        private IEnumerable<IMainSceneTrigger> _mainSceneTriggers;
        private IEnumerable<ILoadingInfoProvider> _loadingInfoProviders;
        private List<LoadingInfo> _loadingInfoList;
        private List<string> _loadingErrors;
        private GameObjectPool<SceneTransitionUI> _sceneTransitionUiPool;
        private SceneTransitionUI sceneTransitionUI;

        private SceneName _mainSceneName;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            if (_specificSceneTriggers != null
                || _mainSceneTriggers != null
                || _loadingInfoProviders != null)
            {
                UnlistenToTriggers();
                _loadingInfoProviders = null;
                _mainSceneTriggers = null;
                _loadingInfoProviders = null;
            }

            _specificSceneTriggers = finder.FindGameObjectOfType<ISpecificSceneTrigger>();
            _mainSceneTriggers = finder.FindGameObjectOfType<IMainSceneTrigger>();
            _loadingInfoProviders = finder.FindGameObjectOfType<ILoadingInfoProvider>();

            ListenToTriggers();
        }

        protected override void Awake()
        {
            base.Awake();

            if (_isDuplicated == false)
            {
                _loadingInfoList = new List<LoadingInfo>();
                _loadingErrors = new List<string>();
                _sceneTransitionUiPool = new GameObjectPool<SceneTransitionUI>(
                    parent: _sceneTransitionUiRoot,
                    prefab: _sceneTransitionUiPrefab.gameObject
                );
            }
        }

        private void OnDestroy()
        {
            if (_isDuplicated == false)
            {
                UnlistenToTriggers();
            }
        }

        private void ListenToTriggers()
        {
            foreach (ISpecificSceneTrigger trigger in _specificSceneTriggers)
            {
                trigger.requestSpecificScene += LoadSpecificScene;
            }

            foreach (IMainSceneTrigger trigger in _mainSceneTriggers)
            {
                trigger.requestMainScene += LoadMainScene;
            }
        }

        private void UnlistenToTriggers()
        {
            foreach (ISpecificSceneTrigger trigger in _specificSceneTriggers)
            {
                trigger.requestSpecificScene -= LoadSpecificScene;
            }

            foreach (IMainSceneTrigger trigger in _mainSceneTriggers)
            {
                trigger.requestMainScene -= LoadMainScene;
            }
        }

        private void LoadMainScene()
        {
            if (_mainSceneName != SceneName.None)
            {
                LoadSpecificScene(_mainSceneName);
            }
            else
            {
                LoadSpecificScene(SceneName.RuntimeLevelEditorScene);
            }
        }

        private void LoadSpecificScene(SceneName nextSceneName)
        {
            if (Enum.TryParse(SceneManager.GetActiveScene().name, out SceneName currSceneName))
            {
                if (currSceneName == SceneName.RuntimeLevelEditorScene
                    || currSceneName == SceneName.LevelMapScene)
                {
                    _mainSceneName = currSceneName;
                }
            }
            Debug.Log($"LoadSpecificScene : {_mainSceneName} -> {nextSceneName}");

            sceneTransitionUI = _sceneTransitionUiPool.Get();
            sceneTransitionUI.CachedTransform.SetParent(_sceneTransitionUiRoot);
            sceneTransitionUI.UpdateSlider(0);
            sceneTransitionUI.UpdateLoadingText("");
            sceneTransitionUI.PlayTransition(
                nextState: SceneTransitionState.FadeOut,
                onTransitionFinished: () => LoadLoadingInfoList(nextSceneName)
            );
        }

        public void LoadLoadingInfoList(SceneName sceneName)
        {
            _loadingInfoList.Clear();
            foreach (ILoadingInfoProvider provider in _loadingInfoProviders)
            {
                if (provider.HasLoadingInfo(sceneName))
                {
                    _loadingInfoList.Add(provider.GetLoadingInfo());
                }
            }

            StartCoroutine(LoadLoadingInfoListCoroutine(
                _loadingInfoList,
                onProgress: (float progress) =>
                {
                    sceneTransitionUI.UpdateSlider(progress);
                    sceneTransitionUI.UpdateLoadingText("Load addressables assets");
                },
                onComplete: () =>
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    SceneManager.LoadScene(sceneName.ToString());
                },
                showLog: false
            ));
        }

        private IEnumerator LoadLoadingInfoListCoroutine(
            List<LoadingInfo> loadingInfoList,
            Action<float> onProgress,
            Action onComplete,
            bool showLog = false
        )
        {
            // 1. 모든 아이템들의 로딩을 한 번에 시작
            foreach (LoadingInfo loadingInfo in loadingInfoList)
            {
                loadingInfo.Reset();

                string name = loadingInfo.Name;
                StartCoroutine(loadingInfo.Coroutine(
                    /* onProgress */ (float value) =>
                                     {
                                         loadingInfo.UpdateProgress(value);
                                     },
                    /* onComplete */ () =>
                                     {
                                         loadingInfo.UpdateDone(true);
                                         Debug.Log($"{name} is Done");
                                     },
                    /* onError */ (string error) =>
                                  {
                                      loadingInfo.UpdateError(error);
                                  }
                ));

                yield return null;
            }

            // 2. 로딩 화면 갱신
            float totalDuration = 0f;
            foreach (LoadingInfo loadingInfo in loadingInfoList)
            {
                if (showLog)
                {
                    Debug.Log($"duration: {loadingInfo.Name} : {loadingInfo.Duration}");
                }
                totalDuration += loadingInfo.Duration;
            }
            while (true)
            {
                float totalProgress = 0f;
                int totalDoneCount = 0;
                _loadingErrors.Clear();
                foreach (LoadingInfo itemInfo in loadingInfoList)
                {
                    totalProgress += itemInfo.Progress;
                    if (itemInfo.IsDone)
                    {
                        totalDoneCount++;

                        if (string.IsNullOrEmpty(itemInfo.Error) == false)
                        {
                            _loadingErrors.Add(itemInfo.Error);
                        }
                    }
                    else
                    {
                        if (showLog)
                        {
                            //Debug.Log($"{itemInfo.Name} is still in Loading");
                        }
                    }
                }

                if (showLog)
                {
                    Debug.Log($"progress: {totalDoneCount}, {totalProgress} / {totalDuration}");
                }

                float progress = totalProgress / totalDuration;
                if (progress >= 1f)
                {
                    progress = 1f;
                }

                if (totalDuration != 0)
                {
                    onProgress?.Invoke(progress);
                }

                bool isTotalDone = totalDoneCount == loadingInfoList.Count;
                if (isTotalDone)
                {
                    onComplete?.Invoke();
                    if (progress < 1f)
                    {
                        yield return new WaitForSeconds(.2f);
                    }
                    break;
                }

                yield return null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            sceneTransitionUI.PlayTransition(
                nextState: SceneTransitionState.FadeIn,
                onTransitionFinished: () =>
                {
                    sceneTransitionUI.UpdateLoadingText("");

                    _sceneTransitionUiPool.Release(sceneTransitionUI);
                    sceneTransitionUI = null;
                }
            );
        }
    }
}
