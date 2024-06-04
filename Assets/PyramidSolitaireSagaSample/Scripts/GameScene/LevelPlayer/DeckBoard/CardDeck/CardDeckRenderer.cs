using DG.Tweening;
using PyramidSolitaireSagaSample.GameCommon.UI;
using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardDeck
{
    public struct CardDeckItemPositionInfo
    {
        public int itemIndex;
        public Vector3 itemPosition;

        public CardDeckItemPositionInfo(int itemIndex, Vector3 itemPosition)
        {
            this.itemIndex = itemIndex;
            this.itemPosition = itemPosition;
        }
    }

    public class CardDeckRenderer : MonoBehaviour
    {
        [SerializeField] private Transform _itemRendererRoot;
        [SerializeField] private SpriteRendererContainer _itemRendererPrefab;
        [SerializeField] private TextContainer _countText;
        [SerializeField] private SoundPlayer _drawSound;

        public event Action onItemClick;

        public int ItemCount => _itemRendererQueue.Count;
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

        internal void AddItemRenderer(
            int itemIndex,
            Sprite cardSprite,
            Vector3 itemPosition,
            Vector3 itemEulerAngles,
            int cardSortingOrder,
            Sprite revealedSprite,
            int revealedSortingOrder
        )
        {
            SpriteRendererContainer itemRenderer = _itemRendererPool.Get();
            itemRenderer.CachedTransform.SetParent(_itemRendererRoot);
            _itemRendererQueue.Enqueue(itemRenderer);

            itemRenderer.CollierObject.SetActive(false);

            itemRenderer.gameObject.SetActive(true);
            itemRenderer.Init(itemIndex, OnItemClick);
            itemRenderer.UpdateSpriteRenderer(cardSprite);
            itemRenderer.UpdateSortingOrder(cardSortingOrder);
            itemRenderer.CachedTransform.localPosition = itemPosition;
            itemRenderer.CachedTransform.localEulerAngles = itemEulerAngles;
            itemRenderer.UpdateRevealedRenderer(revealedSprite);
            itemRenderer.UpdateRevealedSortingOrder(revealedSortingOrder);
        }

        internal void ReorderItemRenderer(IReadOnlyList<CardDeckItemPositionInfo> itemPositionInfoList)
        {
            int itemIndex = 0;
            foreach (SpriteRendererContainer itemRenderer in _itemRendererQueue)
            {
                CardDeckItemPositionInfo itemPositionInfo = itemPositionInfoList[itemIndex];
                itemRenderer.CachedTransform.localPosition = itemPositionInfo.itemPosition;
                itemIndex += 1;
            }
        }

        private void OnItemClick(int itemIndex, Vector3 itemPosition, Quaternion itemRotation)
        {
            onItemClick?.Invoke();
        }

        internal void UpdateCount()
        {
            _countText.TextMeshPro.text = _itemRendererQueue.Count.ToString();
        }

        internal void DrawPeekItemRenderer(
            Transform itemRoot,
            Sprite itemSprite,
            Vector3 itemPosition,
            Vector3 itemEulerAngles,
            int sortingOrder,
            float cardMoveDuration,
            Ease cardMoveEase,
            bool useTween = true
        )
        {
            SpriteRendererContainer itemRenderer = _itemRendererQueue.Dequeue();
            itemRenderer.CachedTransform.SetParent(itemRoot);
            itemRenderer.UpdateSortingOrder(sortingOrder);
            if (useTween)
            {
                _drawSound.Play();
                itemRenderer.CachedTransform.DOLocalRotate(itemEulerAngles, cardMoveDuration)
                                            .OnUpdate(() =>
                                            {
                                                if (itemRenderer.CachedTransform.localEulerAngles.y <= 90)
                                                {
                                                    itemRenderer.UpdateSpriteRenderer(itemSprite);
                                                }
                                            })
                                            .SetEase(cardMoveEase);
                itemRenderer.CachedTransform.DOMove(itemPosition, cardMoveDuration)
                                            .SetEase(cardMoveEase);
            }
            else
            {
                itemRenderer.CachedTransform.position = itemPosition;
                itemRenderer.CachedTransform.localEulerAngles = itemEulerAngles;
                itemRenderer.UpdateSpriteRenderer(itemSprite);
            }
            itemRenderer.CollierObject.SetActive(false);
            itemRenderer.RevealedObject.SetActive(false);

            UpdateCount();
        }

        internal void RevealPeekItemRenderer(Sprite revealedSprite)
        {
            SpriteRendererContainer itemRenderer = PeekItem;
            itemRenderer.RevealedObject.SetActive(true);
            itemRenderer.UpdateRevealedRenderer(revealedSprite);
        }
    }
}
