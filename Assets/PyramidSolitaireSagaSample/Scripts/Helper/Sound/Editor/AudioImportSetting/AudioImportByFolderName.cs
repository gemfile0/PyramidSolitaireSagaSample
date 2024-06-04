using UnityEditor;

namespace PyramidSolitaireSagaSample.Helper.Sound
{
    public class AudioImportByFolderName : AssetPostprocessor
    {
#if SLOTFACTORY && (UNITY_2017 || UNITY_2018_1_OR_NEWER)
        void OnPreprocessAudio()
        {
            string folderName = Path.GetFileName(Path.GetDirectoryName(assetPath));
            while (!string.IsNullOrEmpty(folderName))
            {
                string[] presetGuids = AssetDatabase.FindAssets("t:AudioImportSetting");
                string[] presetPaths = presetGuids.Select(presetGuid => AssetDatabase.GUIDToAssetPath(presetGuid)).ToArray();
                string presetPathFound = presetPaths.FirstOrDefault(presetPath => presetPath.Contains(folderName));
                if (presetPathFound == null)
                {
                    // Debug.LogWarning(string.Format("{0} 폴더와 같은 이름의 프리셋이 존재하지 않습니다.", folderName));
                    return; 
                }

                AudioImportSetting preset = AssetDatabase.LoadAssetAtPath<AudioImportSetting>(presetPathFound);
                
                ApplySetting((AudioImporter)assetImporter, preset);

                Debug.Log(string.Format("{0} 오디오 파일에 {1} 프리셋이 자동으로 적용되었습니다.", Path.GetFileName(assetPath), Path.GetFileName(presetPathFound)));
                return;
            }
        }

        private void ApplySetting(AudioImporter audioImporter, AudioImportSetting setting)
		{
			audioImporter.forceToMono = setting.forceToMono;
			audioImporter.loadInBackground = setting.loadInBackground;
			audioImporter.ambisonic = setting.ambisonic;

			AudioImporterSampleSettings audioImporterSampleSettings = audioImporter.defaultSampleSettings;
			audioImporterSampleSettings.loadType = setting.audioClipLoadType;
			if (audioImporterSampleSettings.loadType != AudioClipLoadType.Streaming)
			{
#if UNITY_2022_2_OR_NEWER
                audioImporterSampleSettings.preloadAudioData = setting.preloadAudioData;
#elif UNITY_2017 || UNITY_2018_1_OR_NEWER
				audioImporter.preloadAudioData = setting.preloadAudioData;
#endif
			}
			
			audioImporterSampleSettings.compressionFormat = setting.audioCompressionFormat;
			if (audioImporterSampleSettings.compressionFormat == AudioCompressionFormat.Vorbis)
			{
				audioImporterSampleSettings.quality = setting.quality / 100;
			}
			audioImporterSampleSettings.sampleRateSetting = setting.audioSampleRateSetting;

			audioImporter.defaultSampleSettings = audioImporterSampleSettings;
		}
#endif
    }
}