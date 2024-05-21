using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameSetting
{
    public class GameSettingUI : MonoBehaviour
    {
        public event Action onSettingButtonClick;

        public void OnSettingButtonClick()
        {
            onSettingButtonClick?.Invoke();
        }
    }
}
