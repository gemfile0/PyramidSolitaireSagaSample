using PyramidSolitaireSagaSample.GameData;
using PyramidSolitaireSagaSample.System.PreferenceDataManager;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PyramidSolitaireSagaSample.System.LevelDataManager
{
    public partial class LevelDataManager : MonoBehaviour,
                                            IGameObjectFinderSetter,
                                            ILevelDataManager,
                                            IPreferenceSavable
    {
        public void SaveLevelData()
        {
#if UNITY_EDITOR
            //Debug.Log("SaveLevelDataToFile");
            string levelDataPath = GetSaveLevelDataPath();
            LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(levelDataPath);
            if (levelData == null)
            {
                levelData = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(levelData, levelDataPath);
            }

            _SaveWholeLevelData(levelData);
#endif
        }

        public void PingFilePath()
        {
#if UNITY_EDITOR
            string levelDataPath = GetSaveLevelDataPath();
            var levelData = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(levelDataPath);
            if (levelData != null)
            {
                EditorGUIUtility.PingObject(levelData);
            }
#endif
        }

#if UNITY_EDITOR
        private void _RestoreLevelData()
        {
            string levelDataPath = GetSaveLevelDataPath();
            LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(levelDataPath);
            if (levelData != null)
            {
                OnAssetLoaded(levelData);
            }
            else
            {
                Debug.LogWarning($"레벨 데이터가 존재하지 않습니다 : {levelDataPath}");
                _onRestoringComplete?.Invoke(false);
            }
        }

        private void _SaveWholeLevelData(LevelData levelData)
        {
            //Debug.Log("_SaveLevelDataToFile");
            levelData.SaveDataList.Clear();
            foreach (ILevelSavable savable in _levelSavableList)
            {
                levelData.SaveDataList.Add(new SaveData
                {
                    ID = savable.RestoreLevelID,
                    Data = savable.SaveLevelData()
                });
            }
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
