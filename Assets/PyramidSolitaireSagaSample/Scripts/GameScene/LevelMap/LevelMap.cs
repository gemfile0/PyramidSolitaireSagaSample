using PyramidSolitaireSagaSample.LevelMap.LevelPage;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoosters;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelMap
{
    public class LevelMap : MonoBehaviour
    {
        [SerializeField] private LevelPagePresenter _levelPagePresenter;
        [SerializeField] private GameBoostersPresenter _gameBoostersPresenter;

        private void Start()
        {
            _gameBoostersPresenter.RestorePreferenceData();
        }

        private void OnEnable()
        {
            _levelPagePresenter.onLevelButtonClick += _gameBoostersPresenter.OpenBoosterPopup;
            _gameBoostersPresenter.onPlayButtonClick += _levelPagePresenter.SetButtonInteractableAsFalse;
        }

        private void OnDisable()
        {
            _levelPagePresenter.onLevelButtonClick -= _gameBoostersPresenter.OpenBoosterPopup;
            _gameBoostersPresenter.onPlayButtonClick -= _levelPagePresenter.SetButtonInteractableAsFalse;
        }
    }
}
