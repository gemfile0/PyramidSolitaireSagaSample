using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    SetupInstance();
                }
                return instance;
            }
        }
        private static T instance;

        private const string RootObjectName = "SingletonObjects";
        private static GameObject _rootObject;
        protected bool _isDuplicated;

        protected virtual void Awake()
        {
            RemoveDuplicates();
        }

        private static void SetupInstance()
        {
            CreateRootObject();

            instance = (T)FindObjectOfType(typeof(T));
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = typeof(T).Name;
                instance = gameObject.AddComponent<T>();
            }

            instance.transform.SetParent(_rootObject.transform);
        }

        private void RemoveDuplicates()
        {
            _isDuplicated = instance != null;
            if (_isDuplicated)
            {
                Destroy(gameObject);
            }
            else
            {
                CreateRootObject();

                instance = this as T;
                transform.SetParent(_rootObject.transform);
            }
        }

        private static void CreateRootObject()
        {
            _rootObject = GameObject.Find(RootObjectName);
            if (_rootObject == null)
            {
                _rootObject = new GameObject(RootObjectName);
                DontDestroyOnLoad(_rootObject);
            }
        }
    }
}