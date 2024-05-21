using System;
using TMPro;
using UnityEngine;
using PyramidSolitaireSagaSample.Helper.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.LevelPreset
{
    public class LevelPresetEditorUI : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _gameDifficultyDropdown;
        [SerializeField] private TMP_InputField _jokerCountInputField;

        public event Action<GameDifficultyType> onGameDifficultyTypeChanged;
        public event Action<int> onJokerCountStepperClick;
        public event Action<int> onJokerCountValueChanged;

        private string _latestJokderCountStr;

        private void Awake()
        {
            _gameDifficultyDropdown.FillDropdownOptionValues<GameDifficultyType>(50, 20);
        }

        internal void UpdateGameDifficulty(int value)
        {
            _gameDifficultyDropdown.value = value;
        }

        public void UpdateJokerCountText(string jokerCountStr)
        {
            _latestJokderCountStr = jokerCountStr;
            _jokerCountInputField.SetTextWithoutNotify(jokerCountStr);
        }

        internal void RevertJokerCountText()
        {
            _jokerCountInputField.SetTextWithoutNotify(_latestJokderCountStr);
        }

        public void OnGameDifficultyValueChanged(int value)
        {
            var type = (GameDifficultyType)value;
            onGameDifficultyTypeChanged?.Invoke(type);
        }

        public void OnJokerCountStepperClick(int offset)
        {
            onJokerCountStepperClick?.Invoke(offset);
        }

        public void OnJokerCountValueChanged(string valueStr)
        {
            if (int.TryParse(valueStr, out int value))
            {
                onJokerCountValueChanged?.Invoke(value);
            }
        }
    }
}
