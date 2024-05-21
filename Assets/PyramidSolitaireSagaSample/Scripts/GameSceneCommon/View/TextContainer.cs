using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameCommon.UI
{
    public class TextContainer : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textRenderer;

        public Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = transform;
                }
                return _cachedTransform;
            }
        }
        private Transform _cachedTransform;

        public TextMeshPro TextMeshPro => _textRenderer;

        internal void UpdateText(string text)
        {
            _textRenderer.text = text;
        }

        internal void UpdateSortingOrder(int sortingOrder)
        {
            _textRenderer.sortingOrder = sortingOrder;
        }

        internal void UpdateSortingLayer(string sortingLayerName)
        {
            _textRenderer.sortingLayerID = SortingLayer.NameToID(sortingLayerName);
        }
    }
}
