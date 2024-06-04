using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PyramidSolitaireSagaSample.GameData
{
    public enum LevelTutorialCloneType
    {
        None,
        GameBoard,
        CardDeck,
        CardCollector,
        JokerDeck,
        Mission,
        Streaks
    }

    public enum CardRendererCloneType
    {
        None,
        CardIndex,
        CardType,
        SubCardType,
    }

    [Serializable]
    public struct CardRendererCloneData
    {
        public CardRendererCloneType cloneType;
        [ConditionalEnumHide("cloneType", (int)CardRendererCloneType.CardIndex, true)]
        public Vector2Int cardIndex;
        [ConditionalEnumHide("cloneType", (int)CardRendererCloneType.CardType, true)]
        public CardType cardType;
        [ConditionalEnumHide("cloneType", (int)CardRendererCloneType.SubCardType, true)]
        public SubCardType subCardType;
    }

    public enum UiCloneType
    {
        None,
        GameMission,
    }

    [Serializable]
    public struct UiCloneData
    {
        public UiCloneType cloneType;
        [ConditionalEnumHide("cloneType", (int)UiCloneType.GameMission, true)]
        public GameMissionType gameMissionType;
    }

    public enum GameTutorialStepUiLocation
    {
        Top,
        Bottom
    }

    [Serializable]
    public class LevelTutorialStepData
    {
        [Header("Card Clone")]
        [FormerlySerializedAs("cloneType")]
        public LevelTutorialCloneType cardCloneType;
        public IEnumerable<CardRendererCloneData> CardCloneDataList => cardCloneDataList;
        [FormerlySerializedAs("cardRendererCloneDataList")]
        [SerializeField] private List<CardRendererCloneData> cardCloneDataList;

        [Header("Ui Clone")]
        public bool hasUiClone;
        [ConditionalHide("hasUiClone", true)]
        public LevelTutorialCloneType uiCloneType;
        [ConditionalHide("hasUiClone", true)]
        public UiCloneData uiCloneData;

        [Header("Others")]
        public GameTutorialStepUiLocation uiLocation;
        [TextArea] public string message;
        public bool triggerDrawCard;
    }

    [CreateAssetMenu(menuName = "PyramidSolitaireSagaSample/Level Tutorial Data")]
    public class LevelTutorialData : ScriptableObject
    {
        [SerializeField] protected List<LevelTutorialStepData> _stepDataList;

        public IEnumerable<LevelTutorialStepData> StepDataList => _stepDataList;
    }
}
