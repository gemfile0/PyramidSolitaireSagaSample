using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameItems
{
    public enum GameItemType
    {
        AddJoker,
        MagicHammer,
        AddSevenCards,
    }

    [Serializable]
    public struct GameItemData
    {
        public GameItemType type;
        public int count;
    }

    [Serializable]
    public class GameItemsSaveData
    {
        public List<GameItemData> itemDataList;
    }

    public class GameItemsModel : MonoBehaviour
    {
        public event Action<ReadOnlyDictionary<GameItemType, int>> onItemModelRestored;

        private Dictionary<GameItemType, int> _itemCountDict;
        private ReadOnlyDictionary<GameItemType, int> _itemCountDictReadOnly;

        private void Awake()
        {
            _itemCountDict = new();
            _itemCountDictReadOnly = new(_itemCountDict);
        }

        internal void RestoreSaveData(string dataStr)
        {
            _itemCountDict.Clear();
            foreach (GameItemType itemType in Enum.GetValues(typeof(GameItemType)))
            {
                _itemCountDict[itemType] = 0;
            }

            if (string.IsNullOrEmpty(dataStr) == false)
            {
                GameItemsSaveData saveData = JsonUtility.FromJson<GameItemsSaveData>(dataStr);
                foreach (GameItemData itemData in saveData.itemDataList)
                {
                    GameItemType itemType = itemData.type;
                    int itemCount = itemData.count;
                    if (_itemCountDict.ContainsKey(itemType))
                    {
                        _itemCountDict[itemType] = itemCount;
                    }
                }
            }

            onItemModelRestored?.Invoke(_itemCountDictReadOnly);
        }
    }
}
