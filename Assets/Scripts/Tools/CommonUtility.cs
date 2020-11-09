using UnityEngine.Events;
using UnityEngine;
using System;
using UnityEngine.UI;
using XLua;

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
    public static void SetEventHandler(UnityEvent @event, UnityAction callback)
    {
        @event.RemoveAllListeners();
        @event.AddListener(callback);
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

    public static Color GetColorFromColor32(int r, int g, int b, int a)
    {
        return (Color)new Color32((byte)r, (byte)g, (byte)b, (byte)a);
    }
    public static Sprite GetSpriteFromTexture2D(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
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
    public static void SetGraphicRaycastTarget(this GameObject target, bool value)
    {
        Graphic graphic;
        if (target.TryGetComponent<Graphic>(out graphic))
        {
            graphic.raycastTarget = value;
        }
    }
    #endregion
}