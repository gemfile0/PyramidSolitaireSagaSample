using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PyramidSolitaireSagaSample.System.PreferenceDataManager
{
    public interface IPreferenceRestorable
    {
        event Func<IPreferenceRestorable, string> requestRestorePreferenceData;
        PreferenceDataKey PreferenceDataKey { get; }
    }

    public interface IPreferenceSavable : IPreferenceRestorable
    {
        event Action<IPreferenceRestorable, string> requestSavePreferenceData;
    }

    public enum PreferenceDataKey
    {
        LevelFileManager,
        GameItems,
        GameBoosters,
    }

    public class PreferenceDataManager : MonoBehaviour,
                                         IGameObjectFinderSetter
    {
        private const string PreferenceDataKey = "PreferenceData";

        private IEnumerable<IPreferenceRestorable> _preferenceRestorableList;
        private IEnumerable<IPreferenceSavable> _preferenceSavableList;

        private StringBuilder _dataKeyBuilder;

        private void Awake()
        {
            _dataKeyBuilder = new StringBuilder();
        }

        public void OnGameObjectFinderAwake(IGameObjectFinder finder)
        {
            _preferenceRestorableList = finder.FindGameObjectOfType<IPreferenceRestorable>();
            _preferenceSavableList = _preferenceRestorableList.OfType<IPreferenceSavable>();
        }

        private void OnEnable()
        {
            foreach (IPreferenceSavable preferenceSavable in _preferenceSavableList)
            {
                preferenceSavable.requestSavePreferenceData += SavePreferenceData;
            }

            foreach (IPreferenceRestorable preferenceRestorable in _preferenceRestorableList)
            {
                preferenceRestorable.requestRestorePreferenceData += RestorePreferenceData;
            }
        }

        private void OnDisable()
        {
            foreach (IPreferenceSavable preferenceSavable in _preferenceSavableList)
            {
                preferenceSavable.requestSavePreferenceData -= SavePreferenceData;
            }

            foreach (IPreferenceRestorable preferenceRestorable in _preferenceRestorableList)
            {
                preferenceRestorable.requestRestorePreferenceData -= RestorePreferenceData;
            }
        }

        private string RestorePreferenceData(IPreferenceRestorable sender)
        {
            string dataKey = MakeDataKey(sender);
            string dataStr = PlayerPrefs.GetString(dataKey);
            Debug.Log($"RestorePreferenceData : {dataKey}, {dataStr}");
            return dataStr;
        }

        public void SavePreferenceData(IPreferenceRestorable sender, string dataStr)
        {
            string dataKey = MakeDataKey(sender);
            Debug.Log($"SavePreferenceData : {dataKey}, {dataStr}");
            PlayerPrefs.SetString(dataKey, dataStr);
        }

        private string MakeDataKey(IPreferenceRestorable sender)
        {
            _dataKeyBuilder.Length = 0;
            _dataKeyBuilder.Append(PreferenceDataKey);
            _dataKeyBuilder.Append(sender.PreferenceDataKey);
            return _dataKeyBuilder.ToString();
        }
    }
}
