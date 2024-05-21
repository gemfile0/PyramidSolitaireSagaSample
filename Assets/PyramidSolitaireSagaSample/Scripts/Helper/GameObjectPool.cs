using UnityEngine;
using UnityEngine.Pool;

namespace PyramidSolitaireSagaSample.Helper
{
    public enum GameObjectPoolType
    {
        Stack,
        LinkedList,
    }

    public class GameObjectPool<T> where T : Component
    {
        private IObjectPool<T> _pool;
        private GameObject _prefab;
        private Transform _rootTransform;

        public GameObjectPool(
            Transform parent,
            GameObject prefab,
            GameObjectPoolType poolType = GameObjectPoolType.Stack,
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10
        )
        {
            _prefab = prefab;

            var rootObject = new GameObject($"{prefab.name}Pool");
            rootObject.SetActive(false);
            _rootTransform = rootObject.transform;
            _rootTransform.SetParent(parent, false);

            if (poolType == GameObjectPoolType.Stack)
            {
                _pool = new ObjectPool<T>(OnCreate, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize);
            }
            else
            {
                _pool = new LinkedPool<T>(OnCreate, OnGet, OnRelease, OnDestroy, collectionCheck, maxSize);
            }
        }

        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T item)
        {
            item.transform.SetParent(_rootTransform, false);
            _pool.Release(item);
        }

        private T OnCreate()
        {
            GameObject go = GameObject.Instantiate(_prefab);
            go.transform.SetParent(_rootTransform, false);
            T component = go.GetComponent<T>();
            return component;
        }

        // Called when an item is taken from the pool using Get
        private void OnGet(T component)
        {

        }

        // Called when an item is returned to the pool using Release
        private void OnRelease(T component)
        {

        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        private void OnDestroy(T component)
        {

        }
    }
}
