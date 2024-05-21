using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDeck
{
    public class CardDeckEditorUI : MonoBehaviour
    {
        [SerializeField] private GameObject _cardDeckPanel;
        [SerializeField] private Transform _itemRoot;
        [SerializeField] private CardDeckEditorItemUI _itemPrefab;
        [SerializeField] private CardDeckCountUI _cardDeckCountUI;
        [SerializeField] private TextMeshProUGUI _cardDeckText;

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private float _scrollSensitivity = 1f;

        public event Action<int> onValueChanged;
        public event Action<int> onStepperClick;
        public event Action<bool> onPanelActive;
        public event Action<int> onItemSelected;

        private GameObjectPool<CardDeckEditorItemUI> _itemPool;
        private List<CardDeckEditorItemUI> _itemList;

        public void Init()
        {
            _itemPool = new(parent: _itemRoot, prefab: _itemPrefab.gameObject, maxSize: 100);
            _itemList = new();

            _scrollRect.scrollSensitivity = _scrollSensitivity;
        }

        private void Start()
        {
            OnCloseButtonClick();
        }

        public void OnValueChanged(string valueStr)
        {
            if (int.TryParse(valueStr, out int value))
            {
                onValueChanged?.Invoke(value);
            }
        }

        public void OnStepperClick(int offset)
        {
            onStepperClick?.Invoke(offset);
        }

        public void OnOpenButtonClick()
        {
            onPanelActive?.Invoke(true);
        }

        public void OnCloseButtonClick()
        {
            onPanelActive?.Invoke(false);
        }

        internal void UpdatePanelUI(bool value)
        {
            if (value)
            {
                _cardDeckPanel.SetActive(true);
            }
            else
            {
                _cardDeckPanel.SetActive(false);
            }
        }

        internal void CloseItemUI()
        {
            foreach (CardDeckEditorItemUI item in _itemList)
            {
                item.gameObject.SetActive(false);
            }
        }

        internal void UpdateItemUI(int itemIndex, Sprite itemSprite)
        {
            CardDeckEditorItemUI item = GetItem(itemIndex);
            item.gameObject.SetActive(true);

            item.UpdateUI(itemIndex, itemSprite, OnItemClick);
        }

        internal void UpdateCountUI(string name, int count)
        {
            _cardDeckText.text = $"{name} ({count})";
            _cardDeckCountUI.UpdateUI(count);
        }

        public void RevertCountUI()
        {
            _cardDeckCountUI.RevertAsLatestCount();
        }

        private void OnItemClick(int itemIndex)
        {
            onItemSelected?.Invoke(itemIndex);
        }

        private CardDeckEditorItemUI GetItem(int itemIndex)
        {
            CardDeckEditorItemUI item;
            if (itemIndex < _itemList.Count)
            {
                item = _itemList[itemIndex];
            }
            else
            {
                item = _itemPool.Get();
                item.CachedRectTransform.SetParent(_itemRoot);
                _itemList.Add(item);
            }
            return item;
        }

        internal void SelectItemUI(int itemIndex)
        {
            _itemList[itemIndex].Select();
        }

        internal void DeselectItemUI(int itemIndex)
        {
            _itemList[itemIndex].Deselect();
        }
    }
}
