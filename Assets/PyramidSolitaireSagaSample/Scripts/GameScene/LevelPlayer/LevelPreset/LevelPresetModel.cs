using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.LevelPreset
{
    [SerializeField]
    public class LevelPresetSaveData
    {
        public int level;
        public int jokerCount;
        public GameDifficultyType gameDifficultyType;
    }

    public class LevelPresetModel : MonoBehaviour
    {
        public event Action<int, int, GameDifficultyType> onLevelPresetRestored;
        public event Action<int> onJokerCountChanged;
        public event Action<GameDifficultyType> onGameDifficultyChanged;
        public event Action<int> onCurrentLevelChanged;

        public GameDifficultyType GameDifficultyType { get; private set; }

        public int JokerCount { get; private set; }
        public int Level { get; private set; }

        internal void ChangeGameDifficultyType(GameDifficultyType type)
        {
            GameDifficultyType prevGameDifficultyType = GameDifficultyType;
            if (prevGameDifficultyType != type)
            {
                ChangeGameDifficultyTypeWithoutNotify(type);
                onGameDifficultyChanged?.Invoke(type);
            }
        }

        internal void UpdateJokerCount(int jokerCount)
        {
            if (JokerCount != jokerCount)
            { 
                UpdateJokerCountWithoutNotify(jokerCount);
                onJokerCountChanged?.Invoke(jokerCount);
            }
        }

        public void UpdateCurrentLevel(int level)
        {
            if (Level != level)
            {
                UpdateCurrentLevelWithoutNotify(level);
                onCurrentLevelChanged?.Invoke(level);
            }
        }

        private void ChangeGameDifficultyTypeWithoutNotify(GameDifficultyType type)
        {
            GameDifficultyType = type;
        }

        private void UpdateJokerCountWithoutNotify(int jokerCount)
        {
            JokerCount = jokerCount;
        }

        public void UpdateCurrentLevelWithoutNotify(int level)
        {
            Level = level;
        }

        internal void RestoreSaveData(string data)
        {
            if (string.IsNullOrEmpty(data) == false)
            {
                LevelPresetSaveData saveData = JsonUtility.FromJson<LevelPresetSaveData>(data);
                int level = saveData.level;
                int jokerCount = saveData.jokerCount;
                GameDifficultyType gameDifficultyType = saveData.gameDifficultyType;

                UpdateCurrentLevelWithoutNotify(level);
                UpdateJokerCountWithoutNotify(jokerCount);
                ChangeGameDifficultyTypeWithoutNotify(gameDifficultyType);

                onLevelPresetRestored?.Invoke(level, jokerCount, gameDifficultyType);
            }
        }
    }
}

