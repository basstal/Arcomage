using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;
using Object = UnityEngine.Object;

[Serializable]
public class Injection
{
    public string name;
    public Object value;
}

public class LuaBehaviour : MonoBehaviour
{
    [SerializeField]
    public AssetReference script;
    [SerializeField]
    public Injection[] injections;
    public LuaTable sandbox { get; private set; }
    Action m_luaAwake;
    Action m_luaStart;
    Action m_luaLateUpdate;
    Action m_luaFixedUpdate;
    Action m_luaOnEnable;
    Action m_luaUpdate;
    Action m_luaOnDisable;
    Action m_luaOnDestroy;
    private bool m_shouldDelayStart = false;
    private bool m_shouldDelayOnEnable = false;
    private bool m_completed = false;
    private void Awake()
    {
        script.LoadAssetAsync<TextAsset>().Completed += (op) =>
        {
            if (op.Result == null)
            {
#if UNITY_EDITOR
                Debug.LogError("LuaBehaviour Awake load script asset failed.");
#endif
                return;
            }
            var luaManager = LuaManager.Instance;
            Action luaBehaviourInit = () =>
            {
                sandbox = luaManager.CreateSandbox(gameObject);
                luaManager.DoChunk(sandbox, op.Result.bytes);

                sandbox.Get("REF", out LuaTable injectionTable);
                foreach (var injection in injections)
                {
                    injectionTable.Set(injection.name, injection.value);
                }
                injectionTable.Dispose();
                
                Debug.Log($" sandbox : {sandbox}, op.Result : {op.Result}");
                sandbox.Get("Awake", out m_luaAwake);
                sandbox.Get("Start", out m_luaStart);
                sandbox.Get("LateUpdate", out m_luaLateUpdate);
                sandbox.Get("FixedUpdate", out m_luaFixedUpdate);
                sandbox.Get("OnEnable", out m_luaOnEnable);
                sandbox.Get("Update", out m_luaUpdate);
                sandbox.Get("OnDisable", out m_luaOnDisable);
                sandbox.Get("OnDestroy", out m_luaOnDestroy);

                m_luaAwake.SafeInvoke();
                m_completed = true;
            };
            if (luaManager.luaEnv != null)
            {
                luaBehaviourInit.Invoke();
            }
            else
            {
                luaManager.OnInitFinished += luaBehaviourInit;
            }
        };
    }
    private void Start()
    {
        if (!m_completed)
        {
            m_shouldDelayStart = true;
        }
        else
        {
            m_luaStart.SafeInvoke();
        }
    }

    private void LateUpdate()
    {
        m_luaLateUpdate.SafeInvoke();
    }

    private void FixedUpdate()
    {
        m_luaFixedUpdate.SafeInvoke();
    }

    private void OnEnable()
    {
        if (!m_completed)
        {
            m_shouldDelayOnEnable = true;
        }
        else
        {
            m_luaOnEnable.SafeInvoke();
        }
    }

    private void Update()
    {
        if (!m_completed) return;
        if (m_shouldDelayStart)
        {
            m_luaStart.SafeInvoke();
            m_shouldDelayStart = false;
        }

        if (m_shouldDelayOnEnable)
        {
            m_luaOnEnable.SafeInvoke();
            m_shouldDelayOnEnable = false;
        }
        m_luaUpdate.SafeInvoke();
    }

    private void OnDisable()
    {
        m_luaOnDisable.SafeInvoke();
    }

    private void OnDestroy()
    {
        m_luaOnDestroy.SafeInvoke();
        m_luaAwake = null;
        m_luaStart = null;
        m_luaLateUpdate = null;
        m_luaFixedUpdate = null;
        m_luaOnEnable = null;
        m_luaUpdate = null;
        m_luaOnDisable = null;
        m_luaOnDestroy = null;
        LuaManager.Instance.DestroySandbox(sandbox);
    }
}