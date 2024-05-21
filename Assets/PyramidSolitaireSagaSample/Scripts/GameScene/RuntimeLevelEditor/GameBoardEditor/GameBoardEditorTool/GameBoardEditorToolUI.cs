using PyramidSolitaireSagaSample.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard
{
    public class GameBoardEditorToolUI : MonoBehaviour
    {
        [SerializeField] private List<Toggle> _toolToggles;
        [SerializeField] private Button undoButton;
        [SerializeField] private GameObject undoTextObject;

        [SerializeField] private Button redoButton;
        [SerializeField] private GameObject redoTextObject;
        
        public event Action<int> onSelectedIndex;
        public event Action onUndoButtonClick;
        public event Action onRedoButtonClick;

        public void SetAsOff()
        {
            foreach (Toggle toggle in _toolToggles)
            {
                toggle.SetIsOnWithoutNotify(false);
            }
        }

        internal void SelectTool(GameBoardToolType toolType)
        {
            int selectedIndex = (int)toolType;
            _toolToggles[selectedIndex].isOn = true;
        }

        public void SwapTool(GameBoardToolType toolType)
        {
            int selectedIndex = (int)toolType;
            _toolToggles[selectedIndex].isOn = !_toolToggles[selectedIndex].isOn;
        }

        public void SetButtonInteractable(bool undoInteratable, bool redoInteratable)
        {
            undoButton.interactable = undoInteratable;
            undoTextObject.gameObject.SetActive(undoInteratable);

            redoButton.interactable = redoInteratable;
            redoTextObject.gameObject.SetActive(redoInteratable);
        }

        public void OnToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                int selectedIndex = -1;
                for (int i = 0; i < _toolToggles.Count; i++)
                {
                    Toggle toggle = _toolToggles[i];
                    if (toggle.isOn)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                onSelectedIndex?.Invoke(selectedIndex);
            }
            else
            {
                if (_toolToggles.All(toggle => !toggle.isOn))
                {
                    onSelectedIndex?.Invoke(-1);
                }
            }
        }

        public void OnUndoButtonClick()
        {
            onUndoButtonClick?.Invoke();
        }

        public void OnRedoButtonClick()
        {
            onRedoButtonClick?.Invoke();
        }
    }
}
