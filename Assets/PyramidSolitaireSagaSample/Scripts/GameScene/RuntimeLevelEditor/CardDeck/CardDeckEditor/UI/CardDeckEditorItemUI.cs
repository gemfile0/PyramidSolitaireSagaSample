using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDeck
{
    public class CardDeckItemInfo
    {
        public CardNumber CardNumber { get; private set; }
        public CardColor CardColor { get; private set; }

        public CardDeckItemInfo(CardNumber cardNumber, CardColor cardColor)
        {
            CardNumber = cardNumber;
            CardColor = cardColor;
        }

        public override string ToString()
        {
            return $"{CardNumber}, {CardColor}";
        }
    }

    public class CardDeckEditorItemUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _panelImage;
        [SerializeField] private TextMeshProUGUI _logText;
        [SerializeField] private Image _selectionImage;

        public RectTransform CachedRectTransform
        {
            get
            {
                if (_cachedRectTransform == null)
                {
                    _cachedRectTransform = GetComponent<RectTransform>();
                }
                return _cachedRectTransform;
            }
        }
        private RectTransform _cachedRectTransform;

        public CardDeckItemModel ItemModel { get; private set; }

        private int _itemIndex;
        private Action<int> _onItemClick;

        private void Start()
        {
            Deselect();
        }

        internal void UpdateUI(int itemIndex, Sprite itemSprite, Action<int> onItemClick)
        {
            _itemIndex = itemIndex;
            _onItemClick = onItemClick;

            _panelImage.sprite = itemSprite;
            _logText.text = (itemIndex + 1).ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onItemClick.Invoke(_itemIndex);
        }

        public void Select()
        {
            _selectionImage.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            _selectionImage.gameObject.SetActive(false);
        }
    }
}
