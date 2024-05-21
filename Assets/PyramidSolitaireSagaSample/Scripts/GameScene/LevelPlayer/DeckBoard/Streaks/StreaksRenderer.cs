using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.Streaks
{
    public class StreaksRenderer : MonoBehaviour
    {
        [SerializeField] private TextMeshPro countText;

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

        internal void UpdateRenderer(int value)
        {
            countText.text = value.ToString();
        }
    }
}
