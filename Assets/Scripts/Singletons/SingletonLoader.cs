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

    private void InitSingletons()
    {
        singletons = singletons.Where(v => v != null).ToArray();
        foreach (var singleton in singletons)
        {
            var singletonBase = singleton.GetComponent<SingletonBase>();
            if (singletonBase == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"SingletonLoader, {singleton} illegal.");
#endif
                continue;
            }
            singletonBase.Init();
        }
    }
    private void UninitSingletons()
    {
        foreach(var singleton in singletons)
        {
            var singletonBase = singleton.GetComponent<SingletonBase>();
            singletonBase.Uninit();
        }
    }
}