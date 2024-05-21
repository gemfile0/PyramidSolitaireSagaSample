using PyramidSolitaireSagaSample.LevelPlayer.BonusCards;
using PyramidSolitaireSagaSample.LevelPlayer.GameMission;
using PyramidSolitaireSagaSample.LevelPlayer.Input;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using PyramidSolitaireSagaSample.System.Popup;
using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using System.Collections;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer
{
    public class LevelClearChecker : MonoBehaviour,
                                     IPopupManagerOpener,
                                     ILevelDataManagerSetter,
                                     ISpecificSceneTrigger,
                                     ILevelPlayerInputLimiter
    {
        [Header("Presenter")]
        [SerializeField] private GameMissionPresenter _gameMissionPresenter;
        [SerializeField] private BonusCardsPresenter _bonusCardsPresenter;

        [Header("Internal")]
        [SerializeField] private float _delayBeforePopup = .5f;

        [SerializeField] private GameObject _inputBlockerPanelObject;

        public IPopupManager PopupManager { set; private get; }
        public ILevelDataManager LevelDataManager { set; private get; }

        public event Action<SceneName> requestSpecificScene;
        public event Action<LevelPlayerInputLimitType> requestLimitInput;
        public event Action requestUnlimitInput;

        private BonusCardsTransitionState _bonusCardsTransitionState;
        private Coroutine _levelFailedCoroutine;
        private Coroutine _levelClearedCoroutine;

        private void OnEnable()
        {
            _gameMissionPresenter.onMissionCleared += UpdateLevelCleared;
            _gameMissionPresenter.onMissionFailed += UpdateLevelFailed;
            _gameMissionPresenter.onMissionNotFailed += UpdateLevelNotFailed;
            _bonusCardsPresenter.onBonusCardsTransitionState += UpdateBonusCardsTransitionState;
        }

        private void OnDisable()
        {
            _gameMissionPresenter.onMissionCleared -= UpdateLevelCleared;
            _gameMissionPresenter.onMissionFailed -= UpdateLevelFailed;
            _gameMissionPresenter.onMissionNotFailed -= UpdateLevelNotFailed;
            _bonusCardsPresenter.onBonusCardsTransitionState -= UpdateBonusCardsTransitionState;
        }

        private void Start()
        {
            _inputBlockerPanelObject.SetActive(false);
        }

        internal void UpdateBonusCardsTransitionState(BonusCardsTransitionState bonusCardsTransitionState)
        {
            Debug.Log($"UpdateBonusCardsTransitionState : {bonusCardsTransitionState}");
            _bonusCardsTransitionState = bonusCardsTransitionState;
        }

        private void UpdateLevelNotFailed(int value)
        {
            if (_levelFailedCoroutine != null)
            {
                StopCoroutine(_levelFailedCoroutine);
                _levelFailedCoroutine = null;

                UnlimitInput();
            }
        }

        private void UpdateLevelFailed(int level)
        {
            if (_levelFailedCoroutine == null)
            {
                _levelFailedCoroutine = StartCoroutine(OpenLevelFailedPopupCoroutine(level));
            }
        }

        private IEnumerator OpenLevelFailedPopupCoroutine(int level)
        {
            LimitInput();
            while (_bonusCardsTransitionState == BonusCardsTransitionState.Started)
            {
                yield return null;
            }

            yield return new WaitForSeconds(_delayBeforePopup);

            var popup = PopupManager.OpenPopup<LevelFailedPopup>();
            popup.Init(level, OnReplayCurrentLevel);
            yield return popup.WaitForClose();
        }

        private void UpdateLevelCleared()
        {
            if (_levelClearedCoroutine == null)
            {
                LevelDataManager.UpdateUnlockedLevel(LevelDataManager.CurrentLevel + 1);
                _levelClearedCoroutine = StartCoroutine(OpenLevelClearPopupCoroutine());
            }
        }

        private IEnumerator OpenLevelClearPopupCoroutine()
        {
            LimitInput();
            while (_bonusCardsTransitionState == BonusCardsTransitionState.Started)
            {
                yield return null;
            }

            yield return new WaitForSeconds(_delayBeforePopup);

            var popup = PopupManager.OpenPopup<LevelClearPopup>();
            popup.Init(OnReplayCurrentLevel, OnPlayNextLevel);
            yield return popup.WaitForClose();
        }

        private void LimitInput()
        {
            _inputBlockerPanelObject.SetActive(true);
            requestLimitInput?.Invoke(LevelPlayerInputLimitType.UI);
        }

        private void UnlimitInput()
        {
            _inputBlockerPanelObject.SetActive(false);
            requestUnlimitInput?.Invoke();
        }

        private void OnPlayNextLevel()
        {
            LevelDataManager.UpdateCurrentLevel(LevelDataManager.CurrentLevel + 1);

            requestSpecificScene?.Invoke(SceneName.LevelPlayerScene);
        }

        private void OnReplayCurrentLevel()
        {
            requestSpecificScene?.Invoke(SceneName.LevelPlayerScene);
        }
    }
}
