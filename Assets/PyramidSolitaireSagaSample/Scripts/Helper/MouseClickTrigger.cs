using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class MouseClickTrigger : MonoBehaviour
    {
        private Action _onMouseClick;

        public void Init(Action onMouseDown)
        {
            _onMouseClick = onMouseDown;
        }

        private void OnMouseUpAsButton()
        {
            _onMouseClick?.Invoke();
        }
    }
}
