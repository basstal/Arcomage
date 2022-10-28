// using UnityEditor;
// using System.Collections.Generic;
// using System.Linq;
// public class AssetProcessor : AssetPostprocessor
// {
//     public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//     {
//         if (GetAssetToProcess(importedAssets, deletedAssets, movedAssets).Any(e => e.Contains("/Scripts/")))
//         {
//             LuaBehaviourInspector.LuaAssetCandidates = null;
//         }
//     }
//     public static IEnumerable<string> GetAssetToProcess(string[] importedAssets, string[] deletedAssets, string[] movedAssets)
//     {
//         foreach (var e in importedAssets) yield return e.Replace(@"\", "/");
//         foreach (var e in deletedAssets) yield return e.Replace(@"\", "/");
//         foreach (var e in movedAssets) yield return e.Replace(@"\", "/");
//     }
// }