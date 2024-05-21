using PyramidSolitaireSagaSample.System.LevelDataManager;
using PyramidSolitaireSagaSample.System.Popup;
using PyramidSolitaireSagaSample.System.PreferenceDataManager;
using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameBoosters
{
    public class GameBoostersPresenter : MonoBehaviour,
                                         IPreferenceSavable,
                                         IPopupManagerOpener,
                                         ISpecificSceneTrigger,
                                         ILevelDataManagerSetter
    {
        [SerializeField] private int _boosterOpenLevel = 19;

        [Header("Model")]
        [SerializeField] private GameBoostersModel _gameBoostersModel;

        [Header("View")]
        [SerializeField] private GameBoostersUI _gameBoostersUI;
        [SerializeField] private float _boosterTransitionDuration = .5f;

        public event Action<IEnumerable<GameBoosterType>> onBoosterSelectionListRestored
        {
            add => _gameBoostersModel.onBoosterSelectionListRestored += value;
            remove => _gameBoostersModel.onBoosterSelectionListRestored -= value;
        }

        public event Action<IEnumerable<GameBoosterData>> onBoosterDataListRestored
        {
            add => _gameBoostersModel.onBoosterDataListRestored += value;
            remove => _gameBoostersModel.onBoosterDataListRestored -= value;
        }

        public event Action onPlayButtonClick;

        public PreferenceDataKey PreferenceDataKey => PreferenceDataKey.GameBoosters;

        public IPopupManager PopupManager { private get; set; }
        public ILevelDataManager LevelDataManager { private get; set; }

        public event Func<IPreferenceRestorable, string> requestRestorePreferenceData;
        public event Action<IPreferenceRestorable, string> requestSavePreferenceData;
        public event Action<SceneName> requestSpecificScene;

        private int _selectedLevel;

        public void RestorePreferenceData()
        {
            string dataStr = requestRestorePreferenceData?.Invoke(this);
            _gameBoostersModel.RestoreSaveData(dataStr);
        }

        internal void OpenBoosterPopup(int itemIndex)
        {
            _selectedLevel = itemIndex + 1;
            if (_selectedLevel >= _boosterOpenLevel)
            {
                var boosterSelectionPopup = PopupManager.OpenPopup<BoosterSelectionPopup>();
                boosterSelectionPopup.Init(OnInfoButtonClick, OnPlayButtonClick);
                boosterSelectionPopup.UpdateTitleLevel(_selectedLevel);
                IEnumerable<GameBoosterData> boosterDataList = _gameBoostersModel.BoosterDataList;
                foreach (GameBoosterData boosterData in boosterDataList)
                {
                    boosterSelectionPopup.UpdateItemCount(boosterData.type, boosterData.count);
                }
            }
            else
            {
                PlayLevelPlayer();
            }
        }

        public List<GameBoosterType> ConsumeBoosterSelectionList()
        {
            List<GameBoosterType> consumedBoosterSelectionList = _gameBoostersModel.ConsumeBoosterSelectionList();
            requestSavePreferenceData?.Invoke(this, _gameBoostersModel.SavePreferenceData());
            return consumedBoosterSelectionList;
        }

        private void OnPlayButtonClick(IEnumerable<GameBoosterType> selectedBoosterTypeList)
        {
            _gameBoostersModel.UseBoosters(selectedBoosterTypeList);
            requestSavePreferenceData?.Invoke(this, _gameBoostersModel.SavePreferenceData());

            PlayLevelPlayer();
        }

        private void PlayLevelPlayer()
        {
            LevelDataManager.UpdateCurrentLevel(_selectedLevel);

            onPlayButtonClick?.Invoke();
            requestSpecificScene?.Invoke(SceneName.LevelPlayerScene);
        }

        private void OnInfoButtonClick()
        {
            PopupManager.OpenPopup<HintsPopup>("BoosterHintsPopup", PopupOpenMode.Immediate);
        }

        internal IEnumerator HideScreen(GameBoosterType boosterType)
        {
            yield return _gameBoostersUI.HideScreen(boosterType);
        }

        internal IEnumerator ShowScreen()
        {
            yield return _gameBoostersUI.ShowScreen();
        }

        internal IEnumerator ShowBoosterTransition(GameBoosterType boosterType)
        {
            yield return new WaitForSeconds(_boosterTransitionDuration);
        }
    }
}
