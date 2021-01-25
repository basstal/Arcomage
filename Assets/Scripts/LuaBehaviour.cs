using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
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
    [ValueDropdown("Candidates")]
    [InlineButton("Select")]
    public string script;
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
    private List<Button> m_bindButtonCache;
    private void Awake()
    {
        var luaManager = LuaManager.Instance;
        Action luaBehaviourInit = () =>
        {
            Sandbox = luaManager.CreateSandbox(this);
            luaManager.DoChunk(Sandbox, script, false);

            Sandbox.Get("REF", out LuaTable injectionTable);
            for (int i = 0; i < injections.Length; ++i)
            {
                var injection = injections[i];
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
            // ** set delegate lua function
            Sandbox.Set<string, Action<GameObject, UnityAction>>("BindButtonEvent", BindButtonEvent);
            Sandbox.Set<string, Func<int, Action, IEnumerator>>("DelayInvokeInFrames", DelayInvokeInFrames);
            Sandbox.Set<string, Func<Action, IEnumerator>>("DelayInvokeEndOfFrame", DelayInvokeEndOfFrame);

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
    }
    IEnumerator DelayInvokeInFramesImpl(int frame, Action callback)
    {
        for (int i = 0; i < frame; i++) yield return null;
        callback();
    }

    IEnumerator DelayInvokeEndOfFrameImpl(Action callback)
    {
        yield return new WaitForEndOfFrame();
        callback();
    }

    private IEnumerator DelayInvokeInFrames(int frames, Action callback)
    {
        var enumerator = DelayInvokeInFramesImpl(frames, callback);
        StartCoroutine(enumerator);
        return enumerator;
    }

    private IEnumerator DelayInvokeEndOfFrame(Action callback)
    {
        var enumerator = DelayInvokeEndOfFrameImpl(callback);
        StartCoroutine(enumerator);
        return enumerator;
    }
    private void BindButtonEvent(GameObject obj, UnityAction callback)
    {
        if (obj == null)
            return;
        var button = obj.GetOrAddComponent<Button>();
        CommonUtility.SetEventHandler(button.onClick, callback);
        obj.SetGraphicRaycastTarget(true);
        m_bindButtonCache = m_bindButtonCache ?? new List<Button>();
        if (!m_bindButtonCache.Contains(button))
        {
            m_bindButtonCache.Add(button);
        }
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
            var count = m_bindButtonCache.Count;
            for (var i = 0; i < count; ++i)
            {
                var button = m_bindButtonCache[i];
                if (button == null)
                    continue;
#if UNITY_EDITOR
                // Debug.Log($" release button {button}");
#endif
                button.onClick?.RemoveAllListeners();
                button.onClick = null;
            }
        }
#if UNITY_EDITOR
        // Debug.Log($"LuaBehaviour {this} disposed");
#endif
    }
    private void OnDestroy()
    {
        Dispose();
        if (LuaManager.Instance != null && LuaManager.Instance.LuaEnv != null)
            LuaManager.Instance.DestroySandbox(this);
    }
#if UNITY_EDITOR
    private IEnumerable<string> Candidates()
    {
        var searchPath = LuaManager.UniqueLuaScriptsPath;
        var p = from path in Directory.GetFiles(searchPath, "*.bytes", SearchOption.AllDirectories)
                select path.Substring(0, path.LastIndexOf('.')).Replace(searchPath, "").Replace(@"\", "/");
        var result = p.ToList();
        result.Add(script);
        return result;
    }
    private void Select()
    {
        if (!string.IsNullOrEmpty(script))
        {
            var fullPath = Path.Combine(LuaManager.UniqueLuaScriptsPath, $"{script}.bytes");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(fullPath));
        }
    }
#endif
}