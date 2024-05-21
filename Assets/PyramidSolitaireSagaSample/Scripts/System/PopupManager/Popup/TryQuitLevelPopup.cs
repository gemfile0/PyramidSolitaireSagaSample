using PyramidSolitaireSagaSample.GameCommon.UI;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class TryQuitLevelPopup : BasePopup
    {
        [SerializeField] private HardLevelDisplay _hardLevelDisplay;

        private Action _onYes;
        private Action _onNo;

        public void Init(Action onYes, Action onNo)
        {
            _onYes = onYes;
            _onNo = onNo;
        }

        internal void UpdateHardLevelDisplay(GameDifficultyType gameDifficultyType)
        {
            _hardLevelDisplay.UpdateHardLevelDisplay(gameDifficultyType);
        }

        public void OnYesClick()
        {
            _onYes?.Invoke();
            Close();
        }

        public void OnNoClick()
        {
            _onNo?.Invoke();
            Close();
        }

        public override void Close()
        {
            base.Close();

            _onYes = null;
            _onNo = null;
        }
    }
}
