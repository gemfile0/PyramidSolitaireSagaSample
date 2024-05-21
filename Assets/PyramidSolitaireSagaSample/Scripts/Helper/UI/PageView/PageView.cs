using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.Helper.UI
{
    public enum PagingState
    {
        Stop,
        Wait,
        Move,
    }

    public class PageDragInfo
    {
        public enum DragDirection
        {
            None, Vertical, Horizontal
        }

        public bool IsInDrag
        {
            get;
            private set;
        }
        public float ElapsedTime
        {
            get;
            private set;
        }
        private float beginTime;

        public float DeltaAbsX
        {
            get;
            private set;
        }
        public float DeltaX
        {
            get;
            private set;
        }

        private DragDirection currentDirection;
        private bool showLog;
        private float decelerationRate;

        public PageDragInfo(bool showLog, float decelerationRate)
        {
            this.showLog = showLog;
            this.decelerationRate = decelerationRate;
        }

        public void UpdateDelta(float deltaX, float deltaY)
        {
            if (currentDirection == DragDirection.None)
            {
                currentDirection = deltaX != 0 ? DragDirection.Horizontal :
                                   deltaY != 0 ? DragDirection.Vertical :
                                   DragDirection.None;
            }

            if (currentDirection == DragDirection.Horizontal)
            {
                DeltaX += deltaX;
                float fraction = 1 - decelerationRate;
                DeltaX *= fraction;
                DeltaAbsX = Mathf.Abs(DeltaX);
                if (DeltaAbsX < 0.05f)
                {
                    DeltaX = 0f;
                    DeltaAbsX = 0f;
                }
            }

            if (showLog)
            {
                Debug.Log($"UpdateDelta : {currentDirection}, {DeltaX}");
            }
        }

        public void Begin()
        {
            IsInDrag = true;
            beginTime = Time.time;
        }

        public void End()
        {
            IsInDrag = false;

            ElapsedTime = Time.time - beginTime;
            beginTime = 0f;
            currentDirection = DragDirection.None;

            DeltaX = 0f;
            DeltaAbsX = 0f;
        }
    }

    /// <summary>
    /// ScrollRect 컴포넌트를 이용해서 구현한 페이지뷰 입니다. 가로 모드 구현만 완료된 상태입니다.
    /// </summary>
    public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private bool showLog;

        [Space]
        [SerializeField] private float _deltaThreshold = 1f;
        [SerializeField] private float _moveDuration = .25f;
        [SerializeField] private Ease _moveEase = Ease.OutSine;
        [SerializeField] private ScrollRect _scrollRect;

        public event Action<int> onPageUpdated;

        private List<RectTransform> _pageTransforms;
        private RectTransform _cachedTransform;
        private RectTransform _contentTransform;
        private List<float> _pagePositionList;

        private float _width;
        private float _contentWidth;
        private float _scrollEndPosition;

        private Vector2 _prevPosition;
        private EnumState<PagingState> _pagingState;
        private PageDragInfo _pageDragInfo;
        private int _dragDirectionX;
        private int _pageIndex;
        private Coroutine _moveContentCoroutine;

        public void Init(List<RectTransform> pageTransforms)
        {
            _pageTransforms = pageTransforms;
            _cachedTransform = GetComponent<RectTransform>();
            _contentTransform = _scrollRect.content;

            UpdateContentSize();

            _pagingState = new EnumState<PagingState>();
            _pageDragInfo = new PageDragInfo(showLog, _scrollRect.decelerationRate);
        }

        private void UpdateContentSize()
        {
            float beginPosition = 0;
            float endPosition = 0;
            RectTransform endTransform = _pageTransforms[0];

            if (_pagePositionList == null)
            {
                _pagePositionList = new List<float>();
            }
            _pagePositionList.Clear();
            foreach (RectTransform pageTransform in _pageTransforms)
            {
                _pagePositionList.Add(-1 * pageTransform.anchoredPosition.x);
                if (beginPosition > pageTransform.anchoredPosition.x)
                {
                    beginPosition = pageTransform.anchoredPosition.x;
                }
                if (endPosition < pageTransform.anchoredPosition.x)
                {
                    endPosition = pageTransform.anchoredPosition.x;
                    endTransform = pageTransform;
                }
            }

            _width = _cachedTransform.rect.width;
            _contentWidth = endPosition + endTransform.rect.width - beginPosition;
            _scrollEndPosition = -endPosition;

            if (showLog)
            {
                Debug.Log($"pageView.width : {_width}, contentSize: {_contentWidth}, scrollEndPosition : {_scrollEndPosition}");
            }
        }

        public void ChangeIndex(int pageIndex, bool immediately = false)
        {
            SetPagingState(PagingState.Stop);
            _scrollRect.velocity = Vector2.zero;

            float pagePosition = _pagePositionList[pageIndex];

            UpdatePageIndex(pageIndex, notifyEvent: false);

            if (immediately)
            {
                _contentTransform.anchoredPosition = new Vector2(pagePosition, 0);
            }
            else
            {
                _contentTransform.DOAnchorPos(new Vector2(pagePosition, 0), _moveDuration)
                                 .SetEase(_moveEase);
            }

            if (showLog)
            {
                Debug.Log($"GoTo : {pageIndex}");
            }
        }

        private void UpdatePageIndex(int value, bool notifyEvent = true)
        {
            if (showLog)
            {
                //Debug.Log($"PageIndex : {value}");
            }

            _pageIndex = value;
            if (notifyEvent)
            {
                onPageUpdated?.Invoke(_pageIndex);
            }
        }

        private void Update()
        {
            if (_contentTransform == null)
            {
                return;
            }

            Vector2 currPosition = _contentTransform.anchoredPosition;
            Vector2 delta = currPosition - _prevPosition;

            bool isInScrolling = currPosition.x != _prevPosition.x;
            bool isInScrollArea = currPosition.x < 0 && currPosition.x > _scrollEndPosition;
            if (_pageDragInfo.IsInDrag == true)
            {
                _pageDragInfo.UpdateDelta(delta.x, delta.y);
                if (_pageDragInfo.DeltaAbsX > _deltaThreshold)
                {
                    _dragDirectionX = (int)(_pageDragInfo.DeltaX / _pageDragInfo.DeltaAbsX) * -1;
                }
                else
                {
                    _dragDirectionX = 0;
                }
            }
            else
            {
                if (_pagingState.CurrState == PagingState.Wait)
                {
                    if (isInScrollArea)
                    {
                        SetPagingState(PagingState.Move);
                    }
                    else if (isInScrolling == false)
                    {
                        SetPagingState(PagingState.Stop);
                    }
                }
            }
            _prevPosition = currPosition;
        }

        private void SetPagingState(PagingState nextState)
        {
            if (showLog)
            {
                Debug.Log($"SetPagingState : {nextState}");
            }
            _pagingState.Set(nextState);

            switch (nextState)
            {
                case PagingState.Move:
                    if (_dragDirectionX != 0)
                    {
                        MoveContentToDragDirection();
                    }
                    else
                    {
                        MoveContentToNearestPage();
                    }
                    break;

                case PagingState.Stop:
                    Clear();
                    break;

                case PagingState.Wait:
                    break;
            }
        }

        private int ConsumeDragDirection()
        {
            int result = _dragDirectionX;
            _dragDirectionX = 0;
            return result;
        }

        private void Clear()
        {
            ConsumeDragDirection();
            StopMoveCoroutine();
        }

        private void StopMoveCoroutine()
        {
            if (_moveContentCoroutine != null)
            {
                StopCoroutine(_moveContentCoroutine);
                _moveContentCoroutine = null;
            }
        }

        private void MoveContentToDragDirection()
        {
            if (showLog)
            {
                Debug.Log("MoveContentToDragDirection");
            }
            int dragDirection = ConsumeDragDirection();
            _scrollRect.velocity = Vector2.zero;

            int nextPageIndex = _pageIndex + dragDirection;
            if (nextPageIndex < 0)
            {
                nextPageIndex = 0;
            }
            else if (nextPageIndex >= _pagePositionList.Count)
            {
                nextPageIndex = _pagePositionList.Count - 1;
            }

            UpdatePageIndex(nextPageIndex);

            float pagePosition = _pagePositionList[nextPageIndex];
            _moveContentCoroutine = StartCoroutine(MoveContentCoroutine(pagePosition));
        }

        private void MoveContentToNearestPage()
        {
            if (showLog)
            {
                Debug.Log("MoveContentToNearestPage");
            }
            int _pagingOffset = ConsumeDragDirection();
            _scrollRect.velocity = Vector2.zero;

            float nearestDelta = 0;
            float nearestPosition = 0;
            int nearestPageIndex = 0;

            float currPosition = _contentTransform.anchoredPosition.x;
            for (int i = 0; i < _pagePositionList.Count; i++)
            {
                float pagePosition = _pagePositionList[i];
                float delta = Mathf.Abs(pagePosition - currPosition);

                if (nearestDelta == 0
                    || nearestDelta > delta)
                {
                    nearestDelta = delta;
                    nearestPosition = pagePosition;
                    nearestPageIndex = i;
                }
            }

            UpdatePageIndex(nearestPageIndex);
            _moveContentCoroutine = StartCoroutine(MoveContentCoroutine(nearestPosition));
        }

        private IEnumerator MoveContentCoroutine(float nearestPosition)
        {
            float elasticity = _scrollRect.elasticity;
            while (true)
            {
                float currPosition = _contentTransform.anchoredPosition.x;
                float delta = nearestPosition - currPosition;
                float delta2 = delta * elasticity;
                float nextPosition = currPosition + delta2;
                _contentTransform.anchoredPosition = new Vector2(nextPosition, 0);
                if (Mathf.Abs(delta2) < 0.1f)
                {
                    _contentTransform.anchoredPosition = new Vector2(nearestPosition, 0);
                    break;
                }

                yield return null;
            }

            SetPagingState(PagingState.Stop);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //if (eventData.pointerDrag == scrollRect.gameObject)
            //{
            SetPagingState(PagingState.Stop);
            _pageDragInfo.Begin();
            //}
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //if (eventData.pointerDrag == scrollRect.gameObject)
            //{
            _pageDragInfo.End();

            if (_pagingState.CurrState == PagingState.Stop
                || _pagingState.CurrState == PagingState.Move)
            {
                StopMoveCoroutine();
                SetPagingState(PagingState.Wait);
            }
            //}
        }
    }
}
