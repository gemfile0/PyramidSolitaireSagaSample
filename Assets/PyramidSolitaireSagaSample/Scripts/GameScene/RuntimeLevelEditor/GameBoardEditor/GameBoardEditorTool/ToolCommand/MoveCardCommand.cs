using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public class MoveCardCommand : BaseToolCommand, IGameBoardToolCommand
    {
        public MoveCardCommand(GameBoardEditorPresenter gameBoardEditorPresenter) : base(gameBoardEditorPresenter)
        {
        }

        public void LeftTap()
        {
            CardModel = _gameBoardEditorPresenter.MoveCard();
            SaveCommandParameters();
        }

        public void RightClick()
        {
            _gameBoardEditorPresenter.FlipCard();
        }

        public void Undo()
        {
            CardModel = _gameBoardEditorPresenter.MoveCard(_nextIndex, _nextStackIndex, _prevIndex, _prevStackIndex, _prevID, _prevParentSet, _prevChildSet);
            SaveCommandParameters();
        }

        public void Redo()
        {
            CardModel = _gameBoardEditorPresenter.MoveCard(_nextIndex, _nextStackIndex, _prevIndex, _prevStackIndex, _prevID, _prevParentSet, _prevChildSet);
            SaveCommandParameters();
        }
    }
}
