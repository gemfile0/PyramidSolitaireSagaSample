using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameItems
{
    public class GameItemUI : MonoBehaviour
    {
        [SerializeField] private GameItemType _itemType;
        [SerializeField] private TextMeshProUGUI _countText;

        public GameItemType ItemType => _itemType;

        internal void UpdateUI(int count)
        {
            _countText.text = count.ToString();
        }

        public void OnItemClick()
        {

        }
    }
}
