using PyramidSolitaireSagaSample.LevelPlayer.CardCollector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class GameObjectFinderPerformanceTest : MonoBehaviour
{
    private GameObject[] _rootObjectArray;
    private HashSet<string> _resultSet = new HashSet<string>();
    private Dictionary<Type, List<MonoBehaviour>> _interfaceToComponentsMap;

    void Start()
    {
        _rootObjectArray = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        PopulateInterfaceToComponentsMap();

        TestPerformance<ICardCollectRequester>();
    }

    private void PopulateInterfaceToComponentsMap()
    {
        _interfaceToComponentsMap = new Dictionary<Type, List<MonoBehaviour>>();
        var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();

        foreach (var component in allMonoBehaviours)
        {
            Type[] interfaces = component.GetType().GetInterfaces();
            foreach (Type @interface in interfaces)
            {
                if (!_interfaceToComponentsMap.ContainsKey(@interface))
                {
                    _interfaceToComponentsMap[@interface] = new List<MonoBehaviour>();
                }
                _interfaceToComponentsMap[@interface].Add(component);
            }
        }
    }

    public IEnumerable<T> FindGameObjectOfType<T>()
    {
        List<T> result = new List<T>();
        foreach (GameObject rootObject in _rootObjectArray)
        {
            foreach (T component in rootObject.GetComponentsInChildren<T>())
            {
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

    void TestPerformance<T>() where T : class
    {
        int iterations = 100;
        long dictionaryMethodTime = 0;
        long customMethodTime = 0;
        bool areResultsEqual = true;

        Stopwatch stopwatch = new Stopwatch();

        // Measure dictionary method
        List<T> dictionaryResult = new List<T>();
        for (int i = 0; i < iterations; i++)
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int j = 0; j < 10; j++)
            {
                dictionaryResult.Clear();
                if (_interfaceToComponentsMap.ContainsKey(typeof(T)))
                {
                    foreach (var component in _interfaceToComponentsMap[typeof(T)])
                    {
                        dictionaryResult.Add(component as T);
                    }
                }
                // Do something with dictionaryResult if needed
            }
            stopwatch.Stop();
            dictionaryMethodTime += stopwatch.ElapsedTicks;
        }

        // Measure custom method
        List<T> customResult = new List<T>();
        for (int i = 0; i < iterations; i++)
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (int j = 0; j < 10; j++)
            {
                customResult = FindGameObjectOfType<T>().ToList();
                // Do something with customResult if needed
            }
            stopwatch.Stop();
            customMethodTime += stopwatch.ElapsedTicks;
        }

        // Compare the results from both methods
        if (dictionaryResult.Count != customResult.Count || !dictionaryResult.SequenceEqual(customResult))
        {
            areResultsEqual = false;
        }

        long dictionaryMethodAverageTicks = dictionaryMethodTime / iterations;
        long customMethodAverageTicks = customMethodTime / iterations;
        double dictionaryMethodAverageMilliseconds = dictionaryMethodAverageTicks * 1000.0 / Stopwatch.Frequency;
        double customMethodAverageMilliseconds = customMethodAverageTicks * 1000.0 / Stopwatch.Frequency;

        UnityEngine.Debug.Log($"Dictionary Method Average Time: {dictionaryMethodAverageMilliseconds} ms");
        UnityEngine.Debug.Log($"Custom Method Average Time: {customMethodAverageMilliseconds} ms");
        UnityEngine.Debug.Log($"Results are equal: {areResultsEqual}");
    }
}
