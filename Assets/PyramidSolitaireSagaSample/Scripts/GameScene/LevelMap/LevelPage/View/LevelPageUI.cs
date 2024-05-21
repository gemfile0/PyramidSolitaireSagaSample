using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.Helper.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelMap.LevelPage
{
    public class LevelPageUI : MonoBehaviour
    {
        [SerializeField] private RectTransform _levelButtonGroupRoot;
        [SerializeField] private LevelButtonGroup _levelButtonGroupPrefab;
        [SerializeField] private PageView _pageView;
        [SerializeField] private Pagenation _pagenation;

        public event Action<int> onLevelButtonClick;

        private GameObjectPool<LevelButtonGroup> _levelButtonGroupPool;
        private List<RectTransform> _levelButtonGroupTransformList;

        private void Awake()
        {
            _levelButtonGroupPool = new GameObjectPool<LevelButtonGroup>(
                _levelButtonGroupRoot,
                _levelButtonGroupPrefab.gameObject,
                defaultCapacity: 5
            );

            _levelButtonGroupTransformList = new List<RectTransform>();
        }

        private void OnEnable()
        {
            _pageView.onPageUpdated += ChangePagenationIndex;
            _pagenation.onPageIndexChanged += ChangePageViewIndex;
        }

        private void OnDisable()
        {
            _pageView.onPageUpdated -= ChangePagenationIndex;
            _pagenation.onPageIndexChanged -= ChangePageViewIndex;
        }

        private void ChangePagenationIndex(int value)
        {
            _pagenation.ChangeIndex(value);
        }

        private void ChangePageViewIndex(int value)
        {
            _pageView.ChangeIndex(value);
        }

        internal void UpdateUI(int levelButtonCount, int highestUnlockedLevelIndex)
        {
            Vector2 originAnchorMin = _levelButtonGroupRoot.anchorMin;
            Vector2 originAnchorMax = _levelButtonGroupRoot.anchorMax;
            Vector2 originPivot = _levelButtonGroupRoot.pivot;
            float originWidth = _levelButtonGroupRoot.rect.width;
            _levelButtonGroupRoot.anchorMin = new Vector2(0f, originAnchorMin.y);
            _levelButtonGroupRoot.anchorMax = new Vector2(0f, originAnchorMax.y);
            _levelButtonGroupRoot.pivot = new Vector2(0f, originPivot.y);
            _levelButtonGroupRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originWidth);

            Vector2 originSizeDelta = _levelButtonGroupRoot.sizeDelta;
            float buttonGroupWidth = originSizeDelta.x;

            int pageCount = (int)((levelButtonCount - 1) / 10f) + 1;
            _levelButtonGroupRoot.sizeDelta = new Vector2(buttonGroupWidth * pageCount, originSizeDelta.y);

            for (int i = 0; i < pageCount; i++)
            {
                int activeButtonCount = levelButtonCount >= 10 ?
                                        10 :
                                        levelButtonCount;

                levelButtonCount -= activeButtonCount;

                LevelButtonGroup levelButtonGroup = _levelButtonGroupPool.Get();
                levelButtonGroup.CachedTransform.SetParent(_levelButtonGroupRoot);
                levelButtonGroup.Init(i, activeButtonCount, highestUnlockedLevelIndex, OnLevelButtonClick);

                levelButtonGroup.UpdateSize(originSizeDelta);
                levelButtonGroup.CachedTransform.anchoredPosition = new Vector2(
                    buttonGroupWidth * i, 0
                );

                _levelButtonGroupTransformList.Add(levelButtonGroup.CachedTransform);
            }

            _pageView.Init(_levelButtonGroupTransformList);
            _pagenation.Init(pageCount);

            int pageIndex = highestUnlockedLevelIndex / 10;
            if (pageIndex >= pageCount)
            {
                Debug.LogWarning($"{pageIndex} 값이 존재하지 않아 수정합니다 : {(pageCount - 1)}");
                pageIndex = pageCount - 1;
            }
            _pageView.ChangeIndex(pageIndex);
            _pagenation.ChangeIndex(pageIndex);
        }

        public void SetButtonInteractable(bool value)
        {
            foreach (RectTransform transform in _levelButtonGroupTransformList)
            {
                var levelButtonGroup = transform.GetComponent<LevelButtonGroup>();
                levelButtonGroup.SetButtonInteractable(value);
            }
        }

        private void OnLevelButtonClick(int itemIndex)
        {
            onLevelButtonClick?.Invoke(itemIndex);
        }
    }
}
