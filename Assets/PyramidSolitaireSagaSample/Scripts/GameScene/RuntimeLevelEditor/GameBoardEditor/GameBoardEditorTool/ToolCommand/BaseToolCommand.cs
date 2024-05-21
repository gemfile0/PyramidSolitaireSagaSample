using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public abstract class BaseToolCommand
    {
        public GameBoardCardModel CardModel { get; protected set; }

        protected Vector2Int _prevIndex;
        protected Vector2Int _nextIndex;
        protected int _prevStackIndex;
        protected int _nextStackIndex;
        protected int _prevID;
        protected int _nextID;
        protected List<GameBoardCardModel> _prevParentSet;
        protected List<GameBoardCardModel> _prevChildSet;
        protected CommonCardInfo _cardInfo;

        protected GameBoardEditorPresenter _gameBoardEditorPresenter;

        public BaseToolCommand(GameBoardEditorPresenter gameBoardEditorPresenter)
        {
            _gameBoardEditorPresenter = gameBoardEditorPresenter;

            _prevParentSet = new List<GameBoardCardModel>();
            _prevChildSet = new List<GameBoardCardModel>();
        }

        protected void SaveCommandParameters()
        {
            if (CardModel != null)
            {
                _prevIndex = CardModel.PrevIndex;
                _nextIndex = CardModel.Index;
                _prevStackIndex = CardModel.PrevStackIndex;
                _nextStackIndex = CardModel.StackIndex;
                _prevID = CardModel.PrevID;
                _nextID = CardModel.ID;
                _prevParentSet.Clear();
                _prevParentSet.AddRange(CardModel.PrevParentSet);
                _prevChildSet.Clear();
                _prevChildSet.AddRange(CardModel.PrevChildSet);
                _cardInfo = CardModel.CardInfo;
            }
        }
    }
}
