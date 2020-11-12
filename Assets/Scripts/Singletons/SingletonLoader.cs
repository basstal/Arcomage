using System;
using System.Linq;
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
    public void ReloadScene()
    {
        SceneManager.LoadSceneAsync("GamePlay");
    }
    private void OnDestroy()
    {
        UninitSingletons();
    }
}