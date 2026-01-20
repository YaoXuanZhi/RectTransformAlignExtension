using UnityEditor;

namespace L10n.Editor
{
    public class LocalizationProvider
    {
        private static readonly string[] _languageOptions = LocalizationDefine.LanguageOptions;
        private static readonly string[] _languageCodes = LocalizationDefine.LanguageCodes;
        private int _selectedLanguage = -1;

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            if (_selectedLanguage < 0)
            {
                int lastLanguageIndex = 0;
                for (int i = 0; i < _languageCodes.Length; i++)
                {
                    if (LocalizationManager.CurrentLanguage == _languageCodes[i])
                    {
                        lastLanguageIndex = i;
                        break;
                    }
                }

                _selectedLanguage = lastLanguageIndex;
            }

            _selectedLanguage = EditorGUILayout.Popup("Language", _selectedLanguage, _languageOptions);
            if (EditorGUI.EndChangeCheck())
            {
                LocalizationManager.SetLanguage(_languageCodes[_selectedLanguage]);
            }
        }
    }
}
