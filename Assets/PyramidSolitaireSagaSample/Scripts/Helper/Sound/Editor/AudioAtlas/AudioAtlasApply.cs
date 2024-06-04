
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper.Sound
{
    public static class AudioAtlasApply
    {
        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;

        }

        public static void Do(AudioAtlas audioAtlas)
        {
            foreach (AudioList audioList in audioAtlas.audioLists)
            {
                if (audioAtlas.audioImportSettings == null)
                {
                    Debug.LogWarningFormat("==== {0} 에 포함된 AudioImportSetting 배열이 비어 있습니다. ", audioList.name);
                    continue;
                }
                AudioImportSetting setting = audioAtlas.audioImportSettings.FirstOrDefault(finding => finding != null && finding.name == audioList.name);
                if (setting == null)
                {
                    Debug.LogWarningFormat("==== {0} 과 일치하는 AudioImportSetting 이 없습니다.", audioList.name);
                    continue;
                }


                // 오디오 클립의 경로를 모읍니다.
                List<string> allAudioPaths = new List<string>();
                foreach (Object asset in audioList.assets)
                {
                    if (asset is DefaultAsset)
                    {
                        var folderPath = AssetDatabase.GetAssetPath(asset);
                        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new string[] { folderPath });
                        string[] audioPaths = audioGuids.Select(presetGuid => AssetDatabase.GUIDToAssetPath(presetGuid)).ToArray();
                        allAudioPaths.AddRange(audioPaths);
                    }
                    else if (asset is AudioClip)
                    {
                        allAudioPaths.Add(AssetDatabase.GetAssetPath(asset));
                    }
                }

                // 세팅을 적용합니다.
                foreach (string audioPath in allAudioPaths)
                {
                    AudioImporter audioImporter = AssetImporter.GetAtPath(audioPath) as AudioImporter;
                    if (ApplySetting(audioImporter, setting))
                    {
                        AssetDatabase.ImportAsset(audioPath);
                        Debug.Log($"{Path.GetFileName(audioPath)} 오디오 파일에 {Path.GetFileName(AssetDatabase.GetAssetPath(setting))} 프리셋을 수동으로 적용했습니다.");
                    }
                }
            }
        }

        private static bool ApplySetting(AudioImporter audioImporter, AudioImportSetting setting)
        {
            bool result = false;

            if (audioImporter.forceToMono != setting.forceToMono)
            {
                audioImporter.forceToMono = setting.forceToMono;

                // Import 를 덜 하기 위해 일일이 체크
                result = true;
            }

            if (audioImporter.loadInBackground != setting.loadInBackground)
            {
                audioImporter.loadInBackground = setting.loadInBackground;
                result = true;
            }

            if (audioImporter.ambisonic != setting.ambisonic)
            {
                audioImporter.ambisonic = setting.ambisonic;
                result = true;
            }

            AudioImporterSampleSettings audioImporterSampleSettings = audioImporter.defaultSampleSettings;
            if (audioImporterSampleSettings.loadType != setting.audioClipLoadType)
            {
                audioImporterSampleSettings.loadType = setting.audioClipLoadType;
                result = true;
            }

            if (audioImporterSampleSettings.loadType != AudioClipLoadType.Streaming)
            {
#if UNITY_2022_2_OR_NEWER
                if (audioImporterSampleSettings.preloadAudioData != setting.preloadAudioData)
                {
                    audioImporterSampleSettings.preloadAudioData = setting.preloadAudioData;
                    result = true;
                }
#elif UNITY_2017 || UNITY_2018_1_OR_NEWER
				if (audioImporter.preloadAudioData != setting.preloadAudioData)
                {
					audioImporter.preloadAudioData = setting.preloadAudioData;
					result = true;
                }
#endif

            }

            if (audioImporterSampleSettings.compressionFormat != setting.audioCompressionFormat)
            {
                audioImporterSampleSettings.compressionFormat = setting.audioCompressionFormat;
                result = true;
            }

            if (audioImporterSampleSettings.compressionFormat == AudioCompressionFormat.Vorbis)
            {
                float newQuality = setting.quality / 100;
                if (audioImporterSampleSettings.quality != newQuality)
                {
                    audioImporterSampleSettings.quality = newQuality;
                    result = true;
                }
            }

            if (audioImporterSampleSettings.sampleRateSetting != setting.audioSampleRateSetting)
            {
                audioImporterSampleSettings.sampleRateSetting = setting.audioSampleRateSetting;
                result = true;
            }

            audioImporter.defaultSampleSettings = audioImporterSampleSettings;

            return result;
        }
    }
}