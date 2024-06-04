using UnityEditor;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper.Sound
{
    [CustomEditor(typeof(AudioImportSetting))]
    public class AudioImportSettingInspector : Editor
    {
        private SerializedProperty forceToMono;
        // private SerializedProperty normalize;
        private SerializedProperty loadInBackground;
        private SerializedProperty ambisonic;
        private SerializedProperty audioClipLoadType;
        private SerializedProperty preloadAudioData;
        private SerializedProperty audioCompressionFormat;
        private SerializedProperty quality;
        private SerializedProperty audioSampleRateSetting;

        private void OnEnable()
        {
            forceToMono = serializedObject.FindProperty("forceToMono");
            // normalize = serializedObject.FindProperty("normalize");
            loadInBackground = serializedObject.FindProperty("loadInBackground");
            ambisonic = serializedObject.FindProperty("ambisonic");
            audioClipLoadType = serializedObject.FindProperty("audioClipLoadType");
            preloadAudioData = serializedObject.FindProperty("preloadAudioData");
            audioCompressionFormat = serializedObject.FindProperty("audioCompressionFormat");
            quality = serializedObject.FindProperty("quality");
            audioSampleRateSetting = serializedObject.FindProperty("audioSampleRateSetting");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //
            EditorGUILayout.PropertyField(forceToMono);
            //
            EditorGUI.BeginDisabledGroup(forceToMono.boolValue == false);
            // EditorGUILayout.PropertyField(normalize);
            EditorGUI.EndDisabledGroup();
            //
            EditorGUILayout.PropertyField(loadInBackground);
            //
            EditorGUILayout.PropertyField(ambisonic);

            EditorGUILayout.Separator();

            //
            EditorGUILayout.PropertyField(audioClipLoadType);
            //
            EditorGUI.BeginDisabledGroup((AudioClipLoadType)audioClipLoadType.enumValueIndex == AudioClipLoadType.Streaming);
            EditorGUILayout.PropertyField(preloadAudioData);
            EditorGUI.EndDisabledGroup();
            //
            EditorGUILayout.PropertyField(audioCompressionFormat);
            //
            if ((AudioCompressionFormat)audioCompressionFormat.enumValueIndex == AudioCompressionFormat.Vorbis)
            {
                EditorGUILayout.Slider(quality, 0f, 100f);
            }
            //
            EditorGUILayout.PropertyField(audioSampleRateSetting);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
