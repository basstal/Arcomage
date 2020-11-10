using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

public class LuaManager : Singleton<LuaManager>
{
    [CSharpCallLua]private delegate void DelegateCSCollectGarbage();
    [CSharpCallLua]private delegate LuaTable DelegateCSCreateSandbox(GameObject go);
    [CSharpCallLua]private delegate void DelegateCSDestroySandbox(LuaTable sandbox);
    [CSharpCallLua]private delegate void DelegateCSDoChunk(LuaTable sandbox, string path, bool forceReload);
#pragma warning disable 649
    private DelegateCSCollectGarbage m_luaCollectGarbage;
    private DelegateCSCreateSandbox m_luaCreateSandbox;
    private DelegateCSDestroySandbox m_luaDestroySandbox;
    private DelegateCSDoChunk m_luaDoChunk;
#pragma warning restore 649
    private readonly List<LuaTable> m_sandboxes = new List<LuaTable>();
    private readonly Dictionary<string, byte[]> m_scriptName2Content = new Dictionary<string, byte[]>();
    public event Action OnInitFinished;
    public static string luaScriptsLabel = "Lua";
    public LuaEnv luaEnv { get; private set; }

    public override async Task Init()
    {
        await base.Init();
        DestroyAllLuaSandbox();
        if (luaEnv == null)
        {
            var newEnv = new LuaEnv();
            newEnv.AddLoader(Loader);
            await ReloadAllLuaContent();
#if UNITY_EDITOR
            newEnv.Global.Set("__UNITY_EDITOR", true);
#endif
            var objs = newEnv.DoString("return require('InitLua')");
            if (objs != null && objs[0] is LuaTable injectedLuaFunctions)
            {
                injectedLuaFunctions.Get("CollectGarbage", out m_luaCollectGarbage);
                injectedLuaFunctions.Get("CreateSandbox", out m_luaCreateSandbox);
                injectedLuaFunctions.Get("DestroySandbox", out m_luaDestroySandbox);
                injectedLuaFunctions.Get("DoChunk", out m_luaDoChunk);
            }
            luaEnv = newEnv;
            OnInitFinished?.Invoke();
        }
    }

    private async Task ReloadAllLuaContent()
    {
        List<TextAsset> result = new List<TextAsset>();
        await AddressableUtility.InitByNameOrLabel(luaScriptsLabel, result);
        foreach (var asset in result)
        {
            Debug.Log($"asset.name : {asset.name}");
            m_scriptName2Content.Add(asset.name, asset.bytes);
        }
    }

    private byte[] Loader(ref string requireName)
    {
        m_scriptName2Content.TryGetValue(requireName, out var result);
        return result;
    }

    private bool CheckInitiated()
    {
        if (luaEnv == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("LuaManager has not initiated properly");
#endif
            return false;
        }
        return true;
    }
    public void DoChunk(LuaTable sandbox, byte[] content)
    {
        if (sandbox == null || content == null || !CheckInitiated())
            return;

        luaEnv.DoString(content, "", sandbox);
    }
    public void DoChunk(LuaTable sandbox, string path, bool forceReload)
    {
        if (sandbox == null || string.IsNullOrWhiteSpace(path) || !CheckInitiated())
            return;
        m_luaDoChunk(sandbox, path, forceReload);
    }
    public LuaTable CreateSandbox(GameObject go)
    {
        if (go == null || !CheckInitiated())
            return null;
        var sandbox = m_luaCreateSandbox(go);
        m_sandboxes.Add(sandbox);
        return sandbox;
    }

    public void DestroySandbox(LuaTable sandbox)
    {
        if (sandbox == null || !CheckInitiated())
            return;
        m_sandboxes.Remove(sandbox);
        m_luaDestroySandbox(sandbox);
    }

    public void CollectGarbage()
    {
        if (!CheckInitiated())
            return;
        m_luaCollectGarbage();
    }

    private void Update()
    {
        luaEnv?.Tick();
    }

    private void DestroyAllLuaSandbox()
    {
        foreach (var luaTable in m_sandboxes.Where(t => t != null))
        {
            m_luaDestroySandbox(luaTable);
        }
        m_sandboxes.Clear();
    }
    public override async Task Uninit()
    {
        await base.Uninit();
        DestroyAllLuaSandbox();
        m_luaCollectGarbage = null;
        m_luaCreateSandbox = null;
        m_luaDestroySandbox = null;
        m_luaDoChunk = null;
        luaEnv?.Dispose();
        luaEnv = null;
    }
}