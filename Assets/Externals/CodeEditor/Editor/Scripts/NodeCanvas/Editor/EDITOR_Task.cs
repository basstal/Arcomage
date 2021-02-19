#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



partial class Task
{

    private bool _isUnfolded = true;
    private object _icon;

    //The icon if any of the task
    public Texture2D icon
    {
        get
        {
            if (_icon == null)
            {
                var iconAtt = this.GetType().RTGetAttribute<IconAttribute>(true);
                _icon = iconAtt != null ? UserTypePreferences.GetTypeIcon(iconAtt, this) : null;
                if (_icon == null) { _icon = new object(); }
            }
            return _icon as Texture2D;
        }
    }

    private bool isUnfolded
    {
        get { return _isUnfolded; }
        set { _isUnfolded = value; }
    }

    //A placeholder for a copy/paste operation
    public static Task copiedTask { get; set; }

    //Draw the task inspector GUI
    public static void ShowTaskInspectorGUI(Task task, Action<Task> callback, bool showTitlebar = true)
    {

        if (task.ownerSystem == null)
        {
            GUILayout.Label("<b>Owner System is null! This should have not happen! Please report a bug</b>");
            return;
        }

        UndoTools.CheckUndo(task.ownerSystem.contextObject, "Task Inspector");

        if (task.obsolete != string.Empty)
        {
            EditorGUILayout.HelpBox(string.Format("This is an obsolete Task:\n\"{0}\"", task.obsolete), MessageType.Warning);
        }

        if (!showTitlebar || ShowTaskTitlebar(task, callback) == true)
        {

            if (Preferences.showNodeInfo && !string.IsNullOrEmpty(task.description))
            {
                EditorGUILayout.HelpBox(task.description, MessageType.None);
            }

            ShowWarnings(task);
            SpecialCaseInspector(task);
            ShowAgentField(task);
            task.OnTaskInspectorGUI();
        }

        UndoTools.CheckDirty(task.ownerSystem.contextObject);
    }

    static void ShowWarnings(Task task)
    {
        if (task.firstWarningMessage != null)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Box(EditorTools.warningIcon, GUIStyle.none, GUILayout.Width(16));
            GUILayout.Label(string.Format("<size=9>{0}</size>", task.firstWarningMessage));
            GUILayout.EndHorizontal();
        }
    }

    //Some special cases for Action & Condition. A bit weird but better that creating a virtual method in this case
    static void SpecialCaseInspector(Task task)
    {

        EditorTools.DrawIMessage(task.message);
        if (task is ActionTask)
        {
            if (Application.isPlaying)
            {
                if ((task as ActionTask).elapsedTime > 0) GUI.color = Color.yellow;
                EditorGUILayout.LabelField("Elapsed Time", (task as ActionTask).elapsedTime.ToString());
                GUI.color = Color.white;
            }
        }

        if (task is ConditionTask)
        {
            GUI.color = (task as ConditionTask).invert ? Color.white : new Color(1f, 1f, 1f, 0.5f);
            (task as ConditionTask).invert = EditorGUILayout.ToggleLeft("Invert Condition", (task as ConditionTask).invert);
            GUI.color = Color.white;
        }
    }

    ///Optional override to show custom controls whenever the ShowTaskInspectorGUI is called. By default controls will automaticaly show for most types
    virtual protected void OnTaskInspectorGUI() { DrawDefaultInspector(); }
    ///Draw an automatic editor inspector for this task.
    protected void DrawDefaultInspector() { EditorTools.ShowAutoEditorGUI(this); }


    //a Custom titlebar for tasks
    static bool ShowTaskTitlebar(Task task, Action<Task> callback)
    {

        GUI.backgroundColor = new Color(1, 1, 1, 0.8f);
        GUILayout.BeginHorizontal("box");
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            if (callback != null)
            {
                callback(null);
            }
            return false;
        }

        GUILayout.Label("<b>" + (task.isUnfolded ? "▼ " : "► ") + task.name + "</b>" + (task.isUnfolded ? "" : "\n<i><size=10>(" + task.summaryInfo + ")</size></i>"));

        if (GUILayout.Button(EditorTools.csIcon, (GUIStyle)"label", GUILayout.Width(20), GUILayout.Height(20)))
        {
            EditorTools.OpenScriptOfType(task.GetType());
        }

        GUILayout.EndHorizontal();
        var titleRect = GUILayoutUtility.GetLastRect();
        EditorGUIUtility.AddCursorRect(titleRect, MouseCursor.Link);

        var e = Event.current;

        if (e.type == EventType.ContextClick && titleRect.Contains(e.mousePosition))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open Script"), false, () => { EditorTools.OpenScriptOfType(task.GetType()); });
            menu.AddItem(new GUIContent("Copy"), false, () => { Task.copiedTask = task; });
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                if (callback != null)
                {
                    callback(null);
                }
            });

            menu.ShowAsContext();
            e.Use();
        }

        if (e.button == 0 && e.type == EventType.MouseDown && titleRect.Contains(e.mousePosition))
        {
            e.Use();
        }

        if (e.button == 0 && e.type == EventType.MouseUp && titleRect.Contains(e.mousePosition))
        {
            task.isUnfolded = !task.isUnfolded;
            e.Use();
        }

        return task.isUnfolded;
    }


    //Shows the agent field in case an agent type is specified either with [AgentType] attribute or through the use of the generic versions of Actio or Condition Task
    static void ShowAgentField(Task task)
    {

        if (task.agentType == null)
        {
            return;
        }

        var isMissingType = task.agent == null;
        var infoString = isMissingType ? "<color=#ff5f5f>" + task.agentType.FriendlyName() + "</color>" : task.agentType.FriendlyName();

        GUI.color = new Color(1f, 1f, 1f, 0.5f);
        GUI.backgroundColor = GUI.color;
        GUILayout.BeginVertical("button");
        GUILayout.BeginHorizontal();

        GUILayout.BeginHorizontal();
        var compIcon = EditorGUIUtility.ObjectContent(null, task.agentType).image;
        if (compIcon != null)
        {
            GUILayout.Label(compIcon, GUILayout.Width(16), GUILayout.Height(16));
        }
        GUILayout.Label(string.Format("Use Self ({0})", infoString));
        GUILayout.EndHorizontal();

        GUI.color = Color.white;

        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}

#endif
