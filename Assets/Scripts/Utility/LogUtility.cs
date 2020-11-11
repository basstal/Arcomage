using UnityEngine;
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
            if (results[0] is string stack)
            {
                var lines = stack.Split('\n').Skip(4).Select(s => s.Trim()).ToArray();
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var match = stackRegex.Match(line);
                    var requirePath = match.Groups[1].Value;
                    var lineNumber = match.Groups[2].Value;
                    var message = match.Groups[3].Value;
                    // ** TODO make it can be clicked on console
                    lines[i] = $"{message} () (at {requirePath}:{lineNumber})";
                }
                result = string.Join("\n  ", lines);
            }
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
    delegate void LogAction(object message, Object context = null);
    public static void LogImpl(LogLevel level, string tag, object message, Object context = null)
    {
        LogAction action = Debug.Log;
        switch (level)
        {
            case LogLevel.Debug:
                action = Debug.Log;
                break;
            case LogLevel.Info:
                action = Debug.Log;
                break;
            case LogLevel.Warning:
                action = Debug.LogWarning;
                break;
            case LogLevel.Error:
                action = Debug.LogError;
                break;
        }
#if UNITY_EDITOR
        var color = GetLogColor(level);

        string fullMessage = $"<color='#{ColorUtility.ToHtmlStringRGB(color)}'><b>[{level}|{tag}]</b></color>\n{message}";
#else
        string fullMessage = $"[{level}|{tag}]\n{message}";
#endif
        action(fullMessage, context);
    }
    public static void LogDebug(string tag, object message, Object context = null)
    {
        LogImpl(LogLevel.Debug, tag, message, context);
    }

    public static void LogInfo(string tag, object message, Object context = null)
    {
        LogImpl(LogLevel.Info, tag, message, context);
    }

    public static void LogWarning(string tag, object message, Object context = null)
    {
        LogImpl(LogLevel.Warning, tag, message, context);
    }

    public static void LogError(string tag, object message, Object context = null)
    {
        LogImpl(LogLevel.Error, tag, message, context);
    }
    public static bool Assert(bool condition, object message, Object context = null)
    {
        if (!condition)
        {
            LogImpl(LogLevel.Error, "Assert", message, context);
        }
        return condition;
    }
}