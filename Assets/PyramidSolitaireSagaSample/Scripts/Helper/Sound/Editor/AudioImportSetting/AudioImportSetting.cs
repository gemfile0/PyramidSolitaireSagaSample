using UnityEditor;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper.Sound
{
    [CreateAssetMenu(fileName = "Audio Import Setting", menuName = "PyramidSolitaireSagaSample/Audio Import Setting")]
    public class AudioImportSetting : ScriptableObject
    {
        public bool forceToMono;
        // public bool normalize;
        public bool loadInBackground;
        public bool ambisonic;
        public AudioClipLoadType audioClipLoadType;
        public bool preloadAudioData;
        public AudioCompressionFormat audioCompressionFormat;
        public float quality;
        public AudioSampleRateSetting audioSampleRateSetting;
    }
}
