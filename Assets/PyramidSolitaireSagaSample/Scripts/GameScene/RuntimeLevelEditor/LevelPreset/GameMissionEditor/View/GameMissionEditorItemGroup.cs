using PyramidSolitaireSagaSample.Helper.UI;
using System;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    public class GameMissionEditorItemGroup : MonoBehaviour, ILayoutRebuilderInvoker
    {
        [SerializeField] private TMP_Dropdown _missionTypeDropdown;
        [SerializeField] private TMP_InputField _missionCountInputField;
        [SerializeField] private GameObject _countSelectionGroup;

        public RectTransform CachedRectTransform
        {
            get
            {
                if (_cachedRectTransform == null)
                {
                    _cachedRectTransform = GetComponent<RectTransform>();
                }
                return _cachedRectTransform;
            }
        }
        private RectTransform _cachedRectTransform;

        private int _itemGroupIndex;
        private int _latestMissionTypeValue;
        private int _latestMissionCount;
        private Action<int> _onRemoveMissionClick;
        private Action<int, GameMissionType> _onMissionTypeChanged;
        private Action<int, int> _onMissionCountChanged;
        private Action<int, int> _onMissionStepperClick;

        public event Action requestLayoutRebuild;

        internal void Init(
            int itemGroupIndex,
            int missionTypeValue,
            int missionCount,
            Action<int> onRemoveMissionClick,
            Action<int, GameMissionType> onMissionTypeChanged,
            Action<int, int> onMissionCountChanged,
            Action<int, int> onMissionStepperClick
        )
        {
            _itemGroupIndex = itemGroupIndex;
            _latestMissionTypeValue = missionTypeValue;
            _latestMissionCount = missionCount;
            _onRemoveMissionClick = onRemoveMissionClick;
            _onMissionTypeChanged = onMissionTypeChanged;
            _onMissionCountChanged = onMissionCountChanged;
            _onMissionStepperClick = onMissionStepperClick;

            UpdateMissionType();
            UpdateMissionCount();
        }

        private void UpdateMissionType()
        {
            _missionTypeDropdown.FillDropdownOptionValues<GameMissionType>(50, 20);
            _missionTypeDropdown.value = _latestMissionTypeValue;
        }

        private void UpdateMissionCount()
        {
            bool prevCountSelectionGroupActive = _countSelectionGroup.activeSelf;
            GameMissionType gameMissionType = (GameMissionType)_missionTypeDropdown.value;
            bool nextCountSelectionGroupActive = gameMissionType != GameMissionType.GoldCard
                                                 && gameMissionType != GameMissionType.BlueCard;
            _countSelectionGroup.SetActive(nextCountSelectionGroupActive);
            requestLayoutRebuild?.Invoke();

            string _latestMissionCountStr = _latestMissionCount.ToString();
            if (_missionCountInputField.text != _latestMissionCountStr)
            {
                _missionCountInputField.SetTextWithoutNotify(_latestMissionCountStr);
            }
        }

        public void UpdateItemGroupIndex(int itemGroupIndex)
        {
            _itemGroupIndex = itemGroupIndex;
        }

        public void OnMissionTypeChanged(int value)
        {
            _onMissionTypeChanged?.Invoke(_itemGroupIndex, (GameMissionType)value);
        }

        public void OnRemoveMissionClick()
        {
            _onRemoveMissionClick?.Invoke(_itemGroupIndex);
        }

        public void OnMissionCountChanged(string valueStr)
        {
            if (int.TryParse(valueStr, out int value))
            {
                _onMissionCountChanged?.Invoke(_itemGroupIndex, value);
            }
        }

        public void OnMissionStepperClick(int valueOffset)
        {
            _onMissionStepperClick?.Invoke(_itemGroupIndex, valueOffset);
        }

        internal void RevertUI()
        {
            UpdateMissionCount();
        }

        internal void UpdateUI(int typeValue, int count)
        {
            _latestMissionTypeValue = typeValue;
            _latestMissionCount = count;

            UpdateMissionType();
            UpdateMissionCount();
        }
    }
}
