using UnityEngine;
using Object = UnityEngine.Object;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
}

namespace GameScripts.Utils
{
    public static class Log
    {
        public static Color GetLogColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return Color.green;
                case LogLevel.Info:
                    return Color.green;
                case LogLevel.Warning:
                    return Color.yellow;
                case LogLevel.Error:
                    return Color.red;
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
}