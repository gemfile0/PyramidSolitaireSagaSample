using DG.Tweening;
using PyramidSolitaireSagaSample.Helper;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class BonusLabelRenderer : MonoBehaviour
    {
        [SerializeField] private AnimatorParser _animatorParser;

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

        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
                return _spriteRenderer;
            }
        }
        private SpriteRenderer _spriteRenderer;

        internal void UpdateSortingOrder(int sortingOrder)
        {
            SpriteRenderer.sortingOrder = sortingOrder;
        }

        internal void SetVisible(bool value)
        {
            gameObject.SetActive(value);
        }

        internal virtual void Unpack(float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            _animatorParser.SetTrigger();
        }
    }
}
