using PyramidSolitaireSagaSample.System.SceneTransition;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PyramidSolitaireSagaSample.LoadLevelPlayer
{
    [InitializeOnLoad]
    public class PreloadSceneSwitcher
    {
        private const string PreloadScenePath = "Assets/PyramidSolitaireSagaSample/Scenes/PreloadScene.unity";

        private static StringBuilder _scenePathBuilder;
        private static HashSet<SceneName> _sceneNameSet;
        private static bool _initOnce = false;

        static PreloadSceneSwitcher()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void Init()
        {
            if (_initOnce == false)
            {
                _initOnce = true;
                _scenePathBuilder = new StringBuilder();
                _sceneNameSet = new HashSet<SceneName>
                {
                    { SceneName.LevelPlayerScene },
                    { SceneName.LevelMapScene }
                };
            }
        }

        // 플레이 모드 상태가 변경될 때 호출되는 메소드
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            Init();

            Debug.Log($"OnPlayModeChanged 1 : {state}");
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    string currentScenePath = EditorSceneManager.GetActiveScene().path;
                    Debug.Log($"OnPlayModeChanged 2 : {currentScenePath}");
                    foreach (SceneName sceneName in _sceneNameSet)
                    {
                        string sceneNameStr = sceneName.ToString();
                        Debug.Log($"OnPlayModeChanged 2-1 : {sceneNameStr}, {currentScenePath.Contains(sceneNameStr)}");
                        if (currentScenePath.Contains(sceneNameStr))
                        {
                            PlayerPrefs.SetString("LoadingSceneName", sceneName.ToString());
                            PlayerPrefs.Save();

                            SceneAsset playModeScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(PreloadScenePath);
                            EditorSceneManager.playModeStartScene = playModeScene;
                            break;
                        }
                    }
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    EditorSceneManager.playModeStartScene = null;
                    break;
            }
        }
    }
}
