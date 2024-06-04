using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameData
{
    [Serializable]
    public class CardTypeSpriteData
    {
        [SerializeField] private List<Sprite> _blackCardSpriteList;
        [SerializeField] private List<Sprite> _redCardSpriteList;
        [SerializeField] private List<Sprite> _randomColorCardSpriteList;
        [SerializeField] private List<Sprite> _randomNumberCardSpriteList;
        [SerializeField] private Sprite _faceDownSprite;
        [SerializeField] private Sprite _jokerCardSprite;

        internal Sprite GetCardSprite(CardNumber cardNumber, CardColor cardColor, CardFace cardFace)
        {
            Sprite result = null;
            if (cardFace == CardFace.Face_Down)
            {
                result = _faceDownSprite;
            }
            else if (cardNumber == CardNumber.Num_Random)
            {
                int cardColorValue = (int)cardColor;
                result = _randomNumberCardSpriteList[cardColorValue];
            }
            else if (cardNumber == CardNumber.Num_Joker)
            {
                result = _jokerCardSprite;
            }
            else if (cardColor == CardColor.Color_Random)
            {
                int cardNumberValue = (int)cardNumber - 1;
                result = _randomColorCardSpriteList[cardNumberValue];
            }
            else if (cardColor == CardColor.Color_Black)
            {
                int cardNumberValue = (int)cardNumber - 1;
                result = _blackCardSpriteList[cardNumberValue];
            }
            else if (cardColor == CardColor.Color_Red)
            {
                int cardNumberValue = (int)cardNumber - 1;
                result = _redCardSpriteList[cardNumberValue];
            }
            return result;
        }
    }

    [CreateAssetMenu(menuName = "PyramidSolitaireSagaSample/Card Data")]
    public class CardData : ScriptableObject
    {
        [SerializeField] private Vector2 _cardSize;
        [SerializeField] private float _cardPileGap = 0.005f;

        [Header("카드 이동 설정")]
        [SerializeField] private float _cardMoveDuration = 0.5f;
        [SerializeField] private Ease _cardMoveEase = Ease.OutQuint;
        [SerializeField] private float _cardScaleDuration = 0.5f;
        [SerializeField] private Ease _cardScaleEase = Ease.OutQuint;
        [SerializeField] private Vector2 _cardScale = new Vector2(1.1f, 1.1f);

        [Header("게임 시작할 때 카드 이동 설정")]
        [SerializeField] private float _cardMoveDelay = 0.2f;

        [Header("카드 스킨 해제 설정")]
        [SerializeField] private float _cardUnpackDelay = 0.5f;
        [SerializeField] private float _cardUnpackDuration = 0.5f;
        [SerializeField] private Ease _cardUnpackEase = Ease.InOutQuad;

        [Header("Card Sprite")]
        [SerializeField] CardTypeSpriteData _whiteCardSpriteData;
        [SerializeField] CardTypeSpriteData _goldCardSpriteData;
        [SerializeField] CardTypeSpriteData _blueCardSpriteData;

        [Header("Sub Card Type - Lock Color")]
        [SerializeField] private List<Color> _subCardTypeLockColorList;

        [Header("Sub Card Type - Tied Color")]
        [SerializeField] private List<Color> _subCardTypeTiedColorList;

        public Vector2 CardSize => _cardSize;
        public float CardPileGap => _cardPileGap;
        public float CardMoveDuration => _cardMoveDuration;
        public Ease CardMoveEase => _cardMoveEase;
        public float CardScaleDuration => _cardScaleDuration;
        public Ease CardScaleEase => _cardScaleEase;
        public Vector2 CardScale => _cardScale;

        public float CardMoveDelay => _cardMoveDelay;

        public float CardUnpackDelay => _cardUnpackDelay;
        public float CardUnpackDuration => _cardUnpackDuration;
        public Ease CardUnpackEase => _cardUnpackEase;

        public Sprite GetCardSprite(CardNumber cardNumber, CardColor cardColor, CardFace cardFace, CardType cardType)
        {
            Sprite result = null;
            if (cardType == CardType.Type_None)
            {
                result = _whiteCardSpriteData.GetCardSprite(cardNumber, cardColor, cardFace);
            }
            else if (cardType == CardType.Gold)
            {
                result = _goldCardSpriteData.GetCardSprite(cardNumber, cardColor, cardFace);
            }
            else if (cardType == CardType.Blue)
            {
                result = _blueCardSpriteData.GetCardSprite(cardNumber, cardColor, cardFace);
            }

            return result;
        }
    }
}
