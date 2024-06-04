using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace PyramidSolitaireSagaSample.GameData
{
    [Serializable]
    public class GameMissionSkin
    {
        [field: SerializeField] public GameMissionType MissionType { get; private set; }
        [field: SerializeField] public Sprite IconSprite { get; private set; }
        [field: SerializeField] public Color IconColor { get; private set; }

        public override string ToString()
        {
            return $"{MissionType}, {IconSprite}, {IconColor}";
        }
    }

    public class MissionDescription
    {
        private Dictionary<GameMissionType, int> _missionCountDict = new Dictionary<GameMissionType, int>();
        private StringBuilder _textBuilder = new StringBuilder();

        public void AddMission(GameMissionType type, int count)
        {
            if (count > 0)
            {
                _missionCountDict[type] = count;
            }
        }

        public string GenerateMissionText()
        {
            _textBuilder.Length = 0;
            _textBuilder.Append("Collect ");

            bool isFirst = true;

            foreach (GameMissionTypeOrder missionTypeOrder in Enum.GetValues(typeof(GameMissionTypeOrder)))
            {
                GameMissionType missionType = missionTypeOrder.AsGameMissionType();
                if (_missionCountDict.TryGetValue(missionType, out int missionCount))
                {
                    if (!isFirst)
                    {
                        _textBuilder.Append(" and ");
                    }

                    switch (missionType)
                    {
                        case GameMissionType.GoldCard:
                            _textBuilder.Append(missionCount == 1 ? "a Gold card" : "all the Gold cards");
                            break;
                        case GameMissionType.BlueCard:
                            _textBuilder.Append(missionCount == 1 ? "a Blue card" : "all the Blue cards");
                            break;
                        case GameMissionType.Streaks:
                            _textBuilder.Append(missionCount == 1 ? "1 card in a row" : $"{missionCount} cards in a row");
                            break;
                    }
                    isFirst = false;
                }
            }

            _textBuilder.Append(".");

            return _textBuilder.ToString();
        }
    }

    [CreateAssetMenu(menuName = "PyramidSolitaireSagaSample/Game Mission Data")]
    public class GameMissionData : ScriptableObject
    {
        [FormerlySerializedAs("_gameMissionIconList")]
        [SerializeField] private List<GameMissionSkin> _gameMissionSkinList;

        private Dictionary<GameMissionType, GameMissionSkin> GameMissionSkinDict
        {
            get
            {
                if (_gameMissionIconDict == null)
                {
                    _gameMissionIconDict = new Dictionary<GameMissionType, GameMissionSkin>();
                    foreach (GameMissionSkin gameMissionIcon in _gameMissionSkinList)
                    {
                        _gameMissionIconDict.Add(gameMissionIcon.MissionType, gameMissionIcon);
                    }
                }
                return _gameMissionIconDict;
            }
        }
        private Dictionary<GameMissionType, GameMissionSkin> _gameMissionIconDict;

        public GameMissionSkin GetGameMissionSkin(GameMissionType missionType)
        {
            if (GameMissionSkinDict.TryGetValue(missionType, out GameMissionSkin result) == false)
            {
                throw new NotImplementedException($"GameMissionType not found : {missionType}");
            }
            return result;
        }
    }
}
