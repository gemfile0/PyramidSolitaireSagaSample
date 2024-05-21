using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelMap.LevelPage
{
    public class LevelButtonGroup : MonoBehaviour
    {
        [SerializeField] private List<LevelButton> _levelButtonList;

        public RectTransform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = GetComponent<RectTransform>();
                }
                return _cachedTransform;
            }
        }
        private RectTransform _cachedTransform;

        private int _pageIndex;
        private StringBuilder _levelTextBuilder;

        internal void Init(int pageIndex, int activeButtonCount, int highestUnlockedLevel, Action<int> onLevelButtonClick)
        {
            //Debug.Log($"Init : {pageIndex}, {activeButtonCount}, {highestUnlockedLevel}");
            _pageIndex = pageIndex;

            _levelTextBuilder = new StringBuilder();
            for (int i = 0; i < _levelButtonList.Count; i++)
            {
                int itemIndex = _pageIndex * 10 + i;
                LevelButton levelButton = _levelButtonList[i];
                levelButton.Init(itemIndex, onLevelButtonClick);
                levelButton.UpdateMarkerImageActive(i < activeButtonCount && itemIndex == highestUnlockedLevel);
                levelButton.UpdateButtonInteractable(i < activeButtonCount);

                _levelTextBuilder.Length = 0;
                _levelTextBuilder.Append("Level ");
                _levelTextBuilder.Append(itemIndex + 1);
                levelButton.UpdateButtonText(_levelTextBuilder.ToString());
                levelButton.UpdateButtonTextActive(i < activeButtonCount);
            }
        }

        internal void UpdateSize(Vector2 size)
        {
            _cachedTransform.sizeDelta = size;
        }

        internal void SetButtonInteractable(bool value)
        {
            foreach (LevelButton levelButton in _levelButtonList)
            {
                levelButton.UpdateButtonInteractable(value);
            }
        }
    }
}
