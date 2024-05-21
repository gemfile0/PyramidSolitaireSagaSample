using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.LevelPlayerMenu
{
    public class LevelPlayerMenuPresenter : MonoBehaviour,
                                            ISpecificSceneTrigger,
                                            IMainSceneTrigger
    {
        [SerializeField] private LevelPlayerMenuUI _levelPlayerMenuUI;

        public event Action<SceneName> requestSpecificScene;
        public event Action requestMainScene;

        private void OnEnable()
        {
            _levelPlayerMenuUI.onBackToPrevSceneClick += OnBackToLevelEditorClick;
            _levelPlayerMenuUI.onReplayCurrentLevelClick += OnReplayCurrentLevelClick;
        }

        private void OnDisable()
        {
            _levelPlayerMenuUI.onBackToPrevSceneClick -= OnBackToLevelEditorClick;
            _levelPlayerMenuUI.onReplayCurrentLevelClick -= OnReplayCurrentLevelClick;
        }

        private void OnReplayCurrentLevelClick()
        {
            requestSpecificScene?.Invoke(SceneName.LevelPlayerScene);
        }

        private void OnBackToLevelEditorClick()
        {
            requestMainScene?.Invoke();
        }

        internal void UpdateLevelPreset(int level, int jokerCount, GameDifficultyType type)
        {
            _levelPlayerMenuUI.UpdateLevelUI(level);
        }
    }
}
