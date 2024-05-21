using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardPool;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardPool
{
    public struct PickedCardInfo
    {
        public bool hasPoolItem;
        public CardNumber cardNumber;
        public CardColor cardColor;

        public PickedCardInfo(bool hasPoolItem, CardNumber cardNumber, CardColor cardColor)
        {
            this.hasPoolItem = hasPoolItem;
            this.cardNumber = cardNumber;
            this.cardColor = cardColor;
        }
    }

    public interface ICardPoolPicker
    {
        PickedCardInfo PickRandomCard(CardNumber cardNumber, CardColor cardColor);
    }

    public class CardPoolPresenter : MonoBehaviour,
                                     ILevelRestorable,
                                     ICardPoolPicker
    {
        [Header("Model")]
        [SerializeField] private CardPoolModel _cardPoolModel;

        [Header("View")]
        [SerializeField] private CardPoolEditorUI _cardPoolUI;

        public event Action<int> onDeckCountUpdated
        {
            add { _cardPoolModel.onDeckCountUpdated += value; }
            remove { _cardPoolModel.onDeckCountUpdated -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.CardPool;

        private void Awake()
        {
            // 게임 오브젝트가 꺼져있기 때문에 수동으로 초기화
            _cardPoolUI.Init();
        }

        private void OnEnable()
        {
            _cardPoolModel.onItemModelUpdated += _cardPoolUI.UpdateUI;
            _cardPoolModel.onItemModelRestored += _cardPoolUI.UpdateUI;
        }

        private void OnDisable()
        {
            _cardPoolModel.onItemModelUpdated -= _cardPoolUI.UpdateUI;
            _cardPoolModel.onItemModelRestored -= _cardPoolUI.UpdateUI;
        }

        public void RestoreLevelData(string data)
        {
            _cardPoolModel.RestoreSaveData(data);
        }

        public void UpdateBoardCardModelList(IEnumerable<IGameBoardCardModel> boardCardModelList)
        {
            _cardPoolModel.UpdatePoolItemModelDict(boardCardModelList);
        }

        public void UpdateCardDeckItemModelList(IEnumerable<CardDeckItemModel> deckItemModelList)
        {
            _cardPoolModel.UpdatePoolItemModelDict(deckItemModelList);
        }

        public void UpdateCardCollectorItemModelList(IEnumerable<CardCollectorItemModel> collectorItemModelList)
        {
            _cardPoolModel.UpdatePoolItemModelDict(collectorItemModelList);
        }

        public PickedCardInfo PickRandomCard(CardNumber cardNumber, CardColor cardColor)
        {
            CardNumber pickedCardNumber = default;
            CardColor pickedCardColor = default;

            CardPoolItemModel? itemModel = _cardPoolModel.GetRandomItem(cardNumber, cardColor);
            bool hasPoolItem = itemModel != null;
            if (itemModel != null)
            {
                pickedCardNumber = itemModel.Value.cardNumber;
                pickedCardColor = itemModel.Value.cardColor;
            }

            PickedCardInfo pickedCardInfo = new PickedCardInfo(
                hasPoolItem,
                pickedCardNumber,
                pickedCardColor
            );
            return pickedCardInfo;
        }
    }
}
