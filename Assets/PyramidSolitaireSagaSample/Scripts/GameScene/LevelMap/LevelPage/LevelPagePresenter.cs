using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelMap.LevelPage
{
    public class LevelPagePresenter : MonoBehaviour,
                                      ILevelDataManagerSetter
    {
        [SerializeField] private LevelDataPath _levelDataPath = LevelDataPath.LevelSample;
        [SerializeField] private LevelPageModel _levelPageModel;
        [SerializeField] private LevelPageUI _levelPageUI;

        public event Action<int> onLevelButtonClick
        {
            add => _levelPageUI.onLevelButtonClick += value;
            remove => _levelPageUI.onLevelButtonClick -= value;
        }

        public ILevelDataManager LevelDataManager { private get; set; }
        private int _highestUnlockedLevel;

        private void Start()
        {
            LevelDataManager.LoadLevelDataPathSet(_levelDataPath, OnPathSetLoad);

            (int currentLevel, int highestUnlockedLevel, LevelDataPath latestLevelDataPath) = LevelDataManager.RestorePreferenceData();
            LevelDataManager.UpdateLevelDataPath(_levelDataPath);
            _highestUnlockedLevel = highestUnlockedLevel;
        }

        private void OnEnable()
        {
            _levelPageModel.onPathListUpdated += UpdatePageUI;
        }

        private void OnDisable()
        {
            _levelPageModel.onPathListUpdated -= UpdatePageUI;
        }

        private void UpdatePageUI(IEnumerable<string> pathList)
        {
            _levelPageUI.UpdateUI(levelButtonCount: pathList.Count(), highestUnlockedLevelIndex: _highestUnlockedLevel - 1);
        }

        private void OnPathSetLoad(IEnumerable<string> pathSet)
        {
            List<string> sortedPathList = SortByNumericIndex(pathSet);
            _levelPageModel.UpdatePathList(sortedPathList);
        }

        private List<string> SortByNumericIndex(IEnumerable<string> pathSet)
        {
            int prefixLength = $"{_levelDataPath}/LevelData_".Length;
            int suffixLength = ".asset".Length;

            return pathSet
                .Select(_path => new
                {
                    path = _path,
                    index = int.Parse(_path.Substring(prefixLength, _path.Length - prefixLength - suffixLength))
                })
                .OrderBy(x => x.index)
                .Select(x => x.path)
                .ToList();
        }

        internal void SetButtonInteractableAsFalse()
        {
            _levelPageUI.SetButtonInteractable(false);
        }
    }
}
