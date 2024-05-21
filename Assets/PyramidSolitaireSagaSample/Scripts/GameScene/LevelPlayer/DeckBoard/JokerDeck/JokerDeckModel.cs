using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.JokerDeck
{
    public class JokerDeckModel : MonoBehaviour
    {
        public event Action<int, int> onJokerCountUpdated;
        public event Action<int> onJokerCountRestored;
        public event Action onItemDrawn;

        public int JokerCount { get; private set; }

        public void RestoreJokerCount(int count)
        {
            UpdateJokerCountWithoutNotify(count);
            onJokerCountRestored?.Invoke(JokerCount);
        }

        public void AddJokerCount(int addingCount)
        {
            UpdateJokerCountWithoutNotify(JokerCount + addingCount);
            onJokerCountUpdated?.Invoke(JokerCount, addingCount);
        }

        private void UpdateJokerCountWithoutNotify(int count)
        {
            JokerCount = count;
        }

        internal void DrawPeekItem()
        {
            onItemDrawn?.Invoke();

            int nextJokerCount = JokerCount - 1;
            if (nextJokerCount < 0)
            {
                nextJokerCount = 0;
            }
            UpdateJokerCountWithoutNotify(nextJokerCount);
            onJokerCountUpdated?.Invoke(JokerCount, -1);
        }
    }
}
