namespace PyramidSolitaireSagaSample
{
    public enum CardType
    {
        Type_None = 0,
        Gold = 1,
        Blue = 2,
    }

    public enum SubCardType
    {
        SubType_None = 0,
        Taped = 2,
        Lock = 3,
        Key = 4,
        Tied = 5,
        UnTie = 6
    }

    public enum SubCardTypeTiedColor
    {
        Green,
        Purple
    }

    public enum SubCardTypeLockColor
    {
        LockGrey = 0,
        LockRed,
        LockBlue,
        LockGreen,
        LockYellow,
        LockOrange,
        LockPurple
    }
}
