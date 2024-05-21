using PyramidSolitaireSagaSample.GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PyramidSolitaireSagaSample
{
    public class MissionPopupItemUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;

        public void UpdateUI(GameMissionSkin gameMissionSkin, string missionCountStr)
        {
            _iconImage.sprite = gameMissionSkin.IconSprite;
            _iconImage.color = gameMissionSkin.IconColor;
            _countText.text = missionCountStr;
        }
    }
}
