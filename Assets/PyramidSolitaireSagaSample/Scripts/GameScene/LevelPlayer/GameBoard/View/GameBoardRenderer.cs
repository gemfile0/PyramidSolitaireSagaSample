using DG.Tweening;
using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class GameBoardRenderer : MonoBehaviour
    {
        [Header("Tile")]
        [SerializeField] private Transform _tileRendererRoot;
        [SerializeField] private GameBoardTileRenderer _tileRendererPrefab;

        [Header("Butterfly")]
        [SerializeField] private Transform _butterflyRendererRoot;
        [SerializeField] private GameBoardButterflyRenderer _butterflyRendererPrefab;

        [Header("Card")]
        [SerializeField] private Transform _cardRendererRoot;
        [SerializeField] private GameBoardCardRenderer _cardRendererPrefab;

        [Header("Highlight")]
        [SerializeField] private Transform _highlightTransform;
        [SerializeField] private SoundPlayer _dealingSound;

        public event Action<Vector3/* centerPosition */, Vector3/* halfSize */> onTileBoundHalfSize;
        public event Action<Vector2Int, int> onCardClick;

        public int CardRendererCount => _cardRendererStackDict.Count;
        public Transform HighlightTransform => _highlightTransform;

        public GameObject HighlightGameObject
        {
            get
            {
                if (_highlightGameObject == null)
                {
                    _highlightGameObject = _highlightTransform.gameObject;
                }
                return _highlightGameObject;
            }
        }
        private GameObject _highlightGameObject;
        private Transform _cachedTransform;
        private GameObjectPool<GameBoardCardRenderer> _cardRendererPool;
        private Dictionary<Vector2Int, List<GameBoardCardRenderer>> _cardRendererStackDict;
        private GameObjectPool<GameBoardButterflyRenderer> _butterflyRendererPool;
        private Dictionary<Vector2Int, GameBoardButterflyRenderer> _butterflyRendererDict;
        private Dictionary<Vector2Int, GameBoardTileRenderer> _tileRendererDict;
        private SpriteRenderer _highlightRenderer;
        private Bounds _tileBounds;

        // External
        private Vector2 _halfTileSize;
        private int _colCount;
        private int _rowCount;
        private int _lastRowIndex;
        private int _lastColIndex;

        public void Init(Vector2 halfTileSize, int rowCount, int colCount, int lastRowIndex, int lastColIndex, bool showGuideText = false)
        {
            _cachedTransform = transform;
            _cardRendererPool = new(_cardRendererRoot, _cardRendererPrefab.gameObject);
            _cardRendererStackDict = new();
            _butterflyRendererPool = new(_butterflyRendererRoot, _butterflyRendererPrefab.gameObject);
            _butterflyRendererDict = new();
            _tileRendererDict = new();
            _highlightRenderer = _highlightTransform.GetComponentInChildren<SpriteRenderer>();

            _halfTileSize = halfTileSize;
            _rowCount = rowCount;
            _colCount = colCount;
            _lastRowIndex = lastRowIndex;
            _lastColIndex = lastColIndex;
            for (int i = 0; i <= _lastRowIndex; i++)
            {
                for (int j = 0; j <= _lastColIndex; j++)
                {
                    Vector2Int cardIndex = new Vector2Int(j, i);
                    _cardRendererStackDict.Add(cardIndex, new List<GameBoardCardRenderer>());
                }
            }

            GenerateTileGrid();
            if (showGuideText)
            {
                UpdateTileGuideText();
            }
            NotifyTileBoundSize();
        }

        internal void SetHighlightSize(Vector2 cardSize)
        {
            _highlightRenderer.transform.localScale = cardSize;
        }

        internal void HideHighlight()
        {
            HighlightGameObject.SetActive(false);
        }

        internal void MoveHighlightPosition(Vector3 snappedPosition)
        {
            HighlightTransform.localPosition = snappedPosition;
            HighlightGameObject.SetActive(true);
        }

        internal void NotifyTileBoundSize()
        {
            SpriteRenderer[] sprites = _tileRendererRoot.GetComponentsInChildren<SpriteRenderer>();
            if (sprites.Length > 0)
            {
                _tileBounds = sprites[0].bounds;
                for (int i = 1; i < sprites.Length; i++)
                {
                    _tileBounds.Encapsulate(sprites[i].bounds);
                }

                Vector3 halfSize = _tileBounds.size / 2;
                onTileBoundHalfSize?.Invoke(_tileBounds.center, halfSize);
                //Debug.Log($"GetBoundSize: Width = {size.x}, Height = {size.y}");
            }
        }

        internal void GenerateTileGrid()
        {
            Vector2 tileOffset = _halfTileSize;
            Vector2 halfTileOffset = tileOffset / 2;
            for (int rowIndex = 0; rowIndex < _rowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < _colCount; colIndex++)
                {
                    float x = colIndex * tileOffset.x;
                    float y = rowIndex * tileOffset.y;
                    GameBoardTileRenderer tileRenderer = Instantiate(_tileRendererPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    tileRenderer.name = $"Tile_{rowIndex}_{colIndex}";
                    tileRenderer.CachedTransform.SetParent(_tileRendererRoot, false);

                    bool isOffset = (colIndex % 2 == 0 && rowIndex % 2 == 1) || (colIndex % 2 == 1 && rowIndex % 2 == 0);
                    tileRenderer.Init(isOffset, tileOffset);

                    _tileRendererDict.Add(new Vector2Int(colIndex, rowIndex), tileRenderer);
                }
            }

            // A-1. Scene 뷰의 그리드와 맞추가 위해 위치 조정
            _cachedTransform.position = new Vector3(halfTileOffset.x, halfTileOffset.y, 0);
        }

        private void UpdateTileGuideText()
        {
            int rowHalfCount = _rowCount / 2;
            int colHalfCount = _colCount / 2;
            int rowGridIndex = 0;
            for (int i = 0; i <= rowHalfCount; i++)
            {
                rowGridIndex += 1;
                _tileRendererDict[new Vector2Int(0, i)].UpdateRowText(rowGridIndex.ToString());
            }
            rowGridIndex = 0;
            for (int i = _rowCount - 1; i >= rowHalfCount; i--)
            {
                rowGridIndex += 1;
                _tileRendererDict[new Vector2Int(0, i)].UpdateRowText(rowGridIndex.ToString());
            }

            int colGridIndex = 0;
            for (int i = 0; i < colHalfCount; i++)
            {
                colGridIndex += 1;
                _tileRendererDict[new Vector2Int(i, 0)].UpdateColText(colGridIndex.ToString());
            }
            colGridIndex = 0;
            for (int i = _colCount - 1; i >= colHalfCount; i--)
            {
                colGridIndex += 1;
                _tileRendererDict[new Vector2Int(i, 0)].UpdateColText(colGridIndex.ToString());
            }
        }

        internal void CreateCardRenderer(Vector2Int cardIndex, int cardStackIndex, int cardID, int sortingOrder, Vector3 cardPosition)
        {
            GameBoardCardRenderer cardRenderer = _cardRendererPool.Get();
            cardRenderer.Init(cardIndex, cardID, onCardClick);
            cardRenderer.UpdateStackIndex(cardStackIndex);
            cardRenderer.UpdateSortingOrder(sortingOrder);
            cardRenderer.UpdateLocalPosition(cardPosition);
            cardRenderer.CachedTransform.SetParent(_cardRendererRoot, false);
            //Debug.Log($"CreateCardRenderer : {cardIndex}, {cardPosition}");
            _cardRendererStackDict[cardIndex].Add(cardRenderer);
        }

        internal void SetButterflyRenderer(Vector2Int cardIndex, Vector3 butterflyPosition, Sprite butterflySprite)
        {
            //Debug.Log($"SetButterflyRenderer : {cardIndex}, {butterflyPosition}");
            if (_butterflyRendererDict.ContainsKey(cardIndex) == false)
            {
                GameBoardButterflyRenderer butterflyRenderer = _butterflyRendererPool.Get();
                butterflyRenderer.UpdateRenderer(butterflySprite);
                butterflyRenderer.CachedTransform.localPosition = butterflyPosition;
                butterflyRenderer.CachedTransform.SetParent(_butterflyRendererRoot, false);

                _butterflyRendererDict.Add(cardIndex, butterflyRenderer);
            }
        }

        internal void MoveCardRenderer(Vector2Int originIndex, int originStackIndex, Vector2Int nextIndex, int nextStackIndex, Vector3 nextPosition)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[originIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[originStackIndex];
            _cardRendererStackDict[originIndex].RemoveAt(originStackIndex);
            _cardRendererStackDict[nextIndex].Insert(nextStackIndex, cardRenderer);

            cardRenderer.UpdateLocalPosition(nextPosition);
        }

        internal void MoveButterflyRenderer(Vector2Int originIndex, Vector2Int nextIndex, Vector3 snappedPosition)
        {
            if (_butterflyRendererDict.ContainsKey(originIndex))
            {
                GameBoardButterflyRenderer butterflyRenderer = _butterflyRendererDict[originIndex];
                _butterflyRendererDict.Remove(originIndex);
                _butterflyRendererDict.Add(nextIndex, butterflyRenderer);

                butterflyRenderer.CachedTransform.localPosition = snappedPosition;
            }
        }

        internal void UpdateCardRendererSortingOrder(Vector2Int cardIndex, int stackIndex, int sortingOrder)
        {
            //Debug.Log($"UpdateCardRendererSortingOrder : {cardIndex}, {stackIndex}, {sortingOrder}");
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[stackIndex];
            cardRenderer.UpdateSortingOrder(sortingOrder);
            cardRenderer.UpdateStackIndex(stackIndex);
        }

        internal void RemoveCardRenderer(Vector2Int cardIndex, int stackIndex)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[stackIndex];
            _cardRendererPool.Release(cardRenderer);
            cardRendererStack.RemoveAt(stackIndex);
        }

        internal void RemoveButterflyRendrerer(Vector2Int cardIndex)
        {
            if (_butterflyRendererDict.ContainsKey(cardIndex))
            {
                GameBoardButterflyRenderer butterflyRenderer = _butterflyRendererDict[cardIndex];
                _butterflyRendererPool.Release(butterflyRenderer);
                _butterflyRendererDict.Remove(cardIndex);
            }
        }

        internal void SelectCardRenderer(Vector2Int cardIndex, int cardStackIndex)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            cardRendererStack[cardStackIndex].Select();
        }

        internal void DeselectCardRenderer(Vector2Int cardIndex, int cardStackIndex)
        {
            if (_cardRendererStackDict.TryGetValue(cardIndex, out List<GameBoardCardRenderer> cardRendererStack))
            {
                if (cardStackIndex < cardRendererStack.Count)
                {
                    cardRendererStack[cardStackIndex].Deselect();
                }
                else
                {
                    Debug.LogWarning($"카드 렌더러가 존재하지 않습니다, cardStackIndex : {cardStackIndex}");
                }
            }
            else
            {
                Debug.LogWarning($"카드 렌더러가 존재하지 않습니다, cardIndex : {cardIndex}");
            }
        }

        internal void SetCardDimmedRenderer(
            Vector2Int cardIndex,
            int cardStackIndex,
            bool animate,
            bool value,
            float fadeDelay,
            float fadeDuration,
            Ease fadeEase
        )
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            cardRendererStack[cardStackIndex].SetDimmedRenderer(animate, value, fadeDelay, fadeDuration, fadeEase);
        }

        internal void SetCardCollider(Vector2Int cardIndex, int cardStackIndex, bool value)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            cardRendererStack[cardStackIndex].SetColliderObject(value);
        }

        internal void SetCardRenderer(
            Vector2Int cardIndex,
            int cardStackIndex,
            Sprite cardSprite,
            SubCardTypeRendererInfo subCardTypeRendererInfo,
            bool animateSubCardType,
            bool dimmedValue,
            Vector3 initialEulerAngles
        )
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[cardStackIndex];
            cardRenderer.UpdateAllRenderers(cardSprite, subCardTypeRendererInfo, animateSubCardType, dimmedValue, initialEulerAngles);
        }

        internal void ClearAllCardRenderer()
        {
            foreach (KeyValuePair<Vector2Int, List<GameBoardCardRenderer>> pair in _cardRendererStackDict)
            {
                List<GameBoardCardRenderer> cardRendererStack = pair.Value;
                foreach (GameBoardCardRenderer cardRenderer in cardRendererStack)
                {
                    _cardRendererPool.Release(cardRenderer);
                }
                cardRendererStack.Clear();
            }
        }

        internal void ClearAllButterflyRenderer()
        {
            foreach (KeyValuePair<Vector2Int, GameBoardButterflyRenderer> pair in _butterflyRendererDict)
            {
                GameBoardButterflyRenderer butterflyRenderer = pair.Value;
                _butterflyRendererPool.Release(butterflyRenderer);
            }
            _butterflyRendererDict.Clear();
        }

        internal void SetStartingPosition(Vector3 cardDeckRendererPosition, int sortingOrderBaseValue, int sortingOrderStepValue)
        {
            foreach (List<GameBoardCardRenderer> cardRendererStack in _cardRendererStackDict.Values)
            {
                foreach (GameBoardCardRenderer cardRenderer in cardRendererStack)
                {
                    cardRenderer.CachedGameObject.SetActive(false);
                    cardRenderer.CachedTransform.position = cardDeckRendererPosition;
                    // A-1. 카드 덱에서 보드로 이동하는 연출을 위해
                    // A-2. 시작점에서는 카드 ID 역순으로 SortingOrder 를 정렬해뒀다가 (카드 덱에 쌓여 있음)
                    cardRenderer.UpdateSortingOrderWithoutCache(sortingOrderBaseValue + cardRenderer.CardID * -1 * sortingOrderStepValue);
                }
            }
        }

        public IEnumerator MoveToOwnLocalPosition(float moveDuration, Ease moveEase, float moveDelay, int sortingOrderBaseValue, int sortingOrderStepValue)
        {
            Sequence sequence = DOTween.Sequence();
            foreach (List<GameBoardCardRenderer> cardRendererStack in _cardRendererStackDict.Values)
            {
                foreach (GameBoardCardRenderer cardRenderer in cardRendererStack)
                {
                    int movingSortingOrder = sortingOrderBaseValue + cardRenderer.CardID * sortingOrderStepValue;

                    cardRenderer.CachedGameObject.SetActive(true);

                    sequence.Insert(
                        0,
                        cardRenderer.CachedTransform
                                    .DOLocalMove(cardRenderer.LatestLocalPosition, moveDuration)
                                    .SetDelay(cardRenderer.CardID * moveDelay + moveDelay)
                                    // A-3. 제자리로 찾아갈 때 카드들이 CardCollector 를 스쳐 지나가므로 임시로 SortingOrder 값을 바꿨다가,
                                    .OnStart(() =>
                                    {
                                        _dealingSound.Play();
                                        cardRenderer.UpdateSortingOrderWithoutCache(movingSortingOrder);
                                    })
                                    // A-4. 제자리로 찾아간 후에 본래의 카드 ID 순으로 SortingOrder 값을 되돌려준다.
                                    .OnComplete(() => cardRenderer.UpdateSortingOrder(cardRenderer.LatestSortingOrder))
                                    .SetEase(moveEase)
                    );
                }
            }

            yield return sequence.WaitForCompletion();
        }

        internal void DrawCardRenderer(
            Transform itemRoot,
            Vector2Int cardIndex,
            int cardStackIndex,
            Vector3 itemPosition,
            int sortingOrder,
            float moveDuration,
            Ease moveEase,
            Vector2 scaleValue,
            float scaleDuration,
            Ease scaleEase
        )
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[cardStackIndex];
            cardRenderer.CachedTransform.SetParent(itemRoot, true);
            cardRenderer.UpdateSortingOrder(sortingOrder);
            cardRenderer.SetColliderObject(false);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(cardRenderer.CachedTransform.DOMove(itemPosition, moveDuration)
                                        .SetEase(moveEase));
            sequence.Join(cardRenderer.CachedTransform.DOScale(scaleValue, scaleDuration)
                                      .SetEase(scaleEase)
                                      .SetLoops(2, LoopType.Yoyo));
        }

        internal void DrawButterflyRendrerer(Vector2Int cardIndex, float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            if (_butterflyRendererDict.TryGetValue(cardIndex, out GameBoardButterflyRenderer butterflyRenderer))
            {
                butterflyRenderer.SpriteRenderer
                                 .DOFade(0f, fadeDuration)
                                 .SetDelay(fadeDelay)
                                 .SetEase(fadeEase)
                                 .OnComplete(() =>
                                 {
                                     RemoveButterflyRendrerer(cardIndex);
                                     Color originColor = butterflyRenderer.SpriteRenderer.color;
                                     butterflyRenderer.SpriteRenderer.color = new Color(originColor.r, originColor.g, originColor.b, 1f);
                                 });
            }
        }

        internal void DrawCardRendererAsFail(Vector2Int cardIndex, int cardStackIndex, int sortingOrder, Vector2 scaleValue, float scaleDuration, Ease scaleEase)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[cardStackIndex];
            int originSortingOrder = cardRenderer.LatestSortingOrder;
            cardRenderer.UpdateSortingOrder(sortingOrder);
            cardRenderer.SetColliderObject(false);
            cardRenderer.CachedTransform.DOScale(scaleValue, scaleDuration)
                                        .SetEase(scaleEase)
                                        .SetLoops(2, LoopType.Yoyo)
                                        .OnComplete(() =>
                                        {
                                            cardRenderer.UpdateSortingOrder(originSortingOrder);
                                            cardRenderer.SetColliderObject(true);
                                        });
        }

        internal void FlipCardRenderer(
            Vector2Int cardIndex,
            int cardStackIndex,
            Sprite cardSprite,
            SubCardTypeRendererInfo subCardTypeRendererInfo,
            float rotateDuration,
            Ease rotateEase
        )
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[cardStackIndex];
            cardRenderer.FlipCardRenderer(cardSprite, subCardTypeRendererInfo, rotateDuration, rotateEase);
        }

        internal void SetHighlightColor(Color color)
        {
            _highlightRenderer.color = color;
        }

        internal void SetObscuredObject(Vector2Int cardIndex, int cardStackIndex, bool value)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            cardRendererStack[cardStackIndex].SetObscuredObject(value);
        }

        private void OnDrawGizmos()
        {
            if (_tileBounds != default)
            {
                Gizmos.color = Color.red;  // Gizmo 색상 설정
                Gizmos.DrawWireCube(_tileBounds.center, _tileBounds.size);
            }
        }

        internal void UnpackCardRenderer(Vector2Int cardIndex, int cardStackIndex, float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[cardStackIndex];
            cardRenderer.UnpackCardRenderer(fadeDelay, fadeDuration, fadeEase);
        }

        internal void SwapCardRendererStackIndex(Vector2Int cardIndex, int originStackIndex, int nextStackIndex)
        {
            //Debug.Log($"StackSwapCardRenderer : {cardIndex}, {originStackIndex}, {nextStackIndex}");
            List<GameBoardCardRenderer> cardRendererStack = _cardRendererStackDict[cardIndex];
            GameBoardCardRenderer cardRenderer = cardRendererStack[originStackIndex];
            cardRendererStack.RemoveAt(originStackIndex);
            cardRendererStack.Insert(nextStackIndex, cardRenderer);
        }

        internal Vector3 GetCardRendererPosition(Vector2Int cardindex, int stackIndex)
        {
            Vector3 result = Vector3.zero;
            if (_cardRendererStackDict.TryGetValue(cardindex, out List<GameBoardCardRenderer> cardRendererStack))
            {
                if (stackIndex < cardRendererStack.Count)
                {
                    result = cardRendererStack[stackIndex].CachedTransform.position;
                }
            }
            return result;
        }
    }
}
