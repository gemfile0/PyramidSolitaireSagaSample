using DG.Tweening;
using PyramidSolitaireSagaSample.Helper;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    [Serializable]
    public class BaseSubCardTypeRenderer : MonoBehaviour
    {
        [field: SerializeField] public SubCardType SubCardType { get; private set; }
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

        internal void SetVisible(bool value)
        {
            SpriteRenderer.gameObject.SetActive(value);
        }

        internal virtual void UpdateColor(Color value)
        {
            SpriteRenderer.color = value;
        }

        internal virtual void Unpack(float fadeDelay, float fadeDuration, Ease fadeEase)
        {
            _animatorParser.SetTrigger();
        }

        internal virtual void UpdateOption2(int option2, bool animate)
        {
            throw new NotImplementedException();
        }
    }
}
