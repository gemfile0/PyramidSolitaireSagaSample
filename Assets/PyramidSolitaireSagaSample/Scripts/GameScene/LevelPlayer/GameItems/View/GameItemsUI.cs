using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameItems
{
    public class GameItemsUI : MonoBehaviour
    {
        [SerializeField] private List<GameItemUI> _itemUiList;

        public event Action onHintsButtonClick;
        private Dictionary<GameItemType, GameItemUI> _itemUiDict;

        public void Init()
        {
            _itemUiDict = new();
            foreach (GameItemUI item in _itemUiList)
            {
                _itemUiDict.Add(item.ItemType, item);
            }
        }

        internal void UpdateUI(ReadOnlyDictionary<GameItemType, int> itemCountDict)
        {
            foreach (KeyValuePair<GameItemType, int> pair in itemCountDict)
            {
                _itemUiDict[pair.Key].UpdateUI(pair.Value);
            }
        }

        public void OnHintsButtonClick()
        {
            onHintsButtonClick?.Invoke();
        }
    }
}
