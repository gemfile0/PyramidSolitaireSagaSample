using DG.Tweening;
using PyramidSolitaireSagaSample.LevelPlayer.Input;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public struct SubCardTypeRendererInfo
    {
        public CardFace cardFace;
        public SubCardType type;
        public int option;
        public int option2;
        public bool isBonusLabel;

        public SubCardTypeRendererInfo(CardFace cardFace, SubCardType type, int option, int option2, bool isBonusLabel)
        {
            this.cardFace = cardFace;
            this.type = type;
            this.option = option;
            this.option2 = option2;
            this.isBonusLabel = isBonusLabel;
        }
    }

    public class GameBoardCardRenderer : MonoBehaviour, IClickable
    {
        [SerializeField] private SpriteRenderer _cardRenderer;
        [SerializeField] private SortingGroup _subCardTypeSortingGroup;
        [SerializeField] private List<BaseSubCardTypeRenderer> _subCardTypeRendererList;
        [SerializeField] private BonusLabelRenderer _bonusLabelRenderer;
        [SerializeField] private SpriteRenderer _cardDimmedRenderer;
        [SerializeField] private SpriteRenderer _cardHighlightRenderer;
        [SerializeField] private GameObject _colliderObject;

        [Header("레벨 에디터용")]
        [SerializeField] private SpriteRenderer _stackIconRenderer;
        [SerializeField] private TextMeshPro _stackIndexText;
        [SerializeField] private GameObject _obscuredObject;

        [Header("Sub Card Type - Lock Color")]
        [SerializeField] private List<Color> _subCardTypeLockColorList;

        [Header("Sub Card Type - Tied Color")]
        [SerializeField] private List<Color> _subCardTypeTiedColorList;

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

        public GameObject CachedGameObject
        {
            get
            {
                if (_cachedGameObject == null)
                {
                    _cachedGameObject = gameObject;
                }
                return _cachedGameObject;
            }
        }
        private GameObject _cachedGameObject;

        private Transform CardRendererTransform
        {
            get
            {
                if (_cardRendererTransform == null)
                {
                    _cardRendererTransform = _cardRenderer.transform;
                }
                return _cardRendererTransform;
            }
        }
        private Transform _cardRendererTransform;

        private Transform CardDimmedRendererTransform
        {
            get
            {
                if (_cardDimmedRendererTransform == null)
                {
                    _cardDimmedRendererTransform = _cardDimmedRenderer.transform;
                }
                return _cardDimmedRendererTransform;
            }
        }
        private Transform _cardDimmedRendererTransform;

        public Vector3 LatestLocalPosition { get; private set; }
        public int LatestSortingOrder { get; private set; }
        public int CardID { get; private set; }

        public string SortingLayerName => _cardRenderer.sortingLayerName;
        public GameObject ColliderObject => _colliderObject;

        private Vector2Int _cardIndex;
        private int _cardStackIndex;
        private Action<Vector2Int, int> _onClick;
        private Dictionary<SubCardType, BaseSubCardTypeRenderer> _subCardTypeRendererDict;
        private BaseSubCardTypeRenderer _currSubCardTypeRenderer;

        private void OnEnable()
        {
            Deselect();
            SetObscuredObject(false);
        }

        internal void Init(Vector2Int cardIndex, int cardID, Action<Vector2Int, int> onClick)
        {
            _cardIndex = cardIndex;
            CardID = cardID;
            _onClick = onClick;

            _subCardTypeRendererDict = new();
            foreach (BaseSubCardTypeRenderer renderer in _subCardTypeRendererList)
            {
                _subCardTypeRendererDict.Add(renderer.SubCardType, renderer);
            }

            WriteStackIndex();
        }

        public void Select()
        {
            _cardHighlightRenderer.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            _cardHighlightRenderer.gameObject.SetActive(false);
        }

        internal void SetDimmedRenderer(bool animate, bool value, float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            if (animate)
            {
                _cardDimmedRenderer.DOKill(false);
                if (value == true)
                {
                    _cardDimmedRenderer.gameObject.SetActive(true);
                }
                else
                {
                    _cardDimmedRenderer.DOFade(0f, fadeDuration)
                                       .SetDelay(fadeDelay)
                                       .SetEase(fadeEase)
                                       .OnComplete(() =>
                                       {
                                           _cardDimmedRenderer.gameObject.SetActive(false);
                                           Color originColor = _cardDimmedRenderer.color;
                                           _cardDimmedRenderer.color = new Color(originColor.r, originColor.g, originColor.b, 1f);
                                       });
                }
            }
            else
            {
                _cardDimmedRenderer.gameObject.SetActive(value);
            }
        }

        internal void SetColliderObject(bool value)
        {
            _colliderObject.SetActive(value);
        }

        internal void UpdateAllRenderers(Sprite cardSprite, SubCardTypeRendererInfo subCardTypeRendererInfo, bool animateSubCardType, bool dimmedValue, Vector3 initialEulerAngles)
        {
            UpdateCardRenderer(cardSprite);
            UpdateSubCardTypeRenderer(subCardTypeRendererInfo, animateSubCardType);
            UpdateBonusLabelRenderer(subCardTypeRendererInfo);
            _cardDimmedRenderer.gameObject.SetActive(dimmedValue);
            _colliderObject.gameObject.SetActive(dimmedValue == false);

            CardRendererTransform.localEulerAngles = initialEulerAngles;
        }

        private void UpdateCardRenderer(Sprite cardSprite)
        {
            _cardRenderer.sprite = cardSprite;
        }

        private void UpdateSubCardTypeRenderer(SubCardTypeRendererInfo info, bool animateSubCardType)
        {
            foreach (BaseSubCardTypeRenderer renderer in _subCardTypeRendererList)
            {
                renderer.gameObject.SetActive(false);
            }

            if (_subCardTypeRendererDict.TryGetValue(info.type, out _currSubCardTypeRenderer))
            {
                _currSubCardTypeRenderer.gameObject.SetActive(true);

                switch (info.type)
                {
                    case SubCardType.Key:
                    case SubCardType.Lock:
                        UpdateSubCardTypeColor(_subCardTypeLockColorList, info.option);
                        break;

                    case SubCardType.Tied:
                        _currSubCardTypeRenderer.UpdateOption2(info.option2, animateSubCardType);
                        UpdateSubCardTypeColor(_subCardTypeTiedColorList, info.option);
                        break;

                    case SubCardType.UnTie:
                        UpdateSubCardTypeColor(_subCardTypeTiedColorList, info.option);
                        break;
                }
            }
        }

        private void UpdateBonusLabelRenderer(SubCardTypeRendererInfo info)
        {
            _bonusLabelRenderer.SetVisible(info.isBonusLabel && info.cardFace == CardFace.Face_Up);
        }

        private void UpdateSubCardTypeColor(List<Color> colorList, int option)
        {
            if (option != -1
                && option < colorList.Count)
            {
                Color lockColor = colorList[option];
                _currSubCardTypeRenderer.UpdateColor(lockColor);
            }
        }

        internal void UpdateLocalPosition(Vector3 localPosition)
        {
            LatestLocalPosition = localPosition;
            CachedTransform.localPosition = localPosition;
        }

        internal void UpdateStackIndex(int stackIndex)
        {
            _cardStackIndex = stackIndex;
            WriteStackIndex();
        }

        public void OnClick()
        {
            _onClick?.Invoke(_cardIndex, _cardStackIndex);
        }

        internal void UpdateSortingOrder(int sortingOrder)
        {
            LatestSortingOrder = sortingOrder;

            UpdateSortingOrderWithoutCache(sortingOrder);
        }

        public void UpdateSortingOrderWithoutCache(int sortingOrder)
        {
            _cardRenderer.sortingOrder = sortingOrder;
            _bonusLabelRenderer.UpdateSortingOrder(sortingOrder + 1);
            _subCardTypeSortingGroup.sortingOrder = sortingOrder + 2;
            _cardDimmedRenderer.sortingOrder = sortingOrder + 3;
            _stackIconRenderer.sortingOrder = sortingOrder + 4;
            _stackIndexText.sortingOrder = sortingOrder + 5;
        }

        private void WriteStackIndex()
        {
            bool stackVisible = _cardStackIndex > 0;
            _stackIconRenderer.gameObject.SetActive(stackVisible);
            if (stackVisible)
            {
                _stackIndexText.text = (_cardStackIndex + 1).ToString();
            }
        }

        internal void SetObscuredObject(bool value)
        {
            _obscuredObject.SetActive(value);
        }

        internal void UnpackCardRenderer(float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            if (_currSubCardTypeRenderer != null)
            {
                _currSubCardTypeRenderer.Unpack(fadeDelay, fadeDuration, fadeEase);
            }
        }

        internal void FlipCardRenderer(Sprite cardSprite, SubCardTypeRendererInfo subCardTypeRendererInfo, float rotateDuration, Ease rotateEase)
        {
            bool callOnce = false;
            CardRendererTransform.DOLocalRotate(new Vector3(0, 0, 0), rotateDuration)
                                 .SetEase(rotateEase)
                                 .OnUpdate(() =>
                                 {
                                     //
                                     if (CardRendererTransform.localEulerAngles.y > 90)
                                     {
                                         if (_currSubCardTypeRenderer != null)
                                         {
                                             float diff = 180 - CardRendererTransform.localEulerAngles.y;
                                             Vector3 originAngles = _currSubCardTypeRenderer.CachedTransform.localEulerAngles;
                                             _currSubCardTypeRenderer.CachedTransform.localEulerAngles = new Vector3(originAngles.x, diff, originAngles.z);
                                         }
                                     }
                                     else
                                     {
                                         if (_currSubCardTypeRenderer != null)
                                         {
                                             _currSubCardTypeRenderer.CachedTransform.localEulerAngles = CardRendererTransform.localEulerAngles;
                                         }
                                         _bonusLabelRenderer.CachedTransform.localEulerAngles = CardRendererTransform.localEulerAngles;
                                     }

                                     CardDimmedRendererTransform.localEulerAngles = CardRendererTransform.localEulerAngles;

                                     // 
                                     if (CardRendererTransform.localEulerAngles.y <= 90
                                         && callOnce == false)
                                     {
                                         callOnce = true;

                                         UpdateCardRenderer(cardSprite);
                                         UpdateSubCardTypeRenderer(subCardTypeRendererInfo, animateSubCardType: true);
                                         UpdateBonusLabelRenderer(subCardTypeRendererInfo);
                                     }
                                 });
        }
    }
}
