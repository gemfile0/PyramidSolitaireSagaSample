using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.System;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public class GameBoardEditorToolIcon
    {
        public GameBoardToolType toolType;
        public Sprite selectedIcon;
        public Sprite deselectedIcon;
    }

    public class GameBoardEditorTool : MonoBehaviour,
                                       IGameInputSettter
    {
        [Header("Data")]
        [SerializeField] private GameBoardData _gameBoardData;

        [Header("Presenter")]
        [SerializeField] private GameBoardEditorPresenter _gameBoardEditorPresenter;

        [Header("View")]
        [SerializeField] private GameBoardEditorToolUI _gameBoardEditorToolUI;

        [SerializeField] private string _undoKey = "z";
        [SerializeField] private string _redoKey = "y";

        public IGameInput GameInput { set; private get; }

        private GameBoardToolType _prevToolType;
        private GameBoardToolType _currentToolType;

        private Dictionary<GameBoardToolType, Func<IGameBoardToolCommand>> _commandFactory;
        private Stack<IGameBoardToolCommand> undoStack = new Stack<IGameBoardToolCommand>();
        private Stack<IGameBoardToolCommand> redoStack = new Stack<IGameBoardToolCommand>();

        private Dictionary<GameBoardToolType, GameBoardToolData> _toolDataDict;
        private Coroutine listenCoroutine;

        private void Awake()
        {
            _commandFactory = new Dictionary<GameBoardToolType, Func<IGameBoardToolCommand>>()
            {
                { GameBoardToolType.CardStack, () => new StackCardCommand(_gameBoardEditorPresenter) },
                { GameBoardToolType.CardBrush, () => new PlaceCardCommand(_gameBoardEditorPresenter) },
                { GameBoardToolType.CardMover, () => new MoveCardCommand(_gameBoardEditorPresenter) },
                { GameBoardToolType.CardEraser, () => new EraseCardCommand(_gameBoardEditorPresenter) },
            };

            _currentToolType = GameBoardToolType.None;

            _toolDataDict = new Dictionary<GameBoardToolType, GameBoardToolData>();
            foreach (GameBoardToolData toolData in _gameBoardData.ToolDataList)
            {
                if (toolData.Type == GameBoardToolType.None)
                {
                    continue;
                }

                _toolDataDict[toolData.Type] = toolData;
            }

            UpdateButtonInteractable();
        }

        private void OnEnable()
        {
            GameInput.onSwapTool += SwapToolUI;
            GameInput.onHistoryAction += DoHistoryAction;
            GameInput.onActionMapChanged += DeselectTool;

            _gameBoardEditorToolUI.onSelectedIndex += SwapTool;
            _gameBoardEditorToolUI.onUndoButtonClick += UndoCommand;
            _gameBoardEditorToolUI.onRedoButtonClick += RedoCommand;
        }

        private void OnDisable()
        {
            GameInput.onSwapTool -= SwapToolUI;
            GameInput.onHistoryAction -= DoHistoryAction;
            GameInput.onActionMapChanged -= DeselectTool;

            _gameBoardEditorToolUI.onSelectedIndex -= SwapTool;
            _gameBoardEditorToolUI.onUndoButtonClick -= UndoCommand;
            _gameBoardEditorToolUI.onRedoButtonClick -= RedoCommand;
        }

        private void SwapToolUI(string keyboardInput)
        {
            GameBoardToolType toolType =
                keyboardInput == "a" ? GameBoardToolType.CardStack :
                keyboardInput == "q" ? GameBoardToolType.CardBrush :
                keyboardInput == "w" ? GameBoardToolType.CardMover :
                keyboardInput == "e" ? GameBoardToolType.CardEraser :
                GameBoardToolType.None;

            _gameBoardEditorToolUI.SwapTool(toolType);
        }

        private void DoHistoryAction(string keyboardInput)
        {
            if (keyboardInput == _undoKey)
            {
                UndoCommand();
            }
            else if (keyboardInput == _redoKey)
            {
                RedoCommand();
            }
        }

        public void SelectDefaultTool()
        {
            _gameBoardEditorToolUI.SelectTool(GameBoardToolType.CardBrush);
        }

        private void SwapTool(int index)
        {
            OnSwapTool((GameBoardToolType)index);
        }

        private void UndoCommand()
        {
            if (undoStack.Count > 0)
            {
                IGameBoardToolCommand toolCommand = undoStack.Pop();
                toolCommand.Undo();

                redoStack.Push(toolCommand);

                UpdateButtonInteractable();
            }
        }

        private void RedoCommand()
        {
            if (redoStack.Count > 0)
            {
                IGameBoardToolCommand toolCommand = redoStack.Pop();
                toolCommand.Redo();

                undoStack.Push(toolCommand);

                UpdateButtonInteractable();
            }
        }

        public void OnSwapTool(GameBoardToolType nextToolType)
        {
            if (nextToolType != _currentToolType)
            {
                if (_commandFactory.ContainsKey(_currentToolType))
                {
                    UnlistenToGameInput();
                    _gameBoardEditorPresenter.HideHighlight();
                }

                _prevToolType = _currentToolType;
                _currentToolType = nextToolType;

                if (_commandFactory.ContainsKey(nextToolType))
                {
                    ListenToGameInput();

                    _toolDataDict.TryGetValue(_currentToolType, out GameBoardToolData toolData);
                    Color color = (toolData != null) ? toolData.highlightColor : Color.black;

                    _gameBoardEditorPresenter.ShowHighlight(color);
                }
            }
        }

        private void OnLeftTap()
        {
            if (_gameBoardEditorPresenter.IsHighlightOn)
            {
                if (_commandFactory.TryGetValue(_currentToolType, out Func<IGameBoardToolCommand> ToolCommandFunc))
                {
                    IGameBoardToolCommand toolCommand = ToolCommandFunc();
                    toolCommand.LeftTap();
                    if (toolCommand.CardModel != null)
                    {
                        undoStack.Push(toolCommand);
                        redoStack.Clear();
                        UpdateButtonInteractable();
                    }

                    if (_currentToolType == GameBoardToolType.CardStack)
                    {
                        _gameBoardEditorToolUI.SwapTool(_prevToolType);
                    }
                }
            }
        }

        private void OnRightClick()
        {
            if (_gameBoardEditorPresenter.IsHighlightOn)
            {
                if (_commandFactory.TryGetValue(_currentToolType, out Func<IGameBoardToolCommand> ToolCommandFunc))
                {
                    IGameBoardToolCommand toolCommand = ToolCommandFunc();
                    toolCommand.RightClick();
                }
            }
        }

        private void UpdateButtonInteractable()
        {
            //Debug.Log($"UpdateButtonInteractable : {undoStack.Count}, {redoStack.Count}");
            _gameBoardEditorToolUI.SetButtonInteractable(undoStack.Count > 0, redoStack.Count > 0);
        }

        private void ListenToGameInput()
        {
            if (listenCoroutine != null)
            {
                StopCoroutine(listenCoroutine);
            }
            listenCoroutine = StartCoroutine(ListenToGameInputCoroutine());
        }

        private IEnumerator ListenToGameInputCoroutine()
        {
            yield return null;
            GameInput.onPointerMove += _gameBoardEditorPresenter.MoveHighlight;
            GameInput.onLeftTap += OnLeftTap;
            GameInput.onRightClick += OnRightClick;
        }

        public void UnlistenToGameInput()
        {
            GameInput.onPointerMove -= _gameBoardEditorPresenter.MoveHighlight;
            GameInput.onLeftTap -= OnLeftTap;
            GameInput.onRightClick -= OnRightClick;
        }

        public void DeselectTool()
        {
            OnSwapTool(GameBoardToolType.None);
            _gameBoardEditorToolUI.SetAsOff();
        }

        internal void ClearHistoryAction()
        {
            undoStack.Clear();
            redoStack.Clear();
            UpdateButtonInteractable();
        }

        internal void OnCardDeckPanelActive(bool value)
        {
            if (value == false)
            {
                SelectDefaultTool();
            }
        }
    }
}
