using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper.Sound
{
    [CustomEditor(typeof(AudioAtlas), true)]
    public class AudioAtlasInspector : Editor
    {
        private AudioAtlas _audioAtlas;
        private ReorderableList _settingList;

        private SerializedProperty _audioListsProp;
        private Dictionary<int, ReorderableList> _audioLists;
        private List<AudioImportSetting> _audioImportSettingList;

        private void OnEnable()
        {
            _audioAtlas = (AudioAtlas)target;

            if (_audioImportSettingList == null)
            {
                _audioImportSettingList = new();
            }
            _audioImportSettingList.Clear();

            string[] settingGuids = AssetDatabase.FindAssets("t:AudioImportSetting");
            foreach (string settingGuid in settingGuids)
            {
                string settingPath = AssetDatabase.GUIDToAssetPath(settingGuid);
                var setting = AssetDatabase.LoadAssetAtPath<AudioImportSetting>(settingPath);
                _audioImportSettingList.Add(setting);
            }

            if (_audioImportSettingList != null)
            {
                _audioAtlas.audioImportSettings = _audioImportSettingList.ToArray();

                // 추가 또는 삭제를 통해 세팅 파일의 개수에 맞게 AudioList를 맞춤
                // 1-1. 
                for (int i = 0; i < _audioAtlas.audioImportSettings.Length; i++)
                {
                    AudioImportSetting setting = _audioAtlas.audioImportSettings[i];
                    AudioList finding = _audioAtlas.audioLists
                                                  .FirstOrDefault((AudioList audioList) => audioList.name == setting.name);
                    if (finding != null) continue;

                    _audioAtlas.audioLists.Insert(i, new AudioList() { name = setting.name });
                }

                // 1-2. 
                for (int i = _audioAtlas.audioLists.Count - 1; i >= 0; i--)
                {
                    AudioList audioList = _audioAtlas.audioLists[i];
                    AudioImportSetting finding = _audioAtlas.audioImportSettings
                                                            .FirstOrDefault((AudioImportSetting setting) => setting.name == audioList.name);
                    if (finding != null) continue;

                    _audioAtlas.audioLists.RemoveAt(i);
                }
            }

            _settingList = new ReorderableList(serializedObject,
                                               serializedObject.FindProperty("audioImportSettings"),
                                               false, true, false, false);
            _settingList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Audio Import Settings");
            };
            _settingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index == 0) EditorGUILayout.Separator();

                GUI.enabled = false;
                SerializedProperty element = _settingList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, GUIContent.none);
                GUI.enabled = true;

                DrawAudioList(index, element.objectReferenceValue);
            };
            _settingList.elementHeight = EditorGUIUtility.singleLineHeight;

            _audioListsProp = serializedObject.FindProperty("audioLists");
            _audioLists = new Dictionary<int, ReorderableList>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty property = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(property, includeChildren: true, options: new GUILayoutOption[0]);
            GUI.enabled = true;

            _settingList.DoLayoutList();

            EditorGUILayout.Separator();
            if (GUILayout.Button("Apply"))
            {
                AudioAtlasApply.Do(_audioAtlas);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAudioList(int settingIndex, Object target)
        {
            if (target == null || settingIndex >= _audioListsProp.arraySize) return;

            if (_audioLists.ContainsKey(settingIndex) == false)
            {
                SerializedProperty audioListProp = _audioListsProp.GetArrayElementAtIndex(settingIndex);

                _audioLists.Add(settingIndex,
                               new ReorderableList(serializedObject,
                                                   audioListProp.FindPropertyRelative("assets"),
                                                   true, true, true, true));
            }

            ReorderableList audioList = _audioLists[settingIndex];
            audioList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, target.name);
            };
            audioList.drawElementCallback = (Rect rect, int audioIndex, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = audioList.serializedProperty.GetArrayElementAtIndex(audioIndex);
                if (rect.Contains(Event.current.mousePosition))
                {
                    PerformDrag(element);
                }

                EditorGUI.BeginChangeCheck();
                EditorGUI.ObjectField(rect, element, GUIContent.none);
                if (EditorGUI.EndChangeCheck() &&
                    element.objectReferenceValue != null &&
                    IsObjectAcceptable(element.objectReferenceValue) == false)
                {
                    element.objectReferenceValue = null;
                }
            };
            audioList.elementHeight = EditorGUIUtility.singleLineHeight;
            audioList.DoLayoutList();
        }

        private void PerformDrag(SerializedProperty element)
        {
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    Object draggingObject = (Object)DragAndDrop.objectReferences[0];
                    bool isAcceptableType = IsObjectAcceptable(draggingObject);
                    DragAndDrop.visualMode = (isAcceptableType) ?
                                             DragAndDropVisualMode.Copy :
                                             DragAndDropVisualMode.Rejected;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        element.objectReferenceValue = draggingObject;
                    }

                    Event.current.Use();
                    break;
            }
        }

        private bool IsObjectAcceptable(Object target)
        {
            return target is AudioClip ||
                   (target is DefaultAsset && IsDirectory(target));
        }

        private bool IsDirectory(Object target)
        {
            string filePath = AssetDatabase.GetAssetPath(target);
            FileAttributes attr = File.GetAttributes(filePath);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}
