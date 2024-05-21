using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using PyramidSolitaireSagaSample.LevelPlayer.GameBoard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LevelPlayer.GameMission
{
    [Serializable]
    public class GameMissionSaveItemData
    {
        public GameMissionType type;
        public int count;
    }

    [Serializable]
    public class GameMissionSaveData
    {
        public List<GameMissionSaveItemData> itemDataList;
    }

    public struct GameMissionItemModel
    {
        public GameMissionType type;
        public int count;
    }

    public struct GameMissionProgressInfo
    {
        public int current;
        public int total;
    }

    public class GameMissionModel : MonoBehaviour,
                                    ICardCollectChecker
    {
        public event Action<GameMissionItemModel> onMissionAdded;
        public event Action<int> onMissionRemoved;
        public event Action<IEnumerable<GameMissionItemModel>> onMissionChanged;
        public event Action<ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>>> onMissionRestored;
        public event Action<ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>>> onMissionProgress;
        public event Action onMissionCleared;
        public event Action<int> onMissionFailed;
        public event Action<int> onMissionNotFailed;

        public event Func<CardNumber, bool> canCollect;

        public int ItemModelCount => _itemModelList.Count;
        public IEnumerable<GameMissionItemModel> ItemModelList => _itemModelList;
        private List<GameMissionItemModel> _itemModelList;

        private Dictionary<GameMissionType, List<GameMissionProgressInfo>> _progressInfoDict;
        public ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>> ProgressInfoDict
        {
            get;
            private set;
        }

        private bool _isMissionCleared;
        private int _deckCount = -1;
        private int _jokerCount = -1;
        private int _bonusCardCount = -1;
        private IEnumerable<IGameBoardCardModel> _boardCardModelList = null;
        private int _level;

        private void Awake()
        {
            _itemModelList = new List<GameMissionItemModel>();
            _progressInfoDict = new Dictionary<GameMissionType, List<GameMissionProgressInfo>>();
            ProgressInfoDict = new ReadOnlyDictionary<GameMissionType, List<GameMissionProgressInfo>>(_progressInfoDict);
        }

        internal void AddMission()
        {
            AddMission(GameMissionType.GoldCard);
        }

        internal void AddMission(GameMissionType gameMissionType)
        {
            AddMissionWithoutNotifyChanged(gameMissionType, 1);
            onMissionChanged?.Invoke(ItemModelList);
        }

        private void AddMissionWithoutNotifyChanged(GameMissionType type, int count)
        {
            var itemModel = new GameMissionItemModel()
            {
                type = type,
                count = count
            };
            _itemModelList.Add(itemModel);
            onMissionAdded?.Invoke(itemModel);

            //
            if (_progressInfoDict.ContainsKey(type) == false)
            {
                _progressInfoDict.Add(type, new List<GameMissionProgressInfo>());
            }
            List<GameMissionProgressInfo> progressInfoList = _progressInfoDict[type];
            progressInfoList.Add(new GameMissionProgressInfo()
            {
                current = 0,
                total = itemModel.count
            });
        }

        internal void RemoveMission(int itemIndex)
        {
            if (_itemModelList.Count > 1
                && itemIndex < _itemModelList.Count)
            {
                _itemModelList.RemoveAt(itemIndex);

                onMissionRemoved?.Invoke(itemIndex);
                onMissionChanged?.Invoke(ItemModelList);
            }
        }

        internal void ChangeMissionType(int itemIndex, GameMissionType missionType)
        {
            GameMissionItemModel itemModel = _itemModelList[itemIndex];
            if (itemModel.type != missionType)
            {
                itemModel.type = missionType;
                _itemModelList[itemIndex] = itemModel;
                onMissionChanged?.Invoke(ItemModelList);
            }
        }

        internal void ClearMissionTypeList()
        {
            _itemModelList.Clear();
            _progressInfoDict.Clear();
        }

        internal void RestoreSaveData(string data)
        {
            if (string.IsNullOrEmpty(data) == false)
            {
                ClearMissionTypeList();

                GameMissionSaveData saveData = JsonUtility.FromJson<GameMissionSaveData>(data);
                foreach (GameMissionSaveItemData itemData in saveData.itemDataList)
                {
                    AddMissionWithoutNotifyChanged(itemData.type, itemData.count);
                }
                onMissionRestored?.Invoke(ProgressInfoDict);
            }
        }

        internal void ResetProgressInfo(HashSet<GameMissionType> missionTypeSet)
        {
            foreach (GameMissionType missionType in missionTypeSet)
            {
                if (_progressInfoDict.TryGetValue(missionType, out List<GameMissionProgressInfo> progressInfoList))
                {
                    for (int missionIndex = 0; missionIndex < progressInfoList.Count; missionIndex++)
                    {
                        GameMissionProgressInfo progressInfo = progressInfoList[missionIndex];
                        progressInfo.current = 0;
                        progressInfoList[missionIndex] = progressInfo;
                    }
                }
            }
        }

        internal void AddProgressInfo(GameMissionType missionType, int missionIndex)
        {
            if (_progressInfoDict.TryGetValue(missionType, out List<GameMissionProgressInfo> progressInfoList))
            {
                if (missionIndex < progressInfoList.Count)
                {
                    GameMissionProgressInfo progressInfo = progressInfoList[missionIndex];
                    progressInfo.current += 1;
                    progressInfoList[missionIndex] = progressInfo;
                    onMissionProgress?.Invoke(ProgressInfoDict);
                }
                else
                {
                    Debug.LogError($"미션 인덱스가 지정한 타입의 미션 개수를 초과했습니다 : {missionIndex + 1} / {(progressInfoList.Count)}");
                }
            }
        }

        public void SetProgressInfo(GameMissionType missionType, int current)
        {
            if (_progressInfoDict.TryGetValue(missionType, out List<GameMissionProgressInfo> progressInfoList))
            {
                for (int i = 0; i < progressInfoList.Count; i++)
                {
                    GameMissionProgressInfo progressInfo = progressInfoList[i];
                    Debug.Log($"SetProgressInfo : {missionType}, {current}, {progressInfo.current} / {progressInfo.total}");
                    if (progressInfo.current < progressInfo.total)
                    {
                        progressInfo.current = current;
                        progressInfoList[i] = progressInfo;
                    }
                }
                onMissionProgress?.Invoke(ProgressInfoDict);
            }
        }

        public void CheckIfMissionCleared()
        {
            _isMissionCleared = _progressInfoDict.Count > 0;
            foreach (List<GameMissionProgressInfo> progressInfoList in _progressInfoDict.Values)
            {
                foreach (GameMissionProgressInfo progressInfo in progressInfoList)
                {
                    if (progressInfo.current < progressInfo.total)
                    {
                        _isMissionCleared = false;
                        break;
                    }
                }

                if (_isMissionCleared == false)
                {
                    break;
                }
            }

            Debug.Log($"CheckIfMissionCleared : {_isMissionCleared}");
            if (_isMissionCleared)
            {
                onMissionCleared.Invoke();
            }
        }

        internal void ChangeItemCount(int itemIndex, int missionCount)
        {
            GameMissionItemModel itemModel = _itemModelList[itemIndex];
            if (itemModel.count != missionCount)
            {
                itemModel.count = missionCount;
                _itemModelList[itemIndex] = itemModel;
                onMissionChanged?.Invoke(ItemModelList);
            }
        }

        internal GameMissionItemModel GetItemModel(int itemIndex)
        {
            return _itemModelList[itemIndex];
        }

        internal void UpdateDeckCount(int deckCount)
        {
            _deckCount = deckCount;
        }

        internal void UpdateJokerCount(int jokerCount)
        {
            _jokerCount = jokerCount;
        }

        internal void UpdateBoardCardModelList(IEnumerable<IGameBoardCardModel> boardCardModelList)
        {
            _boardCardModelList = boardCardModelList;
        }

        internal void UpdateBonusCardCount(int bonusCardCount)
        {
            _bonusCardCount = bonusCardCount;
        }

        public void CheckIfMissionFailed()
        {
            if (_isMissionCleared == false)
            {
                if (IsMissionFailed())
                {
                    onMissionFailed?.Invoke(_level);
                }
            }
        }

        public void CheckIfMissionNotFailed()
        {
            if (_isMissionCleared == false)
            {
                if (IsMissionFailed() == false)
                {
                    onMissionNotFailed?.Invoke(_level);
                }
            }
        }

        private bool IsMissionFailed()
        {
            bool isAllGoldMissionCleared = false;
            if (_progressInfoDict.TryGetValue(GameMissionType.GoldCard, out List<GameMissionProgressInfo> goldProgressInfoList))
            {
                isAllGoldMissionCleared = goldProgressInfoList.All(progressInfo => progressInfo.current >= progressInfo.total);
            }

            IEnumerable<IGameBoardCardModel> drawableBoardCardModelList = _boardCardModelList.Where(model =>
                model.CannotDrawCard() == false
                && canCollect.Invoke(model.CardInfo.CardNumber)
            );
            int drawableBoardCardCount = drawableBoardCardModelList.Count();

            Debug.Log("CheckIfMissionFailed");
            Debug.Log($"  isAllGoldMissionCleared: {isAllGoldMissionCleared}, drawableBoardCardCount: {drawableBoardCardCount}, _jokerCount: {_jokerCount}, _deckCount: {_deckCount}, _bonusCardCount: {_bonusCardCount}");
            return isAllGoldMissionCleared == true
                   || (drawableBoardCardCount == 0 && _jokerCount == 0 && _deckCount == 0 && _bonusCardCount == 0);
        }

        internal void UpdateLevel(int level)
        {
            _level = level;
        }
    }
}
