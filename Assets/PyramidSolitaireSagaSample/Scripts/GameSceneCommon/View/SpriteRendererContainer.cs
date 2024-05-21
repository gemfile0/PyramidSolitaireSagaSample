using PyramidSolitaireSagaSample.LevelPlayer.Input;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameCommon.UI
{
    public class SpriteRendererContainer : MonoBehaviour, IClickable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject _revealedObject;
        [SerializeField] private SpriteRenderer _revealedRenderer;
        [SerializeField] private GameObject _colliderObject;

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

        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public GameObject CollierObject => _colliderObject;
        public GameObject RevealedObject => _revealedObject;

        public string SortingLayerName => _spriteRenderer.sortingLayerName;

        private int _itemIndex;
        private Action<int, Vector3, Quaternion> _onClick;

        internal void Init(int itemIndex, Action<int, Vector3, Quaternion> onClick)
        {
            _itemIndex = itemIndex;
            _onClick = onClick;
        }

        public void OnClick()
        {
            _onClick?.Invoke(_itemIndex, CachedTransform.position, CachedTransform.rotation);
        }

        internal void UpdateSpriteRenderer(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }

        internal void UpdateSortingOrder(int sortingOrder)
        {
            _spriteRenderer.sortingOrder = sortingOrder;
        }

        internal void UpdateSortingLayer(string sortingLayerName)
        {
            _spriteRenderer.sortingLayerName = sortingLayerName;
        }

        internal void UpdateRevealedRenderer(Sprite sprite)
        {
            _revealedRenderer.sprite = sprite;
        }

        internal void UpdateRevealedSortingOrder(int sortingOrder)
        {
            _revealedRenderer.sortingOrder = sortingOrder;
        }
    }
}
