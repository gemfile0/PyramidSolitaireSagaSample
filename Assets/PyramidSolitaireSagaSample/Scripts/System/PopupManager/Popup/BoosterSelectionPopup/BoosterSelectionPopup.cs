using PyramidSolitaireSagaSample.System.PreferenceDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class BoosterSelectionPopup : AnimationPopup
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private List<BoosterSelectionToggleItem> _toggleItemList;

        private Action _onInfoButtonClick;
        private Action<IEnumerable<GameBoosterType>> _onPlayButtonClick;

        public PreferenceDataKey PreferenceDataKey => PreferenceDataKey.GameBoosters;

        private StringBuilder _levelTextBuilder;
        private List<GameBoosterType> _selectedBoosterTypeList;

        public void Init(Action onInfoButtonClick, Action<IEnumerable<GameBoosterType>> onPlayButtonClick)
        {
            _onInfoButtonClick = onInfoButtonClick;
            _onPlayButtonClick = onPlayButtonClick;

            _levelTextBuilder = new StringBuilder();
            _selectedBoosterTypeList = new List<GameBoosterType>();

            for (int itemIndex = 0; itemIndex < _toggleItemList.Count; itemIndex++)
            {
                _toggleItemList[itemIndex].Init();
            }
        }

        public void OnInfoButtonClick()
        {
            _onInfoButtonClick?.Invoke();
        }

        public void OnPlayButtonClick()
        {
            _selectedBoosterTypeList.Clear();
            foreach (BoosterSelectionToggleItem toggleItem in _toggleItemList)
            {
                if (toggleItem.Toggle.isOn)
                {
                    _selectedBoosterTypeList.Add(toggleItem.BoosterType);
                }
            }

            _onPlayButtonClick?.Invoke(_selectedBoosterTypeList);
            Close();
        }

        internal void UpdateItemCount(GameBoosterType boosterType, int boosterCount)
        {
            BoosterSelectionToggleItem toggleItem = _toggleItemList.FirstOrDefault(item => item.BoosterType == boosterType);
            if (toggleItem != null)
            {
                boosterCount = 9; // todo : 실제 데이터 연동 후 제거할 라인
                toggleItem.UpdateCountText(boosterCount);
            }
        }

        internal void UpdateTitleLevel(int selectedLevel)
        {
            _levelTextBuilder.Length = 0;
            _levelTextBuilder.Append("Level ");
            _levelTextBuilder.Append(selectedLevel.ToString());

            _titleText.text = _levelTextBuilder.ToString();
        }
    }
}
