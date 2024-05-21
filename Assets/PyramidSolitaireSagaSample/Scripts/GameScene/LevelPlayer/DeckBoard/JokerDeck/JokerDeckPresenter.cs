using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using PyramidSolitaireSagaSample.LevelPlayer.LootAnimation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.JokerDeck
{
    public class JokerDeckPresenter : MonoBehaviour,
                                      ICardCollectRequester,
                                      ICardCollectChecker,
                                      ILootAnimationEndPoint,
                                      ILevelTutorialCardCloner
    {
        [Header("Data")]
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private JokerDeckModel _jokerDeckModel;

        [Header("View")]
        [SerializeField] private JokerDeckRenderer _jokerDeckRenderer;

        public event Action<int, int> onJokerCountUpdated
        {
            add { _jokerDeckModel.onJokerCountUpdated += value; }
            remove { _jokerDeckModel.onJokerCountUpdated -= value; }
        }

        public event Action<int> onJokerCountRestored
        {
            add { _jokerDeckModel.onJokerCountRestored += value; }
            remove { _jokerDeckModel.onJokerCountRestored -= value; }
        }

        public event Func<CardCollectorTriggerType, CardNumber, CardColor, CardType, SubCardType, int, bool,
                          (Transform itemRoot, Vector3 itemPosition, int sortingOrder)> requestCollect;
        public event Func<CardNumber, bool> canCollect;

        public LevelTutorialCloneType CardCloneType => LevelTutorialCloneType.JokerDeck;

        private void OnEnable()
        {
            _jokerDeckModel.onJokerCountRestored += CrateItemRenderer;
            _jokerDeckModel.onItemDrawn += DrawItemRenderer;
            _jokerDeckModel.onJokerCountUpdated += CreateItemRenderer;

            _jokerDeckRenderer.onItemClick += DrawItemModel;
        }

        private void OnDisable()
        {
            _jokerDeckModel.onJokerCountRestored -= CrateItemRenderer;
            _jokerDeckModel.onItemDrawn -= DrawItemRenderer;
            _jokerDeckModel.onJokerCountUpdated -= CreateItemRenderer;

            _jokerDeckRenderer.onItemClick -= DrawItemModel;
        }

        public Vector3 GetLootAnimtionEndPoint()
        {
            Vector3 endPoint;
            if (_jokerDeckRenderer.HasNextItem)
            {
                endPoint = _jokerDeckRenderer.PeekItem.CachedTransform.position;
            }
            else
            {
                endPoint = _jokerDeckRenderer.ItemRendererRoot.position
                           + CardSortingPosition.CalculateAsDescending(0, 1, _cardData.CardPileGap);
            }
            return endPoint;
        }

        public void UpdateLevelPreset(int level, int jokerCount, GameDifficultyType gameDifficulyType)
        {
            _jokerDeckModel.RestoreJokerCount(jokerCount);
        }

        private void CrateItemRenderer(int itemCount)
        {
            float cardPileGap = _cardData.CardPileGap;
            for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                _CreateItemRenderer(itemIndex, itemCount, cardPileGap);
            }

            if (_jokerDeckRenderer.HasNextItem)
            {
                _jokerDeckRenderer.PeekItem.CollierObject.SetActive(true);
            }
            _jokerDeckRenderer.UpdateCount();
        }

        private void _CreateItemRenderer(int itemIndex, int itemCount, float cardPileGap)
        {
            Sprite itemSprite = _cardData.GetCardSprite(CardNumber.Num_Joker, CardColor.Color_Black, CardFace.Face_Up, CardType.Type_None);
            Vector3 itemPosition = CardSortingPosition.CalculateAsDescending(itemIndex, itemCount, cardPileGap);
            int sortingOrder = CardSortingOrder.Calculate(CardSortingOrderType.JokerDeck, itemIndex * -1);
            _jokerDeckRenderer.AddItemRenderer(itemIndex, itemSprite, itemPosition, sortingOrder);
        }

        private void CreateItemRenderer(int jokerCount, int addedCount)
        {
            int addedIndex = jokerCount - addedCount;
            float cardPileGap = _cardData.CardPileGap;
            for (int i = 0; i < addedCount; i++)
            {
                int itemIndex = addedIndex + i;
                _CreateItemRenderer(itemIndex, jokerCount, cardPileGap);
            }

            if (_jokerDeckRenderer.HasNextItem)
            {
                _jokerDeckRenderer.PeekItem.CollierObject.SetActive(true);
            }
            _jokerDeckRenderer.UpdateCount();
        }

        private void DrawItemModel()
        {
            if (canCollect.Invoke(CardNumber.Num_Joker))
            {
                _jokerDeckModel.DrawPeekItem();
            }
        }

        private void DrawItemRenderer()
        {
            (Transform itemRoot, Vector3 itemPosition, int sortingOrder) = requestCollect(
                CardCollectorTriggerType.JokerDeck,
                CardNumber.Num_Joker,
                CardColor.Color_Random,
                CardType.Type_None,
                SubCardType.SubType_None,
                -1,
                false
            );
            _jokerDeckRenderer.DrawItem(
                itemRoot,
                itemPosition,
                sortingOrder,
                moveDuration: _cardData.CardMoveDuration,
                moveEase: _cardData.CardMoveEase,
                scaleValue: _cardData.CardScale,
                scaleDuration: _cardData.CardScaleDuration,
                scaleEase: _cardData.CardScaleEase
            );
            if (_jokerDeckRenderer.HasNextItem)
            {
                _jokerDeckRenderer.PeekItem.CollierObject.SetActive(true);
            }
        }

        public void BeginLootAnimation()
        {
            // Do nothing
        }

        public void EndLootAnimation(long bonusCount)
        {
            _jokerDeckModel.AddJokerCount((int)bonusCount);
        }

        public int GetCloneCount(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            return 1;
        }

        public void CloneCardRendererList(IReadOnlyList<GameBoardCardRenderer> cardRendererList, IEnumerable<CardRendererCloneData> cloneDataList)
        {
            GameBoardCardRenderer cardRenderer = cardRendererList[0];

            Sprite cardSprite = _cardData.GetCardSprite(CardNumber.Num_Joker, CardColor.Color_Black, CardFace.Face_Up, CardType.Type_None);
            cardRenderer.UpdateAllRenderers(cardSprite, default, false, false, Vector3.zero);

            cardRenderer.CachedTransform.position = GetLootAnimtionEndPoint();
        }

        public void DrawCard(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            DrawItemModel();
        }
    }
}
