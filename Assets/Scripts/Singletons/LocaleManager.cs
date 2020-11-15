using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using XLua;
using XLua.LuaDLL;

[LuaCallCSharp]
public class LocaleManager : Singleton<LocaleManager>
{
    private Dictionary<uint, string> m_localeMap = new Dictionary<uint, string>();
    private static char[] _cbuffer = new char[128];

    public override async Task Init()
    {
        await base.Init();
        var luaManager = LuaManager.Instance;

        void LoadLocaleMap()
        {
            var envGlobal = luaManager.LuaEnv;
            var objs = envGlobal.DoString(@"return require('Data/LocaleMap')");
            if (objs != null && objs[0] is LuaTable localeMapT)
            {
                localeMapT.Get("EN", out LuaTable englishMapT);
                englishMapT?.ForEach((string k, string v) =>
                {
                    uint hashKey = CommonUtility.CalculateHash(k);
                    m_localeMap.Add(hashKey, v);
#if UNITY_EDITOR
                    Debug.Log($"hashKey :{hashKey}, v : {Regex.Replace(v, @"<[^>]*>", "")}");
#endif
                });
                englishMapT?.Dispose();
                localeMapT.Dispose();
            }
        }

        if (luaManager.LuaEnv != null)
        {
            LoadLocaleMap();
        }
        else
        {
            luaManager.OnInitFinished += LoadLocaleMap;
        }
    }

    public override async Task Uninit()
    {
        await base.Uninit();
#if UNITY_EDITOR
        Debug.Log("LocaleManager Uninit");
#endif
    }
    private static int CopyStrToBuffer(IntPtr L, int idx, ref char[] buffer)
    {
        IntPtr str = Lua.lua_tolstring(L, idx, out IntPtr strLen);
        int len = strLen.ToInt32();
        if (str != IntPtr.Zero)
        {
            if (len <= 0) return 0;
            if (buffer.Length < len)
            {
                Array.Resize(ref buffer, len);
            }
            unsafe
            {
                fixed (char* chars = buffer)
                {
                    len = Encoding.UTF8.GetChars((byte*)str, len, chars, buffer.Length);
                }
            }
        }
        return len;
    }

    private static string L_GetString(IntPtr L, int idx)
    {
        int len = CopyStrToBuffer(L, idx, ref _cbuffer);
        uint key = CommonUtility.CalculateHash(_cbuffer, len);
        string locale = GetString(key);
        if (locale != null)
        {
            if (Lua.lua_gettop(L) > idx)
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                locale = string.Format(locale, translator.GetParams<object>(L, 1 + idx));
            }
        }
        else
        {
            LogUtility.LogWarning("Localization", $"Missing localization: {Lua.lua_tostring(L, idx)}");
        }
        return locale ?? string.Empty;

    }

    [LuaCSFunction]
    public static int GetString(IntPtr L)
    {
        string str = L_GetString(L, 1);
        Lua.lua_pushstring(L, str);
        return 1;
    }

    public bool IsExist(string key)
    {
        return m_localeMap.ContainsKey(CommonUtility.CalculateHash(key));
    }
    private string GetString(string key)
    {
        if (!m_localeMap.TryGetValue(CommonUtility.CalculateHash(key), out var result))
        {
            LogUtility.LogWarning("Localization", $"Missing localization: {key}");
            return key;
        }
        return result;
    }

    private static string GetString(uint hashKey)
    {
        LocaleManager.Instance.m_localeMap.TryGetValue(hashKey, out var result);
        return result;
    }

    public string GetStringFormat(string key, params object[] args)
    {
        return string.Format(GetString(key), args);
    }
}