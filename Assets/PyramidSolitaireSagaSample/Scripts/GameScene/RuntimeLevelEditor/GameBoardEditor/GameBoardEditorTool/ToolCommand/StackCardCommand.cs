using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public class StackCardCommand : BaseToolCommand, IGameBoardToolCommand
    {
        public StackCardCommand(GameBoardEditorPresenter gameBoardEditorPresenter) : base(gameBoardEditorPresenter)
        {
        }

        public void LeftTap()
        {
            CardModel = _gameBoardEditorPresenter.StackCard();
            SaveCommandParameters();
        }

        public void RightClick()
        {
            // Do nothing
        }

        public void Undo()
        {
            CardModel = _gameBoardEditorPresenter.RemoveCard(_nextIndex, _nextStackIndex);
            SaveCommandParameters();
        }

        public void Redo()
        {
            CardModel = _gameBoardEditorPresenter.PlaceCard(_nextID, _nextIndex, _nextStackIndex, _cardInfo, _prevParentSet, _prevChildSet);
            SaveCommandParameters();
        }
    }
}
