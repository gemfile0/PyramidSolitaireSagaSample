using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardPool
{
    public class CardPoolItemGroup : MonoBehaviour
    {
        [SerializeField] private CardPoolItem _redItem;
        [SerializeField] private CardPoolItem _blackItem;

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

        public void UpdateUI(
            CardPoolItemModel deckItemModel,
            string cardNumberStr,
            string poolCountStr,
            string deckCountStr,
            Action<CardPoolItemModel, int> onValueChanged,
            Action<CardPoolItemModel, int> onButtonClick
        )
        {
            if (deckItemModel.cardColor == CardColor.Color_Red)
            {
                _redItem.UpdateUI(deckItemModel, cardNumberStr, poolCountStr, deckCountStr, onValueChanged, onButtonClick);
            }
            else
            {
                _blackItem.UpdateUI(deckItemModel, cardNumberStr, poolCountStr, deckCountStr, onValueChanged, onButtonClick);
            }
        }

        public void RevertUI(CardPoolItemModel itemModel)
        {
            if (itemModel.cardColor == CardColor.Color_Red)
            {
                _redItem.RevertUI();
            }
            else
            {
                _blackItem.RevertUI();
            }
        }

        internal void DisablePoolCountUI(CardPoolItemModel totalItemModel)
        {
            if (totalItemModel.cardColor == CardColor.Color_Red)
            {
                _redItem.DisablePoolCountUI();
            }
            else
            {
                _blackItem.DisablePoolCountUI();
            }
        }
    }
}
