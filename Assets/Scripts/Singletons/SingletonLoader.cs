using System.Linq;
using UnityEngine;

public class SingletonLoader : MonoBehaviour
{
    public GameObject[] singletons;
    private void Awake()
    {
        DontDestroyOnLoad(this);
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
}