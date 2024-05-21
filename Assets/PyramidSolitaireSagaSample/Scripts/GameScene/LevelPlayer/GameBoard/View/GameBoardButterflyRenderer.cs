using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class GameBoardButterflyRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;

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

        internal void UpdateRenderer(Sprite butterflySprite)
        {
            _spriteRenderer.sprite = butterflySprite;
        }
    }
}
