using System;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.Title.Auth
{
    public class AuthUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _saveProgressButton;

        public event Action onPlayClick;
        public event Action onSaveProgressClick;

        public void OnPlayClick()
        {
            onPlayClick?.Invoke();
        }

        public void OnSaveProgressClick()
        {
            onSaveProgressClick?.Invoke();
        }

        public void SetButtonVisible(bool value)
        {
            _playButton.gameObject.SetActive(value);
            _saveProgressButton.gameObject.SetActive(value);
        }

        internal void SetPlayButtonInteractable(bool value)
        {
            _playButton.interactable = value;
        }
    }
}
