using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class GameBoardPresenter : MonoBehaviour,
                                      ILevelRestorable,
                                      ICardCollectRequester,
                                      ICardCollectChecker,
                                      ILevelTutorialCardCloner
    {
        [Header("Data")]
        [SerializeField] private GameBoardData _gameBoardData;
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private GameBoardModel _gameBoardModel;

        [Header("View")]
        [SerializeField] private GameBoardRenderer _gameBoardRenderer;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float cameraOffsetY = -1.5f;

        public event Action<IEnumerable<IGameBoardCardModel>> onGameBoardChanged
        {
            add { _gameBoardModel.onGameBoardChanged += value; }
            remove { _gameBoardModel.onGameBoardChanged -= value; }
        }
        public event Action<IEnumerable<IGameBoardCardModel>> onGameBoardRestored
        {
            add { _gameBoardModel.onGameBoardRestored += value; }
            remove { _gameBoardModel.onGameBoardRestored -= value; }
        }
        public event Action<IEnumerable<IGameBoardCardModel>> onGameBoardDrawn
        {
            add { _gameBoardModel.onGameBoardDrawn += value; }
            remove { _gameBoardModel.onGameBoardDrawn -= value; }
        }
        public event Action<IGameBoardCardModel, SubCardType, int> onSubCardTypeConsumed
        {
            add { _gameBoardModel.onSubCardTypeConsumed += value; }
            remove { _gameBoardModel.onSubCardTypeConsumed -= value; }
        }
        public event Action<Vector3/* centerPosition */, Vector3/* halfSize */> onTileBoundHalfSize
        {
            add { _gameBoardRenderer.onTileBoundHalfSize += value; }
            remove { _gameBoardRenderer.onTileBoundHalfSize -= value; }
        }

        public event Action<Vector3> onMainCameraPositionChanged;
        public event Func<CardCollectorTriggerType, CardNumber, CardColor, CardType, SubCardType, int, bool,
                          (Transform itemRoot, Vector3 itemPosition, int sortingOrder)> requestCollect;
        public event Func<CardNumber, bool> canCollect;

        public string RestoreLevelID => RestoreLevelIdPath.GameBoard;
        public ICardPoolPicker CardPoolPicker { private get; set; }

        public LevelTutorialCloneType CardCloneType => LevelTutorialCloneType.GameBoard;

        private void Awake()
        {
            Vector2 halfTileSize = _gameBoardData.HalfTileSize;
            int rowCount = _gameBoardData.RowCount;
            int colCount = _gameBoardData.ColCount;
            int lastRowIndex = _gameBoardData.LastRowIndex;
            int lastColIndex = _gameBoardData.LastColIndex;
            _gameBoardRenderer.Init(halfTileSize, rowCount, colCount, lastRowIndex, lastColIndex);
            _gameBoardModel.Init(lastRowIndex, lastColIndex);

            // set camera position
            float cameraX = colCount / 2 * halfTileSize.x;
            float cameraY = rowCount / 2 * halfTileSize.y + cameraOffsetY;
            Vector3 mainCameraPosition = new Vector3(cameraX, cameraY, -10f);
            _mainCamera.transform.position = mainCameraPosition;
            onMainCameraPositionChanged?.Invoke(mainCameraPosition);
        }

        private void OnEnable()
        {
            _gameBoardModel.onCardModelCreated += OnCardModelCreated;
            _gameBoardModel.onCardModelChildrenChanged += OnCardModelChildrenChanged;
            _gameBoardModel.onCardInfoChanged += UpdateCardRenderer;
            _gameBoardModel.onCardModelDrawn += DrawCardRenderer;
            _gameBoardModel.onSubCardTypeConsumed += UnpackCardRenderer;

            _gameBoardRenderer.onCardClick += DrawCardModel;
        }

        private void OnDisable()
        {
            _gameBoardModel.onCardModelCreated -= OnCardModelCreated;
            _gameBoardModel.onCardModelChildrenChanged -= OnCardModelChildrenChanged;
            _gameBoardModel.onCardInfoChanged -= UpdateCardRenderer;
            _gameBoardModel.onCardModelDrawn -= DrawCardRenderer;
            _gameBoardModel.onSubCardTypeConsumed -= UnpackCardRenderer;

            _gameBoardRenderer.onCardClick -= DrawCardModel;
        }

        private void DrawCardModel(Vector2Int cardIndex, int cardStackIndex)
        {
            IGameBoardCardModel cardModel = _gameBoardModel.GetCardModel(cardIndex, cardStackIndex);
            if (cardModel.ChildCount == 0)
            {
                CommonCardInfo cardInfo = cardModel.CardInfo;
                CardNumber cardNumber = cardInfo.CardNumber;
                if (canCollect.Invoke(cardNumber))
                {
                    (bool isConsumed, SubCardType subCardType, int subCardTypeOption) = _gameBoardModel.ConsumeSubCardType(cardIndex, cardStackIndex);
                    if (isConsumed)
                    {
                        if (subCardType == SubCardType.Key)
                        {
                            _gameBoardModel.ConsumeAllSubCardType(SubCardType.Lock, subCardTypeOption);
                            StartCoroutine(DrawCardModelCoroutine(delay: _cardData.CardUnpackDelay + _cardData.CardUnpackDuration, cardIndex));
                        }
                        else if (subCardType == SubCardType.UnTie)
                        {
                            _gameBoardModel.UpdateWholeUntieCount();
                            _gameBoardModel.ConsumeAllSubCardType(SubCardType.Tied, subCardTypeOption);
                            _gameBoardModel.DrawCardModel(cardIndex);
                        }
                        else if (subCardType == SubCardType.Taped)
                        {
                            _gameBoardRenderer.DrawCardRendererAsFail(
                                cardIndex,
                                cardStackIndex,
                                CardSortingOrder.GetBaseValue(CardSortingOrderType.CardCollector),
                                scaleValue: _cardData.CardScale,
                                scaleDuration: _cardData.CardScaleDuration,
                                scaleEase: _cardData.CardScaleEase
                            );
                        }
                        else
                        {
                            Debug.LogWarning($"{subCardType} 에 대한 후속 작업은 없습니다.");
                        }
                    }
                    else
                    {
                        if (subCardType == SubCardType.SubType_None)
                        {
                            _gameBoardModel.DrawCardModel(cardIndex);
                        }
                        else
                        {
                            Debug.LogWarning($"정의하지 않은 상황입니다 : {subCardType}");
                        }
                    }
                }
                else
                {
                    _gameBoardRenderer.DrawCardRendererAsFail(
                        cardIndex,
                        cardStackIndex,
                        CardSortingOrder.GetBaseValue(CardSortingOrderType.CardCollector),
                        scaleValue: _cardData.CardScale,
                        scaleDuration: _cardData.CardScaleDuration,
                        scaleEase: _cardData.CardScaleEase
                    );
                }
            }
        }

        private IEnumerator DrawCardModelCoroutine(float delay, Vector2Int cardIndex)
        {
            yield return new WaitForSeconds(delay);
            _gameBoardModel.DrawCardModel(cardIndex);
        }

        private void UnpackCardRenderer(IGameBoardCardModel cardModel, SubCardType subCardType, int subCardTypeOption)
        {
            _gameBoardRenderer.UnpackCardRenderer(
                cardModel.Index,
                cardModel.StackIndex,
                _cardData.CardUnpackDelay,
                _cardData.CardUnpackDuration,
                _cardData.CardUnpackEase
            );
            bool isCardDimmed = cardModel.CannotDrawCard();
            _gameBoardRenderer.SetCardDimmedRenderer(
                cardModel.Index,
                cardModel.StackIndex,
                animate: true,
                value: isCardDimmed,
                _cardData.CardUnpackDelay,
                _cardData.CardUnpackDuration,
                _cardData.CardUnpackEase
            );
            _gameBoardRenderer.SetCardCollider(
                cardModel.Index,
                cardModel.StackIndex,
                value: isCardDimmed == false
            );
        }

        private void DrawCardRenderer(Vector2Int cardIndex, int cardStackIndex, CommonCardInfo cardInfo)
        {
            (Transform itemRoot, Vector3 itemPosition, int sortingOrder) = requestCollect(
                CardCollectorTriggerType.GameBoard,
                cardInfo.CardNumber,
                cardInfo.CardColor,
                cardInfo.CardType,
                cardInfo.SubCardType,
                cardInfo.SubCardTypeOption,
                cardInfo.IsBonusLabel
            );
            _gameBoardRenderer.DrawCardRenderer(
                itemRoot,
                cardIndex,
                cardStackIndex,
                itemPosition,
                sortingOrder,
                moveDuration: _cardData.CardMoveDuration,
                moveEase: _cardData.CardMoveEase,
                scaleValue: _cardData.CardScale,
                scaleDuration: _cardData.CardScaleDuration,
                scaleEase: _cardData.CardScaleEase
            );
            if (cardStackIndex == 0)
            {
                _gameBoardRenderer.DrawButterflyRendrerer(
                    cardIndex,
                    fadeDelay: _gameBoardData.ButterflyMoveDelay,
                    fadeDuration: _cardData.CardMoveDuration,
                    fadeEase: _cardData.CardMoveEase
                );
            }
        }

        public void RestoreLevelData(string data)
        {
            _gameBoardModel.RestoreSaveData(data);
        }

        public IEnumerator DealCardsToBoard()
        {
            if (_gameBoardRenderer.CardRendererCount > 0)
            {
                yield return _gameBoardRenderer.MoveToOwnLocalPosition(
                    _cardData.CardMoveDuration,
                    _cardData.CardMoveEase,
                    _cardData.CardMoveDelay,
                    CardSortingOrder.GetBaseValue(CardSortingOrderType.CardCollector),
                    CardSortingOrder.StepValue
                );
            }
            else
            {
                yield break;
            }
        }

        private void UpdateCardRenderer(IGameBoardCardModel cardModel)
        {
            if (cardModel != null)
            {
                CommonCardInfo cardInfo = cardModel.CardInfo;
                Vector2Int cardIndex = cardModel.Index;
                int cardStackIndex = cardModel.StackIndex;
                Sprite cardSprite = _cardData.GetCardSprite(cardInfo.CardNumber, cardInfo.CardColor, cardInfo.CardFace, cardInfo.CardType);
                SubCardTypeRendererInfo subCardTypeRendererInfo = new(cardInfo.CardFace, cardInfo.SubCardType, cardInfo.SubCardTypeOption, cardInfo.SubCardTypeOption2, cardInfo.IsBonusLabel);
                bool isCardDimmed = cardModel.CannotDrawCard();
                _gameBoardRenderer.SetCardRenderer(
                    cardIndex,
                    cardStackIndex,
                    cardSprite,
                    subCardTypeRendererInfo,
                    animateSubCardType: true,
                    isCardDimmed,
                    initialEulerAngles: cardModel.CardInfo.CardFace == CardFace.Face_Down ?
                                        new Vector3(0, 180, 0) :
                                        Vector3.zero
                );

                if (cardInfo.CardType == CardType.Gold)
                {
                    _gameBoardRenderer.SetButterflyRenderer(cardIndex, _gameBoardData.GetSnappedPosition(cardIndex), _gameBoardData.ButterflySprite);
                }
            }
        }

        private void OnCardModelChildrenChanged(IGameBoardCardModel cardModel)
        {
            //Debug.Log($"OnCardModelChildrenChanged : {cardModel.Index}, {cardModel.StackIndex}, {cardModel.ChildCount}, {cardModel.CardInfo}");
            if (cardModel.ChildCount == 0
                && cardModel.CardInfo.CardFace == CardFace.Face_Down)
            {
                CommonCardInfo cardInfo = cardModel.CardInfo;
                PickedCardInfo pickedCardInfo = PickCardInfo(cardInfo);
                if (pickedCardInfo.hasPoolItem)
                {
                    cardInfo.UpdateInfo(pickedCardInfo.cardNumber, pickedCardInfo.cardColor, cardInfo.CardFace, cardInfo.CardType, cardInfo.SubCardType, cardInfo.SubCardTypeOption, cardInfo.SubCardTypeOption2, cardInfo.IsBonusLabel);
                }
                cardInfo.UpdateInfo(cardInfo.CardNumber, cardInfo.CardColor, CardFace.Face_Up, cardInfo.CardType, cardInfo.SubCardType, cardInfo.SubCardTypeOption, cardInfo.SubCardTypeOption2, cardInfo.IsBonusLabel);
                cardModel.UpdateCardInfoWithoutNotify(cardInfo);

                Sprite cardSprite = _cardData.GetCardSprite(cardInfo.CardNumber, cardInfo.CardColor, cardInfo.CardFace, cardInfo.CardType);
                SubCardTypeRendererInfo subCardTypeRendererInfo = new(cardInfo.CardFace, cardInfo.SubCardType, cardInfo.SubCardTypeOption, cardInfo.SubCardTypeOption2, cardInfo.IsBonusLabel);
                _gameBoardRenderer.FlipCardRenderer(
                    cardModel.Index,
                    cardModel.StackIndex,
                    cardSprite,
                    subCardTypeRendererInfo,
                    _cardData.CardMoveDuration,
                    _cardData.CardMoveEase
                );
            }

            bool isCardDimmed = cardModel.CannotDrawCard();
            _gameBoardRenderer.SetCardDimmedRenderer(
                cardModel.Index,
                cardModel.StackIndex,
                animate: false,
                value: isCardDimmed,
                _cardData.CardUnpackDelay,
                _cardData.CardUnpackDuration,
                _cardData.CardUnpackEase
            );
            _gameBoardRenderer.SetCardCollider(
                cardModel.Index,
                cardModel.StackIndex,
                value: isCardDimmed == false
            );
        }

        private void OnCardModelCreated(Vector2Int cardIndex, int cardStackIndex, int cardID)
        {
            _gameBoardRenderer.CreateCardRenderer(
                cardIndex,
                cardStackIndex,
                cardID,
                CardSortingOrder.Calculate(CardSortingOrderType.GameBoard, cardID),
                _gameBoardData.GetSnappedPosition(cardIndex)
            );
        }

        internal void PickRandomCards()
        {
            foreach (ICardSelectionItemModel cardModel in _gameBoardModel.CardModelList)
            {
                CommonCardInfo cardInfo = cardModel.CardInfo;
                PickedCardInfo pickedCardInfo = PickCardInfo(cardInfo);
                if (pickedCardInfo.hasPoolItem)
                {
                    cardModel.UpdateCardInfo(new CardSelectionInfo(
                        pickedCardInfo.cardNumber,
                        pickedCardInfo.cardColor,
                        cardInfo.CardFace,
                        cardInfo.CardType,
                        cardInfo.SubCardType,
                        cardInfo.SubCardTypeOption,
                        cardInfo.SubCardTypeOption2,
                        cardInfo.IsBonusLabel
                    ));
                }
            }
        }

        private PickedCardInfo PickCardInfo(CommonCardInfo cardInfo)
        {
            PickedCardInfo pickedCardInfo = default;

            if (cardInfo.CardNumber == CardNumber.Num_Random
                || (cardInfo.CardNumber != CardNumber.Num_Joker && cardInfo.CardColor == CardColor.Color_Random))
            {
                pickedCardInfo = CardPoolPicker.PickRandomCard(cardInfo.CardNumber, cardInfo.CardColor);
            }

            return pickedCardInfo;
        }

        internal void SetStartingPosition(Vector3 startingPosition)
        {
            _gameBoardRenderer.SetStartingPosition(
                startingPosition,
                CardSortingOrder.GetBaseValue(CardSortingOrderType.GameBoard),
                CardSortingOrder.StepValue
            );
        }

        public void DrawCard(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            foreach (CardRendererCloneData cloneData in cloneDataList)
            {
                if (cloneData.cloneType != CardRendererCloneType.CardIndex)
                {
                    continue;
                }

                _gameBoardModel.DrawCardModel(cloneData.cardIndex);
            }
        }

        public int GetCloneCount(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            int cloneCount = 0;
            foreach (CardRendererCloneData cloneData in cloneDataList)
            {
                if (cloneData.cloneType == CardRendererCloneType.CardIndex)
                {
                    cloneCount += _gameBoardModel.GetCardModel(cloneData.cardIndex) != null ?
                                  1 :
                                  0;
                }
                else if (cloneData.cloneType == CardRendererCloneType.CardType)
                {
                    cloneCount += _gameBoardModel.GetCardModelCount((CommonCardInfo cardInfo) => cardInfo.CardType == cloneData.cardType);
                }
                else if (cloneData.cloneType == CardRendererCloneType.SubCardType)
                {
                    cloneCount += _gameBoardModel.GetCardModelCount((CommonCardInfo cardInfo) => cardInfo.SubCardType == cloneData.subCardType);
                }
            }

            return cloneCount;
        }

        public void CloneCardRendererList(IReadOnlyList<GameBoardCardRenderer> cardRendererList, IEnumerable<CardRendererCloneData> cloneDataList)
        {
            int cardRendererIndex = 0;
            foreach (CardRendererCloneData cloneData in cloneDataList)
            {
                if (cloneData.cloneType == CardRendererCloneType.CardIndex)
                {
                    Vector2Int cardIndex = cloneData.cardIndex;
                    IGameBoardCardModel cardModel = _gameBoardModel.GetCardModel(cardIndex);
                    if (cardModel != null)
                    {
                        Vector3 cardRendererPosition = _gameBoardRenderer.GetCardRendererPosition(cardIndex, cardModel.StackIndex);
                        CloneCardRenderer(cardModel, cardRendererList[cardRendererIndex], cardRendererPosition);
                        cardRendererIndex += 1;
                    }
                }
                else if (cloneData.cloneType == CardRendererCloneType.CardType)
                {
                    IEnumerable<IGameBoardCardModel> cardModelList = _gameBoardModel.GetCardModelList((CommonCardInfo cardInfo) => cardInfo.CardType == cloneData.cardType);
                    foreach (IGameBoardCardModel cardModel in cardModelList)
                    {
                        GameBoardCardRenderer cardRenderer = cardRendererList[cardRendererIndex];
                        Vector3 cardRendererPosition = _gameBoardRenderer.GetCardRendererPosition(cardModel.Index, cardModel.StackIndex);
                        CloneCardRenderer(cardModel, cardRenderer, cardRendererPosition);
                        cardRendererIndex += 1;
                    }
                }
                else if (cloneData.cloneType == CardRendererCloneType.SubCardType)
                {
                    IEnumerable<IGameBoardCardModel> cardModelList = _gameBoardModel.GetCardModelList((CommonCardInfo cardInfo) => cardInfo.SubCardType == cloneData.subCardType);
                    foreach (IGameBoardCardModel cardModel in cardModelList)
                    {
                        GameBoardCardRenderer cardRenderer = cardRendererList[cardRendererIndex];
                        Vector3 cardRendererPosition = _gameBoardRenderer.GetCardRendererPosition(cardModel.Index, cardModel.StackIndex);
                        CloneCardRenderer(cardModel, cardRenderer, cardRendererPosition);
                        cardRendererIndex += 1;
                    }
                }
            }
        }

        private void CloneCardRenderer(IGameBoardCardModel cardModel, GameBoardCardRenderer cardRenderer, Vector3 cardRendererPosition)
        {
            CommonCardInfo cardInfo = cardModel.CardInfo;
            Sprite cardSprite = _cardData.GetCardSprite(cardInfo.CardNumber, cardInfo.CardColor, cardInfo.CardFace, cardInfo.CardType);
            SubCardTypeRendererInfo subCardTypeRendererInfo = new(cardInfo.CardFace, cardInfo.SubCardType, cardInfo.SubCardTypeOption, cardInfo.SubCardTypeOption2, cardInfo.IsBonusLabel);
            cardRenderer.UpdateAllRenderers(cardSprite, subCardTypeRendererInfo, false, false, Vector3.zero);

            cardRenderer.CachedTransform.position = cardRendererPosition;
        }

        internal void RemoveObstacles()
        {
            _gameBoardModel.RemoveObstacles();
        }

        internal void FlipCardsFaceup()
        {
            _gameBoardModel.FlipCardsFaceup();
        }
    }
}
