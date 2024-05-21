using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.System.SceneTransition
{
    public enum SceneTransitionState
    {
        FadeOut,
        FadeIn,
    }

    public class SceneTransitionUI : MonoBehaviour
    {
        [SerializeField] private float _duration = .5f;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _loadingText;

        public RectTransform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = GetComponent<RectTransform>();
                }
                return _cachedTransform;
            }
        }
        private RectTransform _cachedTransform;

        private Action _onTransitionFinished;

        public void UpdateLoadingText(string value)
        {
            _loadingText.text = value;
        }

        public void UpdateSlider(float value)
        {
            _slider.value = value;
            _slider.gameObject.SetActive(value > 0);
        }

        public void PlayTransition(SceneTransitionState nextState, Action onTransitionFinished)
        {
            _onTransitionFinished = onTransitionFinished;
            if (nextState == SceneTransitionState.FadeOut)
            {
                _canvasGroup.alpha = 0;
            }
            else
            {
                _canvasGroup.alpha = 1;
            }
            StartCoroutine(TransitionCoroutine(nextState));
        }

        private IEnumerator TransitionCoroutine(SceneTransitionState nextState)
        {
            float timePassed = 0;
            while (timePassed < _duration)
            {
                timePassed += Time.deltaTime;
                float alpha = Mathf.Clamp01(timePassed / _duration);
                _canvasGroup.alpha = nextState == SceneTransitionState.FadeOut ? 
                                     alpha : 
                                     1 - alpha;
                yield return null;
            }
            _canvasGroup.alpha = nextState == SceneTransitionState.FadeOut ? 
                                 1 : 
                                 0;
            _onTransitionFinished?.Invoke();
        }
    }
}
