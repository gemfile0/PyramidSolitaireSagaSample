using PyramidSolitaireSagaSample.Helper.UI;
using System;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class SettingsPopup : BasePopup
    {
        [SerializeField] private TextMeshProUGUI _sfxValueText;
        [SerializeField] private TextMeshProUGUI _bgmValueText;
        [SerializeField] private TMP_Dropdown _languageDropdown;

        private Action<float> _onBgmValueChanged;
        private Action<float> _onSfxValueChanged;
        private Action<GameLanguage> _onLanguageValueChanged;
        private Action _onQuitClick;

        public void Init(
            Action<float> onBgmValueChanged, 
            Action<float> onSfxValueChanged, 
            Action<GameLanguage> onLanguageValueChanged, 
            Action onQuitClick
        )
        {
            _onBgmValueChanged = onBgmValueChanged;
            _onSfxValueChanged = onSfxValueChanged;
            _onLanguageValueChanged = onLanguageValueChanged;
            _onQuitClick = onQuitClick;

            _languageDropdown.FillDropdownOptionValues<GameLanguage>(50, 20);
        }

        public void BgmValueChanged(float value)
        {
            _bgmValueText.text = value.ToString();
            _onBgmValueChanged?.Invoke(value);
        }

        public void SfxValueChanged(float value)
        {
            _sfxValueText.text = value.ToString();
            _onSfxValueChanged?.Invoke(value);
        }

        public void LanguageValueChanged(int index)
        {
            _onLanguageValueChanged?.Invoke((GameLanguage)_languageDropdown.value);
        }

        public void QuitClick()
        {
            _onQuitClick?.Invoke();
        }

        public override void Close()
        {
            base.Close();

            _onBgmValueChanged = null;
            _onSfxValueChanged = null;
            _onLanguageValueChanged = null;
            _onQuitClick = null;
        }
    }
}
