using DG.Tweening;
using PyramidSolitaireSagaSample.GameCommon.UI;
using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCards
{
    public class LaunchCardItemInfo
    {
        public Action<Vector3, Quaternion> OnItemClick { get; private set; }
        public Action OnItemComplete { get; private set; }
        public bool IsItemCollected { get; private set; }
        public bool IsItemCompleted { get; private set; }

        public LaunchCardItemInfo(Action<Vector3, Quaternion> onItemClick, Action onItemComplete)
        {
            OnItemClick = onItemClick;
            OnItemComplete = onItemComplete;
            IsItemCollected = false;
            IsItemCompleted = false;
        }

        internal void UpdateIsItemCompleted(bool value)
        {
            IsItemCompleted = value;
        }

        internal void UpdateIsItemCollected(bool value)
        {
            IsItemCollected = value;
        }
    }

    public class BonusCardsLauncher : MonoBehaviour
    {
        [SerializeField] private BonusCardsArrowDrawer _arrowDrawerPrefab;
        [SerializeField] private BonusCardsPointItem _pointItemPrefab;
        [SerializeField] private SpriteRendererContainer _itemRendererPrefab;
        [SerializeField] private Transform _itemRendererRoot;

        private const int SortingOrderPointItem = 0;
        private const int SortingOrderArrowDrawer = 1;
        private const int SortingOrderItemRenderer = 2;

        // External
        private Vector3 _cameraCenterPosition;
        private Vector2 _cameraHalfSize;
        private float _cardHalfMagnitude;
        private Vector3 _tileBoundCenterPosition;
        private Vector3 _tileBoundHalfSize;

        // Internal
        private GameObjectPool<BonusCardsPointItem> _pointItemPool;
        private GameObjectPool<BonusCardsArrowDrawer> _arrowDrawerPool;
        private GameObjectPool<SpriteRendererContainer> _itemRendererPool;

        private List<BonusCardsArrowDrawer> _arrowDrawerList;
        private List<BonusCardsPointItem> _pointItemList;
        private List<SpriteRendererContainer> _itemRendererList;
        private List<LaunchCardItemInfo> _itemInfoList;

        private float _moveDuration;
        private Ease _moveEase;
        private float _fadeDuration;
        private Ease _fadeEase;

        private void Awake()
        {
            _pointItemPool = new(_itemRendererRoot, _pointItemPrefab.gameObject, defaultCapacity: 6);
            _arrowDrawerPool = new(_itemRendererRoot, _arrowDrawerPrefab.gameObject, defaultCapacity: 2);
            _itemRendererPool = new(_itemRendererRoot, _itemRendererPrefab.gameObject, defaultCapacity: 2);

            _pointItemList = new();
            _arrowDrawerList = new();
            _itemRendererList = new();
            _itemInfoList = new();
        }

        internal void UpdateTileBoundHalfSize(Vector3 tileBoundCenterPosition, Vector3 tileBoundHalfSize)
        {
            _tileBoundCenterPosition = tileBoundCenterPosition;
            _tileBoundHalfSize = tileBoundHalfSize;
        }

        internal void UpdateCameraHalfSize(Vector3 cameraCenterPosition, Vector2 cameraHalfSize, float cardHalfMagnitude)
        {
            _cameraCenterPosition = cameraCenterPosition;
            _cameraHalfSize = cameraHalfSize;
            _cardHalfMagnitude = cardHalfMagnitude;
        }

        internal void Clear()
        {
            if (_pointItemList.Count > 0)
            {
                foreach (BonusCardsPointItem pointItem in _pointItemList)
                {
                    _pointItemPool.Release(pointItem);
                }
                _pointItemList.Clear();
            }

            if (_arrowDrawerList.Count > 0)
            {
                foreach (BonusCardsArrowDrawer arrowDrawer in _arrowDrawerList)
                {
                    _arrowDrawerPool.Release(arrowDrawer);
                }
                _arrowDrawerList.Clear();
            }

            if (_itemRendererList.Count > 0)
            {
                foreach (SpriteRendererContainer itemRenderer in _itemRendererList)
                {
                    itemRenderer.CachedTransform.DOKill();
                    _itemRendererPool.Release(itemRenderer);
                }
                _itemRendererList.Clear();
                _itemInfoList.Clear();
            }
        }

        internal void LaunchCard(
            bool showCardPath,
            float moveDuration,
            Ease moveEase,
            float fadeDuration,
            Ease fadeEase,
            Sprite cardSprite,
            Action<Vector3, Quaternion> onItemClick,
            Action onItemComplete
        )
        {
            _moveDuration = moveDuration;
            _moveEase = moveEase;
            _fadeDuration = fadeDuration;
            _fadeEase = fadeEase;

            Vector3 startPoint = GetRandomPointOnRectangle(
                _cameraCenterPosition,
                new Vector2(_cameraHalfSize.x + _cardHalfMagnitude, _cameraHalfSize.y + _cardHalfMagnitude)
            );
            Vector3 throughPoint = GetRandomPointOnRectangle(_tileBoundCenterPosition, _tileBoundHalfSize);

            Vector3 direction = (throughPoint - startPoint).normalized;
            Vector3 endPoint = GetEndPointOnCameraRectangle(startPoint, direction);

            //Debug.Log($"CreateBonusCard : {startPoint}, {throughPoint}, {direction}, {endPoint}");
            if (showCardPath)
            {
                SetPointItem(startPoint, Color.red);
                SetPointItem(throughPoint, Color.yellow);
                SetPointItem(endPoint, Color.blue);

                SetArrowDrawer(startPoint, endPoint);
            }

            _itemInfoList.Add(new LaunchCardItemInfo(onItemClick, onItemComplete));
            _LaunchCard(cardSprite, startPoint, direction, endPoint);
        }

        private void _LaunchCard(Sprite cardSprite, Vector3 startPoint, Vector3 direction, Vector3 endPoint)
        {
            int itemIndex = _itemRendererList.Count;

            SpriteRendererContainer itemRenderer = _itemRendererPool.Get();
            itemRenderer.CachedTransform.SetParent(_itemRendererRoot);
            itemRenderer.CachedTransform.position = startPoint;
            itemRenderer.CachedTransform.DOMove(endPoint, _moveDuration)
                                        .SetEase(_moveEase)
                                        .OnComplete(() => OnItemLaunchComplete(itemIndex));
            Color originColor = itemRenderer.SpriteRenderer.color;
            itemRenderer.SpriteRenderer.color = new Color(originColor.r, originColor.g, originColor.b, 1f);

            float radian = Mathf.Atan2(direction.y, direction.x);
            float angle = radian * Mathf.Rad2Deg;
            itemRenderer.Init(itemIndex, OnItemClick);
            itemRenderer.CachedTransform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            itemRenderer.CollierObject.SetActive(true);
            itemRenderer.UpdateSpriteRenderer(cardSprite);
            itemRenderer.UpdateSortingLayer("UI");
            itemRenderer.UpdateSortingOrder(SortingOrderItemRenderer);

            _itemRendererList.Add(itemRenderer);
        }

        private void OnItemClick(int itemIndex, Vector3 itemPosition, Quaternion itemRotation)
        {
            SpriteRendererContainer itemRenderer = _itemRendererList[itemIndex];
            itemRenderer.CachedTransform.DOKill();
            itemRenderer.CollierObject.SetActive(false);
            itemRenderer.SpriteRenderer.DOFade(0f, _fadeDuration)
                                       .SetEase(_fadeEase)
                                       .OnComplete(CheckIfAllItemLaunchComplete);

            LaunchCardItemInfo itemInfo = _itemInfoList[itemIndex];
            itemInfo.UpdateIsItemCollected(true);
            itemInfo.OnItemClick?.Invoke(itemPosition, itemRotation);
        }

        private void OnItemLaunchComplete(int itemIndex)
        {
            LaunchCardItemInfo itemInfo = _itemInfoList[itemIndex];
            itemInfo.UpdateIsItemCompleted(true);
            itemInfo.OnItemComplete?.Invoke();

            CheckIfAllItemLaunchComplete();
        }

        private void CheckIfAllItemLaunchComplete()
        {
            bool result = true;
            int itemIndex = 0;
            foreach (LaunchCardItemInfo itemInfo in _itemInfoList)
            {
                //Debug.Log($"CheckIfAllItemLaunchComplete : {itemIndex}, {itemInfo.IsItemCollected}, {itemInfo.IsItemCompleted}");
                result &= (itemInfo.IsItemCollected || itemInfo.IsItemCompleted);
                itemIndex += 1;
            }

            if (result)
            {
                Clear();
            }
        }

        private void SetArrowDrawer(Vector3 startPoint, Vector3 endPoint)
        {
            BonusCardsArrowDrawer arrowDrawer = _arrowDrawerPool.Get();
            arrowDrawer.UpdateSortingOrder(SortingOrderArrowDrawer);
            arrowDrawer.transform.SetParent(transform);
            arrowDrawer.DrawArrow(startPoint, endPoint);
            _arrowDrawerList.Add(arrowDrawer);
        }

        private void SetPointItem(Vector3 point, Color color)
        {
            BonusCardsPointItem pointItem = _pointItemPool.Get();
            pointItem.UpdateSortingOrder(SortingOrderPointItem);
            pointItem.transform.SetParent(transform);
            pointItem.transform.position = point;
            pointItem.SetColor(color);
            _pointItemList.Add(pointItem);
        }

        private Vector3 GetEndPointOnCameraRectangle(Vector3 start, Vector3 direction)
        {
            float maxX = _cameraCenterPosition.x + _cameraHalfSize.x + _cardHalfMagnitude;
            float minX = _cameraCenterPosition.x - _cameraHalfSize.x - _cardHalfMagnitude;
            float maxY = _cameraCenterPosition.y + _cameraHalfSize.y + _cardHalfMagnitude;
            float minY = _cameraCenterPosition.y - _cameraHalfSize.y - _cardHalfMagnitude;

            float tX = float.PositiveInfinity;
            float tY = float.PositiveInfinity;

            // 방향 벡터와 경계선의 교차점 계산
            if (direction.x > 0)
            {
                tX = (maxX - start.x) / direction.x;
            }
            else if (direction.x < 0)
            {
                tX = (minX - start.x) / direction.x;
            }

            if (direction.y > 0)
            {
                tY = (maxY - start.y) / direction.y;
            }
            else if (direction.y < 0)
            {
                tY = (minY - start.y) / direction.y;
            }

            // 가장 작은 t 값을 사용하여 교차점 계산
            float t = Mathf.Min(tX, tY);

            return start + direction * t;
        }

        private Vector3 GetRandomPointOnRectangle(Vector3 center, Vector2 halfSize)
        {
            float minX = center.x - halfSize.x;
            float maxX = center.x + halfSize.x;
            float minY = center.y - halfSize.y;
            float maxY = center.y + halfSize.y;

            int side = UnityEngine.Random.Range(0, 4);

            float randomX, randomY;
            switch (side)
            {
                case 0: // 상단 변
                    randomX = UnityEngine.Random.Range(minX, maxX);
                    randomY = maxY;
                    break;

                case 1: // 하단 변
                    randomX = UnityEngine.Random.Range(minX, maxX);
                    randomY = minY;
                    break;

                case 2: // 왼쪽 변
                    randomX = minX;
                    randomY = UnityEngine.Random.Range(minY, maxY);
                    break;

                case 3: // 오른쪽 변
                    randomX = maxX;
                    randomY = UnityEngine.Random.Range(minY, maxY);
                    break;

                default:
                    randomX = center.x;
                    randomY = center.y;
                    break;
            }

            return new Vector3(randomX, randomY, 0);
        }
    }
}
