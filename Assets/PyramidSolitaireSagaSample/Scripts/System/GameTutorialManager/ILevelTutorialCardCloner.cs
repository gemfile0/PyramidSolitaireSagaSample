using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using System.Collections.Generic;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameTutorial
{
    public interface ILevelTutorialCardCloner
    {
        LevelTutorialCloneType CardCloneType { get; }
        int GetCloneCount(IEnumerable<CardRendererCloneData> cloneDataList);
        void CloneCardRendererList(IReadOnlyList<GameBoardCardRenderer> cardRendererList, IEnumerable<CardRendererCloneData> cloneDataList);
        void DrawCard(IEnumerable<CardRendererCloneData> cloneDataList);
    }
}
