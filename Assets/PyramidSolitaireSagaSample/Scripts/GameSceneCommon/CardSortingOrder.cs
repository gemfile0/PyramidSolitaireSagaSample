namespace PyramidSolitaireSagaSample
{
    public enum CardSortingOrderType
    {
        BonusCardDeck = -1000,
        CardDeck = 0,
        JokerDeck = 1000,
        GameBoard = 2000,
        CardCollector = 3000 // baseValue
    }

    public static class CardSortingOrder
    {
        public const int StepValue = 10;

        public static int GetBaseValue(CardSortingOrderType type)
        {
            return (int)type;
        }

        public static int Calculate(CardSortingOrderType type, int index)
        {
            return GetBaseValue(type) + index * StepValue;
        }
    }
}
