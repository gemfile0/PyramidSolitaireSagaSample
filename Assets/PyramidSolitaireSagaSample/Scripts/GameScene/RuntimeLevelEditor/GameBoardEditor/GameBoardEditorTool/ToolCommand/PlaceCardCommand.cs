using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public class PlaceCardCommand : BaseToolCommand, IGameBoardToolCommand
    {
        public PlaceCardCommand(GameBoardEditorPresenter gameBoardEditorPresenter) : base(gameBoardEditorPresenter)
        {
        }

        public void LeftTap()
        {
            CardModel = _gameBoardEditorPresenter.PlaceCard();
            SaveCommandParameters();
        }

        public void RightClick()
        {
            _gameBoardEditorPresenter.FlipCard();
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
