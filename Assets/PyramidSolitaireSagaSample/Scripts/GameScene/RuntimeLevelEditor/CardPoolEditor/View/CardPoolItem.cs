using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using System;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardPool
{
    public class CardPoolItem : MonoBehaviour
    {
        [SerializeField] private string _pressedTrigger = "Pressed";
        [SerializeField] private string _disabledTrigger = "Disabled";

        [SerializeField] private TextMeshProUGUI _cardNumberText;
        [SerializeField] private TextMeshProUGUI _poolCountText;
        [SerializeField] private Animator _poolCountAnimator;
        [SerializeField] private TMP_InputField _deckCountInputField;
        [SerializeField] private Animator _deckCountAnimator;

        private CardPoolItemModel _deckItemModel;
        private string _latestCardNumberStr;
        private string _latestPoolCountStr;
        private string _latestDeckCountStr;
        private Action<CardPoolItemModel, int> _onValueChanged;
        private Action<CardPoolItemModel, int> _onButtonClick;

        internal void UpdateUI(
            CardPoolItemModel deckItemModel,
            string cardNumberStr,
            string poolCountStr,
            string deckCountStr,
            Action<CardPoolItemModel, int> onValueChanged,
            Action<CardPoolItemModel, int> onButtonClick
        )
        {
            _deckItemModel = deckItemModel;
            _latestCardNumberStr = cardNumberStr;
            _latestPoolCountStr = poolCountStr;
            _latestDeckCountStr = deckCountStr;
            _onValueChanged = onValueChanged;
            _onButtonClick = onButtonClick;

            UpdateCardNumber();
            UpdatePoolCount();
            UpdateDeckCount();
        }

        private void UpdateCardNumber()
        {
            if (_cardNumberText.text != _latestCardNumberStr)
            {
                _cardNumberText.text = _latestCardNumberStr;
            }
        }

        private void UpdatePoolCount()
        {
            if (_poolCountText.text != _latestPoolCountStr)
            {
                _poolCountText.text = _latestPoolCountStr;
                _poolCountAnimator.SetTrigger(_pressedTrigger);
            }
        }

        private void UpdateDeckCount()
        {
            if (_deckCountInputField.text != _latestDeckCountStr)
            {
                _deckCountInputField.SetTextWithoutNotify(_latestDeckCountStr);
                _deckCountAnimator.SetTrigger(_pressedTrigger);
            }
        }

        internal void RevertUI()
        {
            UpdateDeckCount();
        }

        public void OnValueChanged(string valueStr)
        {
            if (int.TryParse(valueStr, out int value))
            {
                _onValueChanged?.Invoke(_deckItemModel, value);
            }
        }

        public void OnButtonClick(int valueOffset)
        {
            _onButtonClick?.Invoke(_deckItemModel, valueOffset);
        }

        internal void DisablePoolCountUI()
        {
            _poolCountAnimator.SetTrigger(_disabledTrigger);
        }
    }
}
