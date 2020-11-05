using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(LuaBehaviour))]
public class LuaBehaviourInspector : Editor
{
    public static IEnumerable<string> luaAssetCandidates;

    private Rect rect;
    public override void OnInspectorGUI()
    {
        LuaBehaviour injector = target as LuaBehaviour;

        if (injector != null)
        {
            serializedObject.Update();
            var sp = serializedObject.FindProperty("luaScriptPath");
            GUI.enabled = false;
            EditorGUILayout.PropertyField(sp);
            GUI.enabled = true;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select", EditorStyles.miniButtonLeft))
            {
                AssetSelectPopup.Open(rect,
                () =>
                {
                    if (luaAssetCandidates == null)
                    {
                        luaAssetCandidates = from path in Directory.GetFiles("Assets/Resources/", "*.bytes", SearchOption.AllDirectories).Select(p => p.Replace("\\", "/"))
                                                where path.Contains($"/{LuaManager.LuaPathKeyWord}/")
                                                select path;
                    }
                    return luaAssetCandidates;
                },
                (path) =>
                {
                    sp.stringValue = path;
                    serializedObject.ApplyModifiedProperties();
                });
            }
            if (Event.current.type == EventType.Repaint)
            {
                rect = GUILayoutUtility.GetLastRect();
                rect.width *= 3;
            }
            if (GUILayout.Button("Ping", EditorStyles.miniButtonMid))
            {
                var bundleName = sp.stringValue;
                if (!string.IsNullOrEmpty(bundleName))
                {
                    var assetPath = bundleName.Substring(1);
                    if (assetPath != null)
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                    }
                }
            }
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight))
            {
                sp.stringValue = null;
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

// ** 之后做REF injection缓存的时候使用
// [CustomPropertyDrawer(typeof(InjectionEntry))]
// public class LuaInjectionEntryDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         var gasketName = property.FindPropertyRelative("gasketName");
//         var componentName = property.FindPropertyRelative("componentName");
//         var target = property.FindPropertyRelative("target");
//         var component = target.objectReferenceValue as Component;

//         position.height = EditorGUIUtility.singleLineHeight;
//         var go = component?.gameObject;
//         go = EditorGUI.ObjectField(new Rect(position.x, position.y, 150, position.height), go, typeof(GameObject), true) as GameObject;

//         if (go != null)
//         {
//             if (component != null)
//             {
//                 var image = EditorGUIUtility.ObjectContent(null, component.GetType()).image;
//                 if (image == null)
//                 {
//                     image = EditorGUIUtility.IconContent("dll Script Icon").image;
//                 }
//                 GUI.DrawTexture(new Rect(position.x + 150, position.y, EditorGUIUtility.singleLineHeight, position.height), image);
//             }

//             var comps = go.GetComponents<Component>();
//             var names = comps.Select(e => e?.GetType()?.Name).ToArray();
//             var index = Array.IndexOf(names, componentName.stringValue);
//             index = EditorGUI.Popup(new Rect(position.x + 170, position.y, 130, position.height), index, names);

//             if (index >= 0 && index < comps.Length)
//             {
//                 component = comps[index];
//             }
//             else
//             {
//                 component = comps[0];
//             }
//         }
//         else
//         {
//             EditorGUI.LabelField(new Rect(position.x + 150, position.y, 150, position.height), "Missing Game Object");
//             component = null;
//         }

//         target.objectReferenceValue = component;

//         var path = "<unset>";
//         if (component != null)
//         {
//             gasketName.stringValue = (component.transform == (property.serializedObject.targetObject as Component).transform) ? "$" : component.gameObject.name + ".";
//             componentName.stringValue = component.GetType().Name;
//             path = gasketName.stringValue + componentName.stringValue;
//         }

//         EditorGUI.BeginDisabledGroup(component == null);

//         EditorGUI.LabelField(new Rect(position.x + 300, position.y, position.width - 350, position.height), path);

//         if (GUI.Button(new Rect(position.x + position.width - 50, position.y, 50, position.height), "Copy"))
//         {
//             GUIUtility.systemCopyBuffer = path;
//         }

//         EditorGUI.EndDisabledGroup();
//     }
// }