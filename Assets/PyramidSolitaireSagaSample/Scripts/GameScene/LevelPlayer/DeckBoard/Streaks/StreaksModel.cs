using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.Streaks
{
    public class StreaksModel : MonoBehaviour
    {
        public event Action<int> onStreaksChanged;

        private int _streaks = -1;

        internal void UpdateStreaks(int nextStreaks)
        {
            if (nextStreaks != _streaks)
            {
                _streaks = nextStreaks;
                onStreaksChanged?.Invoke(_streaks);
            }
        }
    }
}
