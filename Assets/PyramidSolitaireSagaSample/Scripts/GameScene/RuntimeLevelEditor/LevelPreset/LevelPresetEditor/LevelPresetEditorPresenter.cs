using PyramidSolitaireSagaSample.LevelPlayer.LevelPreset;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.LevelPreset
{
    public class LevelPresetEditorPresenter : MonoBehaviour, ILevelSavable
    {
        [Header("Model")]
        [SerializeField] private LevelPresetModel _levelPresetModel;

        [Header("View")]
        [SerializeField] private LevelPresetEditorUI _levelPresetUI;

        public event Action<GameDifficultyType> onGameDifficultyChanged
        {
            add { _levelPresetModel.onGameDifficultyChanged += value; }
            remove { _levelPresetModel.onGameDifficultyChanged -= value; }
        }

        public event Action<int> onJokerCountChanged
        {
            add { _levelPresetModel.onJokerCountChanged += value; }
            remove { _levelPresetModel.onJokerCountChanged -= value; }
        }

        public event Action<int> onCurrentLevelChanged
        {
            add { _levelPresetModel.onCurrentLevelChanged += value; }
            remove { _levelPresetModel.onCurrentLevelChanged -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.LevelPreset;

        private void OnEnable()
        {
            _levelPresetModel.onJokerCountChanged += OnJokerCountChanged;

            _levelPresetUI.onGameDifficultyTypeChanged += _levelPresetModel.ChangeGameDifficultyType;
            _levelPresetUI.onJokerCountStepperClick += OnJokerCountStepperClick;
            _levelPresetUI.onJokerCountValueChanged += OnJokerCountValueChanged;
        }

        private void OnDisable()
        {
            _levelPresetModel.onJokerCountChanged -= OnJokerCountChanged;

            _levelPresetUI.onGameDifficultyTypeChanged -= _levelPresetModel.ChangeGameDifficultyType;
            _levelPresetUI.onJokerCountStepperClick -= OnJokerCountStepperClick;
            _levelPresetUI.onJokerCountValueChanged -= OnJokerCountValueChanged;
        }

        internal void UpdateSelectedLevel(int value)
        {
            _levelPresetModel.UpdateCurrentLevel(value);
        }

        internal void RestoreSelectedLevel(int value)
        {
            _levelPresetModel.UpdateCurrentLevelWithoutNotify(value);
        }

        private void OnJokerCountChanged(int jokerCount)
        {
            _levelPresetUI.UpdateJokerCountText(jokerCount.ToString());
        }

        private void OnJokerCountStepperClick(int offset)
        {
            int nextCount = _levelPresetModel.JokerCount + offset;
            if (nextCount >= 0 && nextCount <= 99)
            {
                _levelPresetModel.UpdateJokerCount(nextCount);
            }
        }

        private void OnJokerCountValueChanged(int nextCount)
        {
            if (nextCount >= 0 && nextCount <= 99)
            {
                _levelPresetModel.UpdateJokerCount(nextCount);
            }
            else
            {
                _levelPresetUI.RevertJokerCountText();
            }
        }

        public string SaveLevelData()
        {
            LevelPresetSaveData data = new()
            {
                level = _levelPresetModel.Level,
                jokerCount = _levelPresetModel.JokerCount,
                gameDifficultyType = _levelPresetModel.GameDifficultyType
            };

            //Debug.Log($"GetSaveData : {JsonUtility.ToJson(data)}");
            return JsonUtility.ToJson(data);
        }

        public void RestoreLevelData(string data)
        {
            _levelPresetModel.RestoreSaveData(data);

            _levelPresetUI.UpdateJokerCountText(_levelPresetModel.JokerCount.ToString());
            _levelPresetUI.UpdateGameDifficulty((int)_levelPresetModel.GameDifficultyType);
        }

        internal void NewLevelData()
        {
            _levelPresetModel.ChangeGameDifficultyType(GameDifficultyType.Normal);
            _levelPresetModel.UpdateJokerCount(0);
        }
    }
}
