using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.LevelMap.LevelPage
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Button _levelButton;
        [SerializeField] private TextMeshProUGUI _levelText;
        [FormerlySerializedAs("_markerObject")]
        [SerializeField] private GameObject _markerImageObject;

        private int _itemIndex;
        private Action<int> _onButtonClick;

        public void Init(int itemIndex, Action<int> onButtonClick)
        {
            _itemIndex = itemIndex;
            _onButtonClick = onButtonClick;
        }

        public void OnButtonClick()
        {
            _onButtonClick?.Invoke(_itemIndex);
        }

        internal void UpdateButtonInteractable(bool value)
        {
            _levelButton.interactable = value;
        }

        internal void UpdateButtonText(string value)
        {
            _levelText.text = value;
        }

        public void UpdateMarkerImageActive(bool value)
        {
            _markerImageObject.SetActive(value);
        }

        internal void UpdateButtonTextActive(bool value)
        {
            _levelText.gameObject.SetActive(value);
        }
    }
}
