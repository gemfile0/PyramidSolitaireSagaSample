using DG.Tweening;
using PyramidSolitaireSagaSample.GameCommon.UI;
using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.JokerDeck
{
    public class JokerDeckRenderer : MonoBehaviour
    {
        [SerializeField] private Transform _itemRendererRoot;
        [SerializeField] private SpriteRendererContainer _itemRendererPrefab;
        [SerializeField] private TextContainer _countText;

        public event Action onItemClick;

        public bool HasNextItem => _itemRendererQueue.Count > 0;
        public SpriteRendererContainer PeekItem => _itemRendererQueue.Peek();
        public Transform ItemRendererRoot => _itemRendererRoot;

        private GameObjectPool<SpriteRendererContainer> _itemRendererPool;
        private Queue<SpriteRendererContainer> _itemRendererQueue;

        private void Awake()
        {
            _itemRendererPool = new(_itemRendererRoot, _itemRendererPrefab.gameObject);
            _itemRendererQueue = new();
        }

        internal void AddItemRenderer(int itemIndex, Sprite itemSprite, Vector3 itemPosition, int sortingOrder)
        {
            SpriteRendererContainer itemRenderer = _itemRendererPool.Get();
            itemRenderer.CachedTransform.SetParent(_itemRendererRoot);
            _itemRendererQueue.Enqueue(itemRenderer);

            itemRenderer.CollierObject.SetActive(false);

            itemRenderer.gameObject.SetActive(true);
            itemRenderer.Init(itemIndex, OnItemClick);
            itemRenderer.UpdateSpriteRenderer(itemSprite);
            itemRenderer.CachedTransform.localPosition = itemPosition;
            itemRenderer.UpdateSortingOrder(sortingOrder);
        }

        private void OnItemClick(int itemIndex, Vector3 itemPosition, Quaternion itemRotation)
        {
            onItemClick?.Invoke();
        }

        internal void UpdateCount()
        {
            _countText.TextMeshPro.text = _itemRendererQueue.Count.ToString();
        }

        internal void DrawItem(
            Transform itemRoot, 
            Vector3 itemPosition, 
            int sortingOrder,
            float moveDuration,
            Ease moveEase,
            Vector2 scaleValue,
            float scaleDuration,
            Ease scaleEase
        )
        {
            SpriteRendererContainer itemRenderer = _itemRendererQueue.Dequeue();
            itemRenderer.CachedTransform.SetParent(itemRoot);
            itemRenderer.UpdateSortingOrder(sortingOrder);
            itemRenderer.CollierObject.SetActive(false);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(itemRenderer.CachedTransform.DOMove(itemPosition, moveDuration)
                                        .SetEase(moveEase));
            sequence.Join(itemRenderer.CachedTransform.DOScale(scaleValue, scaleDuration)
                                      .SetEase(scaleEase));
            sequence.Insert(scaleDuration, itemRenderer.CachedTransform.DOScale(Vector2.one, scaleDuration)
                                                       .SetEase(scaleEase));

            UpdateCount();
        }
    }
}
