using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.BonusCards;
using PyramidSolitaireSagaSample.LevelPlayer.BonusCardSequence;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoosters;
using PyramidSolitaireSagaSample.LevelPlayer.GameMission;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using PyramidSolitaireSagaSample.LevelPlayer.Input;
using PyramidSolitaireSagaSample.LevelPlayer.JokerDeck;
using PyramidSolitaireSagaSample.LevelPlayer.LevelPlayerMenu;
using PyramidSolitaireSagaSample.LevelPlayer.LevelPreset;
using PyramidSolitaireSagaSample.LevelPlayer.Streaks;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using PyramidSolitaireSagaSample.System.Popup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer
{

    public class LevelPlayer : MonoBehaviour,
                               ILevelDataManagerSetter,
                               ILevelPlayerInputDisabler,
                               IPopupManagerOpener,
                               IGameTutorialManagerPlayer,
                               ILevelPresetRequester
    {
        public struct IntroState
        {
            public bool isCardDeckItemRendererInit;
            public bool isMainCameraPositionInit;
            public bool isGameBoostersInit;
        }

        [SerializeField] private float introDelay = .5f;

        [Header("Data")]
        [SerializeField] private GameMissionData _gameMissionData;

        [Header("Game Board")]
        [SerializeField] private GameBoardPresenter _gameBoardPresenter;

        [Header("Deck Board")]
        [SerializeField] private Transform _deckBoardTransform;
        [SerializeField] private JokerDeckPresenter _jokerDeckPresenter;
        [SerializeField] private CardDeckPresenter _cardDeckPresenter;
        [SerializeField] private CardPoolPresenter _cardPoolPresenter;
        [SerializeField] private BonusCardSequencePresenter _bonusCardSequencePresenter;
        [SerializeField] private CardCollectorPresenter _cardCollectorPresenter;
        [SerializeField] private StreaksPresenter _streaksPresenter;
        [SerializeField] private BonusCardsPresenter _bonusCardsPresenter;

        [Header("Other")]
        [SerializeField] private LevelPresetPresenter _levelPresetPresenter;
        [SerializeField] private GameMissionPresenter _gameMissionPresenter;
        [SerializeField] private LevelPlayerMenuPresenter _levelPlayerMenuPresenter;
        [SerializeField] private GameBoostersPresenter _gameBoostersPresenter;

        public ILevelDataManager LevelDataManager { private get; set; }
        public IPopupManager PopupManager { private get; set; }
        public IGameTutorialManager GameTutorialManager { private get; set; }

        public event Action requestDisableInput;
        public event Action requestEnableInput;
        public event Func<GameDifficultyType> requestGameDifficultyType;

        private IntroState _introState;

        private void Awake()
        {
            _introState = new IntroState();

            _cardDeckPresenter.BonusCardSequencePicker = _bonusCardSequencePresenter;

            _cardDeckPresenter.CardPoolPicker = _cardPoolPresenter;
            _gameBoardPresenter.CardPoolPicker = _cardPoolPresenter;
        }

        private void OnEnable()
        {
            _gameBoardPresenter.onGameBoardChanged += _cardPoolPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardRestored += _cardPoolPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardRestored += _gameMissionPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardDrawn += _cardPoolPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardDrawn += _gameMissionPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onMainCameraPositionChanged += OnMainCameraPositionChanged;
            _gameBoardPresenter.onTileBoundHalfSize += _bonusCardsPresenter.UpdateTileBoundHalfSize;
            _gameBoardPresenter.onSubCardTypeConsumed += _cardDeckPresenter.DrawItemModel;

            _cardDeckPresenter.onItemModelChanged += _cardPoolPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onItemModelRestored += _cardPoolPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onItemModelRestored += _gameMissionPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onCardDeckDrawn += _cardPoolPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onCardDeckDrawn += _gameMissionPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onItemRendererCreated += OnCardDeckItemRendererCreated;

            _bonusCardSequencePresenter.onBonusCardCountAdded += _cardDeckPresenter.CreateItemRenderer;
            _bonusCardSequencePresenter.onItemModelDrawn += _cardDeckPresenter.DrawPeekItemRenderer;
            _bonusCardSequencePresenter.onItemModelRevealed += _cardDeckPresenter.RevealPeekItemRenderer;
            _bonusCardSequencePresenter.onBonusCardCountUpdated += _gameMissionPresenter.UpdateBonusCardCount;

            _cardCollectorPresenter.onItemModelUpdated += _cardPoolPresenter.UpdateCardCollectorItemModelList;
            _cardCollectorPresenter.onItemModelUpdated += _gameMissionPresenter.UpdateCardCollectorItemModelList;
            _cardCollectorPresenter.onItemModelUpdated += _streaksPresenter.UpdateCardCollectorItemModelList;
            _cardCollectorPresenter.onItemModelUpdated += _bonusCardsPresenter.UpdateCardCollectorItemModelList;

            _streaksPresenter.onStreaksChanged += _gameMissionPresenter.UpdateStreaks;

            _levelPresetPresenter.onLevelPresetRestored += _jokerDeckPresenter.UpdateLevelPreset;
            _levelPresetPresenter.onLevelPresetRestored += _levelPlayerMenuPresenter.UpdateLevelPreset;
            _levelPresetPresenter.onLevelPresetRestored += _gameMissionPresenter.UpdateLevelPreset;

            _gameMissionPresenter.onMissionCleared += _bonusCardsPresenter.UpdateLevelCleared;

            _jokerDeckPresenter.onJokerCountRestored += _gameMissionPresenter.UpdateJokerCount;
            _jokerDeckPresenter.onJokerCountUpdated += _gameMissionPresenter.UpdateJokerCount;

            _bonusCardsPresenter.onBonusCardsTransitionState += _gameMissionPresenter.UpdateBonusCardsTransitionState;
            _gameBoostersPresenter.onBoosterSelectionListRestored += OnBoosterSelectionListRestored;
        }

        private void OnDisable()
        {
            _gameBoardPresenter.onGameBoardChanged -= _cardPoolPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardRestored -= _cardPoolPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardRestored -= _gameMissionPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardDrawn -= _cardPoolPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onGameBoardDrawn -= _gameMissionPresenter.UpdateBoardCardModelList;
            _gameBoardPresenter.onMainCameraPositionChanged -= OnMainCameraPositionChanged;
            _gameBoardPresenter.onTileBoundHalfSize -= _bonusCardsPresenter.UpdateTileBoundHalfSize;
            _gameBoardPresenter.onSubCardTypeConsumed -= _cardDeckPresenter.DrawItemModel;

            _cardDeckPresenter.onItemModelChanged -= _cardPoolPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onItemModelRestored -= _cardPoolPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onItemModelRestored -= _gameMissionPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onCardDeckDrawn -= _cardPoolPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onCardDeckDrawn -= _gameMissionPresenter.UpdateCardDeckItemModelList;
            _cardDeckPresenter.onItemRendererCreated -= OnCardDeckItemRendererCreated;

            _bonusCardSequencePresenter.onBonusCardCountAdded -= _cardDeckPresenter.CreateItemRenderer;
            _bonusCardSequencePresenter.onItemModelDrawn -= _cardDeckPresenter.DrawPeekItemRenderer;
            _bonusCardSequencePresenter.onItemModelRevealed -= _cardDeckPresenter.RevealPeekItemRenderer;
            _bonusCardSequencePresenter.onBonusCardCountUpdated -= _gameMissionPresenter.UpdateBonusCardCount;

            _cardCollectorPresenter.onItemModelUpdated -= _cardPoolPresenter.UpdateCardCollectorItemModelList;
            _cardCollectorPresenter.onItemModelUpdated -= _gameMissionPresenter.UpdateCardCollectorItemModelList;
            _cardCollectorPresenter.onItemModelUpdated -= _streaksPresenter.UpdateCardCollectorItemModelList;
            _cardCollectorPresenter.onItemModelUpdated -= _bonusCardsPresenter.UpdateCardCollectorItemModelList;

            _streaksPresenter.onStreaksChanged -= _gameMissionPresenter.UpdateStreaks;

            _levelPresetPresenter.onLevelPresetRestored -= _jokerDeckPresenter.UpdateLevelPreset;
            _levelPresetPresenter.onLevelPresetRestored -= _levelPlayerMenuPresenter.UpdateLevelPreset;
            _levelPresetPresenter.onLevelPresetRestored -= _gameMissionPresenter.UpdateLevelPreset;

            _gameMissionPresenter.onMissionCleared -= _bonusCardsPresenter.UpdateLevelCleared;

            _jokerDeckPresenter.onJokerCountRestored -= _gameMissionPresenter.UpdateJokerCount;
            _jokerDeckPresenter.onJokerCountUpdated -= _gameMissionPresenter.UpdateJokerCount;

            _bonusCardsPresenter.onBonusCardsTransitionState -= _gameMissionPresenter.UpdateBonusCardsTransitionState;
            _gameBoostersPresenter.onBoosterSelectionListRestored -= OnBoosterSelectionListRestored;
        }

        private void Start()
        {
            LevelDataManager.RestorePreferenceData();
            LevelDataManager.RestoreLevelData();
            _gameBoostersPresenter.RestorePreferenceData();
        }

        private void OnBoosterSelectionListRestored(IEnumerable<GameBoosterType> boosterTypeList)
        {
            _introState.isGameBoostersInit = true;
            CheckAndStartIntro();
        }

        private void OnCardDeckItemRendererCreated()
        {
            _introState.isCardDeckItemRendererInit = true;
            CheckAndStartIntro();
        }

        private void OnMainCameraPositionChanged(Vector3 cameraPosition)
        {
            Vector3 originDeckBoardPosition = _deckBoardTransform.position;
            Vector3 newDeckBoardPosition = new Vector3(cameraPosition.x, originDeckBoardPosition.y, originDeckBoardPosition.z);
            _deckBoardTransform.position = newDeckBoardPosition;

            _introState.isMainCameraPositionInit = true;
            CheckAndStartIntro();
        }

        private void CheckAndStartIntro()
        {
            if (_introState.isCardDeckItemRendererInit
                && _introState.isMainCameraPositionInit
                && _introState.isGameBoostersInit)
            {
                StartCoroutine(IntroCoroutine());
            }
        }

        private IEnumerator IntroCoroutine()
        {
            requestDisableInput?.Invoke();

            // A-1. 게임 보드 위 랜덤 카드들의 숫자와 색깔을 먼저 확정합니다.
            _gameBoardPresenter.PickRandomCards();
            // A-2. 이후 카드 풀에서 남은 카드로 카드 덱의 랜덤 카드들을 확정하고 있습니다.
            _cardDeckPresenter.DrawFirstItem();
            _gameBoardPresenter.SetStartingPosition(_cardDeckPresenter.GetLootAnimtionEndPoint());
            _gameMissionPresenter.SetStartingPosition();

            yield return new WaitForSeconds(introDelay);

            _gameMissionPresenter.ShowMissionUI();
            yield return ShowMissionPopup();

            yield return _gameBoardPresenter.DealCardsToBoard();

            yield return ApplyBooster();

            LevelDataPath levelDataPath = LevelDataManager.LevelDataPath;
            int currentLevel = LevelDataManager.CurrentLevel;
            if (GameTutorialManager.HasLevelTutorial(levelDataPath, currentLevel))
            {
                yield return GameTutorialManager.PlayLevelTutorial(levelDataPath, currentLevel);
            }

            requestEnableInput?.Invoke();
        }

        private IEnumerator ApplyBooster()
        {
            List<GameBoosterType> consumedBoosterSelectionList = _gameBoostersPresenter.ConsumeBoosterSelectionList();
            foreach (GameBoosterType boosterType in consumedBoosterSelectionList)
            {
                yield return _gameBoostersPresenter.HideScreen(boosterType);
                switch (boosterType)
                {
                    case GameBoosterType.RemoveObstacles:
                        _gameBoardPresenter.RemoveObstacles();
                        break;

                    case GameBoosterType.FlipCardsFaceup:
                        _gameBoardPresenter.FlipCardsFaceup();
                        break;

                    case GameBoosterType.RevealCard:
                        _cardDeckPresenter.ShowRevealedRenderer();
                        break;
                }
                yield return _gameBoostersPresenter.ShowBoosterTransition(boosterType);
                yield return _gameBoostersPresenter.ShowScreen();
            }
        }

        private IEnumerator ShowMissionPopup()
        {
            ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>> _missionProgressInfoDict = _gameMissionPresenter.MissionProgressInfoDict;
            GameDifficultyType gameDifficultyType = requestGameDifficultyType();

            var missionPopup = PopupManager.OpenPopup<MissionPopup>();
            missionPopup.Init();
            missionPopup.UpdateHardLevelDisplay(gameDifficultyType);

            int itemIndex = 0;
            var missionDescription = new MissionDescription();
            foreach (GameMissionTypeOrder missionTypeOrder in Enum.GetValues(typeof(GameMissionTypeOrder)))
            {
                GameMissionType missionType = missionTypeOrder.AsGameMissionType();
                if (_missionProgressInfoDict.TryGetValue(missionType, out List<GameMissionProgressInfo> missionProgressInfoList))
                {
                    int missionCount = 0;
                    foreach (GameMissionProgressInfo progressInfo in missionProgressInfoList)
                    {
                        missionCount += progressInfo.total;
                    }
                    missionDescription.AddMission(missionType, missionCount);

                    GameMissionSkin gameMissionSkin = _gameMissionData.GetGameMissionSkin(missionType);
                    missionPopup.UpdateItemUI(itemIndex, gameMissionSkin, missionCount);

                    itemIndex += 1;
                }
            }

            missionPopup.UpdateTextUI(missionDescription.GenerateMissionText());

            yield return missionPopup.WaitForClose();
        }
    }
}
