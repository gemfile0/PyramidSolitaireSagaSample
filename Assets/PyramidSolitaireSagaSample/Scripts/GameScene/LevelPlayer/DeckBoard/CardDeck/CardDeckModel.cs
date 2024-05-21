using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardDeck
{
    [Serializable]
    public class CardDeckSaveItemData
    {
        public int ID;
        public CommonCardInfo CardInfo;
    }

    [Serializable]
    public class CardDeckSaveData
    {
        public List<CardDeckSaveItemData> itemDataList;
    }

    public class CardDeckItemModel : ICardSelectionItemModel
    {
        public int ID => Index;
        public int Index { get; private set; }
        public CommonCardInfo CardInfo { get; private set; }

        private Action<CardDeckItemModel> _onCardInfoChanged;

        public CardDeckItemModel(int index, CommonCardInfo cardInfo, Action<CardDeckItemModel> onCardInfoChanged)
        {
            Index = index;
            CardInfo = cardInfo;
            _onCardInfoChanged = onCardInfoChanged;
        }

        public void UpdateCardInfo(CardSelectionInfo cardSelectionInfo)
        {
            CardInfo.UpdateInfo(cardSelectionInfo);
            _onCardInfoChanged?.Invoke(this);
        }

        public void UpdateCardInfo(
            CardNumber cardNumber,
            CardColor cardColor,
            CardFace cardFace,
            CardType cardType,
            SubCardType subCardType,
            int subCardTypeOption,
            int subCardTypeOption2,
            bool isBonusLabel
        )
        {
            CardInfo.UpdateInfo(cardNumber, cardColor, cardFace, cardType, subCardType, subCardTypeOption, subCardTypeOption2, isBonusLabel);
            _onCardInfoChanged?.Invoke(this);
        }
    }

    public class CardDeckModel : MonoBehaviour
    {
        public event Action<IEnumerable<CardDeckItemModel>> onItemModelRestored;
        public event Action<IEnumerable<CardDeckItemModel>> onItemModelChanged;
        public event Action<IEnumerable<CardDeckItemModel>> onCardDeckDrawn;
        public event Action<CardDeckItemModel, bool> onItemModelSelected;
        public event Action<CardDeckItemModel> onCardInfoChanged;
        public event Action<CardNumber, CardColor, SubCardType> onPeekItemModelDrawn;
        public event Action<CardNumber, CardColor> onPeekItemModelRevealed;

        public int ItemModelCount => _itemModelList.Count;
        public IEnumerable<CardDeckItemModel> ItemModelList => _itemModelList;
        private List<CardDeckItemModel> _itemModelList;

        public CardDeckItemModel PeekItem => 0 < _itemModelList.Count ? _itemModelList[0] : null;
        private CardDeckItemModel _selectedItemModel;

        private void Awake()
        {
            _itemModelList = new();
        }

        internal void ChangeDeckCount(int count)
        {
            if (count < 0)
            {
                Debug.LogWarning("카드 덱은 0보다 작을 수 없습니다.");
                return;
            }

            bool isChanged = count != _itemModelList.Count;
            if (count > _itemModelList.Count)
            {
                while (count > _itemModelList.Count)
                {
                    var cardInfo = new CommonCardInfo(
                        CardNumber.Num_Random,
                        CardColor.Color_Random,
                        CardFace.Face_Up,
                        CardType.Type_None,
                        SubCardType.SubType_None,
                        subCardTypeOption: -1,
                        subCardTypeOption2: -1,
                        isBonusLabel: false
                    );
                    _itemModelList.Add(NewItemModel(cardInfo));
                }
            }
            else if (count < _itemModelList.Count)
            {
                while (count < _itemModelList.Count)
                {
                    _itemModelList.RemoveAt(_itemModelList.Count - 1);
                }
            }

            if (isChanged)
            {
                onItemModelChanged?.Invoke(ItemModelList);
            }
        }

        private CardDeckItemModel NewItemModel(CommonCardInfo cardInfo)
        {
            return new CardDeckItemModel(
                _itemModelList.Count,
                cardInfo,
                OnCardInfoChanged
            );
        }

        private void OnCardInfoChanged(CardDeckItemModel itemModel)
        {
            onCardInfoChanged?.Invoke(itemModel);
            onItemModelChanged?.Invoke(ItemModelList);
        }

        internal void SelectItemModel(int itemIndex)
        {
            DeselectItemModel();

            _selectedItemModel = _itemModelList[itemIndex];
            onItemModelSelected?.Invoke(_selectedItemModel, true);
        }

        internal void DeselectItemModel()
        {
            CardDeckItemModel prevSelectedItemModel = _selectedItemModel;
            if (_selectedItemModel != null)
            {
                _selectedItemModel = null;

                onItemModelSelected?.Invoke(prevSelectedItemModel, false);
            }
        }

        internal void ClearItemModelList()
        {
            _itemModelList.Clear();
            onItemModelChanged?.Invoke(ItemModelList);
        }

        internal void RestoreSaveData(string dataStr)
        {
            _itemModelList.Clear();

            if (string.IsNullOrEmpty(dataStr) == false)
            {
                CardDeckSaveData saveData = JsonUtility.FromJson<CardDeckSaveData>(dataStr);
                foreach (CardDeckSaveItemData itemData in saveData.itemDataList)
                {
                    int itemIndex = itemData.ID;
                    CommonCardInfo cardInfo = itemData.CardInfo;
                    while (itemIndex >= _itemModelList.Count)
                    {
                        _itemModelList.Add(NewItemModel(cardInfo));
                    }
                }

                onItemModelRestored?.Invoke(ItemModelList);
            }
        }

        internal void DrawPeekItemModel(CardNumber cardNumber, CardColor cardColor, SubCardType consumedSubCardType)
        {
            CardDeckItemModel itemModel = PeekItem;
            CommonCardInfo originCardInfo = itemModel.CardInfo;
            itemModel.UpdateCardInfo(cardNumber, cardColor, originCardInfo.CardFace, originCardInfo.CardType, consumedSubCardType, originCardInfo.SubCardTypeOption, -1, false);
            onPeekItemModelDrawn?.Invoke(cardNumber, cardColor, consumedSubCardType);

            _itemModelList.RemoveAt(0);
            onCardDeckDrawn?.Invoke(ItemModelList);
        }

        internal void DrawPeekItemModel(SubCardType consumedSubCardType)
        {
            CardDeckItemModel itemModel = PeekItem;
            CommonCardInfo originCardInfo = itemModel.CardInfo;
            itemModel.UpdateCardInfo(originCardInfo.CardNumber, originCardInfo.CardColor, originCardInfo.CardFace, originCardInfo.CardType, consumedSubCardType, originCardInfo.SubCardTypeOption, -1, false);
            onPeekItemModelDrawn?.Invoke(originCardInfo.CardNumber, originCardInfo.CardColor, consumedSubCardType);

            _itemModelList.RemoveAt(0);
            onCardDeckDrawn?.Invoke(ItemModelList);
        }

        internal void RevealPeekItemModel(CardNumber cardNumber, CardColor cardColor)
        {
            CardDeckItemModel itemModel = PeekItem;
            CommonCardInfo originCardInfo = itemModel.CardInfo;
            itemModel.UpdateCardInfo(cardNumber, cardColor, originCardInfo.CardFace, originCardInfo.CardType, originCardInfo.SubCardType, originCardInfo.SubCardTypeOption, -1, false);
            onPeekItemModelRevealed?.Invoke(cardNumber, cardColor);
        }
    }
}
