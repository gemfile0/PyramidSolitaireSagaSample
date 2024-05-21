using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardCollector
{
    public struct CardCollectorItemModel
    {
        public int itemIndex;
        public CardCollectorTriggerType type;
        public CardNumber cardNumber;
        public CardColor cardColor;
        public CardType cardType;
        public SubCardType subCardType;
        public int subCardTypeOption;
        public bool isBonusLabel;
    }

    public class CardCollectorModel : MonoBehaviour
    {
        public event Action<IEnumerable<CardCollectorItemModel>> onItemModelUpdated;

        public int ItemCount => _itemModelList.Count;
        public CardCollectorItemModel PeekItem => _itemModelList[_itemModelList.Count - 1];

        private List<CardCollectorItemModel> _itemModelList;

        private void Awake()
        {
            _itemModelList = new();
        }

        internal void AddItem(
            CardCollectorTriggerType type,
            CardNumber cardNumber,
            CardColor cardColor,
            CardType cardType,
            SubCardType subCardType,
            int subCardTypeOption,
            bool isBonusLabel)
        {
            _itemModelList.Add(new CardCollectorItemModel()
            {
                itemIndex = _itemModelList.Count,
                type = type,
                cardNumber = cardNumber,
                cardColor = cardColor,
                cardType = cardType,
                subCardType = subCardType,
                subCardTypeOption = subCardTypeOption,
                isBonusLabel = isBonusLabel
            });
            onItemModelUpdated?.Invoke(_itemModelList);
        }
    }
}
