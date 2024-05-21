using PyramidSolitaireSagaSample.GameCommon.UI;
using PyramidSolitaireSagaSample.GameData;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class MissionPopup : AnimationPopup
    {
        [Header("MissionPopup")]
        [SerializeField] private List<MissionPopupItemUI> _missionPopupItemUIList;
        [SerializeField] private TextMeshProUGUI _missionText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private HardLevelDisplay _hardLevelDisplay;

        private StringBuilder _missionTextBuilder = new StringBuilder();

        public void Init()
        {
            foreach (MissionPopupItemUI itemUI in _missionPopupItemUIList)
            {
                itemUI.gameObject.SetActive(false);
            }

            _closeButton.interactable = true;
        }

        internal void UpdateItemUI(int itemIndex, GameMissionSkin gameMissionIcon, int missionCount)
        {
            MissionPopupItemUI itemUI = _missionPopupItemUIList[itemIndex];
            itemUI.gameObject.SetActive(true);

            _missionTextBuilder.Length = 0;
            _missionTextBuilder.Append(" x");
            _missionTextBuilder.Append(missionCount);

            itemUI.UpdateUI(gameMissionIcon, _missionTextBuilder.ToString());
        }

        public void UpdateTextUI(string missionStr)
        {
            _missionText.text = missionStr;
        }

        public override void Close()
        {
            _closeButton.interactable = false;

            base.Close();
        }

        internal void UpdateHardLevelDisplay(GameDifficultyType gameDifficultyType)
        {
            _hardLevelDisplay.UpdateHardLevelDisplay(gameDifficultyType);
        }
    }
}
