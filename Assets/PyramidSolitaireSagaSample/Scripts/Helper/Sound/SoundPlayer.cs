using System.Collections;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _audioDelay = .1f;

        private bool _isInPlay;

        public void Play()
        {
            if (_isInPlay == false)
            {
                StartCoroutine(PlayCoroutine());
            }
        }

        private IEnumerator PlayCoroutine()
        {
            _isInPlay = true;

            _audioSource.PlayOneShot(_audioClip);

            yield return new WaitForSeconds(_audioDelay);
            _isInPlay = false;
        }
    }
}
