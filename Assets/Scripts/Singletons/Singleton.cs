using UnityEngine;

public class SingletonBase : MonoBehaviour
{
#pragma warning disable CS1998
    public virtual async void Init() { }
    public virtual async void Uninit() { }
#pragma warning restore CS1998
}

public abstract class Singleton<T> : SingletonBase where T : MonoBehaviour
{
    public static T Instance;

    private void Awake()
    {
        Instance = this as T;
        // Debug.LogWarning($"Awake {Instance}");
    }
    private void OnDestroy() {
        Instance = null;
    }
}