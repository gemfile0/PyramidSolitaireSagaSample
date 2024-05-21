using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDepth
{
    public class CardDepthEditorToolUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panelObject;
        [SerializeField] private CardDepthEditorToggleItem _toggleItemPrefab;
        [SerializeField] private ToggleGroup _toggleItemGroup;

        public event Action<int> onStackItemClick;
        public event Action<int> onStackChangerClick;

        private RectTransform _toggleItemRoot;
        private GameObjectPool<CardDepthEditorToggleItem> _toggleItemPool;
        private List<CardDepthEditorToggleItem> _toggleItemList;
        private StringBuilder _textBuilder;

        private void Awake()
        {
            _toggleItemRoot = _toggleItemGroup.GetComponent<RectTransform>();
            _toggleItemPool = new GameObjectPool<CardDepthEditorToggleItem>(_toggleItemRoot, _toggleItemPrefab.gameObject);
            _toggleItemList = new List<CardDepthEditorToggleItem>();
            _textBuilder = new StringBuilder();
        }

        private void Start()
        {
            _panelObject.SetActive(false);
        }

        public void OnUpClick()
        {
            onStackChangerClick?.Invoke(1);
        }

        public void OnDownClick()
        {
            onStackChangerClick?.Invoke(-1);
        }

        private void OnStackItemClick(int stackIndex)
        {
            onStackItemClick?.Invoke(stackIndex);
        }

        internal void UpdateStackIndex(int stackIndex, int stackCount)
        {
            //Debug.Log($"UpdateStackIndex : {stackIndex} / {stackCount}");
            Vector2 originSize = _toggleItemRoot.sizeDelta;
            float height = stackCount > 1 ? _toggleItemPrefab.CachedTransform.sizeDelta.y * stackCount : 0;
            _toggleItemRoot.sizeDelta = new Vector2(originSize.x, height);

            if (stackCount > 1)
            {
                for (int i = 0; i < stackCount; i++)
                {
                    CardDepthEditorToggleItem toggleItem = _toggleItemPool.Get();
                    toggleItem.Init(i, onItemClick: OnStackItemClick);
                    toggleItem.UpdateToggleGroup(_toggleItemGroup);
                    toggleItem.UpdateText(MakeToggleItemText(i));
                    toggleItem.CachedTransform.SetParent(_toggleItemRoot);

                    if (i == stackIndex)
                    {
                        toggleItem.SetIsOn(true);
                    }

                    _toggleItemList.Add(toggleItem);
                }
            }
        }

        private string MakeToggleItemText(int i)
        {
            _textBuilder.Length = 0;
            _textBuilder.Append(i + 1);
            return _textBuilder.ToString();
        }

        internal void ReleaseStackIndex()
        {
            foreach (CardDepthEditorToggleItem toggleItem in _toggleItemList)
            {
                toggleItem.UpdateToggleGroup(null);
                toggleItem.SetIsOn(false);
                _toggleItemPool.Release(toggleItem);
            }
            _toggleItemList.Clear();
        }
    }
}
