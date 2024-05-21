using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public enum GameBoosterType
    {
        None,
        RemoveObstacles,
        RevealCard,
        FlipCardsFaceup
    }

    [Serializable]
    public struct GameBoosterData
    {
        public GameBoosterType type;
        public int count;
    }

    [Serializable]
    public class GameBoostersSaveData
    {
        public List<GameBoosterData> boosterDataList;
        public List<GameBoosterType> boosterSelectionList;
    }

    public class GameBoostersModel : MonoBehaviour
    {
        public event Action<IEnumerable<GameBoosterType>> onBoosterSelectionListRestored;
        public event Action<IEnumerable<GameBoosterData>> onBoosterDataListRestored;

        public IEnumerable<GameBoosterData> BoosterDataList => _boosterDataList;

        private List<GameBoosterType> _boosterSelectionList;
        private List<GameBoosterData> _boosterDataList;

        private void Init()
        {
            _boosterSelectionList = new List<GameBoosterType>();
            _boosterDataList = new List<GameBoosterData>();
            foreach (GameBoosterType type in Enum.GetValues(typeof(GameBoosterType)))
            {
                if (type == GameBoosterType.None)
                {
                    continue;
                }

                _boosterDataList.Add(new GameBoosterData
                {
                    type = type,
                    count = 0
                });
            }
        }

        internal void RestoreSaveData(string dataStr)
        {
            Init();

            if (string.IsNullOrEmpty(dataStr) == false)
            {
                GameBoostersSaveData saveData = JsonUtility.FromJson<GameBoostersSaveData>(dataStr);
                _boosterSelectionList.AddRange(saveData.boosterSelectionList);
                foreach (GameBoosterData data in saveData.boosterDataList)
                {
                    for (int i = 0; i < _boosterDataList.Count; i++)
                    {
                        GameBoosterData boosterData = _boosterDataList[i];
                        if (boosterData.type == data.type)
                        {
                            boosterData.count = data.count;
                        }
                        _boosterDataList[i] = boosterData;
                    }
                }
            }

            onBoosterSelectionListRestored?.Invoke(_boosterSelectionList);
            onBoosterDataListRestored?.Invoke(_boosterDataList);
        }

        internal void UseBoosters(IEnumerable<GameBoosterType> selectedBoosterTypeList)
        {
            foreach (GameBoosterType selectedBoosterType in selectedBoosterTypeList)
            {
                for (int i = 0; i < _boosterDataList.Count; i++)
                {
                    GameBoosterData boosterData = _boosterDataList[i];
                    if (boosterData.type == selectedBoosterType
                        && boosterData.count > 0)
                    {
                        boosterData.count -= 1;
                    }
                    _boosterDataList[i] = boosterData;
                }
            }

            _boosterSelectionList.Clear();
            _boosterSelectionList.AddRange(selectedBoosterTypeList);
        }

        internal List<GameBoosterType> ConsumeBoosterSelectionList()
        {
            List<GameBoosterType> consumedBoosterSelectionList = new();
            consumedBoosterSelectionList.AddRange(_boosterSelectionList);

            _boosterSelectionList.Clear();

            return consumedBoosterSelectionList;
        }

        public string SavePreferenceData()
        {
            GameBoostersSaveData data = new()
            {
                boosterDataList = _boosterDataList,
                boosterSelectionList = _boosterSelectionList
            };

            return JsonUtility.ToJson(data);
        }
    }
}
