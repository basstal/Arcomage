// #if UNITY_EDITOR
// using System;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using XLua;
// using LuaAPI = XLua.LuaDLL.Lua;

// public class LuaBinding : ScriptableObject
// {
//     public static LuaBinding instance;
//     public Dictionary<Type, LuaTable> luaBTMap;
//     // public TextAsset BTSelector;
//     // public TextAsset BTSequencer;
//     public LuaEnv luaEnv;
//     [InitializeOnLoadMethod]
//     public static void Init()
//     {
//         instance = AssetDatabase.LoadAssetAtPath<LuaBinding>("Assets/LuaBinding.asset");
//         if (instance == null)
//         {
//             var path = EditorTools.GetAssetUniquePath("LuaBinding.asset");
//             instance = EditorTools.CreateAsset<LuaBinding>(path);
//         }
//         instance.InitLua();
//     }

//     public void InitLua()
//     {
//         luaBTMap = new Dictionary<Type, LuaTable>();
//         luaEnv = new LuaEnv();
//         // var result = luaEnv.DoString(BTSelector.bytes);
//         // var table = (LuaTable)result[0];
//         // luaBTMap.Add(typeof(SelectorLua), table);
//         // result = luaEnv.DoString(BTSequencer.bytes);
//         // table = (LuaTable)result[0];

//         // luaBTMap.Add(typeof(SequencerLua), table);
//     }

//     public LuaTable NewGraphEnv()
//     {
//         return luaEnv.NewTable();
//     }

//     public void StartGraph(Graph graph)
//     {
//         var go = GameObject.Find("/Test");
//         if (go == null)
//         {
//             go = new GameObject("Test");
//             var lua = go.AddComponent<LuaBehaviour>();
//             lua.sandbox = LuaBinding.instance.NewGraphEnv();
//         }
//         //var path = AssetDatabase.GetAssetPath(Selection.activeObject);
//         //Debug.LogWarning($" path  {path }");
//         //EditorTools.CreateAsset<Graph>(path);
//         graph.Pause();
//         graph.StartGraph(go.transform, true);
//     }

    
// }
// #endif