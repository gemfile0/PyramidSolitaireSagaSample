using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    public enum CannotDrawState
    {
        None,
        FaceDown,
        HasChildren,
        Locked,
        Tied
    }

    public interface IGameBoardCardModel : ICardSelectionItemModel
    {
        Vector2Int Index { get; }
        int StackIndex { get; }
        int StackCount { get; }
        string ChildrenNumber { get; }
        string ParentsNumber { get; }
        int ChildCount { get; }
        bool IsObscured { get; }

        void UpdateCardInfoWithoutNotify(CommonCardInfo cardInfo);
        void ChangeCardStackIndex(int offset);
        void ClickCardStackIndex(int stackIndex);
        bool CannotDrawCard();
    }

    public class GameBoardCardModel : IGameBoardCardModel
    {
        public int ID { get; private set; }
        public int PrevID { get; private set; }
        public Vector2Int Index { get; private set; }
        public Vector2Int PrevIndex { get; private set; }
        public int PrevStackIndex { get; private set; }
        public int StackIndex { get; private set; }
        public int StackCount { get; private set; }
        public string ChildrenNumber => MakeNumberString(_childSet);
        public string ParentsNumber => MakeNumberString(_parentSet);
        public CommonCardInfo CardInfo { get; private set; }

        public bool IsObscured { get; private set; }
        public int ChildCount => _childSet.Count;
        public int ParentCount => _parentSet.Count;
        public List<GameBoardCardModel> PrevParentSet
        {
            get
            {
                if (_prevParentSet == null)
                {
                    _prevParentSet = new List<GameBoardCardModel>();
                }
                return _prevParentSet;
            }
        }
        private List<GameBoardCardModel> _prevParentSet;
        public List<GameBoardCardModel> PrevChildSet
        {
            get
            {
                if (_prevChildSet == null)
                {
                    _prevChildSet = new List<GameBoardCardModel>();
                }
                return _prevChildSet;
            }
        }
        private List<GameBoardCardModel> _prevChildSet;

        private Action<GameBoardCardModel> _onCardInfoChanged;
        private Action<GameBoardCardModel> _onCardModelChildrenChanged;
        private Action<Vector2Int, int> _onCardStackIndexClick;
        private Action<int, Vector2Int, int> _onCardStackIndexChanged;

        private List<GameBoardCardModel> _parentSet;
        private List<GameBoardCardModel> _childSet;
        private StringBuilder _logBuilder;

        public GameBoardCardModel(
            int id,
            Vector2Int index,
            int stackIndex,
            Action<GameBoardCardModel> onCardInfoChanged,
            Action<GameBoardCardModel> onCardModelChildrenChanged,
            Action<Vector2Int, int> onCardStackIndexClick,
            Action<int, Vector2Int, int> onCardStackIndexChanged
        )
        {
            ID = id;
            Index = index;
            StackIndex = stackIndex;
            _onCardInfoChanged = onCardInfoChanged;
            _onCardModelChildrenChanged = onCardModelChildrenChanged;
            _onCardStackIndexClick = onCardStackIndexClick;
            _onCardStackIndexChanged = onCardStackIndexChanged;

            _parentSet = new List<GameBoardCardModel>();
            _childSet = new List<GameBoardCardModel>();
            _logBuilder = new StringBuilder();
        }

        public void UpdateCardInfo(CardSelectionInfo cardSelectionInfo)
        {
            CardInfo.UpdateInfo(cardSelectionInfo);
            UpdateCardInfo(CardInfo);
        }

        public void UpdateCardInfo(CardNumber cardNumber, CardColor cardColor, CardFace cardFace, CardType cardType, SubCardType subCardType, int subCardTypeOption, int subCardTypeOption2, bool isCardLabel)
        {
            CardInfo.UpdateInfo(cardNumber, cardColor, cardFace, cardType, subCardType, subCardTypeOption, subCardTypeOption2, isCardLabel);
            UpdateCardInfo(CardInfo);
        }

        public void UpdateCardInfo(CommonCardInfo cardInfo)
        {
            UpdateCardInfoWithoutNotify(cardInfo);

            _onCardInfoChanged?.Invoke(this);
        }

        public void UpdateCardInfoWithoutNotify(CommonCardInfo cardInfo)
        {
            CardInfo = cardInfo;
        }

        public void AddParent(GameBoardCardModel parent)
        {
            _parentSet.Add(parent);

            // ID 내림차순 정렬
            _parentSet.Sort((x, y) => y.ID.CompareTo(x.ID));
            parent.AddChild(this);
        }

        public void RemoveParent(GameBoardCardModel parent)
        {
            _parentSet.Remove(parent);
            parent.RemoveChild(this);
        }

        private void AddChild(GameBoardCardModel child)
        {
            _childSet.Add(child);

            // ID 오름차순 정렬
            _childSet.Sort((x, y) => x.ID.CompareTo(y.ID));

            CheckIfIsObscured();

            //LogChildSet("AddChild()");

            _onCardModelChildrenChanged?.Invoke(this);
        }

        private void LogChildSet(string caller)
        {
            if (Index == new Vector2Int(8, 3))
            {
                Debug.Log(caller);
                foreach (GameBoardCardModel _child in _childSet)
                {
                    Debug.Log($"child.Index: {_child.Index}");
                }
            }
        }

        private void RemoveChild(GameBoardCardModel child)
        {
            _childSet.Remove(child);

            CheckIfIsObscured();

            //LogChildSet("RemoveChild()");

            _onCardModelChildrenChanged?.Invoke(this);
        }

        private void CheckIfIsObscured()
        {
            IsObscured = false;
            bool leftIndexFound = false;
            bool rightIndexFound = false;
            bool sameIndexFound = false;
            Vector2Int leftIndex = Index + Vector2Int.left;
            Vector2Int rightIndex = Index + Vector2Int.right;
            foreach (GameBoardCardModel _child in _childSet)
            {
                Vector2Int childIndex = _child.Index;
                if (childIndex == leftIndex)
                {
                    leftIndexFound = true;
                }
                else if (childIndex == rightIndex)
                {
                    rightIndexFound = true;
                }
                else if (childIndex == Index)
                {
                    sameIndexFound = true;
                }

                if (IsObscured == false)
                {
                    if (sameIndexFound)
                    {
                        IsObscured = true;
                    }
                    else if (leftIndexFound && rightIndexFound)
                    {
                        IsObscured = true;
                    }
                }
            }
        }

        public void RemoveFamily()
        {
            //Debug.Log("RemoveFamily()");
            PrevParentSet.Clear();
            PrevParentSet.AddRange(_parentSet);
            for (int i = _parentSet.Count - 1; i >= 0; i--)
            {
                GameBoardCardModel parent = _parentSet[i];
                RemoveParent(parent);
                //Debug.Log($"parent : {parent.CardInfo}, {parent.ChildCount}");
            }

            PrevChildSet.Clear();
            PrevChildSet.AddRange(_childSet);
            for (int i = _childSet.Count - 1; i >= 0; i--)
            {
                GameBoardCardModel child = _childSet[i];
                //Debug.Log($"parent : {child.CardInfo}, {child.ParentCount}");
                child.RemoveParent(this);
            }
        }

        private string MakeNumberString(List<GameBoardCardModel> cardModelList)
        {
            _logBuilder.Length = 0;
            for (int i = 0; i < cardModelList.Count; i++)
            {
                GameBoardCardModel cardModel = cardModelList[i];
                _logBuilder.Append(cardModel.CardInfo.CardNumber.ToString().Replace("Num_", ""));
                if (i < cardModelList.Count - 1)
                {
                    _logBuilder.Append(", ");
                }
            }
            return _logBuilder.ToString();
        }

        internal void UpdateID(int id)
        {
            PrevID = ID;
            ID = id;
        }

        internal void UpdateStackIndexAndCount(int stackIndex, int stackCount)
        {
            PrevStackIndex = StackIndex;
            StackIndex = stackIndex;
            StackCount = stackCount;
        }

        internal void UpdateIndex(Vector2Int index)
        {
            PrevIndex = Index;
            Index = index;
        }

        public void ClickCardStackIndex(int stackIndex)
        {
            _onCardStackIndexClick?.Invoke(Index, stackIndex);
        }

        public void ChangeCardStackIndex(int offset)
        {
            _onCardStackIndexChanged?.Invoke(offset, Index, StackIndex);
        }

        internal GameBoardCardModel GetParent(int i)
        {
            return _parentSet[i];
        }

        internal GameBoardCardModel GetChild(int i)
        {
            return _childSet[i];
        }

        internal (bool isConsumed, SubCardType subCardType, int subCardTypeOption) ConsumeSubCardType()
        {
            SubCardType subCardType = CardInfo.SubCardType;
            int subCardTypeOption = CardInfo.SubCardTypeOption;
            SubCardType nextSubCardType = SubCardType.SubType_None;
            bool isConsumed = subCardType != nextSubCardType;

            CardInfo.UpdateInfo(CardInfo.CardNumber, CardInfo.CardColor, CardInfo.CardFace, CardInfo.CardType, nextSubCardType, subCardTypeOption: -1, subCardTypeOption2: -1, CardInfo.IsBonusLabel);
            return (isConsumed, subCardType, subCardTypeOption);
        }

        internal bool ConsumeIsBonusLabel()
        {
            bool nextIsBonusLabel = false;
            bool isConsumed = CardInfo.IsBonusLabel != nextIsBonusLabel;

            CardInfo.UpdateInfo(CardInfo.CardNumber, CardInfo.CardColor, CardInfo.CardFace, CardInfo.CardType, CardInfo.SubCardType, CardInfo.SubCardTypeOption, -1, nextIsBonusLabel);
            return isConsumed;
        }

        public bool CannotDrawCard()
        {
            CannotDrawState state = CardInfo.CardFace == CardFace.Face_Down ? CannotDrawState.FaceDown :
                                    (CardInfo.CardFace == CardFace.Face_Up && ChildCount > 0) ? CannotDrawState.HasChildren :
                                    CardInfo.SubCardType == SubCardType.Lock ? CannotDrawState.Locked :
                                    CardInfo.SubCardType == SubCardType.Tied ? CannotDrawState.Tied :
                                    CannotDrawState.None;
            //Debug.Log("CannotDrawCard() : " + state);
            return state != CannotDrawState.None;
        }
    }
}
