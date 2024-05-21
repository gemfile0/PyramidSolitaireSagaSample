using PyramidSolitaireSagaSample.GameData;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameTutorial
{
    public interface ILevelTutorialUiCloner
    {
        LevelTutorialCloneType UiCloneType { get; }

        void CloneUI(Transform uiCloneContainerTransform, UiCloneData uiCloneData);
    }
}
