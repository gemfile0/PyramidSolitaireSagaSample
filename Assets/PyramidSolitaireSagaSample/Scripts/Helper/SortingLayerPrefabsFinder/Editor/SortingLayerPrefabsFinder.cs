using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper
{
    public class SortingLayerPrefabsFinder : EditorWindow
    {
        private string _sortingLayerName = "Default";
        private List<GameObject> _foundPrefabs = new List<GameObject>();

        [MenuItem("PyramidSolitaireSagaSample/Sorting Layer Prefabs Finder", false, 1201)]
        public static void ShowWindow()
        {
            GetWindow<SortingLayerPrefabsFinder>("Sorting Layer Prefabs Finder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Sorting Layer Prefabs Finder", EditorStyles.boldLabel);
            _sortingLayerName = EditorGUILayout.TextField("Sorting Layer Name", _sortingLayerName);

            if (GUILayout.Button("Find Prefabs"))
            {
                FindPrefabs();
            }

            if (_foundPrefabs.Count > 0)
            {
                GUILayout.Label("Found Prefabs:", EditorStyles.boldLabel);

                foreach (GameObject prefab in _foundPrefabs)
                {
                    EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
                }
            }
        }

        private void FindPrefabs()
        {
            _foundPrefabs.Clear();

            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefabGUID in allPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGUID);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (PrefabHasSortingLayer(prefab, _sortingLayerName))
                {
                    _foundPrefabs.Add(prefab);
                }
            }

            if (_foundPrefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("Result", "No prefabs found with the specified sorting layer.", "OK");
            }
        }

        private bool PrefabHasSortingLayer(GameObject prefab, string layerName)
        {
            SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer.sortingLayerName == layerName)
                {
                    return true;
                }
            }

            return false;
        }
    }

}
