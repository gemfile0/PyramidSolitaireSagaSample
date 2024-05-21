using PyramidSolitaireSagaSample.System.SceneTransition;
using System;
using UnityEngine;

namespace PyramidSolitaireSagaSample.PreloadScene
{
    public class PreloadScene : MonoBehaviour,
                                ISpecificSceneTrigger
    {
        public event Action<SceneName> requestSpecificScene;

        private void Start()
        {
            string sceneNameStr = PlayerPrefs.GetString("LoadingSceneName", "");
            Debug.Log($"PreloadScene.Start() : {sceneNameStr}");

            if (Enum.TryParse(sceneNameStr, out SceneName sceneName))
            {
                requestSpecificScene?.Invoke(sceneName);
            }
        }
    }
}
