using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper.UI;
using System;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.SaveLevel
{
    public class LevelSelectionEditorUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _levelInputField;
        [SerializeField] private TMP_Dropdown _levelDataPathDropdown;

        public event Action<int> onDropDownValueChanged;
        public event Action<int> onNavigateClick;
        public event Action<int> onValueChanged;
        public event Action onPingClick;
        public event Action onResetClick;

        private string _latestLevelStr;
        private LevelDataPath _latestLevelDataPath;

        private void Awake()
        {
            _levelDataPathDropdown.FillDropdownOptionValues<LevelDataPath>(50, 20);
        }

        public void UpdateLevelText(string levelStr)
        {
            _latestLevelStr = levelStr;
            _levelInputField.SetTextWithoutNotify(levelStr);
        }

        internal void UpdateLevelDataPath(LevelDataPath levelDataPath)
        {
            _latestLevelDataPath = levelDataPath;
            _levelDataPathDropdown.SetValueWithoutNotify((int)levelDataPath);
        }

        internal void RevertLevelText()
        {
            _levelInputField.SetTextWithoutNotify(_latestLevelStr);
        }

        internal void RevertLevelDataPath()
        {
            _levelDataPathDropdown.SetValueWithoutNotify((int)_latestLevelDataPath);
        }

        public void OnDropdownValueChanged(int index)
        {
            onDropDownValueChanged?.Invoke(index);
        }

        public void OnNavigateClick(int offset)
        {
            onNavigateClick?.Invoke(offset);
        }

        public void OnResetClick()
        {
            onResetClick?.Invoke();
        }

        public void OnValueChanged(string valueStr)
        {
            if (int.TryParse(valueStr, out int value))
            {
                onValueChanged?.Invoke(value);
            }
        }

        public void Ping()
        {
            onPingClick?.Invoke();
        }
    }
}
