using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.LevelPlayerMenu
{
    public class LevelPlayerMenuUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _levelText;

        public event Action onBackToPrevSceneClick;
        public event Action onReplayCurrentLevelClick;

        private StringBuilder _textBuilder;

        private void Awake()
        {
            _textBuilder = new StringBuilder();
        }

        public void UpdateLevelUI(int level)
        {
            _textBuilder.Length = 0;
            _textBuilder.Append("Level ");
            _textBuilder.Append(level);

            _levelText.text = _textBuilder.ToString();
        }

        public void OnBackToPrevSceneClick()
        {
            onBackToPrevSceneClick?.Invoke();
        }

        public void OnReplayCurrentLevelClick()
        {
            onReplayCurrentLevelClick?.Invoke();
        }
    }
}
