#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Graph), true)]
public class GraphInspector : UnityEditor.Editor
{

    private Graph graph
    {
        get { return target as Graph; }
    }

    private string fileExtension
    {
        get { return graph.GetType().Name.GetCapitals(); }
    }

    public override void OnInspectorGUI()
    {

        UndoTools.CheckUndo(this, "Graph Inspector");

        GUI.skin.label.richText = true;
        ShowBasicGUI();
        // if (GUILayout.Button("ExecuteOnce"))
        // {
        //     ExecuteOnce();
        // }
        if (GUILayout.Button("ClearCache"))
        {
            EditorTools.cachedInfos?.Clear();
            EditorTools.cachedInfosIMessage?.Clear();
            EditorTools.cachedSubTypes?.Clear();
        }
        UndoTools.CheckDirty(this);
    }


    //name, description, edit button
    public void ShowBasicGUI()
    {
        GUILayout.Space(10);
        graph.category = GUILayout.TextField(graph.category);
        EditorTools.TextFieldComment(graph.category, "Category...");

        graph.graphComments = GUILayout.TextArea(graph.graphComments, GUILayout.Height(45));
        EditorTools.TextFieldComment(graph.graphComments, "Comments...");

        GUI.backgroundColor = EditorTools.lightBlue;
        if (GUILayout.Button(string.Format("EDIT {0}", graph.GetType().Name.SplitCamelCase().ToUpper())))
        {
            GraphEditor.OpenWindow(graph);
        }

        GUI.backgroundColor = Color.white;
    }

    // public void ExecuteOnce()
    // {
    //     LuaBinding.instance.StartGraph(graph);
    // }
}

#endif