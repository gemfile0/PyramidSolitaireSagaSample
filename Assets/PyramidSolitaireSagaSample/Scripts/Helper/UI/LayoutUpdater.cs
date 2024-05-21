using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.Helper.UI
{
    [ExecuteAlways]
    public class LayoutUpdater : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvasTransform;
        [SerializeField] private LayoutElement _fixedLayoutElement;
        [SerializeField] private List<LayoutElement> _flexibleLayoutElementList;
        [SerializeField] private float _fixedWidth = 850f;

        private float _latestWidth;
        private float _latestHeight;
        private float _latestFixedWidth;

        private void Start()
        {
            UpdateLayout();
        }

        private void Update()
        {
            if (_canvasTransform == null || _fixedLayoutElement == null || _flexibleLayoutElementList == null)
            {
                return;
            }

            Rect canvasRect = _canvasTransform.rect;
            if (canvasRect.width != _latestWidth
                || canvasRect.height != _latestHeight
                || _fixedWidth != _latestFixedWidth)
            {
                _latestWidth = canvasRect.width;
                _latestHeight = canvasRect.height;
                _latestFixedWidth = _fixedWidth;
                UpdateLayout();
            }
        }

        private void UpdateLayout()
        {
            _fixedLayoutElement.minWidth = _fixedWidth;
            _fixedLayoutElement.preferredWidth = _fixedWidth;

            float totalFlexibleWidth = _canvasTransform.rect.width - _fixedWidth;
            float flexibleWidthPerUI = totalFlexibleWidth / _flexibleLayoutElementList.Count;
            //Debug.Log($"UpdateLayout 1 : {totalFlexibleWidth} = {_canvasTransform.rect.width} - {_fixedWidth}");
            //Debug.Log($"UpdateLayout 2 : {flexibleWidthPerUI} = {totalFlexibleWidth} / {_flexibleLayoutElementList.Count}");

            foreach (LayoutElement layoutElement in _flexibleLayoutElementList)
            {
                layoutElement.minWidth = flexibleWidthPerUI;
                layoutElement.preferredWidth = flexibleWidthPerUI;
            }
        }
    }
}
