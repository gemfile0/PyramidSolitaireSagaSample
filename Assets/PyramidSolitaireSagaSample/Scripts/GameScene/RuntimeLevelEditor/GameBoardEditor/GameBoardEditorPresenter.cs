using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class GameBoardEditorPresenter : MonoBehaviour,
                                            ILevelSavable
    {
        [Header("Data")]
        [SerializeField] private GameBoardData _gameBoardData;
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private GameBoardModel _gameBoardModel;

        [Header("View")]
        [SerializeField] private GameBoardRenderer _gameBoardRenderer;
        [SerializeField] private Camera _mainCamera;

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
        public event Action<IGameBoardCardModel, bool> onCardModelSelected
        {
            add { _gameBoardModel.onCardModelSelected += value; }
            remove { _gameBoardModel.onCardModelSelected -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.GameBoard;

        public bool IsHighlightOn => _gameBoardRenderer.HighlightGameObject.activeInHierarchy;

        private void Awake()
        {
            Vector2 halfTileSize = _gameBoardData.HalfTileSize;
            int rowCount = _gameBoardData.RowCount;
            int colCount = _gameBoardData.ColCount;
            int lastRowIndex = _gameBoardData.LastRowIndex;
            int lastColIndex = _gameBoardData.LastColIndex;
            _gameBoardRenderer.Init(halfTileSize, rowCount, colCount, lastRowIndex, lastColIndex, showGuideText: true);
            _gameBoardModel.Init(lastRowIndex, lastColIndex);

            Vector2 cardSize = _cardData.CardSize;
            _gameBoardRenderer.SetHighlightSize(cardSize);

            // set camera position
            float cameraX = colCount / 2 * halfTileSize.x;
            float cameraY = rowCount / 2 * halfTileSize.y;
            Vector3 mainCameraPosition = new Vector3(cameraX, cameraY, -10f);
            _mainCamera.transform.position = mainCameraPosition;
        }

        private void OnEnable()
        {
            _gameBoardModel.onCardModelSelected += OnCardModelSelected;
            _gameBoardModel.onCardModelCreated += OnCardModelCreated;
            _gameBoardModel.onCardModelRemoved += OnCardModelRemoved;
            _gameBoardModel.onCardModelMoved += OnCardModelMoved;
            _gameBoardModel.onCardModelStackIndexSwaped += OnCardModelStackIndexSwaped;
            _gameBoardModel.onCardModelDepthChanged += OnCardModelDepthChanged;
            _gameBoardModel.onCardModelChildrenChanged += OnCardModelChildrenChanged;
            _gameBoardModel.onCardInfoChanged += UpdateCardRenderer;
        }

        private void OnDisable()
        {
            _gameBoardModel.onCardModelSelected -= OnCardModelSelected;
            _gameBoardModel.onCardModelCreated -= OnCardModelCreated;
            _gameBoardModel.onCardModelRemoved -= OnCardModelRemoved;
            _gameBoardModel.onCardModelMoved -= OnCardModelMoved;
            _gameBoardModel.onCardModelStackIndexSwaped -= OnCardModelStackIndexSwaped;
            _gameBoardModel.onCardModelDepthChanged -= OnCardModelDepthChanged;
            _gameBoardModel.onCardModelChildrenChanged -= OnCardModelChildrenChanged;
            _gameBoardModel.onCardInfoChanged -= UpdateCardRenderer;
        }

        public string SaveLevelData()
        {
            GameBoardSaveData data = new()
            {
                cardDataList = _gameBoardModel.CardModelList
                    .Select((IGameBoardCardModel cardModel) => new GameBoardCardSaveData
                    {
                        ID = cardModel.ID,
                        Index = cardModel.Index,
                        StackIndex = cardModel.StackIndex,
                        CardInfo = cardModel.CardInfo
                    })
                    .ToList()
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreLevelData(string data)
        {
            ClearBoard();
            _gameBoardModel.RestoreSaveData(data);
        }

        public void ClearBoard()
        {
            _gameBoardModel.ClearCardModelList();
            _gameBoardRenderer.ClearAllCardRenderer();
            _gameBoardRenderer.ClearAllButterflyRenderer();
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
                _gameBoardRenderer.SetCardRenderer(
                    cardIndex,
                    cardStackIndex,
                    cardSprite,
                    subCardTypeRendererInfo,
                    animateSubCardType: false,
                    cardModel.CannotDrawCard(),
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
            //Debug.Log($"OnCardModelChildrenChanged : {cardModel.Index}, {cardModel.ChildCount}, {cardModel.CardInfo}");
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
            _gameBoardRenderer.SetObscuredObject(
                cardModel.Index,
                cardModel.StackIndex,
                cardModel.IsObscured
            );
        }

        private void OnCardModelSelected(IGameBoardCardModel cardModel, bool isSelected)
        {
            if (isSelected)
            {
                _gameBoardRenderer.SelectCardRenderer(cardModel.Index, cardModel.StackIndex);
            }
            else
            {
                _gameBoardRenderer.DeselectCardRenderer(cardModel.Index, cardModel.StackIndex);
            }
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

        private void OnCardModelDepthChanged(IEnumerable<IGameBoardCardModel> cardModelList)
        {
            foreach (IGameBoardCardModel cardModel in cardModelList)
            {
                _gameBoardRenderer.UpdateCardRendererSortingOrder(cardModel.Index, cardModel.StackIndex, CardSortingOrder.Calculate(CardSortingOrderType.GameBoard, cardModel.ID));
            }
        }

        private void OnCardModelMoved(Vector2Int originIndex, int originStackIndex, Vector2Int nextIndex, int nextStackIndex)
        {
            Vector3 snappedPosition = _gameBoardData.GetSnappedPosition(nextIndex);
            _gameBoardRenderer.MoveCardRenderer(originIndex, originStackIndex, nextIndex, nextStackIndex, snappedPosition);
            _gameBoardRenderer.MoveButterflyRenderer(originIndex, nextIndex, snappedPosition);
        }

        private void OnCardModelStackIndexSwaped(Vector2Int index, int originStackIndex, int nextStackIndex)
        {
            _gameBoardRenderer.SwapCardRendererStackIndex(index, originStackIndex, nextStackIndex);
        }

        private void OnCardModelRemoved(Vector2Int cardIndex, int stackIndex)
        {
            _gameBoardRenderer.RemoveCardRenderer(cardIndex, stackIndex);
            if (stackIndex == 0)
            {
                _gameBoardRenderer.RemoveButterflyRendrerer(cardIndex);
            }
        }

        internal void ShowHighlight(Color color)
        {
            _gameBoardModel.State.Set(GameBoardModelState.Highlight);
            _gameBoardRenderer.SetHighlightColor(color);
        }

        internal void HideHighlight()
        {
            _gameBoardModel.State.Set(GameBoardModelState.None);
            _gameBoardModel.UpdateSnappedIndex(new Vector2Int(-1, -1));

            _gameBoardRenderer.HideHighlight();
        }

        internal void MoveHighlight(Vector2 inputMousePosition)
        {
            if (_gameBoardModel.State.CurrState == GameBoardModelState.Highlight)
            {
                Vector3 worldMousePosition = _mainCamera.ScreenToWorldPoint(inputMousePosition);
                Vector3 highlightPosition = _gameBoardRenderer.HighlightTransform.localPosition;
                Vector3 snappingPosition = new Vector3(worldMousePosition.x, worldMousePosition.y, highlightPosition.z);
                //Debug.Log($"MoveHighlight : {inputMousePosition}, {worldMousePosition}, {highlightPosition}, {snappingPosition}");
                (Vector2Int snappedIndex, bool isOutOfGrid) = _gameBoardData.GetSnappedIndex(snappingPosition);
                _gameBoardModel.UpdateSnappedIndex(snappedIndex);
                if (isOutOfGrid == false)
                {
                    Vector3 snappedPosition = _gameBoardData.GetSnappedPosition(snappedIndex);
                    _gameBoardRenderer.MoveHighlightPosition(snappedPosition);
                }
                else
                {
                    _gameBoardRenderer.HideHighlight();
                }
            }
        }

        internal void DeselectCard()
        {
            _gameBoardModel.DeselectCardModel();
        }

        internal GameBoardCardModel DeselectCardAndRemoveCard()
        {
            GameBoardCardModel removedCardModel = null;

            _gameBoardModel.DeselectCardModel();
            if (IsHighlightOn)
            {
                removedCardModel = _gameBoardModel.RemoveCardModel(
                    _gameBoardModel.LatestSnappedIndex,
                    _gameBoardModel.LatestSnappedStackIndex
                );
            }

            return removedCardModel;
        }

        public GameBoardCardModel RemoveCard(Vector2Int cardIndex, int cardStackIndex)
        {
            _gameBoardModel.DeselectCardModel();
            return _gameBoardModel.RemoveCardModel(cardIndex, cardStackIndex);
        }

        internal GameBoardCardModel PlaceCard()
        {
            GameBoardCardModel justPlacedCardModel = null;
            if (IsHighlightOn)
            {
                (bool isCreated, GameBoardCardModel cardModel) = _gameBoardModel.GetOrCreateCardModel();
                _gameBoardModel.SelectCardModel(cardModel);

                if (isCreated)
                {
                    justPlacedCardModel = cardModel;
                }
            }
            return justPlacedCardModel;
        }

        internal GameBoardCardModel StackCard()
        {
            GameBoardCardModel cardModel = null;
            if (IsHighlightOn)
            {
                cardModel = _gameBoardModel.CreateCardModel();
                _gameBoardModel.SelectCardModel(cardModel);
            }
            return cardModel;
        }

        public GameBoardCardModel PlaceCard(
            int cardID,
            Vector2Int cardIndex,
            int cardStackIndex,
            CommonCardInfo cardInfo,
            List<GameBoardCardModel> parentSet,
            List<GameBoardCardModel> childSet
        )
        {
            return _gameBoardModel.CreateCardModel(
                cardID,
                cardIndex,
                cardStackIndex,
                cardInfo,
                parentSet,
                childSet
            );
        }

        internal GameBoardCardModel MoveCard()
        {
            GameBoardCardModel justMovedCardModel = null;
            if (IsHighlightOn)
            {
                (bool isMoved, GameBoardCardModel cardModel) = _gameBoardModel.GetOrMoveCardModel();
                if (cardModel != null)
                {
                    _gameBoardModel.SelectCardModel(cardModel);
                }

                if (isMoved)
                {
                    justMovedCardModel = cardModel;
                }
            }
            return justMovedCardModel;
        }

        internal GameBoardCardModel MoveCard(
            Vector2Int originCardIndex,
            int originCardStackIndex,
            Vector2Int nextCardIndex,
            int nextCardStackIndex,
            int cardID,
            List<GameBoardCardModel> parentSet,
            List<GameBoardCardModel> childSet
        )
        {
            return _gameBoardModel.MoveCardModel(originCardIndex, originCardStackIndex, nextCardIndex, nextCardStackIndex, cardID, parentSet, childSet);
        }

        public void NewLevelData()
        {
            ClearBoard();
        }

        internal void OnCardDeckPanelActive(bool value)
        {
            if (value)
            {
                DeselectCard();
            }
        }

        internal GameBoardCardModel FlipCard()
        {
            GameBoardCardModel justFlippedCardModel = null;
            if (IsHighlightOn)
            {
                (bool isFlipped, GameBoardCardModel cardModel) = _gameBoardModel.GetOrFlipCardModel();
                if (cardModel != null)
                {
                    _gameBoardModel.SelectCardModel(cardModel);
                }

                if (isFlipped)
                {
                    justFlippedCardModel = cardModel;
                }
            }
            return justFlippedCardModel;
        }
    }
}
