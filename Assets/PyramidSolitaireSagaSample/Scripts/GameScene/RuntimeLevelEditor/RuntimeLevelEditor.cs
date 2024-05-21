using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameMission;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.BonusCardSequence;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardDeck;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardPool;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.CardSelection;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.GameBoard;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.GamePlay;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.LevelPreset;
using PyramidSolitaireSagaSample.RuntimeLevelEditor.SaveLevel;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor
{
    public class RuntimeLevelEditor : MonoBehaviour
    {
        [SerializeField] private GameBoardEditorPresenter _gameBoardEditorPresenter;
        [SerializeField] private GameBoardEditorTool _gameBoardEditorTool;
        [SerializeField] private GameMissionEditorPresenter _gameMissionEditorPresenter;
        [SerializeField] private LevelPresetEditorPresenter _levelPresetEditorPresenter;

        [SerializeField] private CardPoolEditorPresenter _cardPoolEditorPresenter;
        [SerializeField] private CardDeckEditorPresenter _cardDeckEditorPresenter;
        [SerializeField] private BonusCardSequenceEditorPresenter _bonusCardSequenceEditorPresenter;
        [SerializeField] private LevelSelectionEditorPresenter _levelSelectionEditorPresenter;
        [SerializeField] private CardSelectionEditorPresenter _cardSelectionEditorPresenter;

        [SerializeField] private GamePlayEditorPresenter _gamePlayEditorPresenter;
        [SerializeField] private GameBoostersEditorPresenter _gameBoostersEditorPresenter;

        private void OnEnable()
        {
            _gameBoardEditorPresenter.onCardModelSelected += _cardSelectionEditorPresenter.UpdateGameBoardCardModel;
            _gameBoardEditorPresenter.onGameBoardChanged += _levelSelectionEditorPresenter.UpdateBoardCardInfoList;
            _gameBoardEditorPresenter.onGameBoardChanged += _cardPoolEditorPresenter.UpdateBoardCardInfoList;
            _gameBoardEditorPresenter.onGameBoardChanged += _gameMissionEditorPresenter.UpdateBoardCardInfoList;
            _gameBoardEditorPresenter.onGameBoardRestored += _cardPoolEditorPresenter.UpdateBoardCardInfoList;

            _gameMissionEditorPresenter.onMissionChanged += _levelSelectionEditorPresenter.UpdateMissionTypeList;
            _levelPresetEditorPresenter.onGameDifficultyChanged += _levelSelectionEditorPresenter.UpdateGameDifficultyType;
            _levelPresetEditorPresenter.onJokerCountChanged += _levelSelectionEditorPresenter.UpdateJokerCount;
            _levelPresetEditorPresenter.onCurrentLevelChanged += _levelSelectionEditorPresenter.UpdateCurrentLevel;

            _cardDeckEditorPresenter.onItemModelSelected += _cardSelectionEditorPresenter.UpdateCardDeckItemModel;
            _cardDeckEditorPresenter.onItemModelChanged += _levelSelectionEditorPresenter.UpdateCardDeckItemModelList;
            _cardDeckEditorPresenter.onItemModelChanged += _cardPoolEditorPresenter.UpdateCardDeckItemModelList;
            _cardDeckEditorPresenter.onItemModelRestored += _cardPoolEditorPresenter.UpdateCardDeckItemModelList;
            _cardDeckEditorPresenter.onPanelActive += OnDeckEditorPanelActive;

            _bonusCardSequenceEditorPresenter.onSequenceItemModelSelected += _cardSelectionEditorPresenter.UpdateBonusCardSequenceItemModel;
            _bonusCardSequenceEditorPresenter.onSequenceItemModelUpdated += _levelSelectionEditorPresenter.UpdateBonusCardSequenceItemModelList;
            _bonusCardSequenceEditorPresenter.onSequenceCountChanged += _levelSelectionEditorPresenter.UpdateBonusCardSequenceItemModelList;
            _bonusCardSequenceEditorPresenter.onPanelActive += DeselectGameBoardMenu;

            _levelSelectionEditorPresenter.onNewSaveLevel += OnNewSaveLevel;
            _levelSelectionEditorPresenter.onSelectedLevelChanged += OnSelectedLevelChanged;

            _cardPoolEditorPresenter.onDeckCountUpdated += _cardDeckEditorPresenter.UpdateCardPoolDeckCount;
            _cardPoolEditorPresenter.onItemModelUpdated += _levelSelectionEditorPresenter.UpdateCardPoolItemModel;

            _gamePlayEditorPresenter.onPlayButtonClick += _gameBoardEditorTool.UnlistenToGameInput;
            _gamePlayEditorPresenter.onBoosterButtonClick += _gameBoostersEditorPresenter.OnBoosterClick;

            _gameBoostersEditorPresenter.onPlayClick += _gameBoardEditorTool.UnlistenToGameInput;
        }

        private void OnDisable()
        {
            _gameBoardEditorPresenter.onCardModelSelected -= _cardSelectionEditorPresenter.UpdateGameBoardCardModel;
            _gameBoardEditorPresenter.onGameBoardChanged -= _cardPoolEditorPresenter.UpdateBoardCardInfoList;
            _gameBoardEditorPresenter.onGameBoardChanged -= _levelSelectionEditorPresenter.UpdateBoardCardInfoList;
            _gameBoardEditorPresenter.onGameBoardChanged -= _gameMissionEditorPresenter.UpdateBoardCardInfoList;
            _gameBoardEditorPresenter.onGameBoardRestored -= _cardPoolEditorPresenter.UpdateBoardCardInfoList;

            _gameMissionEditorPresenter.onMissionChanged -= _levelSelectionEditorPresenter.UpdateMissionTypeList;
            _levelPresetEditorPresenter.onGameDifficultyChanged -= _levelSelectionEditorPresenter.UpdateGameDifficultyType;
            _levelPresetEditorPresenter.onJokerCountChanged -= _levelSelectionEditorPresenter.UpdateJokerCount;
            _levelPresetEditorPresenter.onCurrentLevelChanged -= _levelSelectionEditorPresenter.UpdateCurrentLevel;

            _cardDeckEditorPresenter.onItemModelSelected -= _cardSelectionEditorPresenter.UpdateCardDeckItemModel;
            _cardDeckEditorPresenter.onItemModelChanged -= _levelSelectionEditorPresenter.UpdateCardDeckItemModelList;
            _cardDeckEditorPresenter.onItemModelChanged -= _cardPoolEditorPresenter.UpdateCardDeckItemModelList;
            _cardDeckEditorPresenter.onItemModelRestored -= _cardPoolEditorPresenter.UpdateCardDeckItemModelList;
            _cardDeckEditorPresenter.onPanelActive -= OnDeckEditorPanelActive;

            _bonusCardSequenceEditorPresenter.onSequenceItemModelSelected -= _cardSelectionEditorPresenter.UpdateBonusCardSequenceItemModel;
            _bonusCardSequenceEditorPresenter.onSequenceItemModelUpdated -= _levelSelectionEditorPresenter.UpdateBonusCardSequenceItemModelList;
            _bonusCardSequenceEditorPresenter.onSequenceCountChanged -= _levelSelectionEditorPresenter.UpdateBonusCardSequenceItemModelList;
            _bonusCardSequenceEditorPresenter.onPanelActive -= DeselectGameBoardMenu;

            _levelSelectionEditorPresenter.onNewSaveLevel -= OnNewSaveLevel;
            _levelSelectionEditorPresenter.onSelectedLevelChanged -= OnSelectedLevelChanged;

            _cardPoolEditorPresenter.onDeckCountUpdated -= _cardDeckEditorPresenter.UpdateCardPoolDeckCount;
            _cardPoolEditorPresenter.onItemModelUpdated -= _levelSelectionEditorPresenter.UpdateCardPoolItemModel;

            _gamePlayEditorPresenter.onPlayButtonClick -= _gameBoardEditorTool.UnlistenToGameInput;
            _gamePlayEditorPresenter.onBoosterButtonClick -= _gameBoostersEditorPresenter.OnBoosterClick;

            _gameBoostersEditorPresenter.onPlayClick -= _gameBoardEditorTool.UnlistenToGameInput;
        }

        private void OnSelectedLevelChanged(int value)
        {
            _levelPresetEditorPresenter.UpdateSelectedLevel(value);
            _gameBoardEditorPresenter.DeselectCard();
            _gameBoardEditorTool.DeselectTool();
            _gameBoardEditorTool.SelectDefaultTool();
            _gameBoardEditorTool.ClearHistoryAction();
        }

        private void OnNewSaveLevel()
        {
            _gameBoardEditorPresenter.NewLevelData();
            _gameBoardEditorTool.ClearHistoryAction();
            _cardDeckEditorPresenter.NewLevelData();
            _bonusCardSequenceEditorPresenter.NewLevelData();
            _gameMissionEditorPresenter.NewLevelData();
            _cardPoolEditorPresenter.NewLevelData();
            _levelPresetEditorPresenter.NewLevelData();
        }

        private void OnDeckEditorPanelActive(bool value)
        {
            _cardPoolEditorPresenter.OnCardDeckPanelActive(value);
            DeselectGameBoardMenu(value);
        }

        private void DeselectGameBoardMenu(bool value)
        {
            _gameMissionEditorPresenter.OnCardDeckPanelActive(value);
            _gameBoardEditorPresenter.OnCardDeckPanelActive(value);
            _gameBoardEditorTool.OnCardDeckPanelActive(value);
        }

        private void Start()
        {
            _levelSelectionEditorPresenter.RestorePreferenceData();
            _levelSelectionEditorPresenter.RestoreWholeLevelData();
            _gameBoardEditorTool.DeselectTool();
            _gameBoardEditorTool.SelectDefaultTool();
            _gameBoostersEditorPresenter.RestorePreferenceData();
        }
    }
}
