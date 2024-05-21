using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PyramidSolitaireSagaSample.System
{
    public interface IGameInputActionMapTrigger
    {
        event Action<string> onEnableActionMap;
        event Action onRevertActionMap;
    }

    public interface IGameInput
    {
        event Action<Vector2> onPointerMove;
        event Action onLeftTap;
        event Action onRightClick;
        event Action<string> onSwapTool;
        event Action<string> onHistoryAction;
        event Action onActionMapChanged;
    }

    public interface IGameInputSettter
    {
        IGameInput GameInput { set; }
    }

    public class GameInput : MonoBehaviour, IGameObjectFinderSetter, IGameInput
    {
        [SerializeField] private PlayerInput _input;

        public event Action<Vector2> onPointerMove;
        public event Action onLeftTap;
        public event Action onRightClick;
        public event Action<string> onSwapTool;
        public event Action<string> onHistoryAction;
        public event Action onActionMapChanged;

        private InputActionMap _prevActionMap;
        private IEnumerable<IGameInputActionMapTrigger> _gameInputTriggers;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _gameInputTriggers = finder.FindGameObjectOfType<IGameInputActionMapTrigger>();
            foreach (var gameInputSetter in finder.FindGameObjectOfType<IGameInputSettter>())
            {
                gameInputSetter.GameInput = this;
            }
        }

        private void OnEnable()
        {
            foreach (IGameInputActionMapTrigger trigger in _gameInputTriggers)
            {
                trigger.onEnableActionMap += EnableActionMap;
                trigger.onRevertActionMap += RevertActionMap;
            }

            InputActionAsset inputActions = _input.actions;
            inputActions["GameBoard/SwapTool"].performed += NotifySwapTool;
            inputActions["GameBoard/HistoryButton"].performed += NotifyHistoryAction;
        }

        private void OnDisable()
        {
            foreach (IGameInputActionMapTrigger gameInputTrigger in _gameInputTriggers)
            {
                gameInputTrigger.onEnableActionMap -= EnableActionMap;
                gameInputTrigger.onRevertActionMap -= RevertActionMap;
            }

            InputActionAsset inputActions = _input.actions;
            inputActions["GameBoard/SwapTool"].performed -= NotifySwapTool;
            inputActions["GameBoard/HistoryButton"].performed -= NotifyHistoryAction;
        }

        public void NotifySwapTool(InputAction.CallbackContext context)
        {
            onSwapTool?.Invoke(context.control.name);
        }

        public void NotifyHistoryAction(InputAction.CallbackContext context)
        {
            onHistoryAction?.Invoke(context.control.name);
        }

        private void Update()
        {
            Vector2 inputPosition = Vector2.zero;

            // 모바일 기기에서의 터치 입력 확인
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                inputPosition = touch.position;

                switch (touch.phase)
                {
                    case UnityEngine.TouchPhase.Began:
                    case UnityEngine.TouchPhase.Moved:
                        onPointerMove?.Invoke(inputPosition);
                        break;
                    case UnityEngine.TouchPhase.Ended:
                        onLeftTap?.Invoke();
                        break;
                }
            }
            // PC에서의 마우스 입력 처리
            else if (Mouse.current != null)
            {
                inputPosition = Mouse.current.position.ReadValue();
                onPointerMove?.Invoke(inputPosition);

                if (Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    onRightClick?.Invoke();
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    onLeftTap?.Invoke();
                }
            }
        }

        public void RevertActionMap()
        {
            if (_prevActionMap != null)
            {
                _prevActionMap.Enable();
                _prevActionMap = null;
            }
        }

        public void EnableActionMap(string mapName)
        {
            _prevActionMap = _input.currentActionMap;
            _input.SwitchCurrentActionMap(mapName);

            onActionMapChanged?.Invoke();
        }
    }
}
