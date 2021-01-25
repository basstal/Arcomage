using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using XLua;

public class LuaManager : Singleton<LuaManager>
{
    [CSharpCallLua] private delegate void DelegateCSCollectGarbage();
    [CSharpCallLua] private delegate LuaTable DelegateCSCreateSandbox(GameObject go);
    [CSharpCallLua] private delegate void DelegateCSDestroySandbox(LuaTable sandbox);
    [CSharpCallLua] private delegate void DelegateCSDoChunk(LuaTable sandbox, string path, bool forceReload);
#pragma warning disable 649
    private DelegateCSCollectGarbage m_luaCollectGarbage;
    private DelegateCSCreateSandbox m_luaCreateSandbox;
    private DelegateCSDestroySandbox m_luaDestroySandbox;
    private DelegateCSDoChunk m_luaDoChunk;
#pragma warning restore 649
    private readonly List<LuaBehaviour> m_luaBehaviours = new List<LuaBehaviour>();
    private readonly Dictionary<string, byte[]> m_scriptName2Content = new Dictionary<string, byte[]>();
    private float m_lastGCTime;
    public event Action OnInitFinished;
    public static string LuaScriptsLabel = "Lua";
    public static string UniqueLuaScriptsPath = "Assets/Lua/";
    public LuaEnv LuaEnv { get; private set; }

    public override async Task Init()
    {
        await base.Init();
        ClearAllLuaBehaviours();
        if (LuaEnv == null)
        {
            var newEnv = new LuaEnv();
            newEnv.AddLoader(Loader);
            await ReloadAllLuaContent();
#if UNITY_EDITOR
            newEnv.Global.Set("__UNITY_EDITOR", true);
#endif
            var objs = newEnv.DoString("return require('InitLua')", "LuaManager_Init");
            if (objs != null && objs[0] is LuaTable injectedLuaFunctions)
            {
                injectedLuaFunctions.Get("CollectGarbage", out m_luaCollectGarbage);
                injectedLuaFunctions.Get("CreateSandbox", out m_luaCreateSandbox);
                injectedLuaFunctions.Get("DestroySandbox", out m_luaDestroySandbox);
                injectedLuaFunctions.Get("DoChunk", out m_luaDoChunk);
                injectedLuaFunctions.Dispose();
            }
            LuaEnv = newEnv;
            OnInitFinished?.Invoke();
        }
    }

    private async Task ReloadAllLuaContent()
    {
        var locations = await Addressables.LoadResourceLocationsAsync(LuaScriptsLabel).Task;

        foreach (var location in locations)
        {
            var asset = await Addressables.LoadAssetAsync<TextAsset>(location).Task;
            string refPath = location.InternalId.Replace(UniqueLuaScriptsPath, "");
            var dotIndex = refPath.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                refPath = refPath.Substring(0, dotIndex);
            }
#if UNITY_EDITOR
            // Debug.Log($"refPath :{refPath}");
#endif
            m_scriptName2Content[refPath] = asset.bytes;
        }
    }

    private byte[] Loader(ref string requireName)
    {
        m_scriptName2Content.TryGetValue(requireName, out var result);
        return result;
    }

    private bool CheckInitiated()
    {
        if (LuaEnv == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("LuaManager has not initiated properly");
#endif
            return false;
        }
        return true;
    }
    public void DoChunk(LuaTable sandbox, string chunkName, byte[] content)
    {
        if (sandbox == null || content == null || !CheckInitiated())
            return;

        LuaEnv.DoString(content, chunkName, sandbox);
    }
    public void DoChunk(LuaTable sandbox, string path, bool forceReload)
    {
        if (sandbox == null || string.IsNullOrWhiteSpace(path) || !CheckInitiated())
            return;
        m_luaDoChunk(sandbox, path, forceReload);
    }
    public LuaTable CreateSandbox(LuaBehaviour luaBehaviour)
    {
        if (luaBehaviour == null || !CheckInitiated())
            return null;
        var sandbox = m_luaCreateSandbox(luaBehaviour.gameObject);
        m_luaBehaviours.Add(luaBehaviour);
        return sandbox;
    }

    public void DestroySandbox(LuaBehaviour luaBehaviour)
    {
        if (luaBehaviour == null || !CheckInitiated())
            return;

        m_luaBehaviours.Remove(luaBehaviour);
        m_luaDestroySandbox(luaBehaviour.Sandbox);
        luaBehaviour.Sandbox.Dispose();
    }

    public void CollectGarbage()
    {
        if (!CheckInitiated())
            return;
        m_luaCollectGarbage();
    }

    private void Update()
    {
        if (Time.time - m_lastGCTime <= 1) return;
        LuaEnv?.Tick();
        m_lastGCTime = Time.time;
    }

    private void ClearAllLuaBehaviours()
    {
        var count = m_luaBehaviours.Count;
        for (var i = 0; i < count; ++i)
        {
            var behaviour = m_luaBehaviours[i];
            behaviour.Dispose();
            m_luaDestroySandbox(behaviour.Sandbox);
            behaviour.Sandbox.Dispose();
        }
        m_luaBehaviours.Clear();
    }
    public override async Task Uninit()
    {
        await base.Uninit();
        ClearAllLuaBehaviours();
        m_luaCollectGarbage = null;
        m_luaCreateSandbox = null;
        m_luaDestroySandbox = null;
        m_luaDoChunk = null;
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
        }
        LuaEnv = null;
#if UNITY_EDITOR
        // Debug.Log($"LuaManager Uninit");
#endif
    }
}