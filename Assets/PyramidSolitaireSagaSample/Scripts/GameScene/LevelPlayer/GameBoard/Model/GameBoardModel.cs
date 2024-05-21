using PyramidSolitaireSagaSample.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoard
{
    [Serializable]
    public class GameBoardCardSaveData
    {
        public int ID;
        public Vector2Int Index;
        public int StackIndex;
        public CommonCardInfo CardInfo;
    }

    [Serializable]
    public class GameBoardSaveData
    {
        public List<GameBoardCardSaveData> cardDataList;
    }

    public enum GameBoardModelState
    {
        None,
        Highlight,
        Restore,
        Untie,
    }

    public class GameBoardModel : MonoBehaviour
    {
        // todo : onCardModelChildrenChanged 의 리스너에서 수행하는 기능들을 onCardModelDepthChanged 의 리스너로 옮기고 제거해야할지 고민
        public event Action<IGameBoardCardModel> onCardModelChildrenChanged;
        public event Action<IGameBoardCardModel> onCardInfoChanged;
        public event Action<IGameBoardCardModel, bool /* isSelected */> onCardModelSelected;
        public event Action<Vector2Int, int, int /* cardID */> onCardModelCreated;
        public event Action<Vector2Int, int> onCardModelRemoved;
        public event Action<Vector2Int, int, Vector2Int, int> onCardModelMoved;
        public event Action<Vector2Int, int, int> onCardModelStackIndexSwaped;
        public event Action<IEnumerable<IGameBoardCardModel>> onCardModelDepthChanged;
        public event Action<IEnumerable<IGameBoardCardModel>> onGameBoardChanged;
        public event Action<IEnumerable<IGameBoardCardModel>> onGameBoardRestored;
        public event Action<IEnumerable<IGameBoardCardModel>> onGameBoardDrawn;
        public event Action<Vector2Int, int, CommonCardInfo> onCardModelDrawn;
        public event Action<IGameBoardCardModel, SubCardType, int> onSubCardTypeConsumed;
        //public event Action<IGameBoardCardModel> onIsBonusLabelConsumed;

        public Vector2Int LatestSnappedIndex { get; private set; } = new Vector2Int(-1, -1);
        public EnumState<GameBoardModelState> State { get; private set; }

        public IEnumerable<IGameBoardCardModel> CardModelList => _cardModelList;

        public int LatestSnappedStackIndex => _cardModelStackDict[LatestSnappedIndex].Count - 1;

        private GameBoardCardModel _selectedCardModel;

        private Dictionary<Vector2Int, List<GameBoardCardModel>> _cardModelStackDict;
        private List<GameBoardCardModel> _cardModelList;
        private List<GameBoardCardModel> _tempNeighborCardModelList;

        private StringBuilder _logBuilder;
        private Dictionary<SubCardTypeTiedColor, int> _untieCountDict;
        private Dictionary<SubCardTypeTiedColor, int> _newUntieCountDict;
        private List<IGameBoardCardModel> _tutorialCardModelList;

        // External
        private int _lastRowIndex;
        private int _lastColIndex;

        public void Init(int lastRowIndex, int lastColIndex)
        {
            State = new(showLog: true);
            _cardModelStackDict = new();
            _cardModelList = new();
            _tempNeighborCardModelList = new();
            _logBuilder = new();
            _untieCountDict = new();
            _newUntieCountDict = new();
            _tutorialCardModelList = new();

            _lastRowIndex = lastRowIndex;
            _lastColIndex = lastColIndex;
            for (int i = 0; i <= _lastRowIndex; i++)
            {
                for (int j = 0; j <= _lastColIndex; j++)
                {
                    Vector2Int cardIndex = new(j, i);
                    _cardModelStackDict.Add(cardIndex, new());
                }
            }
        }

        internal void UpdateSnappedIndex(Vector2Int snappedIndex)
        {
            LatestSnappedIndex = snappedIndex;
        }

        internal (bool, GameBoardCardModel) GetOrCreateCardModel()
        {
            bool isCreated = false;
            GameBoardCardModel cardModel;
            List<GameBoardCardModel> cardStack = _cardModelStackDict[LatestSnappedIndex];
            if (cardStack.Count > 0)
            {
                int cardStackIndex = cardStack.Count - 1;
                cardModel = cardStack[cardStackIndex];

                InitLog();
                AppendCardLog(cardModel);
                Debug.Log($"배치되어 있던 카드를 가져옵니다 : {_logBuilder}");
            }
            else
            {
                isCreated = true;
                cardModel = CreateCardModel();
            }

            return (isCreated, cardModel);
        }

        internal (bool, GameBoardCardModel) GetOrFlipCardModel()
        {
            bool isFlipped = false;
            List<GameBoardCardModel> cardStack = _cardModelStackDict[LatestSnappedIndex];
            GameBoardCardModel cardModel;
            if (cardStack.Count > 0)
            {
                int cardStackIndex = cardStack.Count - 1;
                cardModel = cardStack[cardStackIndex];
                if (cardModel != _selectedCardModel)
                {
                    InitLog();
                    AppendCardLog(cardModel);
                    Debug.Log($"배치되어 있던 카드를 가져옵니다 : {_logBuilder}");
                }
                else
                {
                    isFlipped = true;
                    FlipCardModel();
                }
            }
            else
            {
                cardModel = null;
            }

            return (isFlipped, cardModel);
        }

        private void FlipCardModel()
        {
            CommonCardInfo cardInfo = _selectedCardModel.CardInfo;
            _selectedCardModel.UpdateCardInfo(
                cardInfo.CardNumber,
                cardInfo.CardColor,
                cardInfo.CardFace == CardFace.Face_Up ? CardFace.Face_Down : CardFace.Face_Up,
                cardInfo.CardType,
                cardInfo.SubCardType,
                cardInfo.SubCardTypeOption,
                cardInfo.SubCardTypeOption2,
                cardInfo.IsBonusLabel
            );
        }

        internal GameBoardCardModel CreateCardModel()
        {
            int nextCardStackIndex = LatestSnappedStackIndex + 1;
            GameBoardCardModel cardModel = _CreateCardModel(_cardModelList.Count, LatestSnappedIndex, cardStackIndex: nextCardStackIndex);
            cardModel.UpdateCardInfo(new CommonCardInfo(
                CardNumber.Num_Random,
                CardColor.Color_Random,
                CardFace.Face_Up,
                CardType.Type_None,
                SubCardType.SubType_None,
                subCardTypeOption: -1,
                subCardTypeOption2: -1,
                isBonusLabel: false
            ));

            ReadNeighbors(cardModel.Index, cardModel.StackIndex);
            AddNeighborsAsParent(cardModel);

            return cardModel;
        }

        internal (bool, GameBoardCardModel) GetOrMoveCardModel()
        {
            bool isMoved = false;
            GameBoardCardModel cardModel;
            List<GameBoardCardModel> cardStack = _cardModelStackDict[LatestSnappedIndex];
            if (cardStack.Count > 0)
            {
                int cardStackIndex = cardStack.Count - 1;
                cardModel = cardStack[cardStackIndex];

                InitLog();
                AppendCardLog(cardModel);
                Debug.Log($"배치되어 있던 카드를 가져옵니다 : {_logBuilder}");
            }
            else if (_selectedCardModel != null)
            {
                isMoved = true;
                cardModel = _selectedCardModel;
                MoveCardModel(cardModel.Index, LatestSnappedIndex);
            }
            else
            {
                cardModel = null;
            }

            return (isMoved, cardModel);
        }

        public void MoveCardModel(Vector2Int originCardIndex, Vector2Int nextCardIndex)
        {
            List<GameBoardCardModel> originCardStack = _cardModelStackDict[originCardIndex];
            if (originCardStack.Count > 0)
            {
                int originCardStackIndex = originCardStack.Count - 1;
                GameBoardCardModel cardModel = originCardStack[originCardStackIndex];
                cardModel.RemoveFamily();
                cardModel.UpdateIndex(nextCardIndex);

                _cardModelList.RemoveAt(cardModel.ID);
                originCardStack.RemoveAt(originCardStackIndex);

                _cardModelList.Add(cardModel);
                ReorderCardID();
                List<GameBoardCardModel> nextCardStack = _cardModelStackDict[nextCardIndex];
                int nextCardStackIndex = nextCardStack.Count;
                nextCardStack.Add(cardModel);
                ReorderCardStackIndex(originCardStack);
                ReorderCardStackIndex(nextCardStack);
                Debug.Log($"MoveCardModel : {originCardIndex},{originCardStackIndex} -> {nextCardIndex},{nextCardStackIndex}");

                onCardModelMoved?.Invoke(originCardIndex, originCardStackIndex, nextCardIndex, nextCardStackIndex);

                ReadNeighbors(nextCardIndex, nextCardStackIndex);
                AddNeighborsAsParent(cardModel);

                onCardModelDepthChanged?.Invoke(CardModelList);
                onGameBoardChanged?.Invoke(CardModelList);
                NotifyCardModelChildrenChanged(cardModel);
            }
        }

        public GameBoardCardModel _CreateCardModel(int cardID, Vector2Int cardIndex, int cardStackIndex)
        {
            GameBoardCardModel cardModel;
            List<GameBoardCardModel> cardStack = _cardModelStackDict[cardIndex];
            if (cardStackIndex >= cardStack.Count)
            {
                //Debug.Log($"CreateCardModel : {cardIndex}, {cardStackIndex}");
                cardModel = new GameBoardCardModel(
                    cardID,
                    cardIndex,
                    cardStack.Count + 1,
                    OnCardInfoChanged,
                    NotifyCardModelChildrenChanged,
                    OnCardStackIndexClick,
                    OnCardStackIndexChanged
                );
                _cardModelStackDict[cardIndex].Add(cardModel);
                _cardModelList.Insert(cardID, cardModel);
                ReorderCardStackIndex(cardStack);

                onCardModelCreated?.Invoke(cardIndex, cardStackIndex, cardID);
            }
            else
            {
                cardModel = cardStack[cardStackIndex];

                InitLog();
                AppendCardLog(cardModel);
                Debug.LogWarning($"잘못된 요청입니다. 이미 배치된 카드가 있습니다 : {_logBuilder}");
            }

            return cardModel;
        }

        public GameBoardCardModel CreateCardModel(
            int cardID,
            Vector2Int cardIndex,
            int cardStackIndex,
            CommonCardInfo cardInfo,
            List<GameBoardCardModel> parentSet,
            List<GameBoardCardModel> childSet
        )
        {
            GameBoardCardModel cardModel = _CreateCardModel(cardID, cardIndex, cardStackIndex);
            cardModel.UpdateCardInfo(cardInfo);
            ReorderCardID();

            foreach (GameBoardCardModel parentModel in parentSet)
            {
                cardModel.AddParent(parentModel);
            }
            foreach (GameBoardCardModel childModel in childSet)
            {
                childModel.AddParent(cardModel);
            }

            onCardModelDepthChanged?.Invoke(CardModelList);
            onGameBoardChanged?.Invoke(CardModelList);
            return cardModel;
        }

        internal GameBoardCardModel RestoreCardModel(Vector2Int cardIndex, int cardStackIndex, CommonCardInfo cardInfo)
        {
            GameBoardCardModel cardModel = _CreateCardModel(cardID: _cardModelList.Count, cardIndex, cardStackIndex);
            cardModel.UpdateCardInfo(cardInfo);
            ReadNeighbors(cardModel.Index, cardModel.StackIndex);
            AddNeighborsAsParent(cardModel);
            return cardModel;
        }

        public void ReorderCardID()
        {
            for (int i = 0; i < _cardModelList.Count; i++)
            {
                GameBoardCardModel cardModel = _cardModelList[i];
                cardModel.UpdateID(i);
            }
        }

        private void OnCardStackIndexClick(Vector2Int cardIndex, int cardStackIndex)
        {
            //Debug.Log($"OnCardStackIndexClick : {cardIndex}, {cardStackIndex}");
            List<GameBoardCardModel> cardStack = _cardModelStackDict[cardIndex];
            if (cardStack.Count > 0)
            {
                GameBoardCardModel cardModel = cardStack[cardStackIndex];
                SelectCardModel(cardModel);
            }
        }

        private void OnCardStackIndexChanged(int offset, Vector2Int cardIndex, int cardStackIndex)
        {
            List<GameBoardCardModel> cardStack = _cardModelStackDict[cardIndex];
            if (cardStack.Count > 0)
            {
                GameBoardCardModel cardModel = cardStack[cardStackIndex];

                if (offset > 0)
                {
                    // todo : Up & Down 사이 중복된 코드 정리 필요함
                    // Up
                    if (cardModel.ChildCount > 0)
                    {
                        GameBoardCardModel nextHigherChildModel = cardModel.GetChild(0);

                        _cardModelList.RemoveAt(cardModel.ID);
                        _cardModelList.Insert(nextHigherChildModel.ID, cardModel);
                        ReorderCardID();

                        if (cardModel.Index == nextHigherChildModel.Index)
                        {
                            cardStack.RemoveAt(cardStackIndex);
                            cardStack.Insert(nextHigherChildModel.StackIndex, cardModel);
                            ReorderCardStackIndex(cardStack);
                            onCardModelStackIndexSwaped?.Invoke(cardModel.Index, cardStackIndex, cardModel.StackIndex);
                        }

                        nextHigherChildModel.RemoveParent(cardModel);
                        cardModel.AddParent(nextHigherChildModel);

                        onCardModelDepthChanged?.Invoke(CardModelList);
                        onGameBoardChanged?.Invoke(CardModelList);

                        SelectCardModel(cardModel);
                        Debug.Log($"OnCardStackIndexChanged, parents: {cardModel.ParentsNumber}, children: {cardModel.ChildrenNumber}");
                    }
                }
                else
                {
                    // Down
                    if (cardModel.ParentCount > 0)
                    {
                        GameBoardCardModel nextLowerParentModel = cardModel.GetParent(0);

                        //Debug.Log($"Down : {cardModel.ID} -> {nextLowerParentModel.ID}");
                        _cardModelList.RemoveAt(cardModel.ID);
                        _cardModelList.Insert(nextLowerParentModel.ID, cardModel);
                        ReorderCardID();

                        if (cardModel.Index == nextLowerParentModel.Index)
                        {
                            cardStack.RemoveAt(cardStackIndex);
                            cardStack.Insert(nextLowerParentModel.StackIndex, cardModel);
                            ReorderCardStackIndex(cardStack);
                            onCardModelStackIndexSwaped?.Invoke(cardModel.Index, cardStackIndex, cardModel.StackIndex);
                        }

                        cardModel.RemoveParent(nextLowerParentModel);
                        nextLowerParentModel.AddParent(cardModel);

                        onCardModelDepthChanged?.Invoke(CardModelList);
                        onGameBoardChanged?.Invoke(CardModelList);

                        SelectCardModel(cardModel);
                        Debug.Log($"OnCardStackIndexChanged, parents: {cardModel.ParentsNumber}, children: {cardModel.ChildrenNumber}");
                    }
                }
            }
        }

        private void ReorderCardStackIndex(List<GameBoardCardModel> cardModelList)
        {
            int stackCount = cardModelList.Count;
            for (int i = 0; i < stackCount; i++)
            {
                GameBoardCardModel cardModel = cardModelList[i];
                cardModel.UpdateStackIndexAndCount(i, stackCount);
            }
        }

        private void NotifyCardModelChildrenChanged(GameBoardCardModel cardModel)
        {
            onCardModelChildrenChanged?.Invoke(cardModel);
        }

        private void OnCardInfoChanged(GameBoardCardModel cardModel)
        {
            if (cardModel.CardInfo.PrevSubCardType == SubCardType.UnTie
                || cardModel.CardInfo.SubCardType == SubCardType.UnTie)
            {
                UpdateWholeUntieCount();
            }
            else if (cardModel.CardInfo.PrevSubCardType == SubCardType.Tied
                     || cardModel.CardInfo.SubCardType == SubCardType.Tied)
            {
                UpdateUntieCount(cardModel);
            }

            onCardInfoChanged?.Invoke(cardModel);

            if (State.CurrState != GameBoardModelState.Restore)
            {
                onGameBoardChanged?.Invoke(CardModelList);
            }
        }

        public void UpdateWholeUntieCount()
        {
            _newUntieCountDict.Clear();
            foreach (SubCardTypeTiedColor tiedColor in Enum.GetValues(typeof(SubCardTypeTiedColor)))
            {
                _newUntieCountDict.Add(tiedColor, 0);
            }
            foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
            {
                foreach (GameBoardCardModel cardModel in cardModelStack)
                {
                    CommonCardInfo cardInfo = cardModel.CardInfo;
                    if (cardInfo.SubCardType == SubCardType.UnTie)
                    {
                        SubCardTypeTiedColor tiedColor = (SubCardTypeTiedColor)cardInfo.SubCardTypeOption;
                        _newUntieCountDict[tiedColor] += 1;
                    }
                }
            }

            bool needUpdate = false;
            if (_untieCountDict.Count != _newUntieCountDict.Count)
            {
                needUpdate = true;
            }
            foreach (KeyValuePair<SubCardTypeTiedColor, int> pair in _untieCountDict)
            {
                SubCardTypeTiedColor originKey = pair.Key;
                int originCount = pair.Value;
                if (_newUntieCountDict.TryGetValue(originKey, out int newValue) == false
                    || originCount != newValue)
                {
                    needUpdate = true;
                    break;
                }
            }

            if (needUpdate)
            {
                _untieCountDict.Clear();
                foreach (KeyValuePair<SubCardTypeTiedColor, int> newPair in _newUntieCountDict)
                {
                    //Debug.Log($"UpdateWholeUntieCount : {newPair.Key}, {newPair.Value}");
                    _untieCountDict.Add(newPair.Key, newPair.Value);
                }

                foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
                {
                    foreach (GameBoardCardModel cardModel in cardModelStack)
                    {
                        if (cardModel.CardInfo.SubCardType == SubCardType.Tied)
                        {
                            UpdateUntieCount(cardModel);
                        }
                    }
                }
            }
        }

        private void UpdateUntieCount(GameBoardCardModel cardModel)
        {
            CommonCardInfo cardInfo = cardModel.CardInfo;
            SubCardTypeTiedColor tiedColor = (SubCardTypeTiedColor)cardInfo.SubCardTypeOption;
            int originUntieCount = cardInfo.SubCardTypeOption2;
            _untieCountDict.TryGetValue(tiedColor, out int newUntieCount);
            if (originUntieCount != newUntieCount)
            {
                //Debug.Log($"UpdateUntieCount : {tiedColor}, {originUntieCount} -> {newUntieCount}");
                cardModel.UpdateCardInfo(
                    cardInfo.CardNumber,
                    cardInfo.CardColor,
                    cardInfo.CardFace,
                    cardInfo.CardType,
                    cardInfo.SubCardType,
                    cardInfo.SubCardTypeOption,
                    subCardTypeOption2: newUntieCount,
                    cardInfo.IsBonusLabel
                );
            }
        }

        public void NotifyGameBoardRestored()
        {
            onGameBoardRestored?.Invoke(CardModelList);
        }

        private void AddNeighborsAsParent(GameBoardCardModel cardModel)
        {
            //Debug.Log($"AddNeighborsAsParent : {cardModel.Index}, {cardModel.StackIndex}, {cardModel.CardInfo}");
            foreach (GameBoardCardModel neighborCardModel in _tempNeighborCardModelList)
            {
                //Debug.Log($"neighborCardModel : {neighborCardModel.Index}, {neighborCardModel.StackIndex}, {neighborCardModel.CardInfo}");
                cardModel.AddParent(neighborCardModel);
            }
        }

        private void ReadNeighbors(Vector2Int cardIndex, int stackIndex)
        {
            _tempNeighborCardModelList.Clear();
            for (int rowIndex = -1; rowIndex <= 1; rowIndex++)
            {
                for (int colIndex = -1; colIndex <= 1; colIndex++)
                {
                    Vector2Int neighborIndex = cardIndex + new Vector2Int(colIndex, rowIndex);
                    if (neighborIndex.x < 0
                        || neighborIndex.x > _lastColIndex
                        || neighborIndex.y < 0
                        || neighborIndex.y > _lastRowIndex)
                    {
                        continue;
                    }

                    List<GameBoardCardModel> neighborCardStack = _cardModelStackDict[neighborIndex];
                    if (neighborCardStack.Count > 0)
                    {
                        foreach (GameBoardCardModel neighborCard in neighborCardStack)
                        {
                            if (neighborCard.Index == cardIndex
                                && neighborCard.StackIndex == stackIndex) // 동일한 인덱스에 동일한 스택 인덱스일 때만 스킵
                            {
                                continue;
                            }

                            _tempNeighborCardModelList.Add(neighborCard);
                        }
                    }
                }
            }

            InitLog();
            foreach (GameBoardCardModel neighborCardModel in _tempNeighborCardModelList)
            {
                AppendCardLog(neighborCardModel);
            }
            //Debug.Log($"NeighborCardList : {_tempNeighborCardModelList.Count}");
            //if (_logBuilder.Length > 0)
            //{
            //    Debug.Log(_logBuilder.ToString());
            //}
        }

        private void InitLog()
        {
            _logBuilder.Length = 0;
        }

        private void AppendCardLog(GameBoardCardModel cardModel)
        {
            _logBuilder.Append("ID : ");
            _logBuilder.Append(cardModel.ID);
            _logBuilder.Append(", Index : ");
            _logBuilder.Append(cardModel.Index);
            _logBuilder.Append(", StackIndex : ");
            _logBuilder.Append(cardModel.StackIndex);
        }

        internal GameBoardCardModel RemoveCardModel(Vector2Int cardIndex, int cardStackIndex)
        {
            GameBoardCardModel removedCardModel;
            List<GameBoardCardModel> cardStack = _cardModelStackDict[cardIndex];
            if (cardStackIndex >= 0
                && cardStackIndex < cardStack.Count)
            {
                Debug.Log($"RemoveCardModel : {cardIndex}, {cardStackIndex}");
                GameBoardCardModel cardModel = cardStack[cardStackIndex];
                removedCardModel = cardModel;
                cardModel.RemoveFamily();

                _cardModelList.RemoveAt(cardModel.ID);
                ReorderCardID();
                _cardModelStackDict[cardIndex].RemoveAt(cardStackIndex);
                ReorderCardStackIndex(cardStack);

                if (cardModel.CardInfo.PrevSubCardType == SubCardType.UnTie
                || cardModel.CardInfo.SubCardType == SubCardType.UnTie)
                {
                    UpdateWholeUntieCount();
                }

                onCardModelRemoved?.Invoke(cardIndex, cardStackIndex);
                onCardModelDepthChanged?.Invoke(CardModelList);
                onGameBoardChanged?.Invoke(CardModelList);
            }
            else
            {
                removedCardModel = null;
                Debug.LogWarning($"cardStackIndex 값이 범위를 벗어났습니다. : {cardStackIndex}");
            }
            return removedCardModel;
        }

        internal void ClearCardModelList()
        {
            if (_cardModelList.Count > 0)
            {
                for (int i = _cardModelList.Count - 1; i >= 0; i--)
                {
                    GameBoardCardModel cardModel = _cardModelList[i];
                    cardModel.RemoveFamily();

                    _cardModelList.RemoveAt(i);
                }

                foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
                {
                    cardModelStack.Clear();
                }

                onGameBoardChanged?.Invoke(CardModelList);
            }

            _untieCountDict.Clear();
        }

        internal void SelectCardModel(GameBoardCardModel cardModel)
        {
            DeselectCardModel();

            _selectedCardModel = cardModel;
            Debug.Log($"SelectedCardModel : {_selectedCardModel.Index}, {_selectedCardModel.StackIndex}, {cardModel.CardInfo}");
            onCardModelSelected?.Invoke(_selectedCardModel, true);
        }

        internal void DeselectCardModel(bool notifyEvent = true)
        {
            if (_selectedCardModel != null)
            {
                GameBoardCardModel prevSelectedCardModel = _selectedCardModel;
                Debug.Log($"DeselectCardModel : {_selectedCardModel.Index}, {_selectedCardModel.StackIndex}");
                _selectedCardModel = null;

                if (notifyEvent)
                {
                    onCardModelSelected?.Invoke(prevSelectedCardModel, false);
                }
            }
        }

        public IGameBoardCardModel GetCardModel(Vector2Int cardIndex)
        {
            GameBoardCardModel result = null;
            if (_cardModelStackDict.TryGetValue(cardIndex, out List<GameBoardCardModel> cardStack))
            {
                if (cardStack.Count > 0)
                {
                    result = cardStack[cardStack.Count - 1];
                }
                else
                {
                    Debug.LogWarning($"CardModel 을 찾지 못했습니다 : {cardIndex}, {cardStack.Count}");
                }
            }
            else
            {
                Debug.LogWarning($"CardModel 을 찾지 못했습니다 : {cardIndex}");
            }
            return result;
        }

        internal IGameBoardCardModel GetCardModel(Vector2Int cardIndex, int cardStackIndex)
        {
            GameBoardCardModel result = null;
            if (_cardModelStackDict.TryGetValue(cardIndex, out List<GameBoardCardModel> cardStack))
            {
                if (cardStackIndex < cardStack.Count)
                {
                    result = cardStack[cardStackIndex];
                }
                else
                {
                    Debug.LogWarning($"CardModel 을 찾지 못했습니다 : {cardIndex}, {cardStackIndex}");
                }
            }
            else
            {
                Debug.LogWarning($"CardModel 을 찾지 못했습니다 : {cardIndex}");
            }
            return result;
        }

        public (bool isConsumed, SubCardType subCardType, int subCardTypeOption) ConsumeSubCardType(Vector2Int cardIndex, int cardStackIndex)
        {
            GameBoardCardModel cardModel = _cardModelStackDict[cardIndex][cardStackIndex];
            (bool isConsumed, SubCardType subCardType, int subCardTypeOption) = cardModel.ConsumeSubCardType();
            if (isConsumed)
            {
                onSubCardTypeConsumed?.Invoke(cardModel, subCardType, subCardTypeOption);
            }

            return (isConsumed, subCardType, subCardTypeOption);
        }

        internal void ConsumeAllSubCardType(SubCardType subCardType, int subCardTypeOption)
        {
            foreach (List<GameBoardCardModel> cardStack in _cardModelStackDict.Values)
            {
                foreach (GameBoardCardModel cardModel in cardStack)
                {
                    CommonCardInfo cardInfo = cardModel.CardInfo;
                    if (cardInfo.SubCardType == subCardType
                        && cardInfo.SubCardTypeOption == subCardTypeOption
                        && cardInfo.SubCardTypeOption2 <= 0)
                    {
                        (bool isConsumed, SubCardType _subCardType, int _subCardTypeOption) = cardModel.ConsumeSubCardType();
                        if (isConsumed == false)
                        {
                            continue;
                        }

                        onSubCardTypeConsumed?.Invoke(cardModel, subCardType, subCardTypeOption);
                    }
                }
            }
        }

        internal void DrawCardModel(Vector2Int cardIndex)
        {
            List<GameBoardCardModel> cardStack = _cardModelStackDict[cardIndex];
            if (cardStack.Count > 0)
            {
                int cardStackIndex = cardStack.Count - 1;
                GameBoardCardModel cardModel = cardStack[cardStackIndex];
                CommonCardInfo originCardInfo = cardModel.CardInfo;

                cardModel.RemoveFamily();

                onCardModelDrawn?.Invoke(cardIndex, cardStackIndex, originCardInfo);

                _cardModelStackDict[cardIndex].RemoveAt(cardStackIndex);
                _cardModelList.Remove(cardModel);
                onGameBoardDrawn?.Invoke(CardModelList);
            }
        }

        internal void RestoreSaveData(string data)
        {
            //Debug.Log($"RestoreData : {data}");
            if (string.IsNullOrEmpty(data) == false)
            {
                State.Set(GameBoardModelState.Restore);
                GameBoardSaveData saveData = JsonUtility.FromJson<GameBoardSaveData>(data);
                foreach (GameBoardCardSaveData cardSaveData in saveData.cardDataList)
                {
                    Vector2Int cardIndex = cardSaveData.Index;
                    int cardStackIndex = cardSaveData.StackIndex;
                    CommonCardInfo cardInfo = cardSaveData.CardInfo;
                    //Debug.Log($"RestoreSaveData: {cardIndex}, {cardStackIndex}, {cardInfo}");

                    RestoreCardModel(cardIndex, cardStackIndex, cardInfo);
                }
                State.Set(GameBoardModelState.None);

                NotifyGameBoardRestored();
            }
        }

        internal GameBoardCardModel MoveCardModel(
            Vector2Int originCardIndex,
            int originCardStackIndex,
            Vector2Int nextCardIndex,
            int nextCardStackIndex,
            int cardID,
            List<GameBoardCardModel> parentSet,
            List<GameBoardCardModel> childSet
        )
        {
            List<GameBoardCardModel> originCardStack = _cardModelStackDict[originCardIndex];
            GameBoardCardModel cardModel = originCardStack[originCardStackIndex];
            cardModel.RemoveFamily();
            cardModel.UpdateIndex(nextCardIndex);

            _cardModelList.RemoveAt(cardModel.ID);
            originCardStack.RemoveAt(originCardStackIndex);

            _cardModelList.Insert(cardID, cardModel);
            ReorderCardID();
            List<GameBoardCardModel> nextCardStack = _cardModelStackDict[nextCardIndex];
            nextCardStack.Insert(nextCardStackIndex, cardModel);
            ReorderCardStackIndex(originCardStack);
            ReorderCardStackIndex(nextCardStack);

            onCardModelMoved?.Invoke(originCardIndex, originCardStackIndex, nextCardIndex, nextCardStackIndex);

            foreach (GameBoardCardModel parentModel in parentSet)
            {
                cardModel.AddParent(parentModel);
            }
            foreach (GameBoardCardModel childModel in childSet)
            {
                childModel.AddParent(cardModel);
            }

            onCardModelDepthChanged?.Invoke(CardModelList);
            onGameBoardChanged?.Invoke(CardModelList);
            NotifyCardModelChildrenChanged(cardModel);

            return cardModel;
        }

        internal int GetCardModelCount(Func<CommonCardInfo, bool> _CheckCondition)
        {
            int count = 0;
            foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
            {
                foreach (GameBoardCardModel cardModel in cardModelStack)
                {
                    if (_CheckCondition(cardModel.CardInfo))
                    {
                        count += 1;
                    }
                }
            }
            return count;
        }

        internal void RemoveObstacles()
        {
            foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
            {
                foreach (GameBoardCardModel cardModel in cardModelStack)
                {
                    CommonCardInfo cardInfo = cardModel.CardInfo;
                    cardInfo.UpdateInfo(cardInfo.CardNumber, cardInfo.CardColor, cardInfo.CardFace, cardInfo.CardType, SubCardType.SubType_None, -1, -1, cardInfo.IsBonusLabel);

                    cardModel.UpdateCardInfo(cardInfo);
                }
            }
        }

        internal void FlipCardsFaceup()
        {
            foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
            {
                foreach (GameBoardCardModel cardModel in cardModelStack)
                {
                    CommonCardInfo cardInfo = cardModel.CardInfo;
                    cardInfo.UpdateInfo(cardInfo.CardNumber, cardInfo.CardColor, CardFace.Face_Up, cardInfo.CardType, cardInfo.SubCardType, -1, -1, cardInfo.IsBonusLabel);

                    cardModel.UpdateCardInfo(cardInfo);
                }
            }
        }

        internal IEnumerable<IGameBoardCardModel> GetCardModelList(Func<CommonCardInfo, bool> _CheckCondition)
        {
            _tutorialCardModelList.Clear();
            foreach (List<GameBoardCardModel> cardModelStack in _cardModelStackDict.Values)
            {
                foreach (GameBoardCardModel cardModel in cardModelStack)
                {
                    if (_CheckCondition(cardModel.CardInfo))
                    {
                        _tutorialCardModelList.Add(cardModel);
                    }
                }
            }
            return _tutorialCardModelList;
        }
    }
}