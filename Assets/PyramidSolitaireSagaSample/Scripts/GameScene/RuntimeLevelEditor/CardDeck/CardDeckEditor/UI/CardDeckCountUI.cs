using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDeck
{
    public class CardDeckCountUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _inputField;

        private int _latestCount = -1;

        public void UpdateUI(int nextCount)
        {
            if (_latestCount != nextCount)
            {
                _latestCount = nextCount;
                _inputField.SetTextWithoutNotify(nextCount.ToString());
            }
        }

        public void RevertAsLatestCount()
        {
            _inputField.SetTextWithoutNotify(_latestCount.ToString());
        }
    }
}
