using System;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class WarningPopup : BasePopup
    {
        [SerializeField] private TextMeshProUGUI _messageText;

        private Action _onYes;
        private Action _onNo;

        internal void Init(string message, Action onYes, Action onNo)
        {
            _messageText.text = message;

            _onYes = onYes;
            _onNo = onNo;
        }

        public void OnNoClick()
        {
            _onNo?.Invoke();
            Close();
        }

        public void OnYesClick()
        {
            _onYes?.Invoke();
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
