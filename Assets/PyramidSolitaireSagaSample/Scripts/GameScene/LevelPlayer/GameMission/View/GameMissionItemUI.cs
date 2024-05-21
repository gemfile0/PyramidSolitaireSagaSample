using PyramidSolitaireSagaSample.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    public class GameMissionItemUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TextMeshProUGUI _countText;

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

        internal void UpdateUI(GameMissionSkin gameMissionSkin, bool isCompleted, bool isCountTextVisible, string countStr)
        {
            _iconImage.sprite = gameMissionSkin.IconSprite;
            _iconImage.color = gameMissionSkin.IconColor;
            _toggle.isOn = isCompleted;

            _countText.text = countStr;
            _countText.gameObject.SetActive(isCountTextVisible);
        }
    }
}
