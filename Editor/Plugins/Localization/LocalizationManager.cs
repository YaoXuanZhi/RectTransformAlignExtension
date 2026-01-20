using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace L10n.Editor
{
    public static class LocalizationManager
    {
        private static Dictionary<string, string> _currentLanguageStringDict;

        public static string CurrentLanguage
        {
            get => EditorPrefs.GetString(LocalizationDefine.LANGUAGE_PREF_KEY, LocalizationDefine.DEFAULT_LANGUAGE);
            set => EditorPrefs.SetString(LocalizationDefine.LANGUAGE_PREF_KEY, value);
        }

        public static void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            LoadLanguage();
        }

        private static string GetCurrentDirectoryPath()
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(0);
            string scriptPath = frame.GetFileName();
            return Path.GetDirectoryName(scriptPath);
        }

        public static void LoadLanguage()
        {
            _currentLanguageStringDict = new Dictionary<string, string>();

            string filePath = $"{GetCurrentDirectoryPath()}/L10n/{CurrentLanguage}.json";

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                
                var languageData = JsonUtility.FromJson<LanguageData>(json);

                foreach (var entry in languageData.entries)
                {
                    _currentLanguageStringDict[entry.key] = entry.value;
                }
            }
            else
            {
                Debug.LogWarning($"Language file not found: {filePath}");
            }
        }

        public static string GetString(string key, string defaultValue = "")
        {
            if (_currentLanguageStringDict == null)
            {
                LoadLanguage();
            }

            if (_currentLanguageStringDict != null && _currentLanguageStringDict.TryGetValue(key, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        [System.Serializable]
        private class LanguageData
        {
            public List<LanguageEntry> entries;
        }

        [System.Serializable]
        private class LanguageEntry
        {
            public string key;
            public string value;
        }
        
        [System.Serializable]
        public class DictionaryData<TKey, TValue>
        {
            public List<TKey> Keys;
            public List<TValue> Values;

            public IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
            {
                if (Keys == null || Values == null)
                    yield break;

                for (var n = 0; n < Keys.Count; n++)
                    yield return new KeyValuePair<TKey, TValue>(Keys[n], Values[n]);
            }

            public DictionaryData()
                : this(new List<TKey>(), new List<TValue>())
            {
            }

            public DictionaryData(List<TKey> keys, List<TValue> values)
            {
                Keys = keys;
                Values = values;
            }

            public DictionaryData(IEnumerable<KeyValuePair<TKey, TValue>> data)
            {
                var pairs = data as KeyValuePair<TKey, TValue>[] ?? data.ToArray();
                Keys = pairs.Select(n => n.Key).ToList();
                Values = pairs.Select(n => n.Value).ToList();
            }
        }
    }

    //借鉴Qt多语言思路简化调用接口
    public static class L10nHelper
    {
        public static string Tr(string key) => LocalizationManager.GetString(key, key);
    }
}