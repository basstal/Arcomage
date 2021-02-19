using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using System.Reflection;
using UnityEngine;
using System;

///This is the base and main class of NodeCanvas and graphs. All graph System are deriving from this.
abstract public partial class Graph : ScriptableObject, ITaskSystem, ISerializationCallbackReceiver
{
    [SerializeField]
    private string _serializedGraph;
    private byte[] _serializedGraphData;
    [SerializeField]
    private bool _deserializationFailed = false;
    [System.NonSerialized]
    private bool hasDeserialized = false;
    //These are the data that are serialized and deserialized into/from a 'GraphSerializationData' object
    ///----------------------------------------------------------------------------------------------
    private string _category = string.Empty;
    private string _comments = string.Empty;
    private Vector2 _translation = new Vector2(-5000, -5000);
    private float _zoomFactor = 1f;
    private List<Node> _nodes = new List<Node>();
    private Node _primeNode = null;
    ///----------------------------------------------------------------------------------------------


    ///////////UNITY CALLBACKS///////////
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        Serialize();
    }
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Deserialize();
    }


    public byte[] Serialize()
    {
#if UNITY_EDITOR //we only serialize in the editor
        //if something went wrong on deserialization, dont serialize back, but rather keep what we had
        if (_deserializationFailed)
        {
            _deserializationFailed = false;
            return _serializedGraphData;
        }

        UpdateNodeIDs(true);

        _serializedGraphData = ToBevTree().ToData();
        return _serializedGraphData;
#endif
    }

    public void Deserialize()
    {
        if (hasDeserialized)
        {
            return;
        }
        hasDeserialized = true;
        this.Deserialize(_serializedGraphData, false);
    }


    protected void OnEnable()
    {
        Validate();
    }
    protected void OnDisable() { }
    protected void OnDestroy() { }
    protected void OnValidate() { }
    //////////////////////////////////////
    //////////////////////////////////////

    ///Deserialize the json serialized graph provided. Returns the structure or null if failed.
    public BevTree Deserialize(byte[] data, bool validate)
    {
        if (data == null)
        {
            Debug.LogError("data is null");
            return null;
        }

        try
        {
            //deserialize provided serialized graph into a new GraphSerializationData object and load it
            BevTree bevTree = SerializeTools.FromData<BevTree>(data);
            if (LoadGraphData(bevTree, validate) == true)
            {
                this._deserializationFailed = false;
                this._serializedGraphData = data;
                return bevTree;
            }

            _deserializationFailed = true;
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            _deserializationFailed = true;
            return null;
        }
    }

    //TODO: Move this in GraphSerializationData object Reconstruction?
    bool LoadGraphData(BevTree data, bool validate)
    {

        if (data == null)
        {
            Debug.LogError("Can't Load graph, cause of null GraphSerializationData provided");
            return false;
        }

        //check serialization versions here in the future if needed
        allNodes.Clear();
        Dictionary<string, Node> uid2NodeMap = new Dictionary<string, Node>();
        Dictionary<string, BevNode> uid2BevNodeMap = new Dictionary<string, BevNode>();
        foreach (var bevNode in data.Nodes)
        {
            var node = Node.Create(this, bevNode);
            allNodes.Add(node);
            uid2NodeMap.Add(bevNode.Uid, node);
            uid2BevNodeMap.Add(bevNode.Uid, bevNode);
        }
        //re-link connections for deserialization
        for (var i = 0; i < data.Connections.Count; i++)
        {
            var bevConnection = data.Connections[i];
            var sourceNode = uid2NodeMap[bevConnection.SourceNodeUid];
            var targetNode = uid2NodeMap[bevConnection.TargetNodeUid];
            var sourceBevNode = uid2BevNodeMap[bevConnection.SourceNodeUid];
            Connection.Create(sourceNode, targetNode, sourceBevNode, bevConnection);
        }

        //re-set the node's owner and ID
        for (var i = 0; i < allNodes.Count; i++)
        {
            allNodes[i].graph = this;
            allNodes[i].ID = i + 1;
        }

        //grab the final data and set fields directly
        this._category = data.Category;
        this._comments = data.Comments;
        this._translation = data.Translation.ToVector2();
        this._zoomFactor = data.ZoomFactor;
        uid2NodeMap.TryGetValue(data.PrimeNodeUid, out _primeNode);

        //IMPORTANT: Validate should be called in all deserialize cases outside of Unity's 'OnAfterDeserialize',
        //like for example when loading from json, or manualy calling this outside of OnAfterDeserialize.
        if (validate)
        {
            Validate();
        }

        return true;
    }

    ///Clones the graph and returns the new one. Currently exactly the same as Instantiate, but could change in the future
    public static T Clone<T>(T graph) where T : Graph
    {
        var newGraph = (T)Instantiate(graph);
        newGraph.name = newGraph.name.Replace("(Clone)", "");
        return (T)newGraph;
    }
    ///Validate the graph and it's nodes. Also called from OnEnable callback.
    public void Validate()
    {

#if UNITY_EDITOR
        if (!Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            UpdateReferences();
        }
#endif

        for (var i = 0; i < allNodes.Count; i++)
        {
            try
            {
                allNodes[i].OnValidate(this);
            } //validation could be critical. we always continue
            catch (System.Exception e)
            {
                Debug.LogError(e);
                continue;
            }
        }

        var allTasks = GetAllTasksOfType<Task>();
        for (var i = 0; i < allTasks.Count; i++)
        {
            try
            {
                allTasks[i].OnValidate(this);
            } //validation could be critical. we always continue
            catch (System.Exception e)
            {
                Debug.LogError(e);
                continue;
            }
        }

        OnGraphValidate();
    }

    ///Use this for derived graph Validation
    virtual protected void OnGraphValidate() { }
    ///Raised when the graph is Stoped/Finished if it was Started at all
    public event System.Action<bool> OnFinish;

    [System.NonSerialized]
    private Component _agent;
    [System.NonSerialized]
    private static List<Graph> runningGraphs = new List<Graph>();
    [System.NonSerialized]
    private float timeStarted;
    [System.NonSerialized]
    private bool _isRunning;
    [System.NonSerialized]
    private bool _isPaused;
    /////


    ///The base type of all nodes that can live in this system
    abstract public System.Type baseNodeType { get; }
    ///Is this system allowed to start with a null agent?
    abstract public bool requiresAgent { get; }
    ///Does the system needs a prime Node to be set for it to start?
    abstract public bool requiresPrimeNode { get; }
    ///Should the the nodes be auto sorted on position x?
    abstract public bool autoSort { get; }
    ///The friendly title name of the graph
    new public string name
    {
        get { return base.name; }
        set { base.name = value; }
    }

    ///Graph category
    public string category
    {
        get { return _category; }
        set { _category = value; }
    }

    ///Graph Comments
    public string graphComments
    {
        get { return _comments; }
        set { _comments = value; }
    }

    ///The time in seconds this graph is running
    public float elapsedTime
    {
        get { return isRunning || isPaused ? Time.time - timeStarted : 0; }
    }

    ///Is the graph running?
    public bool isRunning
    {
        get { return _isRunning; }
        private set { _isRunning = value; }
    }

    ///Is the graph paused?
    public bool isPaused
    {
        get { return _isPaused; }
        private set { _isPaused = value; }
    }

    ///All nodes assigned to this system
    public List<Node> allNodes
    {
        get { return _nodes; }
        private set { _nodes = value; }
    }

    ///The node to execute first. aka 'START'
    public Node primeNode
    {
        get { return _primeNode; }
        set
        {
            if (_primeNode != value)
            {

                if (value != null && value.allowAsPrime == false)
                {
                    return;
                }

                if (isRunning)
                {
                    if (_primeNode != null) _primeNode.Reset();
                    if (value != null) value.Reset();
                }

                RecordUndo("Set Start");

                _primeNode = value;
                UpdateNodeIDs(true);
            }
        }
    }
    ///The translation of the graph in the total canvas (Editor purposes)
    public Vector2 translation
    {
        get { return _translation; }
        set { _translation = value; }
    }

    ///The zoom of the graph (Editor purposes)
    public float zoomFactor
    {
        get { return _zoomFactor; }
        set { _zoomFactor = value; }
    }

    ///The agent currently assigned to the graph
    public Component agent
    {
        get { return _agent; }
        set { _agent = value; }
    }
    ///The UnityObject of the ITaskSystem. In this case the graph itself
    UnityEngine.Object ITaskSystem.contextObject
    {
        get { return this; }
    }
    ///Makes a copy of provided nodes and if targetGraph is provided, pastes the new nodes in that graph.
    public static List<Node> CloneNodes(List<Node> originalNodes, Graph targetGraph = null, Vector2 originPosition = default(Vector2))
    {

        if (targetGraph != null)
        {
            if (originalNodes.Any(n => n.GetType().IsSubclassOf(targetGraph.baseNodeType) == false))
            {
                return null;
            }
        }

        var newNodes = new List<Node>();
        var linkInfo = new Dictionary<Connection, KeyValuePair<int, int>>();

        //duplicate all nodes first
        foreach (var original in originalNodes)
        {
            Node newNode = null;
            if (targetGraph != null)
            {
                newNode = original.Duplicate(targetGraph);
                if (targetGraph != original.graph && original.graph != null && original == original.graph.primeNode)
                {
                    targetGraph.primeNode = newNode;
                }
            }
            else
            {
                newNode = original.Clone();
            }
            newNodes.Add(newNode);

            //store the out connections that need dulpicate along with the indeces of source and target
            foreach (var c in original.outConnections)
            {
                var sourceIndex = originalNodes.IndexOf(c.sourceNode);
                var targetIndex = originalNodes.IndexOf(c.targetNode);
                linkInfo[c] = new KeyValuePair<int, int>(sourceIndex, targetIndex);
            }
        }

        //duplicate all connections that we stored as 'need duplicating' providing new source and target
        foreach (var linkPair in linkInfo)
        {
            if (linkPair.Value.Value != -1)
            { //we check this to see if the target node is part of the duplicated nodes since IndexOf returns -1 if element is not part of the list
                var newSource = newNodes[linkPair.Value.Key];
                var newTarget = newNodes[linkPair.Value.Value];
                if (targetGraph != null)
                {
                    linkPair.Key.Duplicate(newSource, newTarget);
                }
                else
                {
                    var newConnection = linkPair.Key.Clone();
                    newConnection.SetSource(newSource, false);
                    newConnection.SetTarget(newTarget, false);
                }
            }
        }

        if (originPosition != default(Vector2) && newNodes.Count > 0)
        {
            if (newNodes.Count == 1)
            {
                newNodes[0].nodePosition = originPosition;
            }
            else
            {
                var diff = newNodes[0].nodePosition - originPosition;
                newNodes[0].nodePosition = originPosition;
                for (var i = 1; i < newNodes.Count; i++)
                {
                    newNodes[i].nodePosition -= diff;
                }
            }
        }

        return newNodes;
    }

    ///Updates the required references: Setting the system to the tasks and blackboard to BBParameters.
    ///This is done when the graph starts and in the editor for convenience.
    public void UpdateReferences()
    {
        SendTaskOwnerDefaults();
    }

    //Sets all graph Tasks' owner (which is this).
    public void SendTaskOwnerDefaults()
    {
        var tasks = GetAllTasksOfType<Task>();
        for (var i = 0; i < tasks.Count; i++)
        {
            tasks[i].SetOwnerSystem(this);
        }
    }

    ///Update the IDs of the nodes in the graph. Is automatically called whenever a change happens in the graph by the adding removing connecting etc.
    public void UpdateNodeIDs(bool alsoReorderList)
    {

        var lastID = 0;

        //start from prime
        if (primeNode != null)
        {
            lastID = primeNode.AssignIDToGraph(lastID);
        }

        //set the rest starting from nodes without parent(s)
        var tempList = allNodes.OrderBy(n => n.inConnections.Count != 0).ToList();
        for (var i = 0; i < tempList.Count; i++)
        {
            lastID = tempList[i].AssignIDToGraph(lastID);
        }

        //reset the check
        for (var i = 0; i < allNodes.Count; i++)
        {
            allNodes[i].ResetRecursion();
        }

        if (alsoReorderList)
        {
            allNodes = allNodes.OrderBy(node => node.ID).ToList();
        }
    }

    ///Start the graph for the agent and blackboard provided.
    ///Optionally provide a callback for when the graph stops or ends
    public void StartGraph(Component agent, bool autoUpdate, System.Action<bool> callback = null)
    {

#if UNITY_EDITOR //prevent the user to accidentaly start the graph while its an asset. At least in the editor
        if (UnityEditor.EditorUtility.IsPersistent(this))
        {
            Debug.LogError("You have tried to start a graph which is an asset, not an instance! You should Instantiate the graph first.");
            return;
        }
#endif

        if (isRunning)
        {
            if (callback != null)
            {
                OnFinish += callback;
            }
            Debug.LogWarning("Graph is already Active.");
            return;
        }

        if (agent == null && requiresAgent)
        {
            Debug.LogWarning("You've tried to start a graph with null Agent.");
            return;
        }

        if (primeNode == null && requiresPrimeNode)
        {
            Debug.LogWarning("You've tried to start graph without a 'Start' node.");
            return;
        }

        this.agent = agent;
        UpdateReferences();

        if (callback != null)
        {
            this.OnFinish = callback;
        }

        isRunning = true;

        ///place after OnGraphStarted?
        runningGraphs.Add(this);

        if (!isPaused)
        {
            timeStarted = Time.time;
            OnGraphStarted();
        }
        else
        {
            OnGraphUnpaused();
        }
        /////

        for (var i = 0; i < allNodes.Count; i++)
        {
            if (!isPaused)
            {
                allNodes[i].OnGraphStarted();
            }
            else
            {
                allNodes[i].OnGraphUnpaused();
            }
        }

        isPaused = false;

        if (autoUpdate)
        {
            UpdateGraph();
        }
    }

    ///Stops the graph completely and resets all nodes.
    public void Stop(bool success = true)
    {

        if (!isRunning && !isPaused)
        {
            return;
        }

        runningGraphs.Remove(this);

        isRunning = false;
        isPaused = false;

        for (var i = 0; i < allNodes.Count; i++)
        {
            allNodes[i].Reset(false);
            allNodes[i].OnGraphStoped();
        }

        OnGraphStoped();

        if (OnFinish != null)
        {
            OnFinish(success);
            OnFinish = null;
        }
    }

    ///Pauses the graph from updating as well as notifying all nodes and tasks.
    public void Pause()
    {

        if (!isRunning)
        {
            return;
        }

        runningGraphs.Remove(this);

        isRunning = false;
        isPaused = true;

        for (var i = 0; i < allNodes.Count; i++)
        {
            allNodes[i].OnGraphPaused();
        }

        OnGraphPaused();
    }

    ///Updates the graph. Normaly this is updated by MonoManager since at StartGraph, this method is registered for updating.
    public void UpdateGraph()
    {
        // UnityEngine.Profiling.Profiler.BeginSample(string.Format("NC Graph ({0})", agent != null? agent.name : this.name) );
        if (isRunning)
        {
            OnGraphUpdate();
        }
        // UnityEngine.Profiling.Profiler.EndSample();
    }

    ///Override for graph specific stuff to run when the graph is started
    virtual protected void OnGraphStarted() { }

    ///Override for graph specific per frame logic. Called every frame if the graph is running
    virtual protected void OnGraphUpdate() { }

    ///Override for graph specific stuff to run when the graph is stoped
    virtual protected void OnGraphStoped() { }

    ///Override this for when the graph is paused
    virtual protected void OnGraphPaused() { }

    ///Override for graph stuff to run when the graph is unpause
    virtual protected void OnGraphUnpaused() { }
    ///Get a node by it's ID, null if not found
    public Node GetNodeWithID(int searchID)
    {
        if (searchID <= allNodes.Count && searchID >= 0)
        {
            return allNodes.Find(n => n.ID == searchID);
        }
        return null;
    }

    ///Get all nodes of a specific type
    public List<T> GetAllNodesOfType<T>() where T : Node
    {
        return allNodes.OfType<T>().ToList();
    }

    ///Get a node by it's tag name
    public T GetNodeWithTag<T>(string tagName) where T : Node
    {
        foreach (var node in allNodes.OfType<T>())
        {
            if (node.tag == tagName)
            {
                return node;
            }
        }
        return null;
    }

    ///Get all nodes taged with such tag name
    public List<T> GetNodesWithTag<T>(string tagName) where T : Node
    {
        var nodes = new List<T>();
        foreach (var node in allNodes.OfType<T>())
        {
            if (node.tag == tagName)
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    ///Get all taged nodes regardless tag name
    public List<T> GetAllTagedNodes<T>() where T : Node
    {
        var nodes = new List<T>();
        foreach (var node in allNodes.OfType<T>())
        {
            if (!string.IsNullOrEmpty(node.tag))
            {
                nodes.Add(node);
            }
        }
        return nodes;
    }

    ///Get a node by it's name
    public T GetNodeWithName<T>(string name) where T : Node
    {
        foreach (var node in allNodes.OfType<T>())
        {
            if (StripNameColor(node.name).ToLower() == name.ToLower())
            {
                return node;
            }
        }
        return null;
    }

    //removes the text color that some nodes add with html tags
    string StripNameColor(string name)
    {
        if (name.StartsWith("<") && name.EndsWith(">"))
        {
            name = name.Replace(name.Substring(0, name.IndexOf(">") + 1), "");
            name = name.Replace(name.Substring(name.IndexOf("<"), name.LastIndexOf(">") + 1 - name.IndexOf("<")), "");
        }
        return name;
    }

    ///Get all nodes of the graph that have no incomming connections
    public List<Node> GetRootNodes()
    {
        return allNodes.Where(n => n.inConnections.Count == 0).ToList();
    }

    ///Get all nodes of the graph that have no outgoing connections
    public List<Node> GetLeafNodes()
    {
        return allNodes.Where(n => n.inConnections.Count == 0).ToList();
    }

    ///Get all Nested graphs of this graph
    public List<T> GetAllNestedGraphs<T>(bool recursive) where T : Graph
    {
        var graphs = new List<T>();
        foreach (var node in allNodes.OfType<IGraphAssignable>())
        {
            if (node.nestedGraph is T)
            {
                if (!graphs.Contains((T)node.nestedGraph))
                {
                    graphs.Add((T)node.nestedGraph);
                }
                if (recursive)
                {
                    graphs.AddRange(node.nestedGraph.GetAllNestedGraphs<T>(recursive));
                }
            }
        }
        return graphs;
    }

    ///Get all runtime instanced Nested graphs of this graph
    public List<Graph> GetAllInstancedNestedGraphs()
    {
        var instances = new List<Graph>();
        foreach (var node in allNodes.OfType<IGraphAssignable>())
        {
            var subInstances = node.GetInstances();
            instances.AddRange(subInstances);
            foreach (var subInstance in subInstances)
            {
                instances.AddRange(subInstance.GetAllInstancedNestedGraphs());
            }
        }
        return instances;
    }


    ///Get ALL assigned node Tasks of type T, in the graph (including tasks assigned on Nodes and on Connections and within ActionList & ConditionList)
    public List<T> GetAllTasksOfType<T>() where T : Task
    {

        var tasks = new List<Task>();
        var resultTasks = new List<T>();
        for (var i = 0; i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            if (node is ITaskAssignable && (node as ITaskAssignable).task != null)
            {
                tasks.Add((node as ITaskAssignable).task);
            }

            if (node is ISubTasksContainer)
            {
                tasks.AddRange((node as ISubTasksContainer).GetSubTasks());
            }

            for (var j = 0; j < node.outConnections.Count; j++)
            {
                var c = node.outConnections[j];
                if (c is ITaskAssignable && (c as ITaskAssignable).task != null)
                {
                    tasks.Add((c as ITaskAssignable).task);
                }
                if (c is ISubTasksContainer)
                {
                    tasks.AddRange((c as ISubTasksContainer).GetSubTasks());
                }
            }
        }

        for (var i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            if (task is T)
            {
                resultTasks.Add((T)task);
            }
        }

        return resultTasks;
    }

    ///Get the parent node on which target task is assigned.
    ///This is done this way, since Tasks have no dependency on where they are assigned.
    public Node GetTaskParent(Task task)
    {
        if (task == null)
        {
            return null;
        }

        for (var i = 0; i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            if (node is ITaskAssignable)
            {
                var assignable = (ITaskAssignable)node;
                var nodeTask = assignable.task;
                if (nodeTask == task)
                {
                    return node;
                }
            }

            if (node is ISubTasksContainer)
            {
                var container = (ISubTasksContainer)node;
                var subTasks = container.GetSubTasks();
                for (var j = 0; j < subTasks.Length; j++)
                {
                    var subTask = subTasks[j];
                    if (subTask == task)
                    {
                        return node;
                    }
                }
            }
        }
        return null;
    }
    ///Given an element returns the graph containing it.
    public static Graph GetElementGraph(object obj)
    {
        if (obj == null) { return null; }
        if (obj is Graph) { return (Graph)obj; }
        if (obj is Node) { return (obj as Node).graph; }
        if (obj is Connection) { return (obj as Connection).sourceNode.graph; }
        if (obj is Task)
        {
            var task = (Task)obj;
            var graph = task.ownerSystem as Graph;
            if (graph != null)
            {
                var parent = graph.GetTaskParent(task);
                if (parent != null)
                {
                    return parent.graph;
                }
            }
        }
        return null;
    }
    ///Add a new node to this graph
    public T AddNode<T>() where T : Node
    {
        return (T)AddNode(typeof(T));
    }

    public T AddNode<T>(Vector2 pos) where T : Node
    {
        return (T)AddNode(typeof(T), pos);
    }

    public Node AddNode(System.Type nodeType)
    {
        return AddNode(nodeType, new Vector2(50, 50));
    }

    ///Add a new node to this graph
    public Node AddNode(System.Type nodeType, Vector2 pos)
    {
        if (nodeType.IsGenericTypeDefinition)
        {
            nodeType = nodeType.RTMakeGenericType(nodeType.GetFirstGenericParameterConstraintType());
        }

        if (!nodeType.RTIsSubclassOf(baseNodeType))
        {
            Debug.LogWarning(nodeType + " can't be added to " + this.GetType().FriendlyName() + " graph");
            return null;
        }

        var newNode = Node.Create(this, nodeType, pos);

        RecordUndo("New Node");

        allNodes.Add(newNode);

        if (primeNode == null)
        {
            primeNode = newNode;
        }

        UpdateNodeIDs(false);
        return newNode;
    }

    ///Disconnects and then removes a node from this graph
    public void RemoveNode(Node node, bool recordUndo = true)
    {

        if (node.GetType().RTIsDefined<ProtectedAttribute>(true))
        {
            return;
        }

        if (!allNodes.Contains(node))
        {
            Debug.LogWarning("Node is not part of this graph");
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            //auto reconnect parent & child of deleted node. Just a workflow convenience
            if (autoSort && node.inConnections.Count == 1 && node.outConnections.Count == 1)
            {
                var relinkNode = node.inConnections[0].targetNode;
                if (relinkNode != node.inConnections[0].sourceNode)
                {
                    RemoveConnection(node.outConnections[0]);
                    node.inConnections[0].SetTarget(relinkNode);
                }
            }
        }

        //TODO: Fix this in the property accessors?
        currentSelection = null;

#endif
        //callback
        node.OnDestroy();

        //disconnect parents
        foreach (var inConnection in node.inConnections.ToArray())
        {
            RemoveConnection(inConnection);
        }

        //disconnect children
        foreach (var outConnection in node.outConnections.ToArray())
        {
            RemoveConnection(outConnection);
        }

        if (recordUndo)
        {
            RecordUndo("Delete Node");
        }

        allNodes.Remove(node);

        if (node == primeNode)
        {
            primeNode = GetNodeWithID(primeNode.ID + 1);
        }

        UpdateNodeIDs(false);
    }

    ///Connect two nodes together to the next available port of the source node
    public Connection ConnectNodes(Node sourceNode, Node targetNode)
    {
        return ConnectNodes(sourceNode, targetNode, sourceNode.outConnections.Count);
    }

    ///Connect two nodes together to a specific port index of the source node
    public Connection ConnectNodes(Node sourceNode, Node targetNode, int indexToInsert)
    {

        if (targetNode.IsNewConnectionAllowed(sourceNode) == false)
        {
            return null;
        }

        RecordUndo("New Connection");

        var newConnection = Connection.Create(sourceNode, targetNode, indexToInsert);

        sourceNode.OnChildConnected(indexToInsert);
        targetNode.OnParentConnected(targetNode.inConnections.IndexOf(newConnection));

        UpdateNodeIDs(false);
        return newConnection;
    }

    ///Removes a connection
    public void RemoveConnection(Connection connection, bool recordUndo = true)
    {
        //for live editing
        if (Application.isPlaying)
        {
            connection.Reset();
        }

        if (recordUndo)
        {
            RecordUndo("Delete Connection");
        }

        //callbacks
        connection.OnDestroy();
        connection.sourceNode.OnChildDisconnected(connection.sourceNode.outConnections.IndexOf(connection));
        connection.targetNode.OnParentDisconnected(connection.targetNode.inConnections.IndexOf(connection));

        connection.sourceNode.outConnections.Remove(connection);
        connection.targetNode.inConnections.Remove(connection);

#if UNITY_EDITOR
        //TODO: FIX in accessors?
        currentSelection = null;
#endif

        UpdateNodeIDs(false);
    }

    //Helper function
    public void RecordUndo(string name)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.RecordObject(this, name);
        }
#endif
    }

    public BevTree ToBevTree()
    {
        var tree = new BevTree();
        tree.Name = name;
        tree.Category = category;
        tree.Comments = graphComments;
        tree.Translation = translation.ToVector2Proto();
        tree.ZoomFactor = zoomFactor;
        tree.Nodes.AddRange(allNodes.Select(n => n.ToBevNode()));
        //connections are serialized seperately and not part of their parent node
        var structConnections = new List<Connection>();
        for (var i = 0; i < allNodes.Count; i++)
        {
            for (var j = 0; j < allNodes[i].outConnections.Count; j++)
            {
                structConnections.Add(allNodes[i].outConnections[j]);
            }
        }

        tree.Connections.AddRange(structConnections.Select(c => c.ToBevConnection()));
        var uid = primeNode?.UID;
        tree.PrimeNodeUid = string.IsNullOrEmpty(uid) ? "" : uid;
        return tree;
    }
}