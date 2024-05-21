using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.PiggyBank
{
    public class PiggyBankUI : MonoBehaviour
    {
        [SerializeField] private float _moveDuration = .5f;
        [SerializeField] private Ease _showEase = Ease.OutQuint;
        [SerializeField] private Ease _hideEase = Ease.InQuint;
        [SerializeField] private Ease _fadeEase = Ease.InSine;
        [SerializeField] private RectTransform _piggyBankGroupTransform;
        [SerializeField] private RectTransform _piggyBankTransform;
        [SerializeField] private CanvasGroup _piggyBankGroup;

        public Vector3 EndPoint { get; private set; }

        private int _showCount;
        private Vector2 _originPiggyBankGroupPosition;
        private Vector2 _piggyBankBottomPosition;

        private void Awake()
        {
            _showCount = 0;
        }

        private void Start()
        {
            _piggyBankGroup.alpha = 0;

            StartCoroutine(CachePiggyBankPositions());
        }

        private IEnumerator CachePiggyBankPositions()
        {
            yield return null;

            EndPoint = _piggyBankTransform.position;

            var _cachedCanvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            _piggyBankBottomPosition = new Vector2(
                _originPiggyBankGroupPosition.x,
                -(_cachedCanvasTransform.sizeDelta.y + _piggyBankGroupTransform.sizeDelta.y) / 2f
            );

            _originPiggyBankGroupPosition = _piggyBankGroupTransform.anchoredPosition;
            _piggyBankGroupTransform.anchoredPosition = _piggyBankBottomPosition;
        }

        internal void Show()
        {
            if (_showCount == 0)
            {
                _piggyBankGroup.alpha = 0;
                _piggyBankGroupTransform.anchoredPosition = _piggyBankBottomPosition;

                Sequence sequence = DOTween.Sequence();
                sequence.Append(_piggyBankGroup.DOFade(1, _moveDuration)
                                               .SetEase(_fadeEase));
                sequence.Join(_piggyBankGroupTransform.DOAnchorPos(_originPiggyBankGroupPosition, _moveDuration)
                                                      .SetEase(_showEase));
            }

            _showCount += 1;
        }

        internal void Hide()
        {
            _showCount -= 1;
            if (_showCount == 0)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.Append(_piggyBankGroup.DOFade(0, _moveDuration)
                                               .SetEase(_fadeEase));
                sequence.Join(_piggyBankGroupTransform.DOAnchorPos(_piggyBankBottomPosition, _moveDuration)
                                                      .SetEase(_hideEase));
            }
        }
    }
}
