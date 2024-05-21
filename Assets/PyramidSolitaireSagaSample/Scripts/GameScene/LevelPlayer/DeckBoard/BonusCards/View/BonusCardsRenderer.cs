using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCards
{
    public class BonusCardsRenderer : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> backGaugeRendrereList;
        [SerializeField] private List<SpriteRenderer> frontGaugeRendrereList;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private Ease fadeEase = Ease.InOutExpo;

        private Color _originColor;

        private void Awake()
        {
            _originColor = frontGaugeRendrereList[0].color;
        }

        internal void UpdateRenderer(int bonusCardsCount)
        {
            for (int i = 0; i < frontGaugeRendrereList.Count; i++)
            {
                bool isVisible = i < bonusCardsCount;
                SpriteRenderer frontGaugeRenderer = frontGaugeRendrereList[i];
                frontGaugeRenderer.color = new Color(_originColor.r, _originColor.g, _originColor.b, 1f);
                frontGaugeRenderer.gameObject.SetActive(isVisible);
            }
        }

        public void FadeOutFrontGauge()
        {
            for (int i = 0; i < frontGaugeRendrereList.Count; i++)
            {
                SpriteRenderer frontGaugeRenderer = frontGaugeRendrereList[i];
                Color targetColor = Color.white;
                targetColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
                frontGaugeRenderer.DOColor(targetColor, fadeDuration)
                                  .SetEase(fadeEase);
            }
        }
    }
}
