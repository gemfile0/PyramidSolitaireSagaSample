using UnityEngine;

namespace PyramidSolitaireSagaSample.GameData
{
    public enum LevelDataPath
    {
        None = 0,
        Level = 1,
        LevelSample = 2
    }

    [CreateAssetMenu(menuName = "Solitaire Makeover/Level File Data")]
    public class LevelFileData : ScriptableObject
    {
        [SerializeField] private string _levelDataPathBase = "Assets/PyramidSolitaireSagaSample/GameDatas";
        [SerializeField] private string _levelDataName = "LevelData";

        public string GetLevelDataPath(LevelDataPath levelDataPath)
        {
            return $"{_levelDataPathBase}/{levelDataPath}";
        }
        public string LevelDataName => _levelDataName;
    }
}
