using PyramidSolitaireSagaSample.GameData;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.SaveLevel
{
    public class LevelSelectionEditorModel : MonoBehaviour
    {
        public event Action<int> onSelectedLevelChanged;
        public event Action<LevelDataPath> onLevelDataPathChanged;

        public int SelectedLevel { get; private set; }
        public LevelDataPath LevelDataPath { get; private set; }

        internal void UpdateSelectedLevel(int nextLevel)
        {
            int prevLevel = SelectedLevel;
            if (prevLevel != nextLevel)
            {
                UpdateSelectedLevel_WithoutNotify(nextLevel);
                onSelectedLevelChanged?.Invoke(nextLevel);
            }
        }

        public void UpdateSelectedLevel_WithoutNotify(int selectedLevel)
        {
            SelectedLevel = selectedLevel;
        }

        internal void UpdateLevelDataPath(LevelDataPath nextLevelDataPath)
        {
            LevelDataPath prevLevelDataPath = LevelDataPath;
            if (prevLevelDataPath != nextLevelDataPath)
            {
                UpdateLevelDataPath_WithoutNotify(nextLevelDataPath);
                onLevelDataPathChanged?.Invoke(nextLevelDataPath);
            }
        }

        internal void UpdateLevelDataPath_WithoutNotify(LevelDataPath levelDataPath)
        {
            LevelDataPath = levelDataPath;
        }
    }
}
