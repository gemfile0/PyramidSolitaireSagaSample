using DG.Tweening;
using PyramidSolitaireSagaSample.GameData;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameTutorial
{
    public class GameTutorialStepUI : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = .3f;
        [SerializeField] private Ease _fadeEase = Ease.OutSine;

        [SerializeField] private float _typeSpeed = 15f;
        [SerializeField] private Ease _textEase = Ease.OutSine;

        [Space]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameTutorialStepUiLocation _location;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _continueMessageText;

        public GameTutorialStepUiLocation Location => _location;

        public GameObject CachedGameObject
        {
            get
            {
                if (_cachedGameObject == null)
                {
                    _cachedGameObject = gameObject;
                }
                return _cachedGameObject;
            }
        }
        private GameObject _cachedGameObject;

        private int _currentVisibleCharacterIndex;
        private Sequence _sequence;

        internal void UpdateMessageText(string message, string continueMessage)
        {
            _messageText.text = message;
            _continueMessageText.text = continueMessage;

            _canvasGroup.alpha = 0;
            _messageText.maxVisibleCharacters = 0;
            _currentVisibleCharacterIndex = 0;

            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }

            _sequence = DOTween.Sequence();
            _sequence.Append(_canvasGroup.DOFade(1f, _fadeDuration).SetEase(_fadeEase));
            _sequence.Append(
                DOTween.To(getter: () => _currentVisibleCharacterIndex,
                           setter: x => _currentVisibleCharacterIndex = x,
                           endValue: message.Length,
                           duration: message.Length / _typeSpeed)
                       .SetEase(_textEase)
                       .OnUpdate(() => _messageText.maxVisibleCharacters = _currentVisibleCharacterIndex)
            );
        }

    }
}
