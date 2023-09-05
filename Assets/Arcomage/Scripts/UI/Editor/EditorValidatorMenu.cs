// using UnityEditor;
// using UnityEngine;
// using System.Linq;
// using System.IO;
// using System.Text;
// using System.Collections.Generic;
// // using NOAH.Core;
//
// namespace NOAH.Utility
// {
//     [InitializeOnLoad]
//     public class EditorValidatorMenu
//     {
//         [MenuItem("⛵NOAH/Validator/Validate Client", false, 1)]
//         static public void ValidateClient()
//         {
//             string errors = "";
//
//             string result = ValidateUI();
//             if (!string.IsNullOrEmpty(result)) errors += "> " + result;
//
//             result = ValidateFileName();
//             if (!string.IsNullOrEmpty(result)) errors += "> " + result;
//
//             StreamWriter sw;
//             FileInfo t = new FileInfo("validate_client_log.log");
//             sw = t.CreateText();            
//             sw.Write(errors);
//             sw.Close();
//             sw.Dispose();
//         }
//
//         [MenuItem("⛵NOAH/Validator/Validate FileName", false, 12)]
//         static string ValidateFileName()
//         {
//             uint errorCount = 0;
//             StringBuilder errorStr = new StringBuilder();
//
//             // DirectoryInfo dir = new DirectoryInfo("Assets/AssetBundle");
//             HashSet<string> prefabFileNames = new HashSet<string>();
//             HashSet<string> nonePrefabFileNames = new HashSet<string>();
//
//             foreach (string dirName in Directory.GetDirectories("Assets/AssetBundle", "*", SearchOption.AllDirectories))
//             {
//                 prefabFileNames.Clear();
//                 nonePrefabFileNames.Clear();
//                 foreach (string fileName in Directory.GetFiles(dirName).Where(p => !p.EndsWithOrdinal(".meta") && !p.EndsWithOrdinal(".tpsheet")))
//                 {
//                     string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
//                     if (fileName.EndsWithOrdinal(".prefab"))
//                     {
//                         if (!prefabFileNames.Add(fileNameWithoutExtension))
//                         {   
//                             errorCount++;
//                             errorStr.Append(fileName + ", ");
//                         }
//                     }
//                     else
//                     {
//                         nonePrefabFileNames.Add(fileNameWithoutExtension);
//                     }
//                 }
//
//                 foreach (string prefabName in prefabFileNames)
//                 {
//                     if (prefabName == "Gacha_CardEffect_B.mat")
//                     {
//                         errorCount++;
//                     }
//                     if (nonePrefabFileNames.Contains(prefabName))
//                     {
//                         errorCount++;
//                         errorStr.Append(Path.Combine(dirName, prefabName + ".prefab") + ", ");
//                     }
//                 }
//                 
//             }
//
//             string result = "";
//             string log = "检查文件夹内的文件名有和prefab名重复的情况，有" + errorCount + "个错误: " + errorStr.ToString();
//             if (errorCount > 0)
//             {
//                 result = log + "\n";
//             }
//             UnityEngine.Debug.Log(log);
//
//             return result;
//         }
//
//         [MenuItem("⛵NOAH/Validator/Validate UI", false, 13)]
//         static string ValidateUI()
//         {
//             uint errorCount = 0;
//             StringBuilder errorStr = new StringBuilder();
//
//             var pathes = from path in
//             Directory.GetFiles("Assets/AssetBundle/UI/Prefab", "*", SearchOption.AllDirectories)
//             where (Path.GetExtension(path) == ".prefab")
//             select path;
//
//             foreach (string path_ in pathes)
//             {
//                 var uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path_);
//                 if (!NOAH.UI.UIValidator.ValidateUI(uiPrefab))
//                 {
//                     errorCount++;
//                     errorStr.Append(path_ + ", ");
//                 }
//             }
//
//             string result = "";
//             string log = "检查UI，有" + errorCount + "个错误: " + errorStr.ToString();
//             if (errorCount > 0)
//             {
//                 result = log + "\n";
//             }
//             UnityEngine.Debug.Log(log);
//
//             return result;
//         }
//     }
// }