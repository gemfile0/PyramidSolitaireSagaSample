using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCardSequence
{
    [Serializable]
    public class BonusCardSequenceSaveItemData
    {
        public int ID;
        public CommonCardInfo CardInfo;
    }

    [Serializable]
    public class BonusCardSequenceSaveData
    {
        public List<BonusCardSequenceSaveItemData> itemDataList;
    }

    public struct BonusCardItemModel
    {
        public CardNumber cardNumber;
        public CardColor cardColor;
    }

    public class BonusCardSequenceItemModel : ICardSelectionItemModel
    {
        public int ID => Index;
        public int Index { get; private set; }
        public CommonCardInfo CardInfo { get; private set; }

        private Action<BonusCardSequenceItemModel> _onCardInfoChanged;

        public BonusCardSequenceItemModel(int index, CommonCardInfo cardInfo, Action<BonusCardSequenceItemModel> onCardInfoChanged)
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
    }

    public class BonusCardSequenceModel : MonoBehaviour
    {
        // Sequence Item Model
        public event Action<IEnumerable<BonusCardSequenceItemModel>> onSequenceItemModelRestored;
        public event Action<IEnumerable<BonusCardSequenceItemModel>> onSequenceItemModelUpdated;
        public event Action<BonusCardSequenceItemModel> onSequenceCardInfoChanged;
        public event Action<BonusCardSequenceItemModel, bool> onSequenceItemModelSelected;
        public event Action<IEnumerable<BonusCardSequenceItemModel>> onSequenceCountChanged;

        public int SequenceItemModelCount => _sequenceItemModelList.Count;
        public IEnumerable<BonusCardSequenceItemModel> SequenceItemModelList => _sequenceItemModelList;
        private List<BonusCardSequenceItemModel> _sequenceItemModelList;
        private BonusCardSequenceItemModel _selectedSequenceItemModel;

        // Item Model
        public event Action<IEnumerable<BonusCardItemModel>, int> onBonusCardCountAdded;
        public event Action<CardNumber, CardColor, SubCardType> onItemModelDrawn;
        public event Action<CardNumber, CardColor> onItemModelRevealed;
        public event Action<int> onBonusCardCountUpdated;

        public IEnumerable<BonusCardItemModel> ItemModelList => _itemModelList;
        public int ItemModelCount => _itemModelList.Count;

        private List<BonusCardItemModel> _itemModelList;
        private List<BonusCardItemModel> _addedItemModelList;
        private List<BonusCardItemModel> _randomItemModelList;

        private int _sequenceItemIndex;

        private void Awake()
        {
            _sequenceItemModelList = new();

            _itemModelList = new();
            _addedItemModelList = new();
            _randomItemModelList = new();

            _sequenceItemIndex = 0;

            foreach (CardNumber cardNumber in Enum.GetValues(typeof(CardNumber)))
            {
                if (cardNumber == CardNumber.Num_Joker
                    || cardNumber == CardNumber.Num_Random)
                {
                    continue;
                }

                foreach (CardColor cardColor in Enum.GetValues(typeof(CardColor)))
                {
                    if (cardColor == CardColor.Color_Random)
                    {
                        continue;
                    }

                    _randomItemModelList.Add(new BonusCardItemModel
                    {
                        cardNumber = cardNumber,
                        cardColor = cardColor
                    });
                }
            }
        }

        public void ChangeSequenceCount(int count)
        {
            if (count < 0)
            {
                Debug.LogWarning("보너스 카드 시퀀스는 0보다 작을 수 없습니다.");
                return;
            }

            if (count > _sequenceItemModelList.Count)
            {
                while (count > _sequenceItemModelList.Count)
                {
                    var cardInfo = new CommonCardInfo(
                        CardNumber.Num_Random,
                        CardColor.Color_Random,
                        CardFace.Face_Up,
                        CardType.Type_None,
                        SubCardType.SubType_None,
                        -1,
                        -1,
                        false
                    );
                    _sequenceItemModelList.Add(NewSequenceItemModel(cardInfo));
                }
            }
            else if (count < _sequenceItemModelList.Count)
            {
                while (count < _sequenceItemModelList.Count)
                {
                    _sequenceItemModelList.RemoveAt(_sequenceItemModelList.Count - 1);
                }
            }

            onSequenceCountChanged?.Invoke(SequenceItemModelList);
        }

        private BonusCardSequenceItemModel NewSequenceItemModel(CommonCardInfo cardInfo)
        {
            return new BonusCardSequenceItemModel(
                _sequenceItemModelList.Count,
                cardInfo,
                OnSequenceCardInfoChanged
            );
        }

        private void OnSequenceCardInfoChanged(BonusCardSequenceItemModel itemModel)
        {
            onSequenceCardInfoChanged?.Invoke(itemModel);
            onSequenceItemModelUpdated?.Invoke(SequenceItemModelList);
        }

        internal void AddBonusCard(int addingCount)
        {
            BonusCardItemModel itemModel = new BonusCardItemModel()
            {
                cardNumber = CardNumber.Num_Random,
                cardColor = CardColor.Color_Random,
            };

            if (_sequenceItemIndex < _sequenceItemModelList.Count)
            {
                BonusCardSequenceItemModel sequenceItemModel = _sequenceItemModelList[_sequenceItemIndex];
                CommonCardInfo cardInfo = sequenceItemModel.CardInfo;

                itemModel.cardNumber = cardInfo.CardNumber;
                itemModel.cardColor = cardInfo.CardColor;
                _sequenceItemIndex += 1;
            }

            _addedItemModelList.Clear();
            for (int i = 0; i < addingCount; i++)
            {
                _addedItemModelList.Add(itemModel);
            }
            onBonusCardCountAdded?.Invoke(_addedItemModelList, _addedItemModelList.Count);

            _itemModelList.AddRange(_addedItemModelList);
            onBonusCardCountUpdated?.Invoke(_itemModelList.Count);
        }

        internal void DrawPeekItemModel(SubCardType consumedSubCardType)
        {
            PickPeekCardInfo();

            BonusCardItemModel itemModel = _itemModelList[0];
            onItemModelDrawn?.Invoke(itemModel.cardNumber, itemModel.cardColor, consumedSubCardType);

            _itemModelList.RemoveAt(0);
            onBonusCardCountUpdated?.Invoke(_itemModelList.Count);
        }

        public void RevealPeekItemModel()
        {
            PickPeekCardInfo();

            BonusCardItemModel itemModel = _itemModelList[0];
            onItemModelRevealed?.Invoke(itemModel.cardNumber, itemModel.cardColor);
        }

        private void PickPeekCardInfo()
        {
            BonusCardItemModel itemModel = _itemModelList[0];
            if (itemModel.cardNumber == CardNumber.Num_Random)
            {
                (itemModel.cardNumber, itemModel.cardColor) = GetRandomNumber(itemModel.cardColor);
            }
            else if (itemModel.cardColor == CardColor.Color_Random)
            {
                (itemModel.cardNumber, itemModel.cardColor) = GetRandomColor(itemModel.cardNumber);
            }
            _itemModelList[0] = itemModel;
        }

        private (CardNumber cardNumber, CardColor cardColor) GetRandomNumber(CardColor cardColor)
        {
            CardNumber randomNumber = default;
            CardColor randomColor = default;

            int poolCount = 0;
            foreach (BonusCardItemModel itemModel in _randomItemModelList)
            {
                if (cardColor == CardColor.Color_Random
                    || itemModel.cardColor == cardColor)
                {
                    poolCount += 1;
                }
            }

            if (poolCount == 0)
            {
                poolCount = _randomItemModelList.Count;
            }

            int randomIndex = UnityEngine.Random.Range(0, poolCount);
            Debug.Log($"GetRandomNumber : {randomIndex} / {poolCount}");
            int totalItemCount = 0;
            foreach (BonusCardItemModel itemModel in _randomItemModelList)
            {
                if (cardColor == CardColor.Color_Random
                    || itemModel.cardColor == cardColor)
                {
                    totalItemCount += 1;
                }

                if (randomIndex < totalItemCount)
                {
                    randomNumber = itemModel.cardNumber;
                    randomColor = itemModel.cardColor;
                    break;
                }
            }

            return (randomNumber, randomColor);
        }

        private (CardNumber cardNumber, CardColor cardColor) GetRandomColor(CardNumber cardNumber)
        {
            CardNumber randomNumber = default;
            CardColor randomColor = default;

            int poolCount = 0;
            foreach (BonusCardItemModel itemModel in _randomItemModelList)
            {
                if (cardNumber == CardNumber.Num_Random
                    || itemModel.cardNumber == cardNumber)
                {
                    poolCount += 1;
                    Debug.Log($"GetRandomColor : {itemModel.cardNumber}, {itemModel.cardColor}");
                }
            }

            if (poolCount == 0)
            {
                poolCount = _randomItemModelList.Count;
            }

            int randomIndex = UnityEngine.Random.Range(0, poolCount);
            Debug.Log($"GetRandomNumber : {randomIndex} / {poolCount}");
            int totalItemCount = 0;
            foreach (BonusCardItemModel itemModel in _randomItemModelList)
            {
                if (cardNumber == CardNumber.Num_Random
                   || itemModel.cardNumber == cardNumber)
                {
                    totalItemCount += 1;
                }

                if (randomIndex < totalItemCount)
                {
                    randomNumber = itemModel.cardNumber;
                    randomColor = itemModel.cardColor;
                    break;
                }
            }

            return (randomNumber, randomColor);
        }

        internal void DeselectSequenceItemModel()
        {
            BonusCardSequenceItemModel prevSelectedSequenceItemModel = _selectedSequenceItemModel;
            if (_selectedSequenceItemModel != null)
            {
                _selectedSequenceItemModel = null;

                onSequenceItemModelSelected?.Invoke(prevSelectedSequenceItemModel, false);
            }
        }

        internal void SelectItemModel(int itemIndex)
        {
            DeselectSequenceItemModel();

            _selectedSequenceItemModel = _sequenceItemModelList[itemIndex];
            onSequenceItemModelSelected?.Invoke(_selectedSequenceItemModel, true);
        }

        internal void ClearItemModelList()
        {
            _sequenceItemModelList.Clear();
            onSequenceItemModelUpdated?.Invoke(SequenceItemModelList);
        }

        internal void RestoreSaveData(string data)
        {
            _sequenceItemModelList.Clear();

            if (string.IsNullOrEmpty(data) == false)
            {
                BonusCardSequenceSaveData saveData = JsonUtility.FromJson<BonusCardSequenceSaveData>(data);
                foreach (BonusCardSequenceSaveItemData itemData in saveData.itemDataList)
                {
                    int itemIndex = itemData.ID;
                    CommonCardInfo cardInfo = itemData.CardInfo;
                    while (itemIndex >= _sequenceItemModelList.Count)
                    {
                        _sequenceItemModelList.Add(NewSequenceItemModel(cardInfo));
                    }
                }

                onSequenceItemModelRestored?.Invoke(SequenceItemModelList);
            }

            onBonusCardCountUpdated?.Invoke(_itemModelList.Count);
        }

    }
}
