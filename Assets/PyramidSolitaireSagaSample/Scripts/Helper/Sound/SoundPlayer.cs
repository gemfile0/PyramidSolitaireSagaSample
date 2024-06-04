using System.Collections;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private AudioSource _audioSourceRef;

        private Transform CachedTransform
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

        private GameObjectPool<AudioSource> _audioSourcePool;

        private void Awake()
        {
            _audioSourcePool = new GameObjectPool<AudioSource>(
                CachedTransform,
                _audioSourceRef.gameObject
            );
        }

        public void Play()
        {
            StartCoroutine(PlayCoroutine());
        }

        private IEnumerator PlayCoroutine()
        {
            AudioSource audioSource = _audioSourcePool.Get();
            audioSource.transform.SetParent(CachedTransform);
            audioSource.clip = _audioClip;
            audioSource.Play();

            yield return new WaitForSeconds(_audioClip.length);
            _audioSourcePool.Release(audioSource);
        }
    }
}
