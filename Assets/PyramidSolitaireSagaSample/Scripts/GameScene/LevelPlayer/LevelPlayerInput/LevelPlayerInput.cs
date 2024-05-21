using PyramidSolitaireSagaSample.System;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.Input
{
    public interface IClickable
    {
        string SortingLayerName { get; }
        void OnClick();
    }

    public interface ILevelPlayerInputDisabler
    {
        event Action requestDisableInput;
        event Action requestEnableInput;
    }

    public interface ILevelPlayerInputLimiter
    {
        event Action<LevelPlayerInputLimitType> requestLimitInput;
        event Action requestUnlimitInput;
    }

    [Flags]
    public enum LevelPlayerInputLimitType
    {
        None = 0,
        UI = 1,
        Loot = 2,
    }

    public class LevelPlayerInput : MonoBehaviour, IGameInputSettter, IGameObjectFinderSetter
    {
        [SerializeField] private Camera mainCamera;

        public IGameInput GameInput { set; private get; }
        public Vector2 _latestPointerPosition { get; private set; }

        private IEnumerable<ILevelPlayerInputDisabler> _disablers;
        private IEnumerable<ILevelPlayerInputLimiter> _limiters;
        private LevelPlayerInputLimitType _currentLimit;

        private int _enableCount = 0;

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _disablers = finder.FindGameObjectOfType<ILevelPlayerInputDisabler>();
            _limiters = finder.FindGameObjectOfType<ILevelPlayerInputLimiter>();
        }

        private void OnEnable()
        {
            foreach (ILevelPlayerInputDisabler trigger in _disablers)
            {
                trigger.requestDisableInput += DisableInput;
                trigger.requestEnableInput += EnableInput;
            }
            foreach (ILevelPlayerInputLimiter limiter in _limiters)
            {
                limiter.requestLimitInput += LimitInput;
                limiter.requestUnlimitInput += UnlimitInput;
            }
            EnableInput();
        }

        private void OnDisable()
        {
            foreach (ILevelPlayerInputDisabler trigger in _disablers)
            {
                trigger.requestEnableInput -= EnableInput;
                trigger.requestDisableInput -= DisableInput;
            }
            foreach (ILevelPlayerInputLimiter limiter in _limiters)
            {
                limiter.requestLimitInput -= LimitInput;
                limiter.requestUnlimitInput -= UnlimitInput;
            }
            DisableInput();
        }

        public void DisableInput()
        {
            _enableCount -= 1;
            if (_enableCount == 0)
            {
                Debug.Log($"DisableInput : {_enableCount}");
                GameInput.onPointerMove -= OnPointerMove;
                GameInput.onLeftTap -= OnLeftTap;
            }
        }

        public void EnableInput()
        {
            _enableCount += 1;
            if (_enableCount == 1)
            {
                Debug.Log($"EnableInput : {_enableCount}");
                GameInput.onPointerMove += OnPointerMove;
                GameInput.onLeftTap += OnLeftTap;
            }
        }

        private void LimitInput(LevelPlayerInputLimitType limitType)
        {
            _currentLimit = limitType;
            Debug.Log($"LimitInput : {_currentLimit}");
        }

        private void UnlimitInput()
        {
            _currentLimit = LevelPlayerInputLimitType.None;
            Debug.Log($"UnlimitInput : {_currentLimit}");
        }

        private void OnLeftTap()
        {
            Ray ray = mainCamera.ScreenPointToRay(_latestPointerPosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                GameObject hitGameObject = hit.collider.gameObject;
                IClickable clickable = hitGameObject.GetComponentInParent<IClickable>();

                //Debug.Log($"OnLeftTap : {_currentLimit}, {clickable.SortingLayerName}");
                if (_currentLimit != LevelPlayerInputLimitType.None)
                {
                    Enum.TryParse(clickable.SortingLayerName, out LevelPlayerInputLimitType sortingLayerType);
                    if ((_currentLimit & sortingLayerType) > 0)
                    {
                        clickable?.OnClick();
                        //Debug.Log("OnLeftTap.Clicked");
                    }
                }
                else
                {
                    clickable?.OnClick();
                    //Debug.Log("OnLeftTap.Clicked");
                }
#if UNITY_EDITOR
                EditorGUIUtility.PingObject(hitGameObject);
#endif
            }
        }

        private void OnPointerMove(Vector2 pointerPosition)
        {
            _latestPointerPosition = pointerPosition;
        }
    }
}
