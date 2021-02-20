using UnityEngine.Events;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using XLua;
using DG.Tweening;

[LuaCallCSharp]
public static class CommonUtility
{
    public static uint CalculateHash(string str)
    {
        uint result = 0x01234567u;
        for (int i = 0; i < str.Length; ++i)
            result = (result ^ ((uint)str[i] & 0xFF)) * 0x89ABCDEFu;
        return result * 0x89ABCDEFu;
    }
    public static uint CalculateHash(char[] chars, int length)
    {
        uint result = 0x01234567u;
        for (int i = 0; i < length; ++i)
        {
            var b = (uint)chars[i];
            result = (result ^ (b & 0xFF)) * 0x89ABCDEFu;
        }

        return result * 0x89ABCDEFu;
    }
#if UNITY_EDITOR
    [BlackList]
    public static void WriteToFile(string path, string content)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;
        content = content.Replace("\r", "");
        var dirty = !File.Exists(path);
        if (!dirty)
        {
            dirty = File.ReadAllText(path) != content;
        }
        Debug.LogWarning($"dirty : {dirty}");
        if (dirty)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, content);
            }
        }
    }
#endif
    public static void SetEventHandler(UnityEvent @event, UnityAction callback)
    {
        @event.RemoveAllListeners();
        @event.AddListener(callback);
    }

    public static void SetTweenOnComplete(DOTweenAnimation animation, LuaFunction callback)
    {
        if (LuaManager.Instance == null) return;
        LuaManager.Instance.disposableCallbacks.Add(callback);
        animation.onComplete.RemoveAllListeners();
        animation.onComplete.AddListener(() =>
        {
            callback.Call();
        });
    }
    static int HexToInt(char hex)
    {
        switch (hex)
        {
            case '0': return 0;
            case '1': return 1;
            case '2': return 2;
            case '3': return 3;
            case '4': return 4;
            case '5': return 5;
            case '6': return 6;
            case '7': return 7;
            case '8': return 8;
            case '9': return 9;
            case 'A': return 10;
            case 'B': return 11;
            case 'C': return 12;
            case 'D': return 13;
            case 'E': return 14;
            case 'F': return 15;
            case 'a': return 10;
            case 'b': return 11;
            case 'c': return 12;
            case 'd': return 13;
            case 'e': return 14;
            case 'f': return 15;
        }
        return 15;
    }
    public static Color32 HexCharsToColor(char[] hexChars, int startIndex, int length)
    {
        if (length == 7)
        {
            byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
            byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
            byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));

            return new Color32(r, g, b, 255);
        }
        else if (length == 9)
        {
            byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
            byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
            byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
            byte a = (byte)(HexToInt(hexChars[startIndex + 7]) * 16 + HexToInt(hexChars[startIndex + 8]));

            return new Color32(r, g, b, a);
        }

        return Color.white;
    }
    #region Object
    public static bool IsNull(this UnityEngine.Object o)
    {
        return o == null;
    }
    #endregion
    #region GameObject
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T t = go.GetComponent<T>();
        if (t == null) t = go.AddComponent<T>();
        return t;
    }
    #endregion
    #region Delegate
    public static void SafeInvoke(this Action func)
    {
        try { func?.Invoke(); }
        catch (Exception e) { LogUtility.LogError("Error", e); }
    }

    public static void SafeInvoke<T1>(this Action<T1> func, T1 arg1)
    {
        try { func?.Invoke(arg1); }
        catch (Exception e) { LogUtility.LogError("Error", e); }
    }

    public static void SafeInvoke<T1, T2>(this Action<T1, T2> func, T1 arg1, T2 arg2)
    {
        try { func?.Invoke(arg1, arg2); }
        catch (Exception e) { LogUtility.LogError("Error", e); }
    }
    #endregion
    #region GameObject/UI
    public static void BindButtonEvent(this GameObject target, UnityAction callback)
    {
        if (target != null)
        {
            SetEventHandler(target.GetOrAddComponent<Button>().onClick, callback);
            target.SetGraphicRaycastTarget(true);
        }
    }

    public static void InvokeButtonEvent(this GameObject target)
    {
        if (target != null)
        {
            var onClick = target.GetComponent<Button>()?.onClick;
            onClick?.Invoke();
        }
    }
    public static void SetGraphicRaycastTarget(this GameObject target, bool value)
    {
        if (target.TryGetComponent(out Graphic graphic))
        {
            graphic.raycastTarget = value;
        }
    }
    #endregion

    #region UGUI
    public static void SetAlpha(this Graphic graphic, float val)
    {
        Color c = graphic.color;
        c.a = Mathf.Clamp(val, 0, 1);
        graphic.color = c;
    }
    public static void SetColor(this Graphic graphic, uint val)
    {
        float inv = 1f / 255f;
        Color c = Color.black;
        c.r = inv * ((val >> 24) & 0xFF);
        c.g = inv * ((val >> 16) & 0xFF);
        c.b = inv * ((val >> 8) & 0xFF);
        c.a = inv * (val & 0xFF);
        graphic.color = c;
    }
    #endregion
}