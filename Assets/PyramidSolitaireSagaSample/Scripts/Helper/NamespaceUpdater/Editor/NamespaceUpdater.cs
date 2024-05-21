using System.IO;
using UnityEditor;
using UnityEngine;

namespace PyramidSolitaireSagaSample
{
    public class NamespaceUpdater
    {
        private const string OldNamespace = "SolitaireMakeover";
        private const string NewNamespace = "PyramidSolitaireSagaSample";

        [MenuItem("PyramidSolitaireSagaSample/Update Namespaces in All Relevant Files", false, 1010)]
        public static void UpdateNamespaces()
        {
            string projectPath = $"{Application.dataPath}/../";
            string assetsPath = $"{projectPath}Assets";
            string projectSettingsPath = $"{projectPath}ProjectSettings";
            UpdateFiles(assetsPath, "*.prefab", OldNamespace, NewNamespace);
            UpdateFiles(assetsPath, "*.unity", OldNamespace, NewNamespace);
            UpdateFiles(assetsPath, "*.cs", OldNamespace, NewNamespace, "NamespaceUpdater.cs");
            UpdateFiles(assetsPath, "*.asset", OldNamespace, NewNamespace);
            UpdateFiles(projectSettingsPath, "*.asset", OldNamespace, NewNamespace);
        }

        private static void UpdateFiles(string baseDirectory, string searchPattern, string oldNamespace, string newNamespace, string exclude = null)
        {
            baseDirectory = Path.GetFullPath(baseDirectory);

            string[] allFiles = Directory.GetFiles(baseDirectory, searchPattern, SearchOption.AllDirectories);
            foreach (var filePath in allFiles)
            {
                if (!string.IsNullOrEmpty(exclude) && Path.GetFileName(filePath) == exclude)
                {
                    Debug.Log($"Skipped: {filePath}");
                    continue;
                }

                string content = File.ReadAllText(filePath);
                if (content.Contains(oldNamespace))
                {
                    content = content.Replace(oldNamespace, newNamespace);
                    File.WriteAllText(filePath, content);
                    Debug.Log($"Updated: {filePath}");
                }
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}