using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDepth
{
    public class CardDepthEditorToggleItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _depthText;
        [SerializeField] private Toggle _depthToggle;

        public RectTransform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = GetComponent<RectTransform>();
                }
                return _cachedTransform;
            }
        }
        private RectTransform _cachedTransform;

        private int _itemIndex;
        private Action<int> _onItemClick;

        internal void Init(int itemIndex, Action<int> onItemClick)
        {
            _itemIndex = itemIndex;
            _onItemClick = onItemClick;
        }

        public void OnItemValueChanged(bool value)
        {
            //Debug.Log($"OnItemValueChanged : {_itemIndex}, {value}");
            if (value)
            {
                _onItemClick?.Invoke(_itemIndex);
            }
        }

        public void SetIsOn(bool value)
        {
            _depthToggle.isOn = value;
        }

        internal void UpdateText(string value)
        {
            _depthText.text = value;
        }

        internal void UpdateToggleGroup(ToggleGroup toggleGroup)
        {
            _depthToggle.group = toggleGroup;
        }
    }
}
