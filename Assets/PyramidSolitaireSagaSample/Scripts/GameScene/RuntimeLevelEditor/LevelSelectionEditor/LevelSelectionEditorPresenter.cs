using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.Helper;
using PyramidSolitaireSagaSample.LevelPlayer.BonusCardSequence;
using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.CardPool;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameMission;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using PyramidSolitaireSagaSample.System.Popup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

namespace PyramidSolitaireSagaSample.RuntimeLevelEditor.SaveLevel
{
    public class LevelSelectionEditorPresenter : MonoBehaviour,
                                                 IPopupManagerOpener,
                                                 ILevelDataManagerSetter
    {
        public enum State
        {
            None,
            Load,
        }

        [Header("Model")]
        [SerializeField] private LevelSelectionEditorModel _levelSelectionModel;

        [Header("View")]
        [SerializeField] private LevelSelectionEditorUI _levelSelectionUI;

        public event Action onNewSaveLevel;
        public event Action<int> onSelectedLevelChanged;

        private EnumState<State> _state;

        public IPopupManager PopupManager { set; private get; }
        public ILevelDataManager LevelDataManager { set; private get; }

        public string RestoreLevelID => RestoreLevelIdPath.LevelSelection;

        private void Awake()
        {
            _state = new EnumState<State>();
        }

        private void OnEnable()
        {
            _levelSelectionModel.onSelectedLevelChanged += OnSelectedLevelChanged;
            _levelSelectionModel.onLevelDataPathChanged += OnLevelDataPathChanged;

            _levelSelectionUI.onDropDownValueChanged += OnDropDownValueChanged;
            _levelSelectionUI.onNavigateClick += OnNavigateClick;
            _levelSelectionUI.onValueChanged += OnValueChanged;
            _levelSelectionUI.onPingClick += LevelDataManager.PingFilePath;
            _levelSelectionUI.onResetClick += OnResetClick;
        }

        private void OnDisable()
        {
            _levelSelectionModel.onSelectedLevelChanged -= OnSelectedLevelChanged;
            _levelSelectionModel.onLevelDataPathChanged -= OnLevelDataPathChanged;

            _levelSelectionUI.onDropDownValueChanged -= OnDropDownValueChanged;
            _levelSelectionUI.onNavigateClick -= OnNavigateClick;
            _levelSelectionUI.onValueChanged -= OnValueChanged;
            _levelSelectionUI.onPingClick -= LevelDataManager.PingFilePath;
            _levelSelectionUI.onResetClick -= OnResetClick;
        }

        private void OnResetClick()
        {
            onNewSaveLevel();
        }

        private void OnDropDownValueChanged(int index)
        {
            if (index != 0)
            {
                _levelSelectionModel.UpdateLevelDataPath((LevelDataPath)index);
            }
            else
            {
                _levelSelectionUI.RevertLevelDataPath();
            }
        }

        private void OnValueChanged(int nextLevel)
        {
            if (nextLevel >= 1 && nextLevel <= 999)
            {
                _levelSelectionModel.UpdateSelectedLevel(nextLevel);
            }
            else
            {
                _levelSelectionUI.RevertLevelText();
            }
        }

        private void OnNavigateClick(int offset)
        {
            int nextLevel = _levelSelectionModel.SelectedLevel + offset;
            //Debug.Log($"OnNavigateClick : {_levelSelectionModel.SelectedLevel}, {offset}");
            if (nextLevel >= 1 && nextLevel <= 999)
            {
                _levelSelectionModel.UpdateSelectedLevel(nextLevel);
            }
        }

        public void RestorePreferenceData()
        {
            (int currentLevel, int highestUnlockedLevel, LevelDataPath levelDataPath) = LevelDataManager.RestorePreferenceData();
            RestoreLevelData(currentLevel, levelDataPath);
        }

        public void UpdateBoardCardInfoList(IEnumerable<IGameBoardCardModel> boardCardInfoList)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log("LevelSelectionPresenter.UpdateCardDeckItemModelList");
                LevelDataManager.SaveLevelData();
            }
        }

        public void UpdateCardDeckItemModelList(IEnumerable<CardDeckItemModel> itemModelList)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log("LevelSelectionPresenter.UpdateCardDeckItemModelList");
                LevelDataManager.SaveLevelData();
            }
        }

        public void UpdateMissionTypeList(IEnumerable<GameMissionItemModel> missionItemModelList)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log("LevelSelectionPresenter.UpdateMissionTypeList");
                LevelDataManager.SaveLevelData();
            }
        }

        public void UpdateCardPoolItemModel(ReadOnlyDictionary<string, CardPoolItemModel> poolItemModelDict, ReadOnlyDictionary<string, CardPoolItemModel> totalItemModelDict)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log("LevelSelectionPresenter.UpdateCardPoolItemModel");
                LevelDataManager.SaveLevelData();
            }
        }

        public void UpdateGameDifficultyType(GameDifficultyType type)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log("LevelSelectionPresenter.UpdateGameDifficultyType");
                LevelDataManager.SaveLevelData();
            }
        }

        public void UpdateJokerCount(int jokerCount)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log("LevelSelectionPresenter.UpdateJokerCount");
                LevelDataManager.SaveLevelData();
            }
        }

        internal void UpdateCurrentLevel(int value)
        {
            if (_state.CurrState == State.None)
            {
                //Debug.Log($"LevelSelectionPresenter.UpdateCurrentLevel : {value}");
                LevelDataManager.SaveLevelData();
            }
        }

        internal void UpdateBonusCardSequenceItemModelList(IEnumerable<BonusCardSequenceItemModel> enumerable)
        {
            //Debug.Log("UpdateBonusCardSequenceItemModelList");
            if (_state.CurrState == State.None)
            {
                LevelDataManager.SaveLevelData();
            }
        }

        private void OnLevelDataPathChanged(LevelDataPath levelDataPath)
        {
            if (_state.CurrState == State.None)
            {
                LevelDataManager.UpdateLevelDataPath(levelDataPath);

                LoadLevelData();
            }
        }

        private void OnSelectedLevelChanged(int selectedLevel)
        {
            if (_state.CurrState == State.None)
            {
                _levelSelectionUI.UpdateLevelText(selectedLevel.ToString());

                LevelDataManager.UpdateCurrentLevel(selectedLevel);

                LoadLevelData();
            }
        }

        private async void LoadLevelData()
        {
            // 여기서 전체 데이터의 Restore 로직이 발생한다.
            bool isFileExists = await RestoreWholeLevelData();
            if (isFileExists == false)
            {
                //Debug.Log("onNewSaveLevel");
                onNewSaveLevel?.Invoke();
            }

            LevelDataManager.PingFilePath();

            onSelectedLevelChanged?.Invoke(_levelSelectionModel.SelectedLevel);
        }

        public Task<bool> RestoreWholeLevelData()
        {
            var tcs = new TaskCompletionSource<bool>();

            _state.Set(State.Load);
            LevelDataManager.RestoreLevelData(isDataExists =>
            {
                _state.Set(State.None);
                tcs.SetResult(isDataExists);
            });

            return tcs.Task;
        }

        public void RestoreLevelData(int currentLevel, LevelDataPath levelDataPath)
        {
            //Debug.Log($"RestoreLevelData : {currentLevel}, {levelDataPath}");
            _levelSelectionModel.UpdateSelectedLevel_WithoutNotify(currentLevel);
            _levelSelectionModel.UpdateLevelDataPath_WithoutNotify(levelDataPath);
            _levelSelectionUI.UpdateLevelText(currentLevel.ToString());
            _levelSelectionUI.UpdateLevelDataPath(levelDataPath);
        }
    }
}
