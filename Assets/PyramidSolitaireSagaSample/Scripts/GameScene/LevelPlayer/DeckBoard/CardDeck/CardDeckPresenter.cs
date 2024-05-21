using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.LevelPlayer.BonusCardSequence;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using PyramidSolitaireSagaSample.LevelPlayer.LootAnimation;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardDeck
{
    public class CardDeckPresenter : MonoBehaviour,
                                     ILevelRestorable,
                                     ICardCollectRequester,
                                     ILootAnimationEndPoint,
                                     ILevelTutorialCardCloner
    {
        public enum State
        {
            None,
            Intro
        }

        [Header("Data")]
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private CardDeckModel _cardDeckModel;

        [Header("View")]
        [SerializeField] private CardDeckRenderer _cardDeckRenderer;

        public event Action<IEnumerable<CardDeckItemModel>> onItemModelChanged
        {
            add { _cardDeckModel.onItemModelChanged += value; }
            remove { _cardDeckModel.onItemModelChanged -= value; }
        }

        public event Action<IEnumerable<CardDeckItemModel>> onCardDeckDrawn
        {
            add { _cardDeckModel.onCardDeckDrawn += value; }
            remove { _cardDeckModel.onCardDeckDrawn -= value; }
        }

        public event Action<IEnumerable<CardDeckItemModel>> onItemModelRestored
        {
            add { _cardDeckModel.onItemModelRestored += value; }
            remove { _cardDeckModel.onItemModelRestored -= value; }
        }

        public event Func<CardCollectorTriggerType, CardNumber, CardColor, CardType, SubCardType, int, bool,
                          (Transform itemRoot, Vector3 itemPosition, int sortingOrder)> requestCollect;
        public event Action onItemRendererCreated;

        public ICardPoolPicker CardPoolPicker { private get; set; }
        public IBonusCardSequencePicker BonusCardSequencePicker { private get; set; }
        public string RestoreLevelID => RestoreLevelIdPath.CardDeck;

        public LevelTutorialCloneType CardCloneType => LevelTutorialCloneType.CardDeck;

        private EnumState<State> _state;
        private bool _showReveledRenderer;
        private List<CardDeckItemPositionInfo> itemPositionInfoList;

        private void Awake()
        {
            _state = new EnumState<State>();
            itemPositionInfoList = new List<CardDeckItemPositionInfo>();
        }

        private void OnEnable()
        {
            _cardDeckModel.onItemModelRestored += CreateItemRenderer;
            _cardDeckModel.onPeekItemModelDrawn += DrawPeekItemRenderer;
            _cardDeckModel.onPeekItemModelRevealed += RevealPeekItemRenderer;

            _cardDeckRenderer.onItemClick += OnDrawItemModel;
        }

        private void OnDisable()
        {
            _cardDeckModel.onItemModelRestored -= CreateItemRenderer;
            _cardDeckModel.onPeekItemModelDrawn -= DrawPeekItemRenderer;
            _cardDeckModel.onPeekItemModelRevealed -= RevealPeekItemRenderer;

            _cardDeckRenderer.onItemClick -= OnDrawItemModel;
        }

        public Vector3 GetLootAnimtionEndPoint()
        {
            Vector3 endPoint;
            if (_cardDeckRenderer.HasNextItem)
            {
                endPoint = _cardDeckRenderer.PeekItem.CachedTransform.position;
            }
            else
            {
                endPoint = _cardDeckRenderer.ItemRendererRoot.position
                           + CardSortingPosition.CalculateAsDescending(0, 1, _cardData.CardPileGap);
            }
            return endPoint;
        }

        public void DrawFirstItem()
        {
            _state.Set(State.Intro);
            DrawItemModel();
            _state.Set(State.None);
        }

        public void DrawPeekItemRenderer(CardNumber cardNumber, CardColor cardColor, SubCardType consumedSubCardType)
        {
            (Transform itemRoot, Vector3 itemPosition, int sortingOrder) = requestCollect(
                CardCollectorTriggerType.CardDeck,
                cardNumber,
                cardColor,
                CardType.Type_None,
                consumedSubCardType,
                -1,
                false
            );
            Sprite itemSprite = _cardData.GetCardSprite(cardNumber, cardColor, CardFace.Face_Up, CardType.Type_None);
            _cardDeckRenderer.DrawPeekItemRenderer(
                itemRoot,
                itemSprite,
                itemPosition,
                Vector3.zero,
                sortingOrder,
                cardMoveDuration: _cardData.CardMoveDuration,
                cardMoveEase: _cardData.CardMoveEase,
                useTween: _state.CurrState == State.None
            );
            if (_cardDeckRenderer.HasNextItem)
            {
                _cardDeckRenderer.PeekItem.CollierObject.SetActive(true);
            }
        }

        public void RevealPeekItemRenderer(CardNumber cardNumber, CardColor cardColor)
        {
            Sprite revealedSprite = _cardData.GetCardSprite(cardNumber, cardColor, CardFace.Face_Up, CardType.Type_None);
            _cardDeckRenderer.RevealPeekItemRenderer(revealedSprite);
        }

        private void OnDrawItemModel()
        {
            DrawItemModel();
        }

        public void DrawItemModel(IGameBoardCardModel cardModel, SubCardType subCardType, int subCardTypeOption)
        {
            if (subCardType == SubCardType.Taped)
            {
                DrawItemModel(SubCardType.Taped);
            }
        }

        private void RevealPeekItemModel()
        {
            if (_cardDeckModel.ItemModelCount > 0)
            {
                PickedCardInfo pickedCardInfo = PickPeekCardInfo();
                _cardDeckModel.RevealPeekItemModel(pickedCardInfo.cardNumber, pickedCardInfo.cardColor);
            }
            else if (BonusCardSequencePicker.ItemModelCount > 0)
            {
                BonusCardSequencePicker.RevealPeekItemModel();
            }
            else
            {
                Debug.LogWarning("카드덱과 보너스 카드덱에 모두 남은 카드가 없습니다.");
            }
        }

        private PickedCardInfo PickPeekCardInfo()
        {
            PickedCardInfo pickedCardInfo = default;

            CardDeckItemModel itemModel = _cardDeckModel.PeekItem;
            CommonCardInfo cardInfo = itemModel.CardInfo;
            if (cardInfo.CardNumber == CardNumber.Num_Random
                || (cardInfo.CardNumber != CardNumber.Num_Joker && cardInfo.CardColor == CardColor.Color_Random))
            {
                pickedCardInfo = CardPoolPicker.PickRandomCard(cardInfo.CardNumber, cardInfo.CardColor);
            }
            else
            {
                pickedCardInfo.cardNumber = cardInfo.CardNumber;
                pickedCardInfo.cardColor = cardInfo.CardColor;
            }

            return pickedCardInfo;
        }

        /// <param name="consumedSubCardType">
        /// 1. 게임 보드 카드의 SubCardType 이 Taped 이면 카드 덱의 드로우(수집)가 발생하는데, 
        /// 2. 수집된 카드에 소모된 SubCardType 을 전달해주어 Streaks 카운팅이 되지 않도록 하기 위함
        /// </param>
        private void DrawItemModel(SubCardType consumedSubCardType = SubCardType.SubType_None)
        {
            if (_cardDeckModel.ItemModelCount > 0)
            {
                // 카드 덱은 늘 수집 가능하기 때문에 canCollect.Invoke(cardNumber) 를 생략함.
                PickedCardInfo pickedCardInfo = PickPeekCardInfo();
                if (pickedCardInfo.hasPoolItem)
                {
                    _cardDeckModel.DrawPeekItemModel(pickedCardInfo.cardNumber, pickedCardInfo.cardColor, consumedSubCardType);
                    if (_showReveledRenderer)
                    {
                        RevealPeekItemModel();
                    }
                }
                else
                {
                    _cardDeckModel.DrawPeekItemModel(consumedSubCardType);
                    if (_showReveledRenderer)
                    {
                        RevealPeekItemModel();
                    }
                }
            }
            else if (BonusCardSequencePicker.ItemModelCount > 0)
            {
                BonusCardSequencePicker.DrawPeekItemModel(consumedSubCardType);
                if (_showReveledRenderer)
                {
                    RevealPeekItemModel();
                }
            }
        }

        private void CreateItemRenderer(IEnumerable<CardDeckItemModel> itemModelList)
        {
            int deckCount = _cardDeckModel.ItemModelCount;
            float cardPileGap = _cardData.CardPileGap;
            foreach (CardDeckItemModel itemModel in itemModelList)
            {
                CommonCardInfo cardInfo = itemModel.CardInfo;
                _CreateItemRenderer(cardInfo.CardNumber, cardInfo.CardColor, itemModel.Index, deckCount, cardPileGap, CardSortingOrderType.CardDeck);
            }

            if (_cardDeckRenderer.HasNextItem)
            {
                _cardDeckRenderer.PeekItem.CollierObject.SetActive(true);
            }
            _cardDeckRenderer.UpdateCount();

            onItemRendererCreated?.Invoke();
        }

        private void _CreateItemRenderer(
            CardNumber cardNumber,
            CardColor cardColor,
            int itemIndex,
            int itemCount,
            float cardPileGap,
            CardSortingOrderType cardSortingOrderType
        )
        {
            Sprite cardSprite = _cardData.GetCardSprite(cardNumber, cardColor, CardFace.Face_Down, CardType.Type_None);
            Sprite revealedSprite = _cardData.GetCardSprite(cardNumber, cardColor, CardFace.Face_Up, CardType.Type_None);
            Vector3 itemPosition = CardSortingPosition.CalculateAsDescending(itemIndex, itemCount, cardPileGap);
            Vector3 itemEulerAngles = new Vector3(0, 180, 0);
            int sortingOrder = CardSortingOrder.Calculate(cardSortingOrderType, itemIndex * -1);
            //Debug.Log($"_CreateItemRenderer : {itemIndex}, {itemCount}, {sortingOrder}");
            _cardDeckRenderer.AddItemRenderer(itemIndex, cardSprite, itemPosition, itemEulerAngles, sortingOrder, revealedSprite, sortingOrder + 1);
        }

        public void CreateItemRenderer(IEnumerable<BonusCardItemModel> bonusCardItemModelList, int addedCount)
        {
            int deckCount = _cardDeckModel.ItemModelCount;
            int bonusCardCount = BonusCardSequencePicker.ItemModelCount;
            int addedIndex = deckCount + bonusCardCount;
            int itemCount = deckCount + bonusCardCount + addedCount;
            float cardPileGap = _cardData.CardPileGap;
            //Debug.Log($"CreateItemRenderer 1 : {addedIndex} = {deckCount} + {bonusCardCount} - {addedCount}");
            //Debug.Log($"CreateItemRenderer 2 : {itemCount} = {deckCount} + {bonusCardCount} + {addedCount}");

            int i = 0;
            foreach (BonusCardItemModel itemModel in bonusCardItemModelList)
            {
                int itemIndex = addedIndex + i;
                _CreateItemRenderer(itemModel.cardNumber, itemModel.cardColor, itemIndex, itemCount, cardPileGap, CardSortingOrderType.BonusCardDeck);

                i += 1;
            }

            itemPositionInfoList.Clear();
            for (int rendererIndex = 0; rendererIndex < _cardDeckRenderer.ItemCount; rendererIndex++)
            {
                Vector3 itemPosition = CardSortingPosition.CalculateAsDescending(rendererIndex, _cardDeckRenderer.ItemCount, cardPileGap);
                itemPositionInfoList.Add(new CardDeckItemPositionInfo(
                    itemIndex: rendererIndex,
                    itemPosition: itemPosition
                ));
            }
            _cardDeckRenderer.ReorderItemRenderer(itemPositionInfoList);

            if (_cardDeckRenderer.HasNextItem)
            {
                _cardDeckRenderer.PeekItem.CollierObject.SetActive(true);
            }
            _cardDeckRenderer.UpdateCount();
        }

        public void RestoreLevelData(string data)
        {
            _cardDeckModel.RestoreSaveData(data);
        }

        public void BeginLootAnimation()
        {
            // Do nothing
        }

        public void EndLootAnimation(long bonusCount)
        {
            BonusCardSequencePicker.AddBonusCard((int)bonusCount);
            if (_showReveledRenderer
                && _cardDeckModel.ItemModelCount == 0)
            {
                BonusCardSequencePicker.RevealPeekItemModel();
            }
        }

        public void DrawCard(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            DrawItemModel();
        }

        public int GetCloneCount(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            return 1;
        }

        public void CloneCardRendererList(IReadOnlyList<GameBoardCardRenderer> cardRendererList, IEnumerable<CardRendererCloneData> cloneDataList)
        {
            GameBoardCardRenderer cardRenderer = cardRendererList[0];

            CardDeckItemModel itemModel = _cardDeckModel.PeekItem;
            CommonCardInfo cardInfo = itemModel.CardInfo;
            Sprite cardSprite = _cardData.GetCardSprite(cardInfo.CardNumber, cardInfo.CardColor, CardFace.Face_Down, cardInfo.CardType);
            SubCardTypeRendererInfo subCardTypeRendererInfo = new(cardInfo.CardFace, cardInfo.SubCardType, cardInfo.SubCardTypeOption, cardInfo.SubCardTypeOption2, cardInfo.IsBonusLabel);
            cardRenderer.UpdateAllRenderers(cardSprite, subCardTypeRendererInfo, false, false, Vector3.zero);

            cardRenderer.CachedTransform.position = GetLootAnimtionEndPoint();
        }

        internal void ShowRevealedRenderer()
        {
            _showReveledRenderer = true;

            RevealPeekItemModel();
        }
    }
}
