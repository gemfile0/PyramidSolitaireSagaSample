using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PyramidSolitaireSagaSample.GameData
{
    [Serializable]
    public struct BonusCardProbabilityData
    {
        public BonusCardType[] cardTypes;
        public float probability;
    }

    public enum BonusCardType
    {
        QuestionCard,
        CoinCard,
        JokerCard
    }

    [Serializable]
    public class BonusCardTypeSkin
    {
        [field: SerializeField] public BonusCardType BonusCardType { get; private set; }
        [field: SerializeField] public Sprite CardSprite { get; private set; }
    }

    [CreateAssetMenu(menuName = "Solitaire Makeover/Bonus Cards Data")]
    public class BonusCardsData : ScriptableObject
    {
        [Header("보너스 카드 이동 설정")]
        [SerializeField] private bool _showCardPath = true;
        [SerializeField] private float _cardMoveDuration = 0.5f;
        [SerializeField] private Ease _cardMoveEase = Ease.OutQuint;
        [SerializeField] private float _cardFadeDuration = 0.5f;
        [SerializeField] private Ease _cardFadeEase = Ease.OutQuint;

        [Header("보너스 카드 생성 확률")]
        [FormerlySerializedAs("_probabilityDatas")]
        [SerializeField] private BonusCardProbabilityData[] _bonusCardProbabilityDatas;

        [Header("보너스 레이블 카드 생성 확률")]
        [SerializeField] private BonusCardProbabilityData[] _bonusLabelCardProbabilityDatas;

        [Header("보너스 코인 카드 생성 확률")]
        [SerializeField] private BonusCardProbabilityData[] _bonusCoinCardProbabilityDatas;

        [Header("보너스 카드 스킨")]
        [SerializeField] private List<BonusCardTypeSkin> _bonusCardTypeSkinList;

        public bool ShowCardPath => _showCardPath;
        public float CardMoveDuration => _cardMoveDuration;
        public Ease CardMoveEase => _cardMoveEase;
        public float CardFadeDuration => _cardFadeDuration;
        public Ease CardFadeEase => _cardFadeEase;

        public BonusCardProbabilityData[] BonusCardProbabilityDatas => _bonusCardProbabilityDatas;
        public BonusCardProbabilityData[] BonusLabelCardProbabilityDatas => _bonusLabelCardProbabilityDatas;
        public BonusCardProbabilityData[] BonusCoinCardProbabilityDatas => _bonusCoinCardProbabilityDatas;

        private Dictionary<BonusCardType, BonusCardTypeSkin> BonusCardTypeSkinDict
        {
            get
            {
                if (_bonusCardTypeSkinDict == null)
                {
                    _bonusCardTypeSkinDict = new();
                    foreach (var bonusCardTypeSkin in _bonusCardTypeSkinList)
                    {
                        _bonusCardTypeSkinDict.Add(bonusCardTypeSkin.BonusCardType, bonusCardTypeSkin);
                    }
                }
                return _bonusCardTypeSkinDict;
            }
        }
        private Dictionary<BonusCardType, BonusCardTypeSkin> _bonusCardTypeSkinDict;

        public (int, BonusCardType[]) GenerateBonusCardType()
        {
            return _GenerateBonusCardType(_bonusCardProbabilityDatas);
        }

        public (int, BonusCardType[]) GenerateBonusLabelCardType()
        {
            return _GenerateBonusCardType(_bonusLabelCardProbabilityDatas);
        }

        public (int, BonusCardType[]) GenerateBonusCoinCardType()
        {
            return _GenerateBonusCardType(_bonusCoinCardProbabilityDatas);
        }

        private (int, BonusCardType[]) _GenerateBonusCardType(BonusCardProbabilityData[] probabilityDatas)
        {
            int dataIndex = -1;
            BonusCardType[] cardTypes = null;
            float randomValue = UnityEngine.Random.Range(0f, 100f);
            float cumulativeProbability = 0f;

            for (int i = 0; i < probabilityDatas.Length; i++)
            {
                BonusCardProbabilityData data = probabilityDatas[i];
                cumulativeProbability += data.probability;
                if (randomValue <= cumulativeProbability)
                {
                    dataIndex = i;
                    cardTypes = data.cardTypes;
                    break;
                }
            }

            return (dataIndex, cardTypes);
        }

        public BonusCardTypeSkin GetBonusCardTypeSkin(BonusCardType bonusCardType)
        {
            if (BonusCardTypeSkinDict.TryGetValue(bonusCardType, out BonusCardTypeSkin result) == false)
            {
                throw new NotImplementedException($"BonusCardTypeSkin not found : {bonusCardType}");
            }
            return result;
        }
    }
}
