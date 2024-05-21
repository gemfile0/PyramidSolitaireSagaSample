using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    [Serializable]
    public class CommonCardInfo
    {
        [field: SerializeField] public CardNumber CardNumber { get; private set; }
        [field: SerializeField] public CardColor CardColor { get; private set; }
        [field: SerializeField] public CardFace CardFace { get; private set; }
        [field: SerializeField] public CardType CardType { get; private set; }
        [field: SerializeField] public SubCardType SubCardType { get; private set; }
        [field: SerializeField] public int SubCardTypeOption { get; private set; }
        [field: SerializeField] public int SubCardTypeOption2 { get; private set; }
        [field: SerializeField] public bool IsBonusLabel { get; private set; }
        public SubCardType PrevSubCardType { get; private set; }

        public CommonCardInfo(
            CardNumber cardNumber,
            CardColor cardColor,
            CardFace cardFace,
            CardType cardType,
            SubCardType
            subCardType,
            int subCardTypeOption,
            int subCardTypeOption2,
            bool isBonusLabel
        )
        {
            UpdateInfo(cardNumber, cardColor, cardFace, cardType, subCardType, subCardTypeOption, subCardTypeOption2, isBonusLabel);
        }

        internal void UpdateInfo(CardSelectionInfo cardSelectionInfo)
        {
            UpdateInfo(
                cardSelectionInfo.cardNumber,
                cardSelectionInfo.cardColor,
                cardSelectionInfo.cardFace,
                cardSelectionInfo.cardType,
                cardSelectionInfo.subCardType,
                cardSelectionInfo.subCardTypeOption,
                cardSelectionInfo.subCardTypeOption2,
                cardSelectionInfo.isBonusLabel
            );
        }

        internal void UpdateInfo(
            CardNumber cardNumber,
            CardColor cardColor,
            CardFace cardFace,
            CardType cardType,
            SubCardType subCardType,
            int subCardTypeOption,
            int subCardTypeOption2,
            bool isBonusLabel)
        {
            CardNumber = cardNumber;
            CardColor = cardColor;
            CardFace = cardFace;
            CardType = cardType;
            PrevSubCardType = SubCardType;
            SubCardType = subCardType;
            SubCardTypeOption = subCardTypeOption;
            SubCardTypeOption2 = subCardTypeOption2;
            IsBonusLabel = isBonusLabel;
        }

        public override string ToString()
        {
            return $"{CardNumber}, {CardColor}, {CardFace}, {CardType}, {SubCardType}, {SubCardTypeOption}, {SubCardTypeOption2}, {IsBonusLabel}";
        }
    }
}
