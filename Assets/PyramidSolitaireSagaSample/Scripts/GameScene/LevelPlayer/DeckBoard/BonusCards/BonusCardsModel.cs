using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCards
{
    public struct BonusCardItemModel
    {
        public CardNumber cardNumber;
        public CardColor cardColor;
    }

    public enum BonusCardType
    {
        BonusCard,
        BonusLabelCard,
        BonusCoinCard,
    }

    public class BonusCardsModel : MonoBehaviour
    {
        public event Action<int> onCountChanged;

        public int NextStartingBonusCardIndex { get; private set; }
        public int NextStartingCollectedCardIndex { get; private set; }
        public IEnumerable<BonusCardType> BonusCardSet => _bonusCardSet;
        private HashSet<BonusCardType> _bonusCardSet;
        private int _count;

        private void Awake()
        {
            NextStartingBonusCardIndex = 0;
            NextStartingCollectedCardIndex = 0;
            _count = -1;
            _bonusCardSet = new HashSet<BonusCardType>();
        }

        internal void UpdateBonusCardsCount(int nextCount, int nextStartingIndex)
        {
            //Debug.Log($"UpdateBonusCardsCount: {_count} -> {nextCount}");
            if (nextCount != _count)
            {
                UpdateBonusCardsCount_WithoutNotify(nextCount);
                onCountChanged?.Invoke(_count);

                if (nextCount == 5)
                {
                    _bonusCardSet.Add(BonusCardType.BonusCard);
                    NextStartingBonusCardIndex = nextStartingIndex;
                    UpdateBonusCardsCount_WithoutNotify(0);
                }
            }
        }

        private void UpdateBonusCardsCount_WithoutNotify(int nextCount)
        {
            _count = nextCount;
        }

        internal void UpdateBonusLabelCardIndex(int itemIndex)
        {
            _bonusCardSet.Add(BonusCardType.BonusLabelCard);

            NextStartingCollectedCardIndex = itemIndex + 1;
        }

        internal void UpdateGoldCardIndex(int itemIndex)
        {
            _bonusCardSet.Add(BonusCardType.BonusCoinCard);

            NextStartingCollectedCardIndex = itemIndex + 1;
        }

        public void AddBonusCard(BonusCardType bonusCardType)
        {
            _bonusCardSet.Add(bonusCardType);
        }

        internal void ResetBonusCardSet()
        {
            _bonusCardSet.Clear();
        }
    }
}
