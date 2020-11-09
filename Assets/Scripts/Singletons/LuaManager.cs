using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : Singleton<LuaManager>
{
    [CSharpCallLua]
    private delegate void DelegateCSCollectGarbage();
    [CSharpCallLua]
    public delegate LuaTable CCL2(GameObject go);
    [CSharpCallLua]
    public delegate void CCL3(LuaTable sandbox);
    [CSharpCallLua]
    public delegate void CCL4(LuaTable sandbox, string path, bool forceReload);
    private DelegateCSCollectGarbage LuaCollectGarbage;
    private CCL2 createSandbox;
    private CCL3 destroySandbox;
    private CCL4 doChunk;
    private LuaEnv luaEnv;
    private List<GameObject> luaBehaviourGameObjects;

    public CCL2 CreateSandbox { set => createSandbox = value; }
    public CCL3 DestroySandbox { set => destroySandbox = value; }
    public CCL4 DoChunk { set => doChunk = value; }
    public static string LuaPathKeyWord = "Lua";
    public override void Init()
    {
        base.Init();
        luaBehaviourGameObjects = new List<GameObject>();
        luaEnv = new LuaEnv();
        // luaEnv.AddLoader((ref string requirePath) =>
        // {
        //     if (requirePath[0] == '/' || requirePath[0] == '\\')
        //     {
        //         requirePath = requirePath.Substring(1);
        //     }
        //     var path = Path.Combine(LuaPathKeyWord, requirePath);
        //     var asset = AssetUtility.LoadResource<TextAsset>(path);
        //     return asset.bytes;
        // });
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
    public override void Uninit()
    {
        base.Uninit();
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