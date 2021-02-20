#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


partial class Graph
{

    private Graph _currentChildGraph;

    private static Rect inspectorRect = default(Rect);
    private static bool isResizingInspectorPanel;
    private static Vector2 nodeInspectorScrollPos;
    private static object _currentSelection;
    private static List<object> _multiSelection = new List<object>();
    public static System.Action PostGUI { get; set; }
    public static bool allowClick { get; private set; }
    //responsible for the breacrumb navigation
    public Graph currentChildGraph
    {
        get { return _currentChildGraph; }
        set
        {
            if (Application.isPlaying && value != null && EditorUtility.IsPersistent(value))
            {
                Debug.LogWarning("You can't view sub-graphs in play mode until they are initialized to avoid editing asset references accidentally");
                return;
            }

            Undo.RecordObject(this, "Change View");
            if (value != null)
            {
                value.currentChildGraph = null;
            }
            _currentChildGraph = value;
        }
    }

    //Selected Node or Connection
    public static object currentSelection
    {
        get
        {
            if (multiSelection.Count > 1)
            {
                return null;
            }
            if (multiSelection.Count == 1)
            {
                return multiSelection[0];
            }
            return _currentSelection;
        }
        set
        {
            if (!multiSelection.Contains(value))
            {
                multiSelection.Clear();
            }
            _currentSelection = value;
            GUIUtility.keyboardControl = 0;
            SceneView.RepaintAll(); //for gizmos
        }
    }

    public static List<object> multiSelection
    {
        get { return _multiSelection; }
        set
        {
            if (value != null && value.Count == 1)
            {
                currentSelection = value[0];
                value.Clear();
            }
            _multiSelection = value != null ? value : new List<object>();
        }
    }

    public static Node selectedNode
    {
        get { return currentSelection as Node; }
    }

    public static Connection selectedConnection
    {
        get { return currentSelection as Connection; }
    }

    private string exportFileExtension
    {
        get { return "json"; }
    }

    private float screenWidth
    { //for retina
        get { return EditorGUIUtility.currentViewWidth; }
    }

    private float screenHeight
    {
        get { return Screen.height; }
    }

    ///

    ///Clears the whole graph
    public void ClearGraph()
    {
        foreach (var node in allNodes.ToArray())
        {
            RemoveNode(node);
        }
    }

    //This is called while within Begin/End windows from the GraphEditor.
    public void ShowNodesGUI(Event e, Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos, float zoomFactor)
    {

        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;

        for (var i = 0; i < allNodes.Count; i++)
        {
            //ensure IDs are updated
            if (allNodes[i].ID != i + 1)
            {
                UpdateNodeIDs(true);
                break;
            }

            allNodes[i].ShowNodeGUI(drawCanvas, fullDrawPass, canvasMousePos, zoomFactor);
        }

        if (primeNode != null)
        {
            GUI.Box(new Rect(primeNode.nodeRect.x, primeNode.nodeRect.y - 20, primeNode.nodeRect.width, 20), "<b>START</b>");
        }
    }

    //This is called outside of windows
    public void ShowGraphControls(Event e, Vector2 canvasMousePos)
    {

        ShowToolbar(e);
        ShowInspectorGUIPanel(e, canvasMousePos);
        ShowGraphCommentsGUI(e, canvasMousePos);
        HandleEvents(e, canvasMousePos);
        AcceptDrops(e, canvasMousePos);

        if (PostGUI != null)
        {
            PostGUI();
            PostGUI = null;
        }
    }

    //This is called outside Begin/End Windows from GraphEditor.
    // NOTE:菜单栏
    void ShowToolbar(Event e)
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);

        ///FILE
        if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Clear"), false, () =>
            {
                if (EditorUtility.DisplayDialog("Clear Canvas", "This will delete all nodes of the currently viewing graph!\nAre you sure?", "YES", "NO!"))
                {
                    ClearGraph();
                    e.Use();
                    return;
                }
            });

            //Import JSON
            menu.AddItem(new GUIContent("Import JSON"), false, () =>
            {
                if (allNodes.Count > 0 && !EditorUtility.DisplayDialog("Import Graph", "All current graph information will be lost. Are you sure?", "YES", "NO"))
                    return;

                var path = EditorUtility.OpenFilePanel(string.Format("Import '{0}' Graph", this.GetType().Name), "Assets", exportFileExtension);
                if (!string.IsNullOrEmpty(path))
                {
                    if (this.Deserialize(System.IO.File.ReadAllBytes(path), true) == null)
                    {
                        EditorUtility.DisplayDialog("Import Failure", "Please read the logs for more information", "OK", "");
                    }
                }
            });

            //Export JSON
            menu.AddItem(new GUIContent("Export JSON"), false, () =>
            {
                var path = EditorUtility.SaveFilePanelInProject(string.Format("Export '{0}' Graph", this.GetType().Name), "", exportFileExtension, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // System.IO.File.WriteAllBytes(path, Serialize());
                    System.IO.File.WriteAllText(path, ToBevTree().ToJson());
                    AssetDatabase.Refresh();
                }
            });

            menu.AddItem(new GUIContent("Import Data"), false, () =>
            {
                if (allNodes.Count > 0 && !EditorUtility.DisplayDialog("Import Graph", "All current graph information will be lost. Are you sure?", "YES", "NO"))
                    return;

                var path = EditorUtility.OpenFilePanel(string.Format("Import '{0}' Graph", this.GetType().Name), "Assets", "bytes");
                if (!string.IsNullOrEmpty(path))
                {
                    if (this.Deserialize(System.IO.File.ReadAllBytes(path), true) == null)
                    {
                        EditorUtility.DisplayDialog("Import Failure", "Please read the logs for more information", "OK", "");
                    }
                }
            });
            menu.AddItem(new GUIContent("Export Data"), false, () =>
            {
                var path = EditorUtility.SaveFilePanelInProject(string.Format("Export '{0}' Graph", this.GetType().Name), "", "bytes", "");
                if (!string.IsNullOrEmpty(path))
                {
                    System.IO.File.WriteAllBytes(path, Serialize());
                    AssetDatabase.Refresh();
                }
            });

            menu.AddItem(new GUIContent("Show JSON"), false, () =>
            {
                Debug.Log(ToBevTree().ToJson());
            });

            menu.AddItem(new GUIContent("Import Old JSON"), false, () =>
            {
                if (allNodes.Count > 0 && !EditorUtility.DisplayDialog("Import Graph", "All current graph information will be lost. Are you sure?", "YES", "NO"))
                    return;

                var path = EditorUtility.OpenFilePanel(string.Format("Import '{0}' Graph", this.GetType().Name), "Assets", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    var data = System.IO.File.ReadAllText(path);
                    CodeNode oldData = SerializeTools.FromJson<CodeNode>(data);
                    byte[] newData = TransformTools.OldData2NewData(oldData);
                    if (this.Deserialize(newData, true) == null)
                    {
                        EditorUtility.DisplayDialog("Import Failure", "Please read the logs for more information", "OK", "");
                    }
                }
            });

            menu.ShowAsContext();
        }

        ///----------------------------------------------------------------------------------------------
        ///PREFS
        if (GUILayout.Button("Prefs", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Use Node Browser"), Preferences.useBrowser, () => { Preferences.useBrowser = !Preferences.useBrowser; });
            menu.AddItem(new GUIContent("Show Icons"), Preferences.showIcons, () =>
            {
                Preferences.showIcons = !Preferences.showIcons;
                foreach (var node in allNodes)
                {
                    node.nodeRect = new Rect(node.nodePosition.x, node.nodePosition.y, Node.minSize.x, Node.minSize.y);
                }
            });
            menu.AddItem(new GUIContent("Show Node Help"), Preferences.showNodeInfo, () => { Preferences.showNodeInfo = !Preferences.showNodeInfo; });
            menu.AddItem(new GUIContent("Show Comments"), Preferences.showComments, () => { Preferences.showComments = !Preferences.showComments; });
            menu.AddItem(new GUIContent("Show Summary Info"), Preferences.showTaskSummary, () => { Preferences.showTaskSummary = !Preferences.showTaskSummary; });
            menu.AddItem(new GUIContent("Show Node IDs"), Preferences.showNodeIDs, () => { Preferences.showNodeIDs = !Preferences.showNodeIDs; });
            menu.AddItem(new GUIContent("Grid Snap"), Preferences.doSnap, () => { Preferences.doSnap = !Preferences.doSnap; });
            menu.AddItem(new GUIContent("Log Events"), Preferences.logEvents, () => { Preferences.logEvents = !Preferences.logEvents; });
            menu.AddItem(new GUIContent("Breakpoints Pause Editor"), Preferences.breakpointPauseEditor, () => { Preferences.breakpointPauseEditor = !Preferences.breakpointPauseEditor; });
            menu.AddItem(new GUIContent("Highlight Active In Hierarchy"), Preferences.highlightOwnersInHierarchy, () => { Preferences.highlightOwnersInHierarchy = !Preferences.highlightOwnersInHierarchy; });
            if (autoSort)
            {
                menu.AddItem(new GUIContent("Automatic Hierarchical Move"), Preferences.hierarchicalMove, () => { Preferences.hierarchicalMove = !Preferences.hierarchicalMove; });
            }
            menu.AddItem(new GUIContent("Connection Style/Curved"), Preferences.connectionStyle == Preferences.ConnectionStyle.Curved, () => { Preferences.connectionStyle = Preferences.ConnectionStyle.Curved; });
            menu.AddItem(new GUIContent("Connection Style/Stepped"), Preferences.connectionStyle == Preferences.ConnectionStyle.Stepped, () => { Preferences.connectionStyle = Preferences.ConnectionStyle.Stepped; });
            menu.AddItem(new GUIContent("Connection Style/Linear"), Preferences.connectionStyle == Preferences.ConnectionStyle.Linear, () => { Preferences.connectionStyle = Preferences.ConnectionStyle.Linear; });

            menu.AddItem(new GUIContent("Node Header Style/Colorize Header"), Preferences.nodeHeaderStyle == Preferences.NodeHeaderStyle.ColorizeHeader, () => { Preferences.nodeHeaderStyle = Preferences.NodeHeaderStyle.ColorizeHeader; });
            menu.AddItem(new GUIContent("Node Header Style/Colorize Title"), Preferences.nodeHeaderStyle == Preferences.NodeHeaderStyle.ColorizeTitle, () => { Preferences.nodeHeaderStyle = Preferences.NodeHeaderStyle.ColorizeTitle; });
            menu.ShowAsContext();
        }

        GUILayout.Space(10);

        if (EditorUtility.IsPersistent(this) && GUILayout.Button("Select Graph", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            Selection.activeObject = this;
            EditorGUIUtility.PingObject(this);
        }

        GUILayout.Space(10);

        ///----------------------------------------------------------------------------------------------
        ///Right side
        ///----------------------------------------------------------------------------------------------

        GUILayout.Space(10);
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = Color.clear;
        GUI.color = new Color(1, 1, 1, 0.3f);
        GUILayout.Space(10);
        GUI.color = Color.white;
        GUI.backgroundColor = Color.white;

        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        GUI.color = Color.white;
    }

    void HandleEvents(Event e, Vector2 canvasMousePos)
    {

        //we also undo graph pans?
        if (e.button == 2 && (e.type == EventType.MouseDown || e.type == EventType.MouseUp))
        {
            Undo.RegisterCompleteObjectUndo(this, "Graph Pan");
        }

        //variable is set as well, so that  nodes know if they can be clicked
        var inspectorWithMargins = inspectorRect.ExpandBy(14);
        allowClick = !inspectorWithMargins.Contains(e.mousePosition);
        if (!allowClick)
        {
            return;
        }

        //Shortcuts
        if (e.type == EventType.ValidateCommand || e.type == EventType.Used)
        {
            if (e.commandName == "Copy" || e.commandName == "Cut")
            {
                List<Node> selection = null; ;
                if (Graph.selectedNode != null)
                {
                    selection = new List<Node> { Graph.selectedNode };
                }
                if (Graph.multiSelection != null && Graph.multiSelection.Count > 0)
                {
                    selection = Graph.multiSelection.Cast<Node>().ToList();
                }
                if (selection != null)
                {
                    Node.copiedNodes = Graph.CloneNodes(selection).ToArray();
                    if (e.commandName == "Cut")
                    {
                        foreach (Node node in selection) { this.RemoveNode(node); }
                    }
                }
                e.Use();
            }
            if (e.commandName == "Paste")
            {
                if (Node.copiedNodes != null && Node.copiedNodes.Length > 0)
                {
                    TryPasteNodesInGraph(Node.copiedNodes, canvasMousePos + new Vector2(500, 500) / zoomFactor);
                }
                e.Use();
            }
        }

        //Shortcuts
        if (e.type == EventType.KeyUp && GUIUtility.keyboardControl == 0)
        {

            //Delete
            if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
            {

                if (multiSelection != null && multiSelection.Count > 0)
                {
                    foreach (var obj in multiSelection.ToArray())
                    {
                        if (obj is Node)
                        {
                            RemoveNode(obj as Node);
                        }
                        if (obj is Connection)
                        {
                            RemoveConnection(obj as Connection);
                        }
                    }
                    multiSelection = null;
                }

                if (selectedNode != null)
                {
                    RemoveNode(selectedNode);
                    currentSelection = null;
                }

                if (selectedConnection != null)
                {
                    RemoveConnection(selectedConnection);
                    currentSelection = null;
                }
                e.Use();
            }

            //Duplicate
            if (e.keyCode == KeyCode.D && e.control)
            {
                if (multiSelection != null && multiSelection.Count > 0)
                {
                    var newNodes = CloneNodes(multiSelection.OfType<Node>().ToList(), this);
                    multiSelection = newNodes.Cast<object>().ToList();
                }
                if (selectedNode != null)
                {
                    currentSelection = selectedNode.Duplicate(this);
                }
                //Connections can't be duplicated by themselves. They do so as part of multiple node duplication.
                e.Use();
            }
        }


        //Tilt '`' or 'space' opens up the complete context menu browser
        if (e.type == EventType.KeyDown && !e.shift && (e.keyCode == KeyCode.BackQuote || e.keyCode == KeyCode.Space))
        {
            CompleteContextMenu.Show(GetAddNodeMenu(canvasMousePos), e.mousePosition, string.Format("Add {0} Node", this.GetType().FriendlyName()), baseNodeType);
        }


        //Right click canvas context menu. Basicaly for adding new nodes.
        if (e.type == EventType.ContextClick)
        {
            var menu = GetAddNodeMenu(canvasMousePos);
            if (Node.copiedNodes != null && Node.copiedNodes[0].GetType().IsSubclassOf(baseNodeType))
            {
                menu.AddSeparator("/");
                if (Node.copiedNodes.Length == 1)
                {
                    menu.AddItem(new GUIContent(string.Format("Paste Node ({0})", Node.copiedNodes[0].GetType().FriendlyName())), false, () => { TryPasteNodesInGraph(Node.copiedNodes, canvasMousePos); });
                }
                else if (Node.copiedNodes.Length > 1)
                {
                    menu.AddItem(new GUIContent(string.Format("Paste Nodes ({0})", Node.copiedNodes.Length.ToString())), false, () => { TryPasteNodesInGraph(Node.copiedNodes, canvasMousePos); });
                }
            }

            if (Preferences.useBrowser)
            {
                menu.ShowAsBrowser(e.mousePosition, string.Format("Add {0} Node", this.GetType().FriendlyName()), baseNodeType);
            }
            else
            {
                menu.ShowAsContext();

            }
            e.Use();
        }
    }

    //Paste nodes in this graph
    void TryPasteNodesInGraph(Node[] nodes, Vector2 originPosition)
    {
        var newNodes = Graph.CloneNodes(nodes.ToList(), this, originPosition);
        multiSelection = newNodes.Cast<object>().ToList();
    }

    ///The final generic menu used for adding nodes in the canvas
    GenericMenu GetAddNodeMenu(Vector2 canvasMousePos)
    {
        System.Action<System.Type> Selected = (type) => { currentSelection = AddNode(type, canvasMousePos); };
        var menu = EditorTools.GetTypeSelectionMenu(baseNodeType, Selected);
        menu = OnCanvasContextMenu(menu, canvasMousePos);
        return menu;
    }

    ///Override to add extra context sensitive options in the right click canvas context menu
    virtual protected GenericMenu OnCanvasContextMenu(GenericMenu menu, Vector2 canvasMousePos)
    {
        return menu;
    }

    //Show the comments window
    void ShowGraphCommentsGUI(Event e, Vector2 canvasMousePos)
    {
        if (Preferences.showComments && !string.IsNullOrEmpty(graphComments))
        {
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.3f);
            GUI.Box(new Rect(10, screenHeight - 100, 330, 70), graphComments, (GUIStyle)"textArea");
            GUI.backgroundColor = Color.white;
        }
    }

    //This is the window shown at the top left with a GUI for extra editing opions of the selected node.
    void ShowInspectorGUIPanel(Event e, Vector2 canvasMousePos)
    {

        if ((selectedNode == null && selectedConnection == null) || Preferences.useExternalInspector)
        {
            inspectorRect.height = 0;
            return;
        }

        inspectorRect.x = 10;
        inspectorRect.y = 30;
        inspectorRect.width = Preferences.inspectorPanelWidth;

        var resizeRect = Rect.MinMaxRect(inspectorRect.xMax - 2, inspectorRect.yMin, inspectorRect.xMax + 2, inspectorRect.yMax);
        EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeHorizontal);
        if (e.type == EventType.MouseDown && resizeRect.Contains(e.mousePosition)) { isResizingInspectorPanel = true; e.Use(); }
        if (isResizingInspectorPanel && e.type == EventType.Layout) { Preferences.inspectorPanelWidth += e.delta.x; }
        if (e.rawType == EventType.MouseUp) { isResizingInspectorPanel = false; }


        var headerRect = new Rect(inspectorRect.x, inspectorRect.y, inspectorRect.width, 30);
        EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);
        if (GUI.Button(headerRect, ""))
        {
            Preferences.showNodePanel = !Preferences.showNodePanel;
        }

        GUI.Box(inspectorRect, "", (GUIStyle)"windowShadow");
        var title = selectedNode != null ? selectedNode.name : "Connection";
        if (Preferences.showNodePanel)
        {

            var lastSkin = GUI.skin;
            var viewRect = new Rect(inspectorRect.x, inspectorRect.y, inspectorRect.width + 18, screenHeight - inspectorRect.y - 30);
            nodeInspectorScrollPos = GUI.BeginScrollView(viewRect, nodeInspectorScrollPos, inspectorRect);

            GUILayout.BeginArea(inspectorRect, title, (GUIStyle)"editorPanel");
            GUILayout.Space(5);
            GUI.skin = null;

            if (selectedNode != null)
            {
                selectedNode.ShowNodeInspectorGUI();
            }
            else if (selectedConnection != null)
            {
                selectedConnection.ShowConnectionInspectorGUI();
            }

            EditorTools.EndOfInspector();
            GUI.skin = lastSkin;
            if (e.type == EventType.Repaint)
            {
                inspectorRect.height = GUILayoutUtility.GetLastRect().yMax + 5;
            }

            GUILayout.EndArea();
            GUI.EndScrollView();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(this);
            }

        }
        else
        {

            inspectorRect.height = 55;
            GUILayout.BeginArea(inspectorRect, title, (GUIStyle)"editorPanel");
            GUI.color = new Color(1, 1, 1, 0.2f);
            if (GUILayout.Button("..."))
            {
                Preferences.showNodePanel = true;
            }
            GUILayout.EndArea();
            GUI.color = Color.white;
        }
    }

    //Handles Drag&Drop operations
    void AcceptDrops(Event e, Vector2 canvasMousePos)
    {

        if (allowClick)
        {

            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length == 1)
            {

                if (e.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                }

                if (e.type == EventType.DragPerform)
                {
                    var value = DragAndDrop.objectReferences[0];
                    DragAndDrop.AcceptDrag();
                    OnDropAccepted(value, canvasMousePos);
                }
            }
        }
    }

    ///Handles drag and drop objects in the graph
    virtual protected void OnDropAccepted(Object o, Vector2 canvasMousePos) { }

    [ContextMenu("Deep Duplicate")]
    public void DeepDuplicate()
    {
        if (EditorUtility.DisplayDialog("Deep Duplicate", "This will create a deep duplicate of this graph asset along with it's subgraphs. Continue?", "Yes", "No"))
        {
            DeepCopy(this);
        }
    }

    ///Make a deep copy of provided graph asset along with it's sub-graphs.
    static Graph DeepCopy(Graph root)
    {
        if (root == null)
        {
            return null;
        }

        var path = EditorUtility.SaveFilePanelInProject("Duplicate of " + root.name, root.name + "_duplicate.asset", "asset", "");
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var copy = (Graph)ScriptableObject.CreateInstance(root.GetType());
        AssetDatabase.CreateAsset(copy, path);
        EditorUtility.CopySerialized(root, copy);

        //make use of IGraphAssignable interface to find nodes that represent a sub-graph.
        foreach (var subGraphNode in copy.allNodes.OfType<IGraphAssignable>())
        {
            if (subGraphNode.nestedGraph != null)
            {
                //duplicate the existing sub-graph and assign the copy to node.
                subGraphNode.nestedGraph = DeepCopy(subGraphNode.nestedGraph); ;
            }
        }

        copy.Validate();
        AssetDatabase.SaveAssets();
        return copy;
    }
}

#endif
