using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonLoader : MonoBehaviour
{
    public GameObject[] singletons;
    private void Awake()
    {
        InitSingletons();
    }

    private async void InitSingletons()
    {
        foreach (var singleton in singletons.Where(v => v != null))
        {
            var singletonBase = singleton.GetComponent<SingletonBase>();
            if (singletonBase == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"SingletonLoader, {singleton} illegal.");
#endif
                continue;
            }
            await singletonBase.Init();
        }
    }
    private async void UninitSingletons()
    {
        foreach(var singleton in singletons.Where(v => v != null))
        {
            var singletonBase = singleton.GetComponent<SingletonBase>();
            await singletonBase.Uninit();
        }
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
    public void ReloadScene()
    {
#if UNITY_EDITOR
        ClearLog();
#endif
        SceneManager.LoadSceneAsync("GamePlay");
    }
    private void OnDestroy()
    {
        UninitSingletons();
    }
}