using PyramidSolitaireSagaSample.LevelPlayer.LevelPreset;
using PyramidSolitaireSagaSample.System.Popup;
using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameSetting
{
    public class GameSettingPresenter : MonoBehaviour,
                                        IPopupManagerOpener,
                                        ISpecificSceneTrigger,
                                        ILevelPresetRequester
    {
        [SerializeField] private GameSettingUI _gameSettingUI;

        public IPopupManager PopupManager { private get; set; }

        public event Action<SceneName> requestSpecificScene;
        public event Func<GameDifficultyType> requestGameDifficultyType;

        private void OnEnable()
        {
            _gameSettingUI.onSettingButtonClick += OpenSettingPopup;
        }

        private void OnDisable()
        {
            _gameSettingUI.onSettingButtonClick -= OpenSettingPopup;
        }

        private void OpenSettingPopup()
        {
            var settingPopup = PopupManager.OpenPopup<SettingsPopup>();
            settingPopup.Init(
                OnBgmValueChanged,
                OnSfxValueChanged,
                OnLanguageValueChanged,
                OnQuitClick
            );
        }

        private void OnQuitClick()
        {
            var popup = PopupManager.OpenPopup<TryQuitLevelPopup>(PopupOpenMode.Immediate);
            popup.Init(OnGiveUp, null);

            GameDifficultyType gameDifficultyType = requestGameDifficultyType();
            popup.UpdateHardLevelDisplay(gameDifficultyType);
        }

        private void OnGiveUp()
        {
            PopupManager.ClearQueue();
            requestSpecificScene.Invoke(SceneName.RuntimeLevelEditorScene);
        }

        private void OnLanguageValueChanged(GameLanguage language)
        {
            Debug.Log($"OnLanguageValueChanged : {language}");
        }

        private void OnSfxValueChanged(float value)
        {
            Debug.Log($"OnSfxValueChanged : {value}");
        }

        private void OnBgmValueChanged(float value)
        {
            Debug.Log($"OnBgmValueChanged : {value}");
        }
    }
}
