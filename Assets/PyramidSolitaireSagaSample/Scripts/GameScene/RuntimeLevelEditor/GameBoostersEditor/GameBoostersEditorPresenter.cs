using PyramidSolitaireSagaSample.System;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using PyramidSolitaireSagaSample.System.Popup;
using PyramidSolitaireSagaSample.System.PreferenceDataManager;
using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class GameBoostersEditorPresenter : MonoBehaviour,
                                               IPreferenceSavable,
                                               IPopupManagerOpener,
                                               ISpecificSceneTrigger,
                                               ILevelDataManagerSetter,
                                               IGameInputActionMapTrigger
    {
        [SerializeField] private GameBoostersModel _gameBoostersModel;
        [SerializeField] private GameBoostersEditorUI _gameBoostersEditorUI;

        public event Action onPlayClick;

        public PreferenceDataKey PreferenceDataKey => PreferenceDataKey.GameBoosters;

        public IPopupManager PopupManager { private get; set; }
        public ILevelDataManager LevelDataManager { private get; set; }

        public event Action<IPreferenceRestorable, string> requestSavePreferenceData;
        public event Func<IPreferenceRestorable, string> requestRestorePreferenceData;
        public event Action<SceneName> requestSpecificScene;
        public event Action<string> onEnableActionMap;
        public event Action onRevertActionMap;

        private int _selectedLevel;

        public void RestorePreferenceData()
        {
            string dataStr = requestRestorePreferenceData?.Invoke(this);
            _gameBoostersModel.RestoreSaveData(dataStr);
        }

        internal void OnBoosterClick()
        {
            _selectedLevel = LevelDataManager.CurrentLevel;
            StartCoroutine(ShowBoosterPopupCoroutine());
        }

        private IEnumerator ShowBoosterPopupCoroutine()
        {
            onEnableActionMap?.Invoke("CardDeck");
            _gameBoostersEditorUI.ShowLoading();

            ILoadingInfoProvider loadingInfoProvider = PopupManager as ILoadingInfoProvider;
            if (loadingInfoProvider.HasLoadingInfo(SceneName.RuntimeLevelEditorScene))
            {
                LoadingInfo loadingInfo = loadingInfoProvider.GetLoadingInfo();
                loadingInfo.Reset();
                yield return loadingInfo.Coroutine(
                    /* onProgress */ (float value) => { },
                    /* onComplete */ () => { },
                    /* onError */ (string error) => { }
                );
            }

            _gameBoostersEditorUI.HideLoading();

            var boosterSelectionPopup = PopupManager.OpenPopup<BoosterSelectionPopup>();
            boosterSelectionPopup.Init(OnInfoButtonClick, OnPlayButtonClick);
            boosterSelectionPopup.UpdateTitleLevel(_selectedLevel);
            IEnumerable<GameBoosterData> boosterDataList = _gameBoostersModel.BoosterDataList;
            foreach (GameBoosterData boosterData in boosterDataList)
            {
                boosterSelectionPopup.UpdateItemCount(boosterData.type, boosterData.count);
            }

            yield return boosterSelectionPopup.WaitForClose();

            onRevertActionMap?.Invoke();
        }

        private void OnPlayButtonClick(IEnumerable<GameBoosterType> selectedBoosterTypeList)
        {
            _gameBoostersModel.UseBoosters(selectedBoosterTypeList);
            requestSavePreferenceData?.Invoke(this, _gameBoostersModel.SavePreferenceData());

            LevelDataManager.UpdateCurrentLevel(_selectedLevel);

            onPlayClick?.Invoke();
            requestSpecificScene?.Invoke(SceneName.LevelPlayerScene);
        }

        private void OnInfoButtonClick()
        {
            PopupManager.OpenPopup<HintsPopup>("BoosterHintsPopup", PopupOpenMode.Immediate);
        }
    }
}
