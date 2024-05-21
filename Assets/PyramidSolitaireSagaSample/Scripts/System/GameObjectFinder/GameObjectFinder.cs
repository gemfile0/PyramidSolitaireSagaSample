using PyramidSolitaireSagaSample.Helper;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PyramidSolitaireSagaSample.System
{
    public interface IGameObjectFinder
    {
        IEnumerable<T> FindGameObjectOfType<T>();
        T FindRootGameObject<T>(string name);
    }

    public interface IGameObjectFinderSetter
    {
        void OnGameObjectFinderAwake(IGameObjectFinder finder);
    }

    public class GameObjectFinder : MonoBehaviour, IGameObjectFinder
    {
        private GameObject[] _rootObjectArray;
        private GameObject[] _rootObjectArray_ofDontDestroyOnLoad;
        private HashSet<string> _resultSet;

        private void Awake()
        {
            _rootObjectArray_ofDontDestroyOnLoad = DontDestroyOnLoadAccessor.Instance.GetRootGameObjects();
            _rootObjectArray = SceneManager.GetActiveScene().GetRootGameObjects();
            _resultSet = new HashSet<string>();

            IEnumerable<IGameObjectFinderSetter> _setters = FindGameObjectOfType<IGameObjectFinderSetter>();
            foreach (IGameObjectFinderSetter setter in _setters)
            {
                setter.OnGameObjectFinderAwake(this);
            }
        }

        public T FindRootGameObject<T>(string name)
        {
            T result = default;
            foreach (GameObject rootObject in _rootObjectArray)
            {
                if (rootObject.name == name)
                {
                    result = rootObject.GetComponent<T>();
                    break;
                }
            }

            return result;
        }

        public IEnumerable<T> FindGameObjectOfType<T>()
        {
            _resultSet.Clear();
            List<T> result = new List<T>();
            // Singleton 을 상속받은 클래스들도 OnGameObjectFinderAwake 호출 대상에 포함시킨다.
            foreach (GameObject rootObject in _rootObjectArray_ofDontDestroyOnLoad)
            {
                foreach (T component in rootObject.GetComponentsInChildren<T>())
                {
                    string componentName = component.GetType().Name;
                    _resultSet.Add(componentName);
                    result.Add(component);
                }
            }

            foreach (GameObject rootObject in _rootObjectArray)
            {
                foreach (T component in rootObject.GetComponentsInChildren<T>())
                {
                    // 1. DontDestroyOnLoad 게임 오브젝트로에 동일한 컴포넌트가 존재한다면,
                    // 2. 이것은 Singleton 클래스를 상속받은 것이고, Awake() 에서 Destory 가 호출 될 것이기 때문에 제외한다.
                    string componentName = component.GetType().Name;
                    if (_resultSet.Contains(componentName))
                    {
                        continue;
                    }

                    result.Add(component);
                }
            }

            return result;
        }
    }
}
