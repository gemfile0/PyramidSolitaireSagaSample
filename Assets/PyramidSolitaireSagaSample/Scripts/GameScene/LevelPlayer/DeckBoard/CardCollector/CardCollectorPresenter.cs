using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using PyramidSolitaireSagaSample.System;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardCollector
{
    public enum CardCollectorTriggerType
    {
        GameBoard,
        CardDeck,
        JokerDeck
    }

    public interface ICardCollectRequester
    {
        event Func<CardCollectorTriggerType, CardNumber, CardColor, CardType, SubCardType, int, bool,
                   (Transform itemRoot, Vector3 itemPosition, int sortingOrder)> requestCollect;
    }

    public interface ICardCollectChecker
    {
        event Func<CardNumber, bool> canCollect;
    }

    public class CardCollectorPresenter : MonoBehaviour,
                                          IGameObjectFinderSetter,
                                          ILevelTutorialCardCloner
    {
        [Header("Data")]
        [SerializeField] private CardData _cardData;

        [Header("Model")]
        [SerializeField] private CardCollectorModel _cardCollectorModel;

        [Header("View")]
        [SerializeField] private CardCollectorRenderer _cardCollectorRenderer;

        public event Action<IEnumerable<CardCollectorItemModel>> onItemModelUpdated
        {
            add { _cardCollectorModel.onItemModelUpdated += value; }
            remove { _cardCollectorModel.onItemModelUpdated -= value; }
        }

        private IEnumerable<ICardCollectRequester> _requesters;
        private IEnumerable<ICardCollectChecker> _checkers;

        public LevelTutorialCloneType CardCloneType => LevelTutorialCloneType.CardCollector;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _requesters = finder.FindGameObjectOfType<ICardCollectRequester>();
            _checkers = finder.FindGameObjectOfType<ICardCollectChecker>();
        }

        private void OnEnable()
        {
            foreach (ICardCollectRequester requester in _requesters)
            {
                requester.requestCollect += RequestCollect;
            }

            foreach (ICardCollectChecker checker in _checkers)
            {
                checker.canCollect += CanCollect;
            }
        }

        private void OnDisable()
        {
            foreach (ICardCollectRequester requester in _requesters)
            {
                requester.requestCollect -= RequestCollect;
            }

            foreach (ICardCollectChecker checker in _checkers)
            {
                checker.canCollect -= CanCollect;
            }
        }

        private bool CanCollect(CardNumber nextCardNumber)
        {
            bool result = false;
            if (_cardCollectorModel.ItemCount > 0)
            {
                CardNumber peekCardNumber = _cardCollectorModel.PeekItem.cardNumber;
                int valueDiff = Math.Abs((int)nextCardNumber - (int)peekCardNumber);

                result = nextCardNumber == CardNumber.Num_Joker
                         || peekCardNumber == CardNumber.Num_Joker
                         || valueDiff == 1
                         || valueDiff == 12; // Card_A vs Card_K
            }
            return result;
        }

        public (Transform, Vector3, int) RequestCollect(
            CardCollectorTriggerType type,
            CardNumber cardNumber,
            CardColor cardColor,
            CardType cardType,
            SubCardType subCardType,
            int subCardTypeOption,
            bool isBonusLabel
        )
        {
            float cardPileGap = _cardData.CardPileGap;
            int itemIndex = _cardCollectorModel.ItemCount;

            Transform itemRoot = _cardCollectorRenderer.ItemRendererRoot;
            Vector3 originPosition = _cardCollectorRenderer.CachedTransform.position;
            Vector3 itemPosition = originPosition + CardSortingPosition.CalculateAsAscending(itemIndex, cardPileGap);
            int sortingOrder = CardSortingOrder.Calculate(CardSortingOrderType.CardCollector, itemIndex);
            _cardCollectorModel.AddItem(type, cardNumber, cardColor, cardType, subCardType, subCardTypeOption, isBonusLabel);
            return (itemRoot, itemPosition, sortingOrder);
        }

        public void DrawCard(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            // Do nothing
        }

        public int GetCloneCount(IEnumerable<CardRendererCloneData> cloneDataList)
        {
            return 1;
        }

        public void CloneCardRendererList(IReadOnlyList<GameBoardCardRenderer> cardRendererList, IEnumerable<CardRendererCloneData> cloneDataList)
        {
            GameBoardCardRenderer cardRenderer = cardRendererList[0];
            CardCollectorItemModel itemModel = _cardCollectorModel.PeekItem;
            CardFace cardFaceUp = CardFace.Face_Up;
            Sprite cardSprite = _cardData.GetCardSprite(itemModel.cardNumber, itemModel.cardColor, cardFaceUp, itemModel.cardType);
            SubCardTypeRendererInfo subCardTypeRendererInfo = new(cardFaceUp, itemModel.subCardType, itemModel.subCardTypeOption, 0, false);
            cardRenderer.UpdateAllRenderers(cardSprite, subCardTypeRendererInfo, false, false, Vector3.zero);

            float cardPileGap = _cardData.CardPileGap;
            int itemIndex = _cardCollectorModel.ItemCount;
            Vector3 originPosition = _cardCollectorRenderer.CachedTransform.position;
            Vector3 itemPosition = originPosition + CardSortingPosition.CalculateAsAscending(itemIndex, cardPileGap);
            cardRenderer.CachedTransform.position = itemPosition;
        }
    }
}
