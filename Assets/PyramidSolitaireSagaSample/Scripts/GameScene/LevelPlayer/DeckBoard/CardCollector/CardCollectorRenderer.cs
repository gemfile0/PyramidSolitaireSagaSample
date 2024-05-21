using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.CardCollector
{
    public class CardCollectorRenderer : MonoBehaviour
    {
        [SerializeField] private Transform _itemRendererRoot;

        public Transform ItemRendererRoot => _itemRendererRoot;

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

    }
}
