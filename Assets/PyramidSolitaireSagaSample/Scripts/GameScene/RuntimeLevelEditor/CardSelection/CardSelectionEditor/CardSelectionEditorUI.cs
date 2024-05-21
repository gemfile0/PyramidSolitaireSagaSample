using PyramidSolitaireSagaSample.Helper.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection
{
    public struct CardSelectionInfo
    {
        public CardNumber cardNumber;
        public CardColor cardColor;
        public CardFace cardFace;
        public CardType cardType;
        public SubCardType subCardType;
        public int subCardTypeOption;
        public int subCardTypeOption2;
        public bool isBonusLabel;

        public CardSelectionInfo(
            CardNumber cardNumber,
            CardColor cardColor,
            CardFace cardFace,
            CardType cardType,
            SubCardType subCardType,
            int subCardTypeOption,
            int subCardTypeOption2,
            bool isBonusLabel
        )
        {
            this.cardNumber = cardNumber;
            this.cardColor = cardColor;
            this.cardFace = cardFace;
            this.cardType = cardType;
            this.subCardType = subCardType;
            this.subCardTypeOption = subCardTypeOption;
            this.subCardTypeOption2 = subCardTypeOption2;
            this.isBonusLabel = isBonusLabel;
        }
    }

    public class CardSelectionEditorUI : MonoBehaviour, ILayoutRebuilderInvoker
    {
        [SerializeField] private GameObject _panelObject;
        [SerializeField] private TMP_Dropdown _cardNumberDropdown;
        [SerializeField] private TMP_Dropdown _cardColorDropdown;
        [SerializeField] private TMP_Dropdown _cardFaceDropdown;
        [SerializeField] private TMP_Dropdown _cardTypeDropdown;
        [SerializeField] private TMP_Dropdown _subCardTypeDropdown;
        [SerializeField] private TMP_Dropdown _subCardTypeOptionDropdown;
        [SerializeField] private Toggle _isBonusLabelToggle;

        public event Action<CardSelectionInfo> onCardSelectionInfoChanged;
        public event Action requestLayoutRebuild;

        private TMP_Dropdown[] _allDropdownList;
        private Dictionary<string, SubCardType> _subCardTypeDict;
        private Dictionary<SubCardType, int> _subCardTypeIndexDict;

        private void Awake()
        {
            _cardNumberDropdown.FillDropdownOptionValues<CardNumber>(50, 20);

            _cardColorDropdown.FillDropdownOptionValues<CardColor>(50, 20);

            _cardFaceDropdown.FillDropdownOptionValues<CardFace>(50, 20);

            _cardTypeDropdown.FillDropdownOptionValues<CardType>(50, 20);

            _subCardTypeDropdown.FillDropdownOptionValues<SubCardType>(50, 20);

            _allDropdownList = GetComponentsInChildren<TMP_Dropdown>();

            _subCardTypeDict = new();
            _subCardTypeIndexDict = new();
            int subCardTypeIndex = 0;
            foreach (SubCardType type in Enum.GetValues(typeof(SubCardType)))
            {
                _subCardTypeDict.Add(type.ToString(), type);
                _subCardTypeIndexDict.Add(type, subCardTypeIndex);
                subCardTypeIndex += 1;
            }
        }

        private void Start()
        {
            _panelObject.SetActive(false);
        }

        public void SetActive(
            bool panelActive,
            bool numberActive,
            bool colorActive,
            bool faceActive,
            bool typeActive,
            bool subCardTypeOptionActive,
            SubCardType subCardType
        )
        {
            _panelObject.SetActive(panelActive);
            _cardNumberDropdown.gameObject.SetActive(numberActive);
            _cardColorDropdown.gameObject.SetActive(colorActive);
            _cardFaceDropdown.gameObject.SetActive(faceActive);
            _cardTypeDropdown.gameObject.SetActive(typeActive);
            _subCardTypeDropdown.gameObject.SetActive(typeActive);

            UpdateSubCardTypeOptionActive(subCardTypeOptionActive);
            UpdateSubCardTypeOptionValues(subCardType);
        }

        public void UpdateUI(
            CardNumber cardNumber,
            CardColor cardColor,
            CardFace cardFace,
            CardType cardType,
            SubCardType subCardType,
            int subCardTypeOption,
            bool isBonusLabel
        )
        {
            _cardNumberDropdown.SetValueWithoutNotify((int)cardNumber);
            _cardColorDropdown.SetValueWithoutNotify((int)cardColor);
            _cardFaceDropdown.SetValueWithoutNotify((int)cardFace);
            _cardTypeDropdown.SetValueWithoutNotify((int)cardType);
            int subCardTypeIndex = _subCardTypeIndexDict[subCardType];
            _subCardTypeDropdown.SetValueWithoutNotify(subCardTypeIndex);

            Debug.Log($"UpdateUI : {subCardType}, {subCardTypeIndex}");
            if (IsSubCardTypeOptionActive(subCardType))
            {
                _subCardTypeOptionDropdown.SetValueWithoutNotify(subCardTypeOption);
            }

            _isBonusLabelToggle.SetIsOnWithoutNotify(isBonusLabel);

            RefreshAllDropdown();
        }

        public bool IsSubCardTypeOptionActive(SubCardType subCardType)
        {
            return IsSubCardTypeLock(subCardType) || IsSubCardTypeTied(subCardType);
        }

        private bool IsSubCardTypeLock(SubCardType subCardType)
        {
            return subCardType == SubCardType.Lock || subCardType == SubCardType.Key;
        }

        private bool IsSubCardTypeTied(SubCardType subCardType)
        {
            return subCardType == SubCardType.Tied || subCardType == SubCardType.UnTie;
        }

        public void ResetUI()
        {
            foreach (TMP_Dropdown dropdown in _allDropdownList)
            {
                dropdown.SetValueWithoutNotify(0);
            }
            RefreshAllDropdown();
        }

        private void RefreshAllDropdown()
        {
            foreach (TMP_Dropdown dropdown in _allDropdownList)
            {
                dropdown.RefreshShownValue();
            }
        }

        public void OnToggleValueChanged(bool isOn)
        {
            OnCardSelectionInfoChanged();
        }

        public void OnDropdownValueChanged(int index)
        {
            OnCardSelectionInfoChanged();
        }

        private void OnCardSelectionInfoChanged()
        {
            string valueText = _subCardTypeDropdown.options[_subCardTypeDropdown.value].text;
            SubCardType subCardType = _subCardTypeDict[valueText];
            bool isSubCardTypeOptionActive = IsSubCardTypeOptionActive(subCardType);
            UpdateSubCardTypeOptionActive(isSubCardTypeOptionActive);
            UpdateSubCardTypeOptionValues(subCardType);

            onCardSelectionInfoChanged?.Invoke(
                new CardSelectionInfo(
                    (CardNumber)_cardNumberDropdown.value,
                    (CardColor)_cardColorDropdown.value,
                    (CardFace)_cardFaceDropdown.value,
                    (CardType)_cardTypeDropdown.value,
                    subCardType,
                    subCardTypeOption: isSubCardTypeOptionActive ? _subCardTypeOptionDropdown.value : -1,
                    subCardTypeOption2: -1,
                    isBonusLabel: _isBonusLabelToggle.isOn
                )
            );
        }

        private void UpdateSubCardTypeOptionActive(bool subCardTypeOptionActive)
        {
            _subCardTypeOptionDropdown.gameObject.SetActive(subCardTypeOptionActive);
            requestLayoutRebuild.Invoke();
        }

        private void UpdateSubCardTypeOptionValues(SubCardType subCardType)
        {
            if (IsSubCardTypeLock(subCardType))
            {
                _subCardTypeOptionDropdown.FillDropdownOptionValues<SubCardTypeLockColor>(50, 20);
            }
            else if (IsSubCardTypeTied(subCardType))
            {
                _subCardTypeOptionDropdown.FillDropdownOptionValues<SubCardTypeTiedColor>(50, 20);
            }
        }
    }
}
