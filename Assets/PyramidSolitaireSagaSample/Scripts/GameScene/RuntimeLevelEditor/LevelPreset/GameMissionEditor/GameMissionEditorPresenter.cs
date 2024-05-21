using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    public class GameMissionEditorPresenter : MonoBehaviour, 
                                              ILevelSavable
    {
        [Header("Model")]
        [SerializeField] private GameMissionModel _gameMissionModel;

        [Header("View")]
        [SerializeField] private GameMissionEditorUI _gameMissionEditorUI;

        public event Action<IEnumerable<GameMissionItemModel>> onMissionChanged
        {
            add { _gameMissionModel.onMissionChanged += value; }
            remove { _gameMissionModel.onMissionChanged -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.GameMission;

        private void Awake()
        {
            _gameMissionEditorUI.Init();
        }

        private void OnEnable()
        {
            _gameMissionEditorUI.onAddMissionClick += _gameMissionModel.AddMission;
            _gameMissionEditorUI.onRemoveMissionClick += _gameMissionModel.RemoveMission;
            _gameMissionEditorUI.onMissionTypeChanged += _gameMissionModel.ChangeMissionType;
            _gameMissionEditorUI.onMissionCountChanged += OnMissionCountChanged;
            _gameMissionEditorUI.onMissionStepperClick += OnMissionStepperClick;

            _gameMissionModel.onMissionAdded += _gameMissionEditorUI.AddMissionItemGroup;
            _gameMissionModel.onMissionRemoved += _gameMissionEditorUI.RemoveMissionItemGroup;
            _gameMissionModel.onMissionChanged += OnMissionChanged;
        }

        private void OnDisable()
        {
            _gameMissionEditorUI.onAddMissionClick -= _gameMissionModel.AddMission;
            _gameMissionEditorUI.onRemoveMissionClick -= _gameMissionModel.RemoveMission;
            _gameMissionEditorUI.onMissionTypeChanged -= _gameMissionModel.ChangeMissionType;
            _gameMissionEditorUI.onMissionCountChanged -= OnMissionCountChanged;
            _gameMissionEditorUI.onMissionStepperClick -= OnMissionStepperClick;

            _gameMissionModel.onMissionAdded -= _gameMissionEditorUI.AddMissionItemGroup;
            _gameMissionModel.onMissionRemoved -= _gameMissionEditorUI.RemoveMissionItemGroup;
            _gameMissionModel.onMissionChanged -= OnMissionChanged;
        }

        private void OnMissionChanged(IEnumerable<GameMissionItemModel> itemModelList)
        {
            int itemIndex = 0;
            foreach (GameMissionItemModel itemModel in itemModelList)
            {
                _gameMissionEditorUI.UpdateUI(itemIndex, itemModel.type, itemModel.count);
                itemIndex += 1;
            }
        }

        public void OnCardDeckPanelActive(bool value)
        {
            _gameMissionEditorUI.gameObject.SetActive(!value);
        }

        public string SaveLevelData()
        {
            GameMissionSaveData data = new()
            {
                itemDataList = _gameMissionModel.ItemModelList
                    .Select((GameMissionItemModel itemModel) => new GameMissionSaveItemData
                    {
                        type = itemModel.type,
                        count = itemModel.count
                    })
                    .ToList()
            };

            return JsonUtility.ToJson(data);
        }

        public void RestoreLevelData(string data)
        {
            _gameMissionEditorUI.ClearMissionItemGroup();
            _gameMissionModel.RestoreSaveData(data);
        }

        public void NewLevelData()
        {
            ClearMissionTypeList();
            _gameMissionModel.AddMission();
        }

        private void ClearMissionTypeList()
        {
            _gameMissionEditorUI.ClearMissionItemGroup();
            _gameMissionModel.ClearMissionTypeList();
        }

        private void OnMissionStepperClick(int itemGroupIndex, int countOffset)
        {
            GameMissionItemModel itemModel = _gameMissionModel.GetItemModel(itemGroupIndex);
            int nextCount = itemModel.count + countOffset;
            if (nextCount >= 1 && nextCount <= 99)
            {
                _gameMissionModel.ChangeItemCount(itemGroupIndex, nextCount);
            }
        }

        private void OnMissionCountChanged(int itemGroupIndex, int nextCount)
        {
            if (nextCount >= 1 && nextCount <= 99)
            {
                _gameMissionModel.ChangeItemCount(itemGroupIndex, nextCount);
            }
            else
            {
                _gameMissionEditorUI.RevertItemGroupUI(itemGroupIndex);
            }
        }

        internal void UpdateBoardCardInfoList(IEnumerable<IGameBoardCardModel> boardCardModelList)
        {
            SyncMissionCountWithCardType(boardCardModelList, CardType.Gold, GameMissionType.GoldCard);
            SyncMissionCountWithCardType(boardCardModelList, CardType.Blue, GameMissionType.BlueCard);
        }

        private void SyncMissionCountWithCardType(IEnumerable<IGameBoardCardModel> boardCardModelList, CardType cardType, GameMissionType missionType)
        {
            int goldCardCount = boardCardModelList.Count(model => model.CardInfo.CardType == cardType);
            int goldMissionCount = _gameMissionModel.ItemModelList.Count(model => model.type == missionType);

            if (goldCardCount > goldMissionCount)
            {
                _gameMissionModel.AddMission(missionType);
            }
            else if (goldCardCount < goldMissionCount)
            {
                int lastItemIndex = -1;
                for (int i = _gameMissionModel.ItemModelCount - 1; i >= 0; i--)
                {
                    GameMissionItemModel itemModel = _gameMissionModel.GetItemModel(i);
                    if (itemModel.type == missionType)
                    {
                        lastItemIndex = i;
                        break;
                    }
                }

                if (lastItemIndex != -1)
                {
                    _gameMissionModel.RemoveMission(lastItemIndex);
                }
            }
        }
    }
}
