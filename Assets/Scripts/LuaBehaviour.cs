using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    [SerializeField]
    public AssetReference script;
    private LuaTable m_sandbox;
    Action m_luaAwake;
    Action m_luaStart;
    Action m_luaLateUpdate;
    Action m_luaFixedUpdate;
    Action m_luaOnEnable;
    Action m_luaUpdate;
    Action m_luaOnDisable;
    Action m_luaOnDestroy;

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
                m_sandbox = luaManager.CreateSandbox(gameObject);
                luaManager.DoChunk(m_sandbox, op.Result.bytes);

                m_sandbox.Get("Awake", out m_luaAwake);
                m_sandbox.Get("Start", out m_luaStart);
                m_sandbox.Get("LateUpdate", out m_luaLateUpdate);
                m_sandbox.Get("FixedUpdate", out m_luaFixedUpdate);
                m_sandbox.Get("OnEnable", out m_luaOnEnable);
                m_sandbox.Get("Update", out m_luaUpdate);
                m_sandbox.Get("OnDisable", out m_luaOnDisable);
                m_sandbox.Get("OnDestroy", out m_luaOnDestroy);

                m_luaAwake.SafeInvoke();
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
        m_luaStart.SafeInvoke();
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
        m_luaOnEnable.SafeInvoke();
    }

    private void Update()
    {
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
        LuaManager.Instance.DestroySandbox(m_sandbox);
    }
}