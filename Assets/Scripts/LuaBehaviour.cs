using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;
using XLua;
using Object = UnityEngine.Object;

[Serializable]
public class Injection
{
    public string name;
    public Object value;
}

public class LuaBehaviour : MonoBehaviour, IDisposable
{
    [SerializeField]
    public AssetReference script;
    [SerializeField]
    public Injection[] injections;
    public LuaTable Sandbox { get; private set; }
    Action m_luaAwake;
    Action m_luaStart;
    Action m_luaLateUpdate;
    Action m_luaFixedUpdate;
    Action m_luaOnEnable;
    Action m_luaUpdate;
    Action m_luaOnDisable;
    Action m_luaOnDestroy;
    private bool m_pendingStart;
    private bool m_pendingOnEnable;
    private bool m_completed;
    private HashSet<Button> m_bindButtonCache;
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
                Sandbox = luaManager.CreateSandbox(this);
                luaManager.DoChunk(Sandbox, op.Result.name, op.Result.bytes);

                Sandbox.Get("REF", out LuaTable injectionTable);
                foreach (var injection in injections)
                {
                    injectionTable.Set(injection.name, injection.value);
                }
                injectionTable.Dispose();
                
                Sandbox.Get("Awake", out m_luaAwake);
                Sandbox.Get("Start", out m_luaStart);
                Sandbox.Get("LateUpdate", out m_luaLateUpdate);
                Sandbox.Get("FixedUpdate", out m_luaFixedUpdate);
                Sandbox.Get("OnEnable", out m_luaOnEnable);
                Sandbox.Get("Update", out m_luaUpdate);
                Sandbox.Get("OnDisable", out m_luaOnDisable);
                Sandbox.Get("OnDestroy", out m_luaOnDestroy);
                Sandbox.Set<string, Action<GameObject, UnityAction>>("BindButtonEventCS", BindButtonEventCS);

                m_luaAwake.SafeInvoke();
                m_completed = true;
            };
            if (luaManager.LuaEnv != null)
            {
                luaBehaviourInit.Invoke();
            }
            else
            {
                luaManager.OnInitFinished += luaBehaviourInit;
            }
        };
    }

    private void BindButtonEventCS(GameObject obj, UnityAction callback)
    {
        if (obj == null)
            return;
        var button = obj.GetOrAddComponent<Button>();
        CommonUtility.SetEventHandler(button.onClick, callback);
        obj.SetGraphicRaycastTarget(true);
        m_bindButtonCache = m_bindButtonCache ?? new HashSet<Button>();
        m_bindButtonCache.Add(button);
    }
    private void Start()
    {
        if (!m_completed)
        {
            m_pendingStart = true;
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
            m_pendingOnEnable = true;
        }
        else
        {
            m_luaOnEnable.SafeInvoke();
        }
    }

    private void Update()
    {
        if (!m_completed) return;
        if (m_pendingStart)
        {
            m_luaStart.SafeInvoke();
            m_pendingStart = false;
        }

        if (m_pendingOnEnable)
        {
            m_luaOnEnable.SafeInvoke();
            m_pendingOnEnable = false;
        }
        m_luaUpdate.SafeInvoke();
    }

    private void OnDisable()
    {
        m_luaOnDisable.SafeInvoke();
    }

    public void Dispose()
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
        if (m_bindButtonCache != null)
        {
            foreach (var button in m_bindButtonCache.Where(button => button != null))
            {
#if UNITY_EDITOR

                Debug.Log($" release button {button}");
#endif
                button.onClick?.RemoveAllListeners();
                button.onClick = null;
            }
        }
#if UNITY_EDITOR
        Debug.Log($"LuaBehaviour {this} disposed");
#endif
    }
    private void OnDestroy()
    {
        Dispose();
        if (LuaManager.Instance != null && LuaManager.Instance.LuaEnv != null)
            LuaManager.Instance.DestroySandbox(this);
    }
}