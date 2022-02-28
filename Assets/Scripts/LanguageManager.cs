using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public enum eLanguages
{
    ENGLISH,
    CATALAN,
    SPANISH
}

public static class LanguageManager
{
    public static void ChangeLanguage(eLanguages language)
    {
        var localesAvaliable = LocalizationSettings.AvailableLocales.Locales;
        int languageNum = (int)language;

        if (languageNum >= localesAvaliable.Count || languageNum < 0)
        {
            Debug.LogError("Trying to change to a language out of bounds");
            return;
        }

        UserConfig.language = language;
        SaveUserConfig.SaveUserConfigData();

        LocalizationSettings.SelectedLocale = localesAvaliable[languageNum];
    }

    public static void NextLanguage()
    {
        UserConfig.language = (eLanguages)(((int)UserConfig.language + 1) % Enum.GetNames(typeof(eLanguages)).Length);
        ChangeLanguage(UserConfig.language);
    }

    public static void ChangeVariable(string variableName, string value)
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<PersistentVariablesSource>();

        var globalVariables = source["global"];

        if (globalVariables.ContainsKey(variableName))
            globalVariables.Remove(variableName);

        Debug.Log("Changing " + variableName + " to " + value);
        globalVariables.Add(variableName, new StringVariable { Value = value });
    }

    public static string GetTranslation(string TextTableId)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(TextTableId);
    }
}
