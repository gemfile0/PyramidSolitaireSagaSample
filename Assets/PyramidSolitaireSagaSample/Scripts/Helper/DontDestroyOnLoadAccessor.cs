using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class DontDestroyOnLoadAccessor : Singleton<DontDestroyOnLoadAccessor>
    {
        public GameObject[] GetRootGameObjects()
        {
            return gameObject.scene.GetRootGameObjects();
        }
    }
}
