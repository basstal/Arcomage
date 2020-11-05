using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XLua;
public static class LocaleUtility
{
    private static string language;
    private static Dictionary<uint, string> localeMap = new Dictionary<uint, string>();

    public static string i18nReplacement = string.Empty;

    // ** todo 这里以后要用数据绑定？
    private static void OnGamePreferencesChanged(LuaTable table)
    {
        var tarLanguage = table.Get<string>("language");

        if (language != tarLanguage)
        {
            language = tarLanguage;
            i18nReplacement = "i18n/" + language + "/";

            byte[] bytes = File.ReadAllBytes("Data/Localization/localization_" + language);
            if (bytes != null)
            {
                localeMap.Clear();
                using (MemoryStream ms = new MemoryStream(bytes))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    while (ms.Position < ms.Length)
                    {
                        uint hash = br.ReadUInt32();
                        int length = br.ReadInt32();
                        byte[] chars = br.ReadBytes(length);
                        string text = System.Text.Encoding.UTF8.GetString(chars);
                        localeMap.Add(hash, text);
                    }
                }
            }
            else
            {
                LogUtility.LogError("Localization", string.Format("localization data for language {0} not found", language));
            }
        }
    }

    public static bool IsExist(string key, GameObject obj = null)
    {
        var temp = key;
        if (!localeMap.TryGetValue(CommonUtility.CalculateHash(key), out temp))
        {
            return false;
        }
        return true;
    }


    public static string GetString(string key)
    {
        var result = key;
        if (!localeMap.TryGetValue(CommonUtility.CalculateHash(key), out result))
        {
            LogUtility.LogWarning("Localization", $"Missing localization: {key}");
            return key;
        }
        return result;
    }

    public static string GetStringFormat(string key, params object[] args)
    {
        return string.Format(GetString(key), args);
    }
}