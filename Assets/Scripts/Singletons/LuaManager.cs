using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class DelegateCSLua
{
    [CSharpCallLua] public delegate void CCL1();
    [CSharpCallLua] public delegate LuaTable CCL2(GameObject go);
    [CSharpCallLua] public delegate void CCL3(LuaTable sandbox);
    [CSharpCallLua] public delegate void CCL4(LuaTable sandbox, string path, bool forceReload);
}

public class LuaManager : MonoBehaviour
{
    private DelegateCSLua.CCL1 collectGarbage;
    private DelegateCSLua.CCL2 createSandbox;
    private DelegateCSLua.CCL3 destroySandbox;
    private DelegateCSLua.CCL4 doChunk;
    private LuaEnv luaEnv;
    private List<GameObject> luaBehaviourGameObjects;

    public DelegateCSLua.CCL2 CreateSandbox { set => createSandbox = value; }
    public DelegateCSLua.CCL3 DestroySandbox { set => destroySandbox = value; }
    public DelegateCSLua.CCL4 DoChunk { set => doChunk = value; }
    public DelegateCSLua.CCL1 CollectGarbage { set => collectGarbage = value; }
    public static string LuaPathKeyWord = "Lua";
    public void Init()
    {
        luaBehaviourGameObjects = new List<GameObject>();
        luaEnv = new LuaEnv();
        luaEnv.AddLoader((ref string requirePath) =>
        {
            if (requirePath[0] == '/' || requirePath[0] == '\\')
            {
                requirePath = requirePath.Substring(1);
            }
            var path = Path.Combine(LuaPathKeyWord, requirePath);
            var asset = AssetUtility.LoadResource<TextAsset>(path);
            return asset.bytes;
        });
        var global = luaEnv.Global;
#if UNITY_EDITOR
        global.Set("__UNITY_EDITOR", true);
#endif
        luaEnv.DoString("require('InitLua')");
    }
    public void DoChunkDelegate(LuaTable sandbox, string path, bool forceReload)
    {
        doChunk(sandbox, path, forceReload);
    }
    public LuaTable CreateSandboxDelegate(GameObject go)
    {
        var sandbox = createSandbox(go);
        luaBehaviourGameObjects.Add(go);
        return sandbox;
    }
    void Update()
    {
        if (luaEnv != null)
        {
            luaEnv.Tick();
        }
    }
    public void Uninit()
    {
        foreach (var gameObject in luaBehaviourGameObjects)
        {
            if (gameObject != null)
            {
                var behaviour = gameObject.GetComponent<LuaBehaviour>();
                behaviour.BehaviourDestroy();
                destroySandbox(behaviour.sandbox);
            }
        }
        luaEnv.DoString("require('UninitLua')");
        luaEnv.Dispose();
    }
}