using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class CardSortingPosition
    {
        private const int CardEndIndex = 51;

        internal static Vector3 CalculateAsDescending(int itemIndex, int itemCount, float cardPileGap)
        {
            float cardPileEnd = CardEndIndex * cardPileGap * -1;
            int lastIndex = itemCount - 1;
            //Debug.Log($"CalculateAsDescending : {cardPileEnd} + ({lastIndex} - {itemIndex}) * {cardPileGap}");
            return new Vector3(0, cardPileEnd + (lastIndex - itemIndex) * cardPileGap, 0);
        }

        internal static Vector3 CalculateAsAscending(int itemIndex, float cardPileGap)
        {
            float cardPileEnd = CardEndIndex * cardPileGap * -1;
            return new Vector3(0, cardPileEnd + itemIndex * cardPileGap, 0);
        }
    }
}
