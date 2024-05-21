using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.System.PreferenceDataManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Serialization;

namespace PyramidSolitaireSagaSample.System.LevelDataManager
{
    public interface ILevelRestorable
    {
        string RestoreLevelID { get; }
        void RestoreLevelData(string dataStr);
    }

    public interface ILevelSavable : ILevelRestorable
    {
        string SaveLevelData();
    }

    public interface ILevelDataManagerSetter
    {
        ILevelDataManager LevelDataManager { set; }
    }

    public interface ILevelDataManager
    {
        int CurrentLevel { get; }
        LevelDataPath LevelDataPath { get; }
        void UpdateCurrentLevel(int currentLevel);
        void UpdateUnlockedLevel(int nextLevel);
        void UpdateLevelDataPath(LevelDataPath levelDataPath);

        void RestoreLevelData(Action<bool> onRestoringComplete = null);
        void SaveLevelData();
        void PingFilePath();
        void LoadLevelDataPathSet(LevelDataPath levelDataPath, Action<IEnumerable<string>> onPathSetLoad);

        (int currentLevel, int highestUnlockedLevel, LevelDataPath levelDataPath) RestorePreferenceData();
        string SavePreferenceData();
    }

    [Serializable]
    public struct LevelDataManagerSaveData
    {
        public int currentLevel;
        [FormerlySerializedAs("levelFilePath")]
        public LevelDataPath levelDataPath;
        public int highestUnlockedLevel;
    }

    public partial class LevelDataManager : MonoBehaviour,
                                            IGameObjectFinderSetter,
                                            ILevelDataManager,
                                            IPreferenceSavable
    {
        [Header("Data")]
        [SerializeField] private LevelFileData _levelFileData;

        public int CurrentLevel => _currentLevel;
        public LevelDataPath LevelDataPath => _levelDataPath;

        public PreferenceDataKey PreferenceDataKey => PreferenceDataKey.LevelFileManager;

        public event Action<IPreferenceRestorable, string> requestSavePreferenceData;
        public event Func<IPreferenceRestorable, string> requestRestorePreferenceData;

        private int _currentLevel;
        private int _highestUnlockedLevel;
        private LevelDataPath _levelDataPath;

        private IEnumerable<ILevelRestorable> _levelRestorableList;
        private IEnumerable<ILevelSavable> _levelSavableList;
        private Action<bool> _onRestoringComplete;
        private Action<IEnumerable<string>> _onPathSetLoad;
        private Dictionary<LevelDataPath, HashSet<string>> _levelDataPathSetDict;

        private void Awake()
        {
            _currentLevel = 1;
            _highestUnlockedLevel = 1;
            _levelDataPath = LevelDataPath.Level;

            _levelDataPathSetDict = new();
            Addressables.LoadResourceLocationsAsync("LevelData").Completed += OnLevelDataLocationsLoaded;
        }

        private void OnLevelDataLocationsLoaded(AsyncOperationHandle<IList<IResourceLocation>> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (IResourceLocation location in handle.Result)
                {
                    string primaryKey = location.PrimaryKey;
                    string levelDataPathStr = primaryKey.Split('/')[0];
                    if (Enum.TryParse(levelDataPathStr, out LevelDataPath levelDataPath))
                    {
                        if (_levelDataPathSetDict.ContainsKey(levelDataPath) == false)
                        {
                            _levelDataPathSetDict.Add(levelDataPath, new HashSet<string>());
                        }

                        _levelDataPathSetDict[levelDataPath].Add(primaryKey);
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load resource locations.");
            }
        }

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _levelRestorableList = finder.FindGameObjectOfType<ILevelRestorable>();
            _levelSavableList = _levelRestorableList.OfType<ILevelSavable>();

            foreach (var popupOpener in finder.FindGameObjectOfType<ILevelDataManagerSetter>())
            {
                popupOpener.LevelDataManager = this;
            }
        }

        public void LoadLevelDataPathSet(LevelDataPath levelDataPath, Action<IEnumerable<string>> onPathSetLoad)
        {
            _onPathSetLoad = onPathSetLoad;
            StartCoroutine(LoadLevelDataPathSetCoroutine(levelDataPath));
        }

        private IEnumerator LoadLevelDataPathSetCoroutine(LevelDataPath levelDataPath)
        {
            while (_levelDataPathSetDict.Count == 0)
            {
                yield return null;
            }

            _onPathSetLoad?.Invoke(_levelDataPathSetDict[levelDataPath]);
        }

        public void RestoreLevelData(Action<bool> onRestoringComplete = null)
        {
            _onRestoringComplete = onRestoringComplete;

#if UNITY_EDITOR
            _RestoreLevelData();
#else
            StartCoroutine(RestoreLevelDataCoroutine());
#endif
        }

        private IEnumerator RestoreLevelDataCoroutine()
        {
            while (_levelDataPathSetDict.Count == 0)
            {
                yield return null;
            }

            string levelDataPath = GetRestoreLevelDataPath();
            if (_levelDataPathSetDict[_levelDataPath].Contains(levelDataPath))
            {
                Addressables.LoadAssetsAsync<LevelData>(levelDataPath, OnAssetLoaded);
            }
            else
            {
                Debug.LogWarning($"레벨 데이터가 존재하지 않습니다 : {levelDataPath}");
                _onRestoringComplete?.Invoke(false);
            }
        }

        private void OnAssetLoaded(LevelData levelData)
        {
            Debug.Log($"OnAssetLoaded: {levelData.name}");

            foreach (ILevelRestorable levelRestorable in _levelRestorableList)
            {
                string dataToLoad = null;
                if (levelData.SaveDataList != null && levelData.SaveDataList.Count > 0)
                {
                    dataToLoad = levelData.SaveDataList.FirstOrDefault(item => item.ID == levelRestorable.RestoreLevelID).Data;
                }
                levelRestorable.RestoreLevelData(dataToLoad);
            }

            _onRestoringComplete?.Invoke(true);
        }

        public string SavePreferenceData()
        {
            LevelDataManagerSaveData data = new()
            {
                currentLevel = _currentLevel,
                levelDataPath = _levelDataPath,
                highestUnlockedLevel = _highestUnlockedLevel
            };

            return JsonUtility.ToJson(data);
        }

        public (int, int, LevelDataPath) RestorePreferenceData()
        {
            string dataStr = requestRestorePreferenceData?.Invoke(this);
            if (string.IsNullOrEmpty(dataStr) == false)
            {
                LevelDataManagerSaveData saveData = JsonUtility.FromJson<LevelDataManagerSaveData>(dataStr);
                if (saveData.currentLevel > 0)
                {
                    _currentLevel = saveData.currentLevel;
                }
                if (saveData.highestUnlockedLevel > 0)
                {
                    _highestUnlockedLevel = saveData.highestUnlockedLevel;
                }
                if (saveData.levelDataPath != LevelDataPath.None)
                {
                    _levelDataPath = saveData.levelDataPath;
                }
            }

            return (_currentLevel, _highestUnlockedLevel, _levelDataPath);
        }

        public void UpdateCurrentLevel(int currentLevel)
        {
            _currentLevel = currentLevel;
            requestSavePreferenceData?.Invoke(this, SavePreferenceData());
        }

        public void UpdateUnlockedLevel(int value)
        {
            if (value > _highestUnlockedLevel)
            {
                _highestUnlockedLevel = value;
                requestSavePreferenceData?.Invoke(this, SavePreferenceData());
            }
        }

        public void UpdateLevelDataPath(LevelDataPath levelDataPath)
        {
            //Debug.Log($"UpdateLevelDataPath : {levelDataPath}");
            _levelDataPath = levelDataPath;
            requestSavePreferenceData?.Invoke(this, SavePreferenceData());
        }

        private string GetSaveLevelDataPath()
        {
            // e.g. {Assets/PyramidSolitaireSagaSample/GameDatas}/{Level/LevelData_1.asset}
            return $"{_levelFileData.GetLevelDataPath(_levelDataPath)}/{GetLevelDataFileName()}";
        }

        private string GetRestoreLevelDataPath()
        {   // e.g. {Level}/{LevelData_1.asset}
            return $"{_levelDataPath}/{GetLevelDataFileName()}";
        }

        private string GetLevelDataFileName()
        {
            // e.g. {LevelData}_{1}.asset
            return $"{_levelFileData.LevelDataName}_{_currentLevel}.asset";
        }
    }
}
