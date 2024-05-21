using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class LevelFailedPopup : BasePopup
    {
        [SerializeField] private TextMeshProUGUI _titleText;

        private StringBuilder _titleTextBuilder = new StringBuilder();

        private int _level;
        private Action _onRetry;

        public void Init(int level, Action onRetry)
        {
            _level = level;
            _onRetry = onRetry;

            UpdateLevelText();
        }

        private void UpdateLevelText()
        {
            _titleTextBuilder.Length = 0;
            _titleTextBuilder.Append("Level ");
            _titleTextBuilder.Append(_level.ToString());
            _titleTextBuilder.Append(" Failed");
            _titleText.text = _titleTextBuilder.ToString();
        }

        public void OnRetryClick()
        {
            _onRetry?.Invoke();
            Close();
        }

        public override void Close()
        {
            base.Close();

            _onRetry = null;
        }
    }
}
