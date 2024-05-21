using UnityEngine;

namespace PyramidSolitaireSagaSample.GameCommon.UI
{
    public class HardLevelDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _normalLevelObject;
        [SerializeField] private GameObject _hardLevelObject;

        internal void UpdateHardLevelDisplay(GameDifficultyType gameDifficultyType)
        {
            bool isHardLevel = gameDifficultyType == GameDifficultyType.Hard;
            _hardLevelObject.SetActive(isHardLevel);
            _normalLevelObject.SetActive(isHardLevel == false);
        }
    }
}
