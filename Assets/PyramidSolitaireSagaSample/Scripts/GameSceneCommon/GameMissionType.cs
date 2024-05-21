using System;

namespace PyramidSolitaireSagaSample
{
    public enum GameMissionType
    {
        GoldCard,
        BlueCard,
        Streaks,
    }

    public enum GameMissionTypeOrder
    {
        Streaks = 0,
        BlueCard = 1,
        GoldCard = 2,
    }

    public static class GameMissionTypeExtensions
    {
        public static GameMissionType AsGameMissionType(this GameMissionTypeOrder typeOrder)
        {
            string typeName = typeOrder.ToString();

            if (Enum.TryParse(typeName, out GameMissionType missionType))
            {
                return missionType;
            }
            else
            {
                throw new InvalidOperationException($"Conversion not possible for {typeName}");
            }
        }
    }
}
