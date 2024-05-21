using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.BonusCards
{
    public class BonusCardsPointItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        internal void SetColor(Color color)
        {
            _spriteRenderer.color = color;
        }

        internal void UpdateSortingOrder(int value)
        {
            _spriteRenderer.sortingOrder = value;
        }
    }
}
