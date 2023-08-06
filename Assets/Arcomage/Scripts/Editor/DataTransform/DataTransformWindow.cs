// using System;
// using System.IO;
// using System.Linq;
// using System.Text.RegularExpressions;
// using Arcomage.GameScripts;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities;
// using Unity.VisualScripting;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Assertions;
// using UnityEngine.UI;
// using XLua;
// using Random = UnityEngine.Random;
//
// namespace Arcomage.GameEditorScripts
// {
//     public class DataTransformWindow : OdinEditorWindow
//     {
//         public LuaEnv luaenv;
//
//         protected override void Initialize()
//         {
//             base.Initialize();
//             Reset();
//         }
//
//         [Button(ButtonSizes.Large)]
//         public void Reset()
//         {
//             luaenv = new LuaEnv();
//             luaenv.AddLoader((ref string filename) =>
//             {
//                 // if (filename == "InMemory")
//                 // {
//                 //     string script = "return {ccc = 9999}";
//                 //     return System.Text.Encoding.UTF8.GetBytes(script);
//                 // }
//                 string path = Path.Combine(Application.dataPath, $"{filename}.bytes");
//                 // Debug.LogWarning($"path : {path}");
//                 if (File.Exists(path))
//                 {
//                     return File.ReadAllBytes(path);
//                 }
//
//                 return null;
//             });
//         }
//
//         public ScriptGraphAsset CreateVisualScriptViaLuaFunction(string cardName, string content)
//         {
//             string path = $"Assets/Data/VisualScripts/{cardName}.asset";
//             string dir = Path.GetDirectoryName(path);
//             UnityEngine.Assertions.Assert.IsNotNull(dir);
//             if (!Directory.Exists(dir))
//             {
//                 Directory.CreateDirectory(dir);
//             }
//
//             string[] lines = content.Split("\n");
//             var flowGraph = new FlowGraph();
//             var units = flowGraph.units;
//             Regex rx = new Regex(@"\bU\.(\w*)\((.*?)\)",
//                 RegexOptions.Compiled | RegexOptions.IgnoreCase);
//             var controlConnections = flowGraph.controlConnections;
//             float y = 100f;
//             var customEventUnit = new CustomEvent();
//             customEventUnit.position = new Vector2(0f, y);
//             units.Add(customEventUnit);
//             customEventUnit.name.SetDefaultValue("Apply");
//             IUnit lastUnit = customEventUnit;
//             int interval = 1;
//             for (int i = 1; i < lines.Length - 1; ++i)
//             {
//                 var line = lines[i];
//                 if (line.Trim().StartsWith("return")) // ** return 的行不管
//                 {
//                     continue;
//                 }
//
//                 var unitName = rx.Match(line).Groups[1].ToString();
//                 if (unitName.Trim().Length == 0)
//                 {
//                     continue;
//                 }
//
//                 var unitParams = rx.Match(line).Groups[2].ToString();
//                 var parameters = unitParams.Split(",");
//                 // foreach (var param in parameters)
//                 // {
//                 //     Debug.LogWarning(param);
//                 // }
//
//                 // Debug.LogWarning(line);
//                 // Debug.LogWarning($"{typeof(ResChange).AssemblyQualifiedName}");
//                 Type unitType = Type.GetType($"GameScripts.{unitName}, Game, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
//                 Assert.IsNotNull(unitType, $"GameScripts.{unitName}, Game, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null not found??\nline : {line}");
//                 // Assert.IsNotNull(unitType);
//                 if (Activator.CreateInstance(unitType) is IUnit unit)
//                 {
//                     unit.position = new Vector2(350f * interval++, y);
//                     units.Add(unit);
//                     for (int input_i = 0; input_i < unit.valueInputs.Count; ++input_i)
//                     {
//                         if (input_i >= parameters.Length) // ** default 参数
//                         {
//                             break;
//                         }
//
//                         var validInput = unit.valueInputs[input_i];
//                         if (validInput.type != typeof(GamePlayer))
//                         {
//                             if (validInput.type == typeof(CostType))
//                             {
//                                 try
//                                 {
//                                     var costTypeRaw = parameters[input_i].Replace("ResourceType.", "").Trim();
//                                     CostType.TryParse(costTypeRaw, out CostType costType);
//                                     validInput.SetDefaultValue(costType);
//                                 }
//                                 catch (Exception e)
//                                 {
//                                     Debug.LogWarning($"{e.Message}\n line : {line}, unitParams : {unitParams}");
//                                 }
//                             }
//                             else if (validInput.type == typeof(BuildingType))
//                             {
//                                 var typeRaw = parameters[input_i].Replace("ResourceType.", "").Trim();
//                                 typeRaw = typeRaw.Replace("\"", "");
//                                 BuildingType.TryParse(typeRaw, out BuildingType type);
//                                 validInput.SetDefaultValue(type);
//                             }
//                             else if (validInput.type == typeof(int))
//                             {
//                                 try
//                                 {
//                                     validInput.SetDefaultValue(int.Parse(parameters[input_i]));
//                                 }
//                                 catch (Exception e)
//                                 {
//                                     Debug.LogWarning($" {e.Message}\n line : {line}\n file : {path}");
//                                 }
//                             }
//                             else if (validInput.type == typeof(bool))
//                             {
//                                 try
//                                 {
//                                     validInput.SetDefaultValue(bool.Parse(parameters[input_i]));
//                                 }
//                                 catch (Exception e)
//                                 {
//                                     Debug.LogWarning($" {e.Message}\n line : {line}");
//                                 }
//                             }
//                             else
//                             {
//                                 throw new Exception($"Handle type {validInput.type.Name}!!\nline : {line}");
//                             }
//                         }
//                     }
//
//                     if (lastUnit != null)
//                     {
//                         controlConnections.Add(new ControlConnection(lastUnit.controlOutputs[0], unit.controlInputs[0]));
//                     }
//
//                     lastUnit = unit;
//                 }
//             }
//
//             VSUsageUtility.isVisualScriptingUsed = true;
//
//             var macro = (IMacro)CreateInstance(typeof(ScriptGraphAsset));
//             var macroObject = (UnityEngine.Object)macro;
//             macro.graph = flowGraph;
//             // if (gameObject != null)
//             // {
//             //     ScriptMachine flowMachine = gameObject.AddComponent<ScriptMachine>();
//             //
//             //     flowMachine.nest.macro = (ScriptGraphAsset)macro;
//             // }
//
//             string filename = Path.GetFileNameWithoutExtension(path);
//
//             // if (updateName)
//             // {
//             //     gameObject.name = filename;
//             // }
//
//             macroObject.name = filename;
//
//             AssetDatabase.CreateAsset(macroObject, path);
//             return (ScriptGraphAsset)macro;
//         }
//
//         [Button(ButtonSizes.Large)]
//         public void CreateVisualScript()
//         {
//             // ArcomageCard asset = ScriptableObject.CreateInstance<graph>();
//
//             // AssetDatabase.CreateAsset(asset, $"Assets/Data/{costType}/{cardName}.asset");
//             // AssetDatabase.SaveAssets();
//             string path = "Assets/test_visualscript.asset";
//             VSUsageUtility.isVisualScriptingUsed = true;
//
//             var macro = (IMacro)CreateInstance(typeof(ScriptGraphAsset));
//             var macroObject = (UnityEngine.Object)macro;
//             var flowGraph = new FlowGraph()
//             {
//                 units =
//                 {
//                     new ResChange() { position = new Vector2(-204, -144) },
//                 }
//             };
//             macro.graph = flowGraph;
//             // if (gameObject != null)
//             // {
//             //     ScriptMachine flowMachine = gameObject.AddComponent<ScriptMachine>();
//             //
//             //     flowMachine.nest.macro = (ScriptGraphAsset)macro;
//             // }
//
//             string filename = Path.GetFileNameWithoutExtension(path);
//
//             // if (updateName)
//             // {
//             //     gameObject.name = filename;
//             // }
//
//             macroObject.name = filename;
//
//             AssetDatabase.CreateAsset(macroObject, path);
//         }
//
//         [Button(ButtonSizes.Large)]
//         public void InitData()
//         {
//             var result = luaenv.DoString("require 'CardArcomage'");
//             // Debug.LogWarning($"result : {result}");
//             LuaTable data = luaenv.Global.Get<LuaTable>("Data");
//             // Debug.LogWarning($"data :{data}");
//             var count = 0;
//             data.ForEach<int, LuaTable>((key, value) =>
//             {
//                 // if (count >= 20)
//                 // {
//                 //     return;
//                 // }
//
//                 count++;
//                 // Debug.LogWarning($"key : {key}, value : {value}");
//                 LuaTable costTable = value.Get<LuaTable>("cost");
//                 int id = value.Get<int>("id");
//                 string cardName = value.Get<string>("name");
//                 string func = value.Get<string>("func");
//                 var logic = CreateVisualScriptViaLuaFunction(cardName, func);
//                 // Debug.LogWarning($"Func : {func}");
//
//                 CostType costType = costTable.Get<CostType>("type");
//                 int cost = costTable.Get<int>("count");
//
//                 ArcomageCard asset = ScriptableObject.CreateInstance<ArcomageCard>();
//                 asset.cardName = cardName;
//                 asset.id = id;
//                 asset.costType = costType;
//                 asset.cost = cost;
//                 asset.logic = logic;
//                 asset.luaSource = func;
//
//                 if (!Directory.Exists(Path.Combine(Application.dataPath, $"Data/{costType}")))
//                 {
//                     Directory.CreateDirectory($"Assets/Data/{costType}");
//                 }
//
//                 AssetDatabase.CreateAsset(asset, $"Assets/Data/{costType}/{cardName}.asset");
//                 AssetDatabase.SaveAssets();
//
//                 // EditorUtility.FocusProjectWindow();
//                 //
//                 // Selection.activeObject = asset;
//             });
//         }
//
//         [Button(ButtonSizes.Large)]
//         public void FillingDatabase()
//         {
//             ArcomageDatabase database = AssetDatabase.LoadAssetAtPath<ArcomageDatabase>($"Assets/Data/ArcomageDatabase.asset");
//             var result = luaenv.DoString("require 'LocaleMap'");
//             // Debug.LogWarning($"result : {result}");
//             LuaTable data = luaenv.Global.Get<LuaTable>("localeMap");
//             // Debug.LogWarning($"data :{data}");
//             LuaTable en = data.Get<LuaTable>("EN");
//             en.ForEach<string, string>((key, value) =>
//             {
//                 if (key.StartsWith("CardName"))
//                 {
//                     string guid = AssetDatabase.AssetPathToGUID($"Assets/Data/Cards/{value}.asset");
//                     database.cardsAssetRef.Add(new AssetReference(guid));
//                 }
//             });
//             AssetDatabase.SaveAssets();
//         }
//
//         [Button(ButtonSizes.Large)]
//         public void LoadLocalization()
//         {
//             var result = luaenv.DoString("require 'LocaleMap'");
//             // Debug.LogWarning($"result : {result}");
//             LuaTable data = luaenv.Global.Get<LuaTable>("localeMap");
//             // Debug.LogWarning($"data :{data}");
//             LuaTable en = data.Get<LuaTable>("EN");
//             LuaTable cn = data.Get<LuaTable>("CN");
//             var count = 0;
//             en.ForEach<string, string>((key, value) =>
//             {
//                 // if (count >= 20)
//                 // {
//                 //     return;
//                 // }
//                 if (key.StartsWith("CardName"))
//                 {
//                     count++;
//                     ArcomageCard asset = AssetDatabase.LoadAssetAtPath<ArcomageCard>($"Assets/Data/Cards/{value}.asset");
//                     Assert.IsTrue(asset.cardName == value);
//
//                     asset.describe_en = en.Get<string>($"CardDescribe_{asset.id}");
//                     asset.describe_cn = cn.Get<string>($"CardDescribe_{asset.id}");
//                     asset.cardName_cn = cn.Get<string>(key);
//                     EditorUtility.SetDirty(asset);
//                     AssetDatabase.SaveAssets();
//                 }
//                 // Debug.LogWarning($"key : {key}, value : {value}");
//                 // LuaTable costTable = value.Get<LuaTable>("cost");
//                 // int id = value.Get<int>("id");
//                 // string cardName = value.Get<string>("name");
//                 // string func = value.Get<string>("func");
//                 // var logic = CreateVisualScriptViaLuaFunction(cardName, func);
//                 // // Debug.LogWarning($"Func : {func}");
//                 //
//                 // CostType costType = costTable.Get<CostType>("type");
//                 // int cost = costTable.Get<int>("count");
//                 //
//                 // ArcomageCard asset = ScriptableObject.CreateInstance<ArcomageCard>();
//                 // asset.cardName = cardName;
//                 // asset.id = id;
//                 // asset.costType = costType;
//                 // asset.cost = cost;
//                 // asset.logic = logic;
//                 // asset.luaSource = func;
//                 //
//                 // if (!Directory.Exists(Path.Combine(Application.dataPath, $"Data/{costType}")))
//                 // {
//                 //     Directory.CreateDirectory($"Assets/Data/{costType}");
//                 // }
//                 //
//                 // AssetDatabase.CreateAsset(asset, $"Assets/Data/{costType}/{cardName}.asset");
//                 // AssetDatabase.SaveAssets();
//
//                 // EditorUtility.FocusProjectWindow();
//                 //
//                 // Selection.activeObject = asset;
//             });
//         }
//
//
//         [Button(ButtonSizes.Large)]
//         public void LoadSprites()
//         {
//             var result = luaenv.DoString("require 'LocaleMap'");
//             // Debug.LogWarning($"result : {result}");
//             LuaTable data = luaenv.Global.Get<LuaTable>("localeMap");
//             // Debug.LogWarning($"data :{data}");
//             LuaTable en = data.Get<LuaTable>("EN");
//             LuaTable cn = data.Get<LuaTable>("CN");
//             var count = 0;
//             var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Textures/UI/cards.png");
//             var sprites = objects.Where(q => q is Sprite).Cast<Sprite>().ToArray();
//             Assert.IsTrue(sprites.Length > 0);
//             en.ForEach<string, string>((key, value) =>
//             {
//                 // if (count >= 20)
//                 // {
//                 //     return;
//                 // }
//                 if (key.StartsWith("CardName"))
//                 {
//                     count++;
//                     ArcomageCard asset = AssetDatabase.LoadAssetAtPath<ArcomageCard>($"Assets/Data/Cards/{value}.asset");
//                     Assert.IsTrue(asset.cardName == value);
//                     //
//                     // asset.describe_en = en.Get<string>($"CardDescribe_{asset.id}");
//                     // asset.describe_cn = cn.Get<string>($"CardDescribe_{asset.id}");
//                     // asset.cardName_cn = cn.Get<string>(key);
//                     foreach (var sprite in sprites)
//                     {
//                         // Debug.LogWarning($"{sprite.name.Split("_")[1]}");
//                         if (sprite.name.StartsWith("cards_") && int.Parse(sprite.name.Split("_")[1]) == asset.id - 1)
//                         {
//                             asset.sprite = sprite;
//                             break;
//                         }
//                     }
//
//                     Assert.IsNotNull(asset.sprite);
//                     EditorUtility.SetDirty(asset);
//                     AssetDatabase.SaveAssets();
//                 }
//             });
//         }
//
//         [MenuItem("Window/DataTransform")]
//         static void Open()
//         {
//             EditorWindow.GetWindow<DataTransformWindow>();
//         }
//     }
// }