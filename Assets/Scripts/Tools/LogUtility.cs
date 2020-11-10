using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using XLua;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
}


public static class LogUtility
{
    static Regex stackRegex = new Regex(@"([^:]+):(\d+): (.*)");
    public static string GetCallStack(LuaFunction debugTraceback)
    {
        string result = null;
        var results = debugTraceback.Call();
        if (results.Length >= 1)
        {
            string stack = results[0] as string;
            var lines = stack.Split('\n').Skip(4).Select(s => s.Trim()).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var match = stackRegex.Match(line);
                if (match != null)
                {
                    var requirePath = match.Groups[1].Value;
                    var lineNumber = match.Groups[2].Value;
                    var message = match.Groups[3].Value;
                    string refPath = $"{Application.dataPath}/Scripts/{requirePath}.bytes";
                    if (requirePath.EndsWith(".bytes"))
                    {
                        refPath = $"{Application.dataPath}/Scripts/{requirePath}";
                    }
                    lines[i] = $"{message} () (at {refPath}:{lineNumber})";
                }
            }
            result = string.Join("\n  ", lines);
        }

        return result;
    }
    public static Color GetLogColor(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Debug:
                return new Color32(211, 211, 211, 255);
            case LogLevel.Info:
                return new Color32(173, 216, 230, 255);
            case LogLevel.Warning:
                return new Color32(255, 165, 0, 255);
            case LogLevel.Error:
                return new Color32(255, 0, 255, 255);
        }
        return Color.white;
    }
    delegate void LogAction(object message, UnityEngine.Object context = null);
    public static void LogImpl(LogLevel level, string tag, object message, UnityEngine.Object context = null)
    {
        LogAction action = null;
        switch (level)
        {
            case LogLevel.Debug:
                action = UnityEngine.Debug.Log;
                break;
            case LogLevel.Info:
                action = UnityEngine.Debug.Log;
                break;
            case LogLevel.Warning:
                action = UnityEngine.Debug.LogWarning;
                break;
            case LogLevel.Error:
                action = UnityEngine.Debug.LogError;
                break;
        }
#if UNITY_EDITOR
        var color = GetLogColor(level);

        string fullMessage = $"<color='#{ColorUtility.ToHtmlStringRGB(color)}'><b>[{level}|{tag}]</b></color>\n{message}";
#else
        string fullMessage = $"[{DateTime.Now:HH:mm:ss}|{level}|{tag}]\n{message}";
#endif
        action(fullMessage, context);
    }
    public static void LogDebug(string tag, object message, UnityEngine.Object context = null)
    {
        LogImpl(LogLevel.Debug, tag, message, context);
    }

    public static void LogInfo(string tag, object message, UnityEngine.Object context = null)
    {
        LogImpl(LogLevel.Info, tag, message, context);
    }

    public static void LogWarning(string tag, object message, UnityEngine.Object context = null)
    {
        LogImpl(LogLevel.Warning, tag, message, context);
    }

    public static void LogError(string tag, object message, UnityEngine.Object context = null)
    {
        LogImpl(LogLevel.Error, tag, message, context);
    }
    public static bool Assert(bool condition, object message, UnityEngine.Object context = null)
    {
        if (!condition)
        {
            LogImpl(LogLevel.Error, "Assert", message, context);
        }
        return condition;
    }
}