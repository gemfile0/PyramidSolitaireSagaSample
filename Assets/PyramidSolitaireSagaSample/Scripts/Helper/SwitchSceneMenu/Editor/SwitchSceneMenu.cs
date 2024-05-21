using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PyramidSolitaireSagaSample.Helper
{
    public class SwitchSceneMenu
    {
        private const string ScenePath = "Assets/PyramidSolitaireSagaSample/Scenes";

        private const string SceneName_RuntimeLevelEditor = "RuntimeLevelEditorScene.unity";
        private const string SceneName_LevelPlayer = "LevelPlayerScene.unity";
        private const string SceneName_PreloadScene = "PreloadScene.unity";
        private const string SceneName_LevelMap = "LevelMapScene.unity";
        private const string SceneName_Title = "TitleScene.unity";

        [MenuItem("PyramidSolitaireSagaSample/Switch Scene/Runtime Level Editor", false, 1001)]
        public static void SwitchSceneToLevelEditor()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string path = Path.Combine(ScenePath, SceneName_RuntimeLevelEditor);

            EditorSceneManager.OpenScene(path);
        }

        [MenuItem("PyramidSolitaireSagaSample/Switch Scene/Level Player", false, 1002)]
        public static void SwitchSceneToLevelPlayer()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string path = Path.Combine(ScenePath, SceneName_LevelPlayer);

            EditorSceneManager.OpenScene(path);
        }

        [MenuItem("PyramidSolitaireSagaSample/Switch Scene/Preload Scene", false, 1003)]
        public static void SwitchSceneToLoadLevelPlayer()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string path = Path.Combine(ScenePath, SceneName_PreloadScene);

            EditorSceneManager.OpenScene(path);
        }

        [MenuItem("PyramidSolitaireSagaSample/Switch Scene/Level Map", false, 1004)]
        public static void SwitchSceneToLevelMap()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string path = Path.Combine(ScenePath, SceneName_LevelMap);

            EditorSceneManager.OpenScene(path);
        }

        [MenuItem("PyramidSolitaireSagaSample/Switch Scene/Title", false, 1005)]
        public static void SwitchSceneToTitle()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            string path = Path.Combine(ScenePath, SceneName_Title);

            EditorSceneManager.OpenScene(path);
        }
    }
}