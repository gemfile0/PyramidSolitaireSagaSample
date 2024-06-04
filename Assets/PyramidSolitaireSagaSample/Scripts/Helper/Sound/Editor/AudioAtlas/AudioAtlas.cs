using System;
using System.Collections.Generic;
using UnityEngine;

namespace PyramidSolitaireSagaSample.Helper.Sound
{
    [CreateAssetMenu(fileName = "AudioAtlas", menuName = "PyramidSolitaireSagaSample/Audio Atlas")]
    public class AudioAtlas : ScriptableObject
    {
        public AudioImportSetting[] audioImportSettings;
        public List<AudioList> audioLists = new List<AudioList>();
    }

    [Serializable]
    public class AudioList
    {
        public string name;
        public UnityEngine.Object[] assets;
    }
}
