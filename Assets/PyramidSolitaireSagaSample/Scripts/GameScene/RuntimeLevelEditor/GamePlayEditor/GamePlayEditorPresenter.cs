using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GamePlay
{
    public class GamePlayEditorPresenter : MonoBehaviour,
                                           ISpecificSceneTrigger
    {
        [Header("View")]
        [SerializeField] private GamePlayEditorUI _gamePlayUI;

        public event Action<SceneName> requestSpecificScene;

        public event Action onPlayButtonClick
        {
            add => _gamePlayUI.onPlayButtonClick += value;
            remove => _gamePlayUI.onPlayButtonClick -= value;
        }

        public event Action onBoosterButtonClick
        {
            add => _gamePlayUI.onBoosterButtonClick += value;
            remove => _gamePlayUI.onBoosterButtonClick -= value;
        }

        private void OnEnable()
        {
            _gamePlayUI.onPlayButtonClick += OnPlayClick;
        }

        private void OnDisable()
        {
            _gamePlayUI.onPlayButtonClick -= OnPlayClick;
        }

        private void OnPlayClick()
        {
            _gamePlayUI.SetButtonInteractable(false);
            requestSpecificScene?.Invoke(SceneName.LevelPlayerScene);
        }
    }
}
