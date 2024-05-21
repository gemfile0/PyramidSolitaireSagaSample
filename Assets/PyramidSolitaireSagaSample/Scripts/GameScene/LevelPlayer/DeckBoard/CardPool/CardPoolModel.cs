using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System;
using UnityEngine;
using System.Linq;
using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardPool
{
    [Serializable]
    public class CardPoolSaveData
    {
        public List<CardPoolItemModel> itemDataList;
    }

    [Serializable]
    public struct CardPoolItemModel
    {
        public CardNumber cardNumber;
        public CardColor cardColor;
        public int count;

        public override string ToString()
        {
            return $"{cardColor}&{cardNumber} : {count}";
        }
    }

    public class CardPoolModel : MonoBehaviour
    {
        public event Action<int> onDeckCountUpdated;
        public event Action<ReadOnlyDictionary<string, CardPoolItemModel>, ReadOnlyDictionary<string, CardPoolItemModel>> onItemModelUpdated;
        public event Action<ReadOnlyDictionary<string, CardPoolItemModel>, ReadOnlyDictionary<string, CardPoolItemModel>> onItemModelRestored;

        public ReadOnlyDictionary<string, CardPoolItemModel> TotalItemModelDictReadOnly => _totalItemModelDictReadOnly;
        private ReadOnlyDictionary<string, CardPoolItemModel> _totalItemModelDictReadOnly;
        public ReadOnlyDictionary<string, CardPoolItemModel> PoolItemModelDictReadOnly => _poolItemModelDictReadOnly;
        private ReadOnlyDictionary<string, CardPoolItemModel> _poolItemModelDictReadOnly;

        private Dictionary<string, CardPoolItemModel> _totalItemModelDict;
        private Dictionary<string, CardPoolItemModel> _poolItemModelDict;
        private Dictionary<string, CardPoolItemModel> _boardItemModelDict;
        private Dictionary<string, CardPoolItemModel> _deckItemModelDict;
        private Dictionary<string, CardPoolItemModel> _colletorItemModelDict;

        private Dictionary<CardNumber, string> _cardNumberStrDictionary;
        private Dictionary<CardColor, string> _cardColorCharDictionary;
        private StringBuilder _cardKeyBuilder;
        private StringBuilder _logBuilder;

        private void Awake()
        {
            _totalItemModelDict = new Dictionary<string, CardPoolItemModel>();
            _poolItemModelDict = new Dictionary<string, CardPoolItemModel>();
            _boardItemModelDict = new Dictionary<string, CardPoolItemModel>();
            _deckItemModelDict = new Dictionary<string, CardPoolItemModel>();
            _colletorItemModelDict = new Dictionary<string, CardPoolItemModel>();

            _totalItemModelDictReadOnly = new ReadOnlyDictionary<string, CardPoolItemModel>(_totalItemModelDict);
            _poolItemModelDictReadOnly = new ReadOnlyDictionary<string, CardPoolItemModel>(_poolItemModelDict);

            _cardNumberStrDictionary = new Dictionary<CardNumber, string>();
            _cardColorCharDictionary = new Dictionary<CardColor, string>();
            _cardKeyBuilder = new StringBuilder();
            _logBuilder = new StringBuilder();

            foreach (CardColor cardColor in Enum.GetValues(typeof(CardColor)))
            {
                _cardColorCharDictionary[cardColor] = cardColor.ToString().Replace("Color_", "").Substring(0, 3);
            }

            foreach (CardNumber cardNumber in Enum.GetValues(typeof(CardNumber)))
            {
                _cardNumberStrDictionary[cardNumber] = cardNumber.ToString().Replace("Num_", "");
                if (cardNumber == CardNumber.Num_Random
                    || cardNumber == CardNumber.Num_Joker)
                {
                    continue;
                }

                foreach (CardColor cardColor in Enum.GetValues(typeof(CardColor)))
                {
                    if (cardColor == CardColor.Color_Random)
                    {
                        continue;
                    }

                    var itemModel = new CardPoolItemModel()
                    {
                        cardNumber = cardNumber,
                        cardColor = cardColor,
                        count = 2,
                    };

                    string cardKey = MakeCardKey(itemModel.cardNumber, itemModel.cardColor);
                    _totalItemModelDict.Add(cardKey, itemModel);
                }
            }
        }

        internal void UpdatePoolItemModelDict(IEnumerable<IGameBoardCardModel> boardCardModelList)
        {
            _boardItemModelDict.Clear();
            foreach (IGameBoardCardModel cardModel in boardCardModelList)
            {
                CommonCardInfo cardInfo = cardModel.CardInfo;
                _UpdateItemModel(
                    _boardItemModelDict, 
                    cardInfo.CardNumber,
                    cardInfo.CardColor,
                    returnCondition: (CardNumber cardNumber, CardColor cardColor) =>
                    {
                        return cardNumber == CardNumber.Num_Joker;
                    }
                );
            }

            UpdatePoolItemModelDict();
        }

        internal void UpdatePoolItemModelDict(IEnumerable<CardDeckItemModel> deckItemModelList)
        {
            _deckItemModelDict.Clear();
            foreach (CardDeckItemModel itemModel in deckItemModelList)
            {
                CommonCardInfo cardInfo = itemModel.CardInfo;
                _UpdateItemModel(
                    _deckItemModelDict,
                    cardInfo.CardNumber,
                    cardInfo.CardColor,
                    returnCondition: (CardNumber cardNumber, CardColor cardColor) =>
                    {
                        return cardNumber == CardNumber.Num_Joker
                               || cardNumber == CardNumber.Num_Random
                               || cardColor == CardColor.Color_Random;
                    }
                );
            }

            UpdatePoolItemModelDict();
        }

        internal void UpdatePoolItemModelDict(IEnumerable<CardCollectorItemModel> collectorItemModelList)
        {
            _colletorItemModelDict.Clear();
            foreach (CardCollectorItemModel itemModel in collectorItemModelList)
            {
                _UpdateItemModel(
                    _colletorItemModelDict, 
                    itemModel.cardNumber, 
                    itemModel.cardColor,
                    returnCondition: (CardNumber cardNumber, CardColor cardColor) =>
                    {
                        return cardNumber == CardNumber.Num_Joker
                               || cardNumber == CardNumber.Num_Random
                               || cardColor == CardColor.Color_Random;
                    }
                );
            }
        }

        private void _UpdateItemModel(
            Dictionary<string, CardPoolItemModel> itemModelDict, 
            CardNumber cardNumber, 
            CardColor cardColor, 
            Func<CardNumber, CardColor, bool> returnCondition
        )
        {
            if (returnCondition.Invoke(cardNumber, cardColor))
            {
                return;
            }

            string cardKey = MakeCardKey(cardNumber, cardColor);
            if (itemModelDict.ContainsKey(cardKey) == false)
            {
                itemModelDict.Add(cardKey, new CardPoolItemModel()
                {
                    cardNumber = cardNumber,
                    cardColor = cardColor,
                    count = 0
                });
            }

            CardPoolItemModel boardItemModel = itemModelDict[cardKey];
            boardItemModel.count += 1;
            itemModelDict[cardKey] = boardItemModel;
        }

        private void UpdatePoolItemModelDict()
        {
            UpdatePoolItemModelDict_WithoutNotify();
            onItemModelUpdated?.Invoke(_poolItemModelDictReadOnly, _totalItemModelDictReadOnly);
        }

        private void UpdatePoolItemModelDict_WithoutNotify()
        {
            _logBuilder.Length = 0;
            _poolItemModelDict.Clear();
            int remainingDeckCount = 0;
            foreach (KeyValuePair<string, CardPoolItemModel> pair in _totalItemModelDict)
            {
                string key = pair.Key;
                CardPoolItemModel totalItemModel = pair.Value;
                int totalCount = pair.Value.count;
                int boardCount = 0;
                if (_boardItemModelDict.TryGetValue(key, out CardPoolItemModel boardItemModel))
                {
                    boardCount = boardItemModel.count;
                }
                int specifiedDeckCount = 0;
                if (_deckItemModelDict.TryGetValue(key, out CardPoolItemModel deckItemModel))
                {
                    specifiedDeckCount = deckItemModel.count;
                }
                int collectorCount = 0;
                if (_colletorItemModelDict.TryGetValue(key, out CardPoolItemModel collectorItemModel))
                {
                    collectorCount = collectorItemModel.count;
                }

                int deckCount = totalCount - boardCount - collectorCount;
                int poolCount = deckCount - specifiedDeckCount;
                remainingDeckCount += deckCount;

                //_logBuilder.Append($"UpdatePoolItemModelDict_WithoutNotify 1-1 : {key}, {deckCount} = {totalCount} - {boardCount} - {collectorCount}");
                //_logBuilder.AppendLine();
                //_logBuilder.Append($"UpdatePoolItemModelDict_WithoutNotify 1-2 : {key}, {poolCount} = {deckCount} - {specifiedDeckCount}");
                //_logBuilder.AppendLine();

                var poolItemModel = new CardPoolItemModel()
                {
                    cardNumber = totalItemModel.cardNumber,
                    cardColor = totalItemModel.cardColor,
                    count = poolCount
                };
                _poolItemModelDict.Add(key, poolItemModel);
            }
            
            //Debug.Log(_logBuilder.ToString());

            int boardCountOfRandomValue = GetBoardCountOfRandomValue();
            remainingDeckCount -= boardCountOfRandomValue;

            onDeckCountUpdated?.Invoke(remainingDeckCount);
        }

        private int GetBoardCountOfRandomValue()
        {
            int randomColorBoardCount = 0;
            int i = 0;
            foreach (KeyValuePair<string, CardPoolItemModel> pair in _boardItemModelDict)
            {
                string key = pair.Key;
                CardPoolItemModel boardItemModel = pair.Value;
                if (boardItemModel.cardNumber == CardNumber.Num_Random
                    || boardItemModel.cardColor == CardColor.Color_Random)
                {
                    //Debug.Log($"GetBoardCountInRandomValue : {i}, {boardItemModel}");
                    randomColorBoardCount += boardItemModel.count;
                    i += 1;
                }
            }
            return randomColorBoardCount;
        }

        private string MakeCardKey(CardNumber cardNumber, CardColor cardColor)
        {
            string cardNumberStr = _cardNumberStrDictionary[cardNumber];
            string cardColorStr = _cardColorCharDictionary[cardColor];

            _cardKeyBuilder.Length = 0;
            _cardKeyBuilder.Append(cardColorStr);
            _cardKeyBuilder.Append(cardNumberStr);
            return _cardKeyBuilder.ToString();
        }

        internal void ChangeTotalCount(CardNumber cardNumber, CardColor cardColor, int nextCount)
        {
            ChangeTotalCountWithoutNotify(cardNumber, cardColor, nextCount);

            UpdatePoolItemModelDict();
        }

        private void ChangeTotalCountWithoutNotify(CardNumber cardNumber, CardColor cardColor, int nextCount)
        {
            if (cardNumber == CardNumber.Num_Random
                || cardColor == CardColor.Color_Random)
            {
                return;
            }

            var nextItemModel = new CardPoolItemModel()
            {
                cardNumber = cardNumber,
                cardColor = cardColor,
                count = nextCount,
            };

            string cardKey = MakeCardKey(cardNumber, cardColor);
            _totalItemModelDict[cardKey] = nextItemModel;
        }

        internal void ResetTotalItemModel()
        {
            foreach (string key in _totalItemModelDict.Keys.ToList())
            {
                CardPoolItemModel itemModel = _totalItemModelDict[key];
                int cardCount = itemModel.cardNumber == CardNumber.Num_Random ? 0 : 2;
                itemModel.count = cardCount;
                _totalItemModelDict[key] = itemModel;
            }

            UpdatePoolItemModelDict();
        }

        internal void RestoreSaveData(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr) == false)
            {
                CardPoolSaveData saveData = JsonUtility.FromJson<CardPoolSaveData>(dataStr);
                foreach (CardPoolItemModel itemData in saveData.itemDataList)
                {
                    ChangeTotalCountWithoutNotify(itemData.cardNumber, itemData.cardColor, itemData.count);
                }

                UpdatePoolItemModelDict_WithoutNotify();
                onItemModelRestored?.Invoke(_poolItemModelDictReadOnly, _totalItemModelDictReadOnly);
            }
        }

        internal CardPoolItemModel? GetRandomItem(CardNumber cardNumber, CardColor cardColor)
        {
            CardPoolItemModel? randomItem = null;

            if (cardNumber == CardNumber.Num_Random)
            {
                randomItem = GetRandomNumberItem(cardColor);
            }
            else if (cardColor == CardColor.Color_Random)
            {
                randomItem = GetRandomColorItem(cardNumber);
            }

            return randomItem;
        }

        private CardPoolItemModel? GetRandomColorItem(CardNumber cardNumber)
        {
            CardPoolItemModel? randomItem = null;

            int poolCount = 0;
            foreach (CardPoolItemModel itemModel in _poolItemModelDict.Values)
            {
                if (cardNumber == CardNumber.Num_Random
                    || itemModel.cardNumber == cardNumber)
                {
                    poolCount += itemModel.count;
                }
            }

            if (poolCount == 0)
            {
                cardNumber = CardNumber.Num_Random;
                foreach (CardPoolItemModel itemModel in _poolItemModelDict.Values)
                {
                    poolCount += itemModel.count;
                }
            }

            int randomIndex = UnityEngine.Random.Range(0, poolCount);
            Debug.Log($"GetRandomColorItem : {randomIndex} / {poolCount}");
            int totalItemCount = 0;
            foreach (CardPoolItemModel itemModel in _poolItemModelDict.Values)
            {
                if (cardNumber == CardNumber.Num_Random
                    || itemModel.cardNumber == cardNumber)
                {
                    totalItemCount += itemModel.count;
                }

                if (randomIndex < totalItemCount)
                {
                    randomItem = itemModel;
                    break;
                }
            }

            return randomItem;
        }

        private CardPoolItemModel? GetRandomNumberItem(CardColor cardColor)
        {
            CardPoolItemModel? randomItem = null;

            int poolCount = 0;
            foreach (CardPoolItemModel itemModel in _poolItemModelDict.Values)
            {
                if (cardColor == CardColor.Color_Random
                    || itemModel.cardColor == cardColor)
                {
                    poolCount += itemModel.count;
                }
            }

            if (poolCount == 0)
            {
                cardColor = CardColor.Color_Random;
                foreach (CardPoolItemModel itemModel in _poolItemModelDict.Values)
                {
                    poolCount += itemModel.count;
                }
            }

            int randomIndex = UnityEngine.Random.Range(0, poolCount);
            Debug.Log($"GetRandomNumberItem : {randomIndex} / {poolCount}");
            int totalItemCount = 0;
            foreach (CardPoolItemModel itemModel in _poolItemModelDict.Values)
            {
                if (cardColor == CardColor.Color_Random
                    || itemModel.cardColor == cardColor)
                {
                    totalItemCount += itemModel.count;
                }

                if (randomIndex < totalItemCount)
                {
                    randomItem = itemModel;
                    break;
                }
            }

            return randomItem;
        }
    }
}
