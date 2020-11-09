using UnityEngine;

public class SingletonBase : MonoBehaviour
{
    public virtual void Init() { }
    public virtual void Uninit() { }
}

public abstract class Singleton<T> : SingletonBase where T : MonoBehaviour
{
    public static T Instance;

    private void Awake()
    {
        Instance = this as T;
    }
    private void OnDestroy() {
        Instance = null;
    }
}