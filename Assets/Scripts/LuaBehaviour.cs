using System;
using UnityEngine;
using XLua;
using UnityEngine.UI;
[Serializable]
public class InjectionEntry
{
    public string gasketName;
    public string componentName;
    public UnityEngine.Object target;
}

public class LuaBehaviour : MonoBehaviour
{
    [SerializeField]
    private string luaScriptPath = "";
    public LuaTable sandbox;
    Action luaAwake;
    Action luaStart;
    Action luaLateUpdate;
    Action luaFixedUpdate;
    Action luaOnEnable;
    Action luaUpdate;
    Action luaOnDisable;
    Action luaOnDestroy;
    void Awake()
    {
        //Debug.Log("awake");
        sandbox = SceneGamePlay.instance.lua.CreateSandboxDelegate(gameObject);

        var i = luaScriptPath.IndexOf(LuaManager.LuaPathKeyWord);
        var requirePath = luaScriptPath.Substring(i + LuaManager.LuaPathKeyWord.Length).Replace(".bytes", "");
        SceneGamePlay.instance.lua.DoChunkDelegate(sandbox, requirePath, false);

        sandbox.Get("Awake", out luaAwake);
        sandbox.Get("Start", out luaStart);
        sandbox.Get("LateUpdate", out luaLateUpdate);
        sandbox.Get("FixedUpdate", out luaFixedUpdate);
        sandbox.Get("OnEnable", out luaOnEnable);
        sandbox.Get("Update", out luaUpdate);
        sandbox.Get("OnDisable", out luaOnDisable);
        sandbox.Get("OnDestroy", out luaOnDestroy);

        luaAwake.SafeInvoke();
    }

    void Start()
    {
        luaStart.SafeInvoke();
    }

    void LateUpdate()
    {
        luaLateUpdate.SafeInvoke();
    }

    void FixedUpdate()
    {
        luaFixedUpdate.SafeInvoke();
    }

    void OnEnable()
    {
        luaOnEnable.SafeInvoke();
    }

    void Update()
    {
        luaUpdate.SafeInvoke();
    }

    void OnDisable()
    {
        luaOnDisable.SafeInvoke();
    }

    private void OnDestroy()
    {
        BehaviourDestroy();
    }
    public void BehaviourDestroy()
    {
        luaOnDestroy.SafeInvoke();
        var buttons = GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            button.onClick = null;
        }
        luaAwake = null;
        luaStart = null;
        luaLateUpdate = null;
        luaFixedUpdate = null;
        luaOnEnable = null;
        luaUpdate = null;
        luaOnDisable = null;
        luaOnDestroy = null;
    }
}