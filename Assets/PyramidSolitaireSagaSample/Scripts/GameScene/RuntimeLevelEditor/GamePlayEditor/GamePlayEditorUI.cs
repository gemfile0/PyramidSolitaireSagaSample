using System;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GamePlay
{
    public class GamePlayEditorUI : MonoBehaviour
    {
        public event Action onPlayButtonClick;
        public event Action onBoosterButtonClick;

        [SerializeField] private Button _gamePlayButton;
        [SerializeField] private Button _selectBoosterButton;

        public void OnPlayClick()
        {
            onPlayButtonClick?.Invoke();
        }

        public void OnBoosterClick()
        {
            onBoosterButtonClick?.Invoke();
        }

        internal void SetButtonInteractable(bool value)
        {
            _gamePlayButton.interactable = value;
            _selectBoosterButton.interactable = value;
        }
    }
}
