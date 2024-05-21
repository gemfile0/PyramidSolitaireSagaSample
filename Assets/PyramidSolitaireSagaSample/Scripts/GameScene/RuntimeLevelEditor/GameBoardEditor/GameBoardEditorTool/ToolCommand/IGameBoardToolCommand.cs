using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public interface IGameBoardToolCommand
    {
        public GameBoardCardModel CardModel { get; }
        void LeftTap();
        void RightClick();
        void Undo();
        void Redo();
    }
}
