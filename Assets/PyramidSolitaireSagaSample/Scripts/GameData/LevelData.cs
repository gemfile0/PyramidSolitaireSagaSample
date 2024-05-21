using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.GameData
{
    [Serializable]
    public struct SaveData
    {
        public string ID;
        public string Data;
    }

    public class LevelData : ScriptableObject
    {
        [field: SerializeField] public List<SaveData> SaveDataList { get; private set; } = new();
    }
}
