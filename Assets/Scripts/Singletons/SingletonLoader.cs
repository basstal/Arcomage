using System.Linq;
using UnityEngine;

public class SingletonLoader : MonoBehaviour
{
    public GameObject[] singletons;
    private SingletonBase[] m_instances;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        InitSingletons();
    }

    private void InitSingletons()
    {
        singletons = singletons.Where(v => v != null).ToArray();
        if (m_instances != null)
        {
            UninitSingletons();
        }
        m_instances = new SingletonBase[singletons.Length];
        int i = 0;
        foreach (var singleton in singletons)
        {
            var singletonBase = singleton.GetComponent<SingletonBase>();
            // Debug.LogWarning($" {singletonBase} before");
            if (singletonBase == null)
            {
                LogUtility.LogError("Launch", $"{singleton} illegal");
                continue;
            }
            var gameObject = Instantiate(singleton, Vector3.zero, Quaternion.identity, transform);
            var instance = gameObject.GetComponent<SingletonBase>();
            // Debug.LogWarning($" {singletonBase} instantiated ");
            m_instances[i++] = instance;
            instance.Init();
        }
    }
    private void UninitSingletons()
    {
        foreach(var instance in m_instances)
        {
            instance.Uninit();
            DestroyImmediate(instance);
        }
        m_instances = null;
    }
}