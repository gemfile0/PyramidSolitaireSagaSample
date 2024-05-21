using PyramidSolitaireSagaSample.System;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.LevelPreset
{
    public interface ILevelPresetRequester
    {
        public event Func<GameDifficultyType> requestGameDifficultyType;
    }

    public class LevelPresetPresenter : MonoBehaviour, IGameObjectFinderSetter, ILevelRestorable
    {
        [Header("Model")]
        [SerializeField] private LevelPresetModel _levelPresetModel;

        public event Action<int> onJokerCountChanged
        {
            add { _levelPresetModel.onJokerCountChanged += value; }
            remove { _levelPresetModel.onJokerCountChanged -= value; }
        }

        public event Action<int /* level */, int /* jokerCount */, GameDifficultyType> onLevelPresetRestored
        {
            add { _levelPresetModel.onLevelPresetRestored += value; }
            remove { _levelPresetModel.onLevelPresetRestored -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.LevelPreset;

        private IEnumerable<ILevelPresetRequester> _levelPresetRequesters;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _levelPresetRequesters = finder.FindGameObjectOfType<ILevelPresetRequester>();
        }

        private void OnEnable()
        {
            foreach (ILevelPresetRequester requester in _levelPresetRequesters)
            {
                requester.requestGameDifficultyType += GetGameDifficultyType;
            }
        }

        private void OnDisable()
        {
            foreach (ILevelPresetRequester requester in _levelPresetRequesters)
            {
                requester.requestGameDifficultyType -= GetGameDifficultyType;
            }
        }

        private GameDifficultyType GetGameDifficultyType()
        {
            return _levelPresetModel.GameDifficultyType;
        }

        public void RestoreLevelData(string data)
        {
            _levelPresetModel.RestoreSaveData(data);
        }
    }
}
