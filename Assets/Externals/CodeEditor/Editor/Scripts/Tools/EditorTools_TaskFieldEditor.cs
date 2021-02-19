#if UNITY_EDITOR

using System;
using System.Collections.Generic;
// using NodeCanvas.Framework;
using UnityEditor;
using UnityEngine;
// using ParadoxNotion;
using UnityObject = UnityEngine.Object;

// namespace ParadoxNotion.Design{

/// <summary>
/// Task specific Editor field
/// </summary>

partial class EditorTools
{

    static string search = string.Empty;

    ///Show a Task's field
    public static void TaskField<T>(T task, ITaskSystem ownerSystem, Action<T> callback) where T : Task
    {
        TaskField(task, ownerSystem, typeof(T), (Task t) => { callback((T)t); });
    }
    public static void TaskField(Task task, ITaskSystem ownerSystem, Type baseType, Action<Task> callback)
    {
        //if null simply show an assignment button
        if (task == null)
        {
            var OnTaskSelectionCallback = new Action<Type>((t) =>
            {
                var newTask = Task.Create(baseType, ownerSystem);
                newTask.message = (Google.Protobuf.IMessage)Activator.CreateInstance(t);
                Undo.RecordObject(ownerSystem.contextObject, "New Task");
                callback(newTask);

            });
            if (baseType == typeof(ActionTask))
            {

                TaskSelectionButton(ownerSystem, 1, OnTaskSelectionCallback);
            }
            else if (baseType == typeof(ConditionTask))
            {
                TaskSelectionButton(ownerSystem, 2, OnTaskSelectionCallback);

            }
            return;
        }

        //Handle Action/ActionLists so that in GUI level a list is used only when needed
        if (baseType == typeof(ActionTask))
        {
            // if (!(task is ActionList)){
            TaskSelectionButton(ownerSystem, 1, (t) =>
                {
                    var newTask = Task.Create(baseType, ownerSystem);
                    newTask.message = (Google.Protobuf.IMessage)Activator.CreateInstance(t);
                    Undo.RecordObject(ownerSystem.contextObject, "New Task");
                    callback(newTask);
                    // var newList = Task.Create<ActionList>(ownerSystem);
                    // newList.AddAction( (ActionTask)task );
                    // newList.AddAction( (ActionTask)t );
                    // callback(newList);
                    callback((ActionTask)task);
                });
            // }

            Task.ShowTaskInspectorGUI(task, callback);

            // if (task is ActionList){
            // 	var list = (ActionList)task;
            // 	if (list.actions.Count == 1){
            // 		callback(list.actions[0]);
            // 	}
            // }
            return;
        }

        //Handle Condition/ConditionLists so that in GUI level a list is used only when needed
        if (baseType == typeof(ConditionTask))
        {
            // if (!(task is ConditionList)){
            TaskSelectionButton(ownerSystem, 2, (t) =>
                {
                    var newTask = Task.Create(baseType, ownerSystem);
                    newTask.message = (Google.Protobuf.IMessage)Activator.CreateInstance(t);
                    Undo.RecordObject(ownerSystem.contextObject, "New Task");
                    callback(newTask);
                    // var newList = Task.Create<ConditionList>(ownerSystem);
                    // newList.AddCondition( (ConditionTask)task );
                    // newList.AddCondition( (ConditionTask)t );
                    // callback(newList);
                    callback((ConditionTask)task);

                });
            // }

            Task.ShowTaskInspectorGUI(task, callback);

            // if (task is ConditionList){
            // 	var list = (ConditionList)task;
            // 	if (list.conditions.Count == 1){
            // 		callback(list.conditions[0]);
            // 	}
            // }
            return;
        }

        //in all other cases where the base type is not a base ActionTask or ConditionTask,
        //(thus lists can't be used unless the base type IS a list), simple show the inspector.
        Task.ShowTaskInspectorGUI(task, callback);
    }


    //Shows a button that when clicked, pops a context menu with a list of tasks deriving the base type specified. When something is selected the callback is called
    //On top of that it also shows a search field for Tasks
    public static void TaskSelectionButton<T>(ITaskSystem ownerSystem, Action<T> callback) where T : Task
    {
        TaskSelectionButton(ownerSystem, typeof(T), (Task t) => { callback((T)t); });
    }
    // TODO:Create new
    public static void TaskSelectionButton(ITaskSystem ownerSystem, int type, Action<Type> callback)
    {
        Func<GenericMenu> GetMenu = () =>
        {

            var menu = GetTypeSelectionMenu(type, callback);
            return menu;
        };

        GUI.backgroundColor = lightBlue;
        var label = "Assign " + (type == 1 ? "Action" : "Condition");
        if (GUILayout.Button(label))
        {
            var menu = GetMenu();
            if (Preferences.useBrowser)
            {
                CompleteContextMenu.Show(menu, Event.current.mousePosition, label, typeof(Task));
            }
            else
            {
                menu.ShowAsContext();
            }
            Event.current.Use();
        }


        GUI.backgroundColor = Color.white;
        GUILayout.BeginHorizontal();
        search = EditorGUILayout.TextField(search, (GUIStyle)"ToolbarSeachTextField");
        if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton"))
        {
            search = string.Empty;
            GUIUtility.keyboardControl = 0;
        }
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(search))
        {
            GUILayout.BeginVertical("TextField");
            foreach (var taskInfo in GetScriptInfosOfType(type))
            {
                if (taskInfo.name.Replace(" ", "").ToUpper().Contains(search.Replace(" ", "").ToUpper()))
                {
                    if (GUILayout.Button(taskInfo.name))
                    {
                        search = string.Empty;
                        GUIUtility.keyboardControl = 0;
                        callback(taskInfo.type);
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }
    public static void TaskSelectionButton(ITaskSystem ownerSystem, Type baseType, Action<Task> callback)
    {

        Action<Type> TaskTypeSelected = (t) =>
        {
            var newTask = Task.Create(t, ownerSystem);
            Undo.RecordObject(ownerSystem.contextObject, "New Task");
            callback(newTask);
        };

        Func<GenericMenu> GetMenu = () =>
        {
            var menu = GetTypeSelectionMenu(baseType, TaskTypeSelected);
            if (Task.copiedTask != null && baseType.IsAssignableFrom(Task.copiedTask.GetType()))
            {
                menu.AddSeparator("/");
                menu.AddItem(new GUIContent(string.Format("Paste ({0})", Task.copiedTask.name)), false, () => { callback(Task.copiedTask.Duplicate(ownerSystem)); });
            }
            return menu;
        };

        GUI.backgroundColor = lightBlue;
        var label = "Assign " + baseType.Name.SplitCamelCase();
        if (GUILayout.Button(label))
        {
            var menu = GetMenu();
            if (Preferences.useBrowser)
            {
                CompleteContextMenu.Show(menu, Event.current.mousePosition, label, typeof(Task));
            }
            else
            {
                menu.ShowAsContext();
            }
            Event.current.Use();
        }


        GUI.backgroundColor = Color.white;
        GUILayout.BeginHorizontal();
        search = EditorGUILayout.TextField(search, (GUIStyle)"ToolbarSeachTextField");
        if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton"))
        {
            search = string.Empty;
            GUIUtility.keyboardControl = 0;
        }
        GUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(search))
        {
            GUILayout.BeginVertical("TextField");
            foreach (var taskInfo in GetScriptInfosOfType(baseType))
            {
                if (taskInfo.name.Replace(" ", "").ToUpper().Contains(search.Replace(" ", "").ToUpper()))
                {
                    if (GUILayout.Button(taskInfo.name))
                    {
                        search = string.Empty;
                        GUIUtility.keyboardControl = 0;
                        TaskTypeSelected(taskInfo.type);
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }

}
// }

#endif