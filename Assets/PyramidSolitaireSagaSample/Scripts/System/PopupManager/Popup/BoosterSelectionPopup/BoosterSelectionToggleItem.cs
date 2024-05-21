using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.System.Popup
{
    public class BoosterSelectionToggleItem : MonoBehaviour
    {
        [SerializeField] private GameBoosterType _boosterType;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private GameObject _checkboxObject;
        [SerializeField] private Toggle _toggle;

        public Toggle Toggle => _toggle;
        public GameBoosterType BoosterType => _boosterType;

        public void Init()
        {
            _toggle.SetIsOnWithoutNotify(false);
            _checkboxObject.SetActive(false);
        }

        public void UpdateCountText(int value)
        {
            _countText.text = value.ToString();
        }

        public void OnItemValueChanged(bool value)
        {
            _checkboxObject.SetActive(value);
        }
    }
}
