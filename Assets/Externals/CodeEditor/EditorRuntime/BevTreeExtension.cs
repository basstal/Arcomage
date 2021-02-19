// #if UNITY_EDITOR
// using System.IO;
// using Google.Protobuf;
// using NOAH.Utility;
// using XLua;

// public static class BevTreeExtension
// {
//     public static Node TableFile2CodeNode(string dir, string snippet)
//     {
//         var luaBridge = EditorRuntimeUtility.GetEditorLuaBridge();
//         var path = dir.Contains(snippet) ? "AI/Snippet" : "AI"; 
//         var luaValues = luaBridge.DoString($@"return doChunk('{path}/{Path.GetFileNameWithoutExtension(dir)}', nil, true)");
//         if (luaValues != null)
//         {
//             var pe = typeof(ProtoExtension);
//             var fromLuaTable = pe.GetMethod("FromLuaTable");
//             if (fromLuaTable != null)
//             {
//                 var gFromLuaTable = fromLuaTable.MakeGenericMethod(typeof(Node));
//                 var result = (Node)gFromLuaTable.Invoke(pe, new object[]{luaValues[0] as LuaTable, luaBridge});
//                 return result;
//             }
//         }
//         return null;
//     }

//     public static void WriteCodeNode(string dir, Node data)
//     {
//         var luaBridge = EditorRuntimeUtility.GetEditorLuaBridge();
//         var luaTable = data.ToLuaTable(luaBridge);
//         var content = luaBridge.ProtobufDump("Node", luaTable);
//         content = "return\n" + content;
//         File.WriteAllText(dir, content);
//     }
// }
// #endif