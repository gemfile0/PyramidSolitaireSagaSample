using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.Input;
using PyramidSolitaireSagaSample.LevelPlayer.LootAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCards
{
    public enum BonusCardsTransitionState
    {
        None,
        Started,
        Ended,
    }

    public class BonusCardsPresenter : MonoBehaviour,
                                       ICameraResizerSettter,
                                       ILootAnimationTrigger,
                                       ILevelPlayerInputLimiter
    {
        public enum ItemState
        {
            BonusCardLaunch,
            LootAnimation
        }

        [Header("Data")]
        [SerializeField] private CardData _cardData;
        [SerializeField] private BonusCardsData _bonusCardsData;

        [Header("Model")]
        [SerializeField] private BonusCardsModel _bonusCardsModel;

        [Header("View")]
        [SerializeField] private float _launchCardDelay = 0.5f;
        [SerializeField] private BonusCardsRenderer _bonusCardsRenderer;
        [SerializeField] private BonusCardsLauncher _bonusCardsLauncher;

        public event Action<BonusCardsTransitionState> onBonusCardsTransitionState;
        public event Action<LootAnimationType, LootAnimationInfo> requestLootAnimation;
        public event Action<LevelPlayerInputLimitType> requestLimitInput;
        public event Action requestUnlimitInput;

        public ICameraResizer CameraResizer { private get; set; }

        private Dictionary<GameData.BonusCardType, LootAnimationType> _lootAnimationTypeDict;
        private Dictionary<int, ItemState> _itemStateDict;
        private IEnumerable<CardCollectorItemModel> _cardCollectorItemModel;
        private bool _shouldLimitInput;
        private IObjectPool<List<GameData.BonusCardType>> _bonusCardTypeListPool;
        private bool _isLevelCleared;
        private int _bonusCardIndex;

        private void Awake()
        {
            _lootAnimationTypeDict = new()
            {
                { GameData.BonusCardType.QuestionCard, LootAnimationType.QuestionCard },
                { GameData.BonusCardType.CoinCard, LootAnimationType.Coin },
                { GameData.BonusCardType.JokerCard, LootAnimationType.JokerCard }
            };

            _itemStateDict = new();
            _bonusCardTypeListPool = new ObjectPool<List<GameData.BonusCardType>>(
                () => new List<GameData.BonusCardType>()
            );

            _bonusCardIndex = 0;
        }

        private void OnEnable()
        {
            _bonusCardsModel.onCountChanged += _bonusCardsRenderer.UpdateRenderer;
        }

        private void OnDisable()
        {
            _bonusCardsModel.onCountChanged -= _bonusCardsRenderer.UpdateRenderer;
        }

        private void Start()
        {
            (Vector3 cameraCenterPosition, Vector2 cameraHalfSize) = CameraResizer.GetCameraHalfSize();

            float cardHalfMagnitude = (_cardData.CardSize / 2).magnitude;
            _bonusCardsLauncher.UpdateCameraHalfSize(
                cameraCenterPosition,
                cameraHalfSize,
                cardHalfMagnitude
            );
        }

        private IEnumerator CreateBonusCardCoroutine(bool containsBonusCard, List<GameData.BonusCardType> bonusCardTypeList)
        {
            onBonusCardsTransitionState?.Invoke(BonusCardsTransitionState.Started);
            UpdateShouldLimitInput(bonusCardTypeList.Any((GameData.BonusCardType bonusCardType) => bonusCardType != GameData.BonusCardType.CoinCard));

            if (containsBonusCard)
            {
                _bonusCardsRenderer.FadeOutFrontGauge();
            }
            yield return new WaitForSeconds(_launchCardDelay);

            foreach (GameData.BonusCardType bonusCardType in bonusCardTypeList)
            {
                int dictItemIndex = GetBonusCardIndex();
                _bonusCardsLauncher.LaunchCard(
                    _bonusCardsData.ShowCardPath,
                    _bonusCardsData.CardMoveDuration,
                    _bonusCardsData.CardMoveEase,
                    _bonusCardsData.CardFadeDuration,
                    _bonusCardsData.CardFadeEase,
                    cardSprite: _bonusCardsData.GetBonusCardTypeSkin(bonusCardType).CardSprite,
                    onItemClick: (Vector3 itemPosition, Quaternion itemRotation) =>
                    {
                        _itemStateDict[dictItemIndex] = ItemState.LootAnimation;
                        BonusCardTypeSkin itemSkin = _bonusCardsData.GetBonusCardTypeSkin(bonusCardType);
                        LootAnimationType lootAnimationType = _lootAnimationTypeDict[bonusCardType];
                        Sprite cardSprite = bonusCardType == GameData.BonusCardType.QuestionCard || bonusCardType == GameData.BonusCardType.JokerCard ?
                                            itemSkin.CardSprite :
                                            null;
                        long bonusCount = bonusCardType == GameData.BonusCardType.CoinCard ? 20 : 1;
                        var info = new LootAnimationInfo(bonusCount, itemSkin.CardSprite, itemPosition, itemRotation, () => OnLootAnimationComplete(dictItemIndex));
                        requestLootAnimation?.Invoke(lootAnimationType, info);
                    },
                    onItemComplete: () =>
                    {
                        if (bonusCardTypeList != null)
                        {
                            _bonusCardTypeListPool.Release(bonusCardTypeList);
                            bonusCardTypeList = null;
                        }
                        OnLaunchComplete(dictItemIndex);
                    }
                );
                //Debug.Log($"CreateBonusCardCoroutine : {dictItemIndex}, {bonusCardType}, {_bonusCardTypeList.Count}");

                _itemStateDict.Add(dictItemIndex, ItemState.BonusCardLaunch);
            }
        }

        private int GetBonusCardIndex()
        {
            int result = _bonusCardIndex;
            _bonusCardIndex += 1;
            return result;
        }

        private void UpdateShouldLimitInput(bool value)
        {
            _shouldLimitInput = value;
            if (_shouldLimitInput)
            {
                requestLimitInput?.Invoke(LevelPlayerInputLimitType.UI);
            }
        }

        private void OnLootAnimationComplete(int itemIndex)
        {
            if (_itemStateDict.TryGetValue(itemIndex, out ItemState state))
            {
                if (state == ItemState.LootAnimation)
                {
                    _itemStateDict.Remove(itemIndex);
                }
            }

            CheckIfItemTransitionComplete();
        }

        private void OnLaunchComplete(int itemIndex)
        {
            if (_itemStateDict.TryGetValue(itemIndex, out ItemState state))
            {
                if (state == ItemState.BonusCardLaunch)
                {
                    _itemStateDict.Remove(itemIndex);
                }
            }

            CheckIfItemTransitionComplete();
        }

        private void CheckIfItemTransitionComplete()
        {
            if (_itemStateDict.Count == 0)
            {
                if (_shouldLimitInput)
                {
                    requestUnlimitInput?.Invoke();
                }
                onBonusCardsTransitionState?.Invoke(BonusCardsTransitionState.Ended);
            }
        }

        internal void UpdateTileBoundHalfSize(Vector3 tileBoundCenterPosition, Vector3 tileBoundHalfSize)
        {
            //Debug.Log($"UpdateTileBoundSize : {tileBoundCenterPosition}, {tileBoundSize}");
            _bonusCardsLauncher.UpdateTileBoundHalfSize(tileBoundCenterPosition, tileBoundHalfSize);
        }

        internal void UpdateCardCollectorItemModelList(IEnumerable<CardCollectorItemModel> cardCollectorItemModel)
        {
            _cardCollectorItemModel = cardCollectorItemModel;

            _bonusCardsModel.ResetBonusCardSet();
            UpdateBonusCardCount();
            UpdateLatestCollectedCard();
            CheckIfCreateBonusCard();
        }

#if UNITY_EDITOR
        public void CreateBonusCard()
        {
            _bonusCardsModel.ResetBonusCardSet();
            _bonusCardsModel.AddBonusCard(BonusCardType.BonusCard);
            CheckIfCreateBonusCard();
        }

        public void CreateBonusCoinCard()
        {
            _bonusCardsModel.ResetBonusCardSet();
            _bonusCardsModel.AddBonusCard(BonusCardType.BonusCoinCard);
            CheckIfCreateBonusCard();
        }
#endif

        private void CheckIfCreateBonusCard()
        {
            bool containsBonusCard = false;
            int dataIndex;
            GameData.BonusCardType[] bonusCardTypes;

            List<GameData.BonusCardType> bonusCardTypeList = _bonusCardTypeListPool.Get();
            bonusCardTypeList.Clear();

            foreach (BonusCardType bonusCardType in _bonusCardsModel.BonusCardSet)
            {
                switch (bonusCardType)
                {
                    case BonusCardType.BonusCard:
                        containsBonusCard = true;
                        (dataIndex, bonusCardTypes) = _bonusCardsData.GenerateBonusCardType();
                        bonusCardTypeList.AddRange(bonusCardTypes);
                        break;

                    case BonusCardType.BonusLabelCard:
                        (dataIndex, bonusCardTypes) = _bonusCardsData.GenerateBonusLabelCardType();
                        bonusCardTypeList.AddRange(bonusCardTypes);
                        break;

                    case BonusCardType.BonusCoinCard:
                        (dataIndex, bonusCardTypes) = _bonusCardsData.GenerateBonusCoinCardType();
                        bonusCardTypeList.AddRange(bonusCardTypes);
                        break;
                }
            }
            if (bonusCardTypeList.Count > 0)
            {
                StartCoroutine(CreateBonusCardCoroutine(containsBonusCard, bonusCardTypeList));
            }
            else
            {
                _bonusCardTypeListPool.Release(bonusCardTypeList);
            }
        }

        private void UpdateLatestCollectedCard()
        {
            int nextStartingCollectedCardIndex = _bonusCardsModel.NextStartingCollectedCardIndex;
            foreach (CardCollectorItemModel itemModel in _cardCollectorItemModel)
            {
                if (itemModel.itemIndex < nextStartingCollectedCardIndex)
                {
                    continue;
                }

                if (itemModel.type == CardCollectorTriggerType.GameBoard)
                {
                    if (itemModel.isBonusLabel)
                    {
                        _bonusCardsModel.UpdateBonusLabelCardIndex(itemModel.itemIndex);
                    }

                    if (itemModel.cardType == CardType.Gold)
                    {
                        _bonusCardsModel.UpdateGoldCardIndex(itemModel.itemIndex);
                    }
                }
            }
        }

        private void UpdateBonusCardCount()
        {
            int bonusCardsCount = 0;
            int nextItemIndex = 0;
            int nextStartingIndex = _bonusCardsModel.NextStartingBonusCardIndex;
            foreach (CardCollectorItemModel itemModel in _cardCollectorItemModel)
            {
                if (itemModel.itemIndex < nextStartingIndex)
                {
                    continue;
                }

                if (itemModel.type == CardCollectorTriggerType.GameBoard)
                {
                    bonusCardsCount += 1;
                }
                else if (itemModel.type == CardCollectorTriggerType.CardDeck
                         && itemModel.subCardType != SubCardType.Taped)
                {
                    bonusCardsCount = 0;
                }

                nextItemIndex = itemModel.itemIndex + 1;
            }

            _bonusCardsModel.UpdateBonusCardsCount(bonusCardsCount, nextStartingIndex: nextItemIndex);
        }

        internal void UpdateLevelCleared()
        {
            if (_isLevelCleared == false)
            {
                UpdateShouldLimitInput(true);
                _isLevelCleared = true;
            }
        }
    }
}
