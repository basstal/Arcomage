#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

/// Specific Editor GUIs
partial class EditorTools
{

    private static readonly Dictionary<object, bool> registeredEditorFoldouts = new Dictionary<object, bool>();

    public static LayerMask LayerMaskField(string prefix, LayerMask layerMask, params GUILayoutOption[] layoutOptions)
    {
        return LayerMaskField(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), layerMask, layoutOptions);
    }
    public static LayerMask LayerMaskField(GUIContent content, LayerMask layerMask, params GUILayoutOption[] layoutOptions)
    {
        var layers = UnityEditorInternal.InternalEditorUtility.layers;
        var layerNumbers = new List<int>();

        for (int i = 0; i < layers.Length; i++)
        {
            layerNumbers.Add(LayerMask.NameToLayer(layers[i]));
        }

        var maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & layerMask.value) > 0)
            {
                maskWithoutEmpty |= (1 << i);
            }
        }

        maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(content, maskWithoutEmpty, layers, layoutOptions);

        var mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
            {
                mask |= (1 << layerNumbers[i]);
            }
        }
        layerMask.value = mask;
        return layerMask;
    }

    //An IList editor (List<T> and Arrays)
    public static IList ListEditor(string prefix, IList list, Type listType, object contextInstance)
    {
        return ListEditor(new GUIContent(prefix), list, listType, contextInstance);
    }
    public static IList ListEditor(GUIContent content, IList list, Type listType, object contextInstance)
    {

        var argType = listType.GetEnumerableElementType();
        if (argType == null)
        {
            return list;
        }

        //register foldout
        if (!registeredEditorFoldouts.ContainsKey(list))
        {
            registeredEditorFoldouts[list] = false;
        }

        GUILayout.BeginVertical();

        var foldout = registeredEditorFoldouts[list];
        foldout = EditorGUILayout.Foldout(foldout, content);
        registeredEditorFoldouts[list] = foldout;

        if (!foldout)
        {
            GUILayout.EndVertical();
            return list;
        }

        if (list.Equals(null))
        {
            GUILayout.Label("Null List");
            GUILayout.EndVertical();
            return list;
        }

        if (GUILayout.Button("Add Element"))
        {

            if (listType.IsArray)
            {

                list = ResizeArray((Array)list, list.Count + 1);
                registeredEditorFoldouts[list] = true;

            }
            else
            {

                var o = argType.IsValueType ? Activator.CreateInstance(argType) : null;
                list.Add(o);
            }
        }

        EditorGUI.indentLevel++;

        EditorTools.ReorderableList(list, (i, r) =>
        {
            GUILayout.BeginHorizontal();
            list[i] = GenericField("Element " + i, list[i], argType, null);
            if (GUILayout.Button("X", GUILayout.Width(18)))
            {

                if (listType.IsArray)
                {

                    list = ResizeArray((Array)list, list.Count - 1);
                    registeredEditorFoldouts[list] = true;

                }
                else
                {

                    list.RemoveAt(i);
                }
            }
            GUILayout.EndHorizontal();
        });

        EditorGUI.indentLevel--;
        Separator();

        GUILayout.EndVertical();
        return list;
    }

    static System.Array ResizeArray(System.Array oldArray, int newSize)
    {
        int oldSize = oldArray.Length;
        System.Type elementType = oldArray.GetType().GetElementType();
        System.Array newArray = System.Array.CreateInstance(elementType, newSize);
        int preserveLength = System.Math.Min(oldSize, newSize);
        if (preserveLength > 0)
        {
            System.Array.Copy(oldArray, newArray, preserveLength);
        }
        return newArray;
    }

    //A dictionary editor
    public static IDictionary DictionaryEditor(string prefix, IDictionary dict, Type dictType, object contextInstance)
    {
        return DictionaryEditor(new GUIContent(prefix), dict, dictType, contextInstance);
    }
    public static IDictionary DictionaryEditor(GUIContent content, IDictionary dict, Type dictType, object contextInstance)
    {

        var keyType = dictType.GetGenericArguments()[0];
        var valueType = dictType.GetGenericArguments()[1];

        //register foldout
        if (!registeredEditorFoldouts.ContainsKey(dict))
        {
            registeredEditorFoldouts[dict] = false;
        }

        GUILayout.BeginVertical();

        var foldout = registeredEditorFoldouts[dict];
        foldout = EditorGUILayout.Foldout(foldout, content);
        registeredEditorFoldouts[dict] = foldout;

        if (!foldout)
        {
            GUILayout.EndVertical();
            return dict;
        }

        if (dict.Equals(null))
        {
            GUILayout.Label("Null Dictionary");
            GUILayout.EndVertical();
            return dict;
        }

        var keys = dict.Keys.Cast<object>().ToList();
        var values = dict.Values.Cast<object>().ToList();

        if (GUILayout.Button("Add Element"))
        {
            if (!typeof(UnityObject).IsAssignableFrom(keyType))
            {
                object newKey = null;
                if (keyType == typeof(string))
                    newKey = string.Empty;
                else newKey = Activator.CreateInstance(keyType);
                if (dict.Contains(newKey))
                {
                    Debug.LogWarning(string.Format("Key '{0}' already exists in Dictionary", newKey.ToString()));
                    return dict;
                }

                keys.Add(newKey);

            }
            else
            {
                Debug.LogWarning("Can't add a 'null' Dictionary Key");
                return dict;
            }

            values.Add(valueType.IsValueType ? Activator.CreateInstance(valueType) : null);
        }

        //clear before reconstruct
        dict.Clear();

        for (var i = 0; i < keys.Count; i++)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Box("", GUILayout.Width(6), GUILayout.Height(35));
            GUILayout.BeginVertical();

            keys[i] = GenericField("K:", keys[i], keyType, null);
            values[i] = GenericField("V:", values[i], valueType, null);
            GUILayout.EndVertical();

            if (GUILayout.Button("X", GUILayout.Width(18), GUILayout.Height(34)))
            {
                keys.RemoveAt(i);
                values.RemoveAt(i);
            }

            GUILayout.EndHorizontal();

            try { dict.Add(keys[i], values[i]); }
            catch { Debug.Log("Dictionary Key removed due to duplicate found"); }
        }

        Separator();

        GUILayout.EndVertical();
        return dict;
    }


    //An editor field where if the component is null simply shows an object field, but if its not, shows a dropdown popup to select the specific component
    //from within the gameobject
    public static Component ComponentField(string prefix, Component comp, Type type, bool allowNone = true)
    {
        return ComponentField(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), comp, type, allowNone);
    }
    public static Component ComponentField(GUIContent content, Component comp, Type type, bool allowNone = true)
    {

        if (comp == null)
        {
            comp = EditorGUILayout.ObjectField(content, comp, type, true, GUILayout.ExpandWidth(true)) as Component;
            return comp;
        }

        var allComp = new List<Component>(comp.GetComponents(type));
        var compNames = new List<string>();

        foreach (var c in allComp.ToArray())
        {
            if (c == null) continue;
            compNames.Add(c.GetType().FriendlyName() + " (" + c.gameObject.name + ")");
        }

        if (allowNone)
        {
            compNames.Add("|NONE|");
        }

        int index;
        var contentOptions = compNames.Select(n => new GUIContent(n)).ToArray();
        index = EditorGUILayout.Popup(content, allComp.IndexOf(comp), contentOptions, GUILayout.ExpandWidth(true));

        if (allowNone && index == compNames.Count - 1)
        {
            return null;
        }

        return allComp[index];
    }


    public static string StringPopup(string selected, List<string> options, bool showWarning = true, bool allowNone = false, params GUILayoutOption[] GUIOptions)
    {
        return StringPopup(string.Empty, selected, options, showWarning, allowNone, GUIOptions);
    }

    //a popup that is based on the string rather than the index
    public static string StringPopup(string prefix, string selected, List<string> options, bool showWarning = true, bool allowNone = false, params GUILayoutOption[] GUIOptions)
    {
        return StringPopup(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, options, showWarning, allowNone, GUIOptions);
    }
    public static string StringPopup(GUIContent content, string selected, List<string> options, bool showWarning = true, bool allowNone = false, params GUILayoutOption[] GUIOptions)
    {

        EditorGUILayout.BeginVertical();
        if (options.Count == 0 && showWarning)
        {
            EditorGUILayout.HelpBox("There are no options to select for '" + content.text + "'", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return null;
        }

        var index = 0;
        var copy = new List<string>(options);
        if (allowNone)
        {
            copy.Insert(0, "|NONE|");
        }

        if (copy.Contains(selected)) index = copy.IndexOf(selected);
        else index = allowNone ? 0 : -1;

        index = EditorGUILayout.Popup(content, index, copy.Select(n => new GUIContent(n)).ToArray(), GUIOptions);

        if (index == -1 || (allowNone && index == 0))
        {
            if (showWarning)
            {
                if (!string.IsNullOrEmpty(selected))
                {
                    EditorGUILayout.HelpBox("The previous selection '" + selected + "' has been deleted or changed. Please select another", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("Please make a selection", MessageType.Warning);
                }
            }
        }

        EditorGUILayout.EndVertical();
        if (allowNone)
        {
            return index == 0 ? string.Empty : copy[index];
        }

        return index == -1 ? string.Empty : copy[index];
    }

    ///Generic Popup for selection of any element within a list
    public static T Popup<T>(string prefix, T selected, List<T> options, params GUILayoutOption[] GUIOptions)
    {
        return Popup<T>(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, options, GUIOptions);
    }
    public static T Popup<T>(GUIContent content, T selected, List<T> options, params GUILayoutOption[] GUIOptions)
    {

        var index = 0;
        if (options.Contains(selected))
        {
            index = options.IndexOf(selected) + 1;
        }

        var stringedOptions = new List<string>();
        if (options.Count == 0)
        {
            stringedOptions.Add("|NONE AVAILABLE|");
        }
        else
        {
            stringedOptions.Add("|NONE|");
            stringedOptions.AddRange(options.Select(o => o != null ? o.ToString() : "|NONE|"));
        }

        GUI.enabled = stringedOptions.Count > 1;
        index = EditorGUILayout.Popup(content, index, stringedOptions.Select(s => new GUIContent(s)).ToArray(), GUIOptions);
        GUI.enabled = true;

        return index == 0 ? default(T) : options[index - 1];
    }


    ///Generic Popup for selection of any element within a list
    public static void ButtonPopup<T>(string prefix, T selected, List<T> options, Action<T> Callback)
    {
        ButtonPopup<T>(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, options, Callback);
    }
    public static void ButtonPopup<T>(GUIContent content, T selected, List<T> options, Action<T> Callback)
    {
        var buttonText = selected != null ? selected.ToString() : "|NONE|";
        GUILayout.BeginHorizontal();
        if (content != null && content != GUIContent.none)
        {
            GUILayout.Label(content, GUILayout.Width(0), GUILayout.ExpandWidth(true));
        }
        if (GUILayout.Button(buttonText, (GUIStyle)"MiniPopup", GUILayout.Width(0), GUILayout.ExpandWidth(true)))
        {
            var menu = new GenericMenu();
            foreach (var _option in options)
            {
                var option = _option;
                menu.AddItem(new GUIContent(option != null ? option.ToString() : "|NONE|"), object.Equals(selected, option), () => { Callback(option); });
            }
            menu.ShowAsContext();
        }
        GUILayout.EndHorizontal();
    }

    ///Specialized Type button popup
    public static void ButtonTypePopup(string prefix, Type selected, Action<Type> Callback)
    {
        ButtonTypePopup(string.IsNullOrEmpty(prefix) ? GUIContent.none : new GUIContent(prefix), selected, Callback);
    }
    public static void ButtonTypePopup(GUIContent content, Type selected, Action<Type> Callback)
    {
        var buttonText = selected != null ? selected.FriendlyName() : "|NONE|";
        GUILayout.BeginHorizontal();
        if (content != null && content != GUIContent.none)
        {
            GUILayout.Label(content, GUILayout.Width(0), GUILayout.ExpandWidth(true));
        }
        if (GUILayout.Button(buttonText, (GUIStyle)"MiniPopup", GUILayout.Width(0), GUILayout.ExpandWidth(true)))
        {
            EditorTools.GetPreferedTypesSelectionMenu(typeof(object), Callback).ShowAsContext();
        }
        GUILayout.EndHorizontal();
    }
    public static void DrawIMessage(Google.Protobuf.IMessage data)
    {
        var type = data.GetType();
        var propertyInfoList = type.GetProperties();
        foreach (var propertyInfo in propertyInfoList)
        {
            var innerType = propertyInfo.PropertyType;
            if (propertyInfo.Name == "Parser" ||
                propertyInfo.Name == "Descriptor")
            {
                continue;
            }
            if (!typeof(IMessage).IsAssignableFrom(innerType))
            {
                var descriptor = Utility.GetDescriptor(type);
                GUILayout.BeginVertical("box");
                {
                    DrawPropertyName(propertyInfo, descriptor);
                    DrawProperty(propertyInfo, data, descriptor);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5f);
            }
        }

        // if (noPropertyInfo && data.NodeType != BevNodeType.None)
        // {
        //     Debug.LogWarning("请检查是否忘记把message注册到CodeNode的oneof prop字段结构中？？");
        // }
    }
    private static void DrawPropertyName(PropertyInfo propertyInfo, MessageDescriptor descriptor)
    {
        var name = Char.ToLower(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1);
        // if (BevTreeWindow.DISPLAY_TYPE == BevTreeDisplayType.FIELD_NAME || BevTreeWindow.DISPLAY_TYPE == BevTreeDisplayType.FIELD_NAME_AND_EXPLANATION)
        // {
        GUILayout.Label(name);
        // }
        // if (BevTreeWindow.DISPLAY_TYPE == BevTreeDisplayType.FIELD_NAME_AND_EXPLANATION || BevTreeWindow.DISPLAY_TYPE == BevTreeDisplayType.EXPLANATION)
        // {
        //     var fullName = $"{descriptor.Name}.{name}";
        //     if (BevTreeWindow.instance.explanations.TryGetValue(fullName, out var exp))
        //     {
        //         GUILayout.Label($"<color=cyan>{exp}</color>", GUIStyle);
        //     }
        //     else
        //     {
        //         //Debug.Log($"节点[{fullName}]没有对应的proto 帮助？？推荐写注释// help=[]来帮助理解和记录修改");
        //     }
        // }
    }

    private static void DrawProperty(PropertyInfo propertyInfo, object obj, MessageDescriptor descriptor)
    {
        var name = Char.ToLower(propertyInfo.Name[0]) + propertyInfo.Name.Substring(1);
        var fieldDescriptor = descriptor.FindFieldByName(name);
        if (fieldDescriptor == null)
        {
            Debug.LogError($"Not found field -> {name} <- in descriptor {descriptor.Name}");
            return;
        }
        // ** 从这里开始记录值是否发生变更
        GUI.changed = false;
        var value = propertyInfo.GetValue(obj);
        if (fieldDescriptor.IsRepeated)
        {
            switch (fieldDescriptor.FieldType)
            {
                case FieldType.String:
                    // ** todo 这里只处理了string 其他也参照这种方式处理 可以提取一个公用函数??
                    List<string> result = Utility.GetRepeatedFields<string>(value);
                    for (int i = 0; i < result.Count; ++i)
                    {
                        object v = result[i];
                        DrawType(fieldDescriptor.FieldType, ref v);
                        if ((string)v != result[i])
                        {
                            Utility.RemoveAtRepeatedField(value, i);
                            Utility.Insert2RepeatedField(value, v, i);
                        }
                    }

                    DrawRepeatedModificators(value, "");
                    break;
                default:
                    Debug.LogError($"未处理的Draw Repeated内置类型： {fieldDescriptor.FieldType}");
                    break;
            }
        }
        else
        {
            DrawType(fieldDescriptor.FieldType, ref value, propertyInfo);
            propertyInfo.SetValue(obj, value);
        }
        // if (GUI.changed)
        // {
        //     BevTreeWindow.instance.bevTreeView.changed = true;
        //     DisplayNameRefresh();
        // }
        EditorGUILayout.Separator();
    }
    private static void DrawType(FieldType t, ref object value, PropertyInfo propertyInfo = null)
    {
        switch (t)
        {
            case FieldType.SFixed64:
            case FieldType.Fixed64:
                float v = Utility.FixedToFloat((long)value);
                v = EditorGUILayout.FloatField(v);
                value = Utility.FloatToFixed(v);
                break;
            case FieldType.Enum:
                if (propertyInfo == null)
                {
                    throw new InvalidOperationException("FieldType.Enum no propertyInfo??");
                }
                var enumNames = Enum.GetNames(propertyInfo.PropertyType);
                var enumName = Enum.GetName(propertyInfo.PropertyType, value);
                var index = Array.IndexOf(enumNames, enumName);
                if (index == -1)
                {
                    GUI.changed = true;
                    index = 0;
                }
                int newIndex = EditorGUILayout.Popup(index, enumNames);
                var newEnumName = enumNames[newIndex];
                value = (int)Enum.Parse(propertyInfo.PropertyType, newEnumName);
                break;
            case FieldType.String:
                value = EditorGUILayout.TextField((string)value);
                break;
            case FieldType.Bool:
                value = EditorGUILayout.Toggle((bool)value);
                break;
            case FieldType.Int32:
                value = EditorGUILayout.IntField((int)value);
                break;
            case FieldType.Int64:
                value = EditorGUILayout.LongField((long)value);
                break;
            case FieldType.Float:
                value = EditorGUILayout.FloatField((float)value);
                break;
            case FieldType.Double:
                value = EditorGUILayout.DoubleField((double)value);
                break;
            default:
                Debug.LogError($"未处理的DrawProperty类型： {t}");
                break;
        }
    }

    private static void DrawRepeatedModificators(object obj, object defaultVal)
    {
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("+"))
            {
                Utility.Insert2RepeatedField(obj, defaultVal);
            }

            if (GUILayout.Button("-"))
            {
                Utility.RemoveAtRepeatedField(obj);
            }

        }
        GUILayout.EndHorizontal();
    }
}

#endif