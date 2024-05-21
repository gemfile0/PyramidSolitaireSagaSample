using PyramidSolitaireSagaSample.System.Popup;
using PyramidSolitaireSagaSample.System.PreferenceDataManager;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameItems
{
    public class GameItemsPresenter : MonoBehaviour,
                                      IPreferenceRestorable,
                                      IPopupManagerOpener
    {
        [SerializeField] private GameItemsModel _gameItemsModel;
        [SerializeField] private GameItemsUI _gameItemsUI;

        public PreferenceDataKey PreferenceDataKey => PreferenceDataKey.GameItems;

        public IPopupManager PopupManager { private get; set; }

        public event Func<IPreferenceRestorable, string> requestRestorePreferenceData;

        private void Awake()
        {
            _gameItemsUI.Init();
        }

        private void Start()
        {
            RestorePreferenceData();
        }

        private void OnEnable()
        {
            _gameItemsModel.onItemModelRestored += _gameItemsUI.UpdateUI;
            _gameItemsUI.onHintsButtonClick += OpenHintsPopup;
        }

        private void OnDisable()
        {
            _gameItemsModel.onItemModelRestored -= _gameItemsUI.UpdateUI;
            _gameItemsUI.onHintsButtonClick -= OpenHintsPopup;
        }

        private void OpenHintsPopup()
        {
            PopupManager.OpenPopup<HintsPopup>("ItemHintsPopup");
        }

        private void RestorePreferenceData()
        {
            string dataStr = requestRestorePreferenceData?.Invoke(this);
            _gameItemsModel.RestoreSaveData(dataStr);
        }
    }
}
