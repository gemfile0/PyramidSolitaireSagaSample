using PyramidSolitaireSagaSample.Helper;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class GameBoostersUI : MonoBehaviour
    {
        [SerializeField] private AnimatorParser _hideAnimatorParser;
        [SerializeField] private AnimatorParser _showAnimatorParser;
        [SerializeField] private TextMeshProUGUI _boosterNameText;

        private StringBuilder _boosterNameBuilder;

        private void Awake()
        {
            _boosterNameBuilder = new StringBuilder();
            _boosterNameText.text = "";
        }

        internal IEnumerator HideScreen(GameBoosterType boosterType)
        {
            _boosterNameText.text = BoosterTypeAsString(boosterType);

            _hideAnimatorParser.SetTrigger();
            yield return _hideAnimatorParser.WaitForDuration();
        }

        internal IEnumerator ShowScreen()
        {
            _showAnimatorParser.SetTrigger();
            yield return _showAnimatorParser.WaitForDuration();
        }

        private string BoosterTypeAsString(GameBoosterType boosterType)
        {
            string boosterTypeStr = boosterType.ToString();

            _boosterNameBuilder.Length = 0;
            _boosterNameBuilder.Append(boosterTypeStr[0]);

            for (int i = 1; i < boosterTypeStr.Length; i++)
            {
                if (char.IsUpper(boosterTypeStr[i]))
                {
                    _boosterNameBuilder.Append(' ');
                }
                _boosterNameBuilder.Append(boosterTypeStr[i]);
            }

            return _boosterNameBuilder.ToString();
        }
    }
}
