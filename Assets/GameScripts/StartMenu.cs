using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameScripts
{
    public class StartMenu : MonoBehaviour
    {
        public void OnReload()
        {
#if UNITY_EDITOR
            ClearLog();
#endif
            SceneManager.LoadSceneAsync("GamePlay");
        }
#if UNITY_EDITOR
        static void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method?.Invoke(new object(), null);
        }
#endif
    }
}