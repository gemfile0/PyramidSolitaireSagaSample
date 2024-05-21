using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public class GameBoardTileRenderer : MonoBehaviour
    {
        [SerializeField] private Color _baseColor;
        [SerializeField] private Color _offsetColor;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TextMeshPro _rowText;
        [SerializeField] private TextMeshPro _colText;

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

        private void Awake()
        {
            _rowText.gameObject.SetActive(false);
            _colText.gameObject.SetActive(false);
        }

        public void Init(bool isOffset, Vector2 tileOffset)
        {
            Transform rendererTransform = _spriteRenderer.transform;
            rendererTransform.localPosition = Vector2.zero;
            rendererTransform.localScale = tileOffset;
            _spriteRenderer.color = isOffset ? _offsetColor : _baseColor;
        }

        public void UpdateRowText(string textStr)
        {
            _rowText.gameObject.SetActive(true);
            _rowText.text = textStr;
        }

        public void UpdateColText(string textStr)
        {
            _colText.gameObject.SetActive(true);
            _colText.text = textStr;
        }
    }
}
