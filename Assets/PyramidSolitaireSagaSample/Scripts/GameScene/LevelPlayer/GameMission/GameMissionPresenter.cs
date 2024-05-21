using DG.Tweening;
using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.LevelPlayer.BonusCards;
using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.CardDeck;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using PyramidSolitaireSagaSample.LevelPlayer.GameTutorial;
using PyramidSolitaireSagaSample.System.LevelDataManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    public class GameMissionPresenter : MonoBehaviour,
                                        ILevelRestorable,
                                        ILevelTutorialUiCloner
    {
        [Header("Data")]
        [SerializeField] private GameMissionData _gameMissionData;
        [SerializeField] private UiMovementData _uiMovementData;

        [Header("Model")]
        [SerializeField] private GameMissionModel _gameMissionModel;

        [Header("View")]
        [SerializeField] private GameMissionUI _gameMissionUI;

        public event Action onMissionCleared
        {
            add { _gameMissionModel.onMissionCleared += value; }
            remove { _gameMissionModel.onMissionCleared -= value; }
        }

        public event Action<int> onMissionFailed
        {
            add { _gameMissionModel.onMissionFailed += value; }
            remove { _gameMissionModel.onMissionFailed -= value; }
        }

        public event Action<int> onMissionNotFailed
        {
            add { _gameMissionModel.onMissionNotFailed += value; }
            remove { _gameMissionModel.onMissionNotFailed -= value; }
        }

        public event Action<ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>>> onMissionRestored
        {
            add { _gameMissionModel.onMissionRestored += value; }
            remove { _gameMissionModel.onMissionRestored -= value; }
        }

        public string RestoreLevelID => RestoreLevelIdPath.GameMission;
        public LevelTutorialCloneType UiCloneType => LevelTutorialCloneType.Mission;
        public ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>> MissionProgressInfoDict => _gameMissionModel.ProgressInfoDict;

        private HashSet<GameMissionType> _missionTypeSet;
        private Coroutine _missionClearOrFailCoroutine;
        private List<GameMissionType> _missionTypeList;
        private BonusCardsTransitionState _bonusCardsTransitionState;
        private Vector2 _originMissionUiPanelPosition;

        private void Awake()
        {
            _missionTypeSet = new();
            _missionTypeList = new();
        }

        private void OnEnable()
        {
            _gameMissionModel.onMissionRestored += UpdateMissionUI;
            _gameMissionModel.onMissionProgress += UpdateMissionUI;
        }

        private void OnDisable()
        {
            _gameMissionModel.onMissionRestored -= UpdateMissionUI;
            _gameMissionModel.onMissionProgress -= UpdateMissionUI;
        }

        public void RestoreLevelData(string data)
        {
            _gameMissionModel.RestoreSaveData(data);
        }

        private void UpdateMissionUI(ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>> missionProgressInfoDict)
        {
            _missionTypeList.Clear();
            int itemIndexOffset = 0;
            foreach (GameMissionTypeOrder missionTypeOrder in Enum.GetValues(typeof(GameMissionTypeOrder)))
            {
                GameMissionType missionType = missionTypeOrder.AsGameMissionType();
                if (missionProgressInfoDict.TryGetValue(missionType, out List<GameMissionProgressInfo> missionProgressInfoList))
                {
                    GameMissionSkin gameMissionSkin = _gameMissionData.GetGameMissionSkin(missionType);
                    _gameMissionUI.UpdateItemUI(
                        itemIndexOffset,
                        gameMissionSkin,
                        missionProgressInfoList,
                        isCountTextVisible: missionType == GameMissionType.Streaks
                    );

                    foreach (var progressInfo in missionProgressInfoList)
                    {
                        _missionTypeList.Add(missionType);
                    }

                    itemIndexOffset += missionProgressInfoList.Count;
                }
            }
        }

        internal void UpdateStreaks(int streaks)
        {
            // A-1. GoldCard, BlueCard 의 진행상태를 갱신하는 방법과 Streaks 의 진행상태를 갱신하는 방법이 달라 로직이 다른 상태
            // A-2. Streaks 에서는 값을 갱신하기 전에 초기화를 수행하지 않는다
            _gameMissionModel.SetProgressInfo(GameMissionType.Streaks, streaks);
            CheckIfMissionClearOrFail();
        }

        internal void UpdateCardCollectorItemModelList(IEnumerable<CardCollectorItemModel> cardCollectorItemModel)
        {
            int goldMissionIndex = 0;
            int blueMissionIndex = 0;

            _missionTypeSet.Clear();
            _missionTypeSet.Add(GameMissionType.GoldCard);
            _missionTypeSet.Add(GameMissionType.BlueCard);
            _gameMissionModel.ResetProgressInfo(_missionTypeSet);
            foreach (CardCollectorItemModel itemModel in cardCollectorItemModel)
            {
                CardType cardType = itemModel.cardType;
                if (cardType == CardType.Gold)
                {
                    _gameMissionModel.AddProgressInfo(GameMissionType.GoldCard, goldMissionIndex);
                    goldMissionIndex += 1;
                }
                else if (cardType == CardType.Blue)
                {
                    _gameMissionModel.AddProgressInfo(GameMissionType.BlueCard, blueMissionIndex);
                    blueMissionIndex += 1;
                }
            }

            CheckIfMissionClearOrFail();
            // 카드덱, 조커덱, 보드카드, 보너스카드덱의 변경시에만 미션 실패를 체크하고 있습니다.
        }

        internal void UpdateCardDeckItemModelList(IEnumerable<CardDeckItemModel> deckItemModelList)
        {
            _gameMissionModel.UpdateDeckCount(deckItemModelList.Count());
            CheckIfMissionClearOrFail();
        }

        internal void UpdateJokerCount(int jokerCount)
        {
            _gameMissionModel.UpdateJokerCount(jokerCount);
            CheckIfMissionClearOrFail();
        }

        internal void UpdateJokerCount(int jokerCount, int addingCount)
        {
            _gameMissionModel.UpdateJokerCount(jokerCount);
            CheckIfMissionClearOrFail();
        }

        internal void UpdateBoardCardModelList(IEnumerable<IGameBoardCardModel> boardCardModelList)
        {
            _gameMissionModel.UpdateBoardCardModelList(boardCardModelList);
            CheckIfMissionClearOrFail();
        }

        internal void UpdateBonusCardCount(int bonusCardCount)
        {
            _gameMissionModel.UpdateBonusCardCount(bonusCardCount);
            CheckIfMissionClearOrFail();
        }

        private void CheckIfMissionClearOrFail()
        {
            if (_missionClearOrFailCoroutine != null)
            {
                StopCoroutine(_missionClearOrFailCoroutine);
            }
            _missionClearOrFailCoroutine = StartCoroutine(CheckIfMissionClearOrFailCoroutine());
        }

        internal void UpdateLevelPreset(int level, int jokerCount, GameDifficultyType type)
        {
            _gameMissionModel.UpdateLevel(level);
        }

        private IEnumerator CheckIfMissionClearOrFailCoroutine([CallerMemberName] string callerName = "")
        {
            Debug.Log(callerName);

            // 카드 수집 이벤트로 BonusCardsTransitionState 값이 변경되므로 한 프레임 기다린 후 판단합니다.
            yield return null;

            _gameMissionModel.CheckIfMissionCleared();
            _gameMissionModel.CheckIfMissionFailed();

            while (_bonusCardsTransitionState == BonusCardsTransitionState.Started)
            {
                yield return null;
            }
            // 보너스 카드를 수집한 이후에 미션 실패 체크를 한 번 더 합니다.
            _gameMissionModel.CheckIfMissionNotFailed();
        }

        internal void UpdateBonusCardsTransitionState(BonusCardsTransitionState bonusCardsTransitionState)
        {
            //Debug.Log($"UpdateBonusCardsTransitionState : {bonusCardsTransitionState}");
            _bonusCardsTransitionState = bonusCardsTransitionState;
        }

        public void CloneUI(Transform uiContainerTransform, UiCloneData uiCloneData)
        {
            for (int missionIndex = 0; missionIndex < _missionTypeList.Count; missionIndex++)
            {
                if (uiCloneData.cloneType != GameData.UiCloneType.GameMission)
                {
                    continue;
                }

                GameMissionType gameMissionType = _missionTypeList[missionIndex];
                if (gameMissionType == uiCloneData.gameMissionType)
                {
                    _gameMissionUI.CloneUI(uiContainerTransform, missionIndex);
                }
            }
        }

        internal void SetStartingPosition()
        {
            _originMissionUiPanelPosition = _gameMissionUI.PanelTransform.anchoredPosition;

            float topPosition = (_gameMissionUI.CachedCanvasTransform.sizeDelta.y + _gameMissionUI.PanelTransform.sizeDelta.y) / 2f;
            _gameMissionUI.PanelTransform.anchoredPosition = new Vector2(_originMissionUiPanelPosition.x, topPosition);
        }

        internal void ShowMissionUI()
        {
            _gameMissionUI.PanelTransform.DOAnchorPos(_originMissionUiPanelPosition, _uiMovementData.MissionUiMoveDuration)
                                         .SetEase(_uiMovementData.MissionUiMoveEase);
        }
    }
}
