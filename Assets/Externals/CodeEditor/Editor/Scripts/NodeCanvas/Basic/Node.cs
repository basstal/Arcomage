using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using UnityEngine;

// #if UNITY_EDITOR //handles missing Nodes
// [fsObject(Processor = typeof(fsRecoveryProcessor<Node, MissingNode>))]
// #endif
// [ParadoxNotion.Design.SpoofAOT]
///The base class for all nodes that can live in a NodeCanvas Graph
abstract public partial class Node
{
    [SerializeField]
    private Vector2 _position;
    [SerializeField]
    private string _UID;
    [SerializeField]
    private string _name;
    [SerializeField]
    private string _tag;
    [SerializeField]
    private string _comment;

    //reconstructed OnDeserialization
    private Graph _graph;
    //reconstructed OnDeserialization
    private List<Connection> _inConnections = new List<Connection>();
    //reconstructed OnDeserialization
    private List<Connection> _outConnections = new List<Connection>();
    //reconstructed OnDeserialization
    private int _ID;

    [System.NonSerialized]
    private Status _status = Status.Resting;
    [System.NonSerialized]
    private string _nodeName;
    [System.NonSerialized]
    private string _nodeDescription;

    /////

    ///The graph this node belongs to.
    public Graph graph
    {
        get { return _graph; }
        set { _graph = value; }
    }

    ///The node's int ID in the graph.
    public int ID
    {
        get { return _ID; }
        set { _ID = value; }
    }

    ///All incomming connections to this node.
    public List<Connection> inConnections
    {
        get { return _inConnections; }
        protected set { _inConnections = value; }
    }

    ///All outgoing connections from this node.
    public List<Connection> outConnections
    {
        get { return _outConnections; }
        protected set { _outConnections = value; }
    }

    ///The position of the node in the graph.
    public Vector2 nodePosition
    {
        get { return _position; }
        set { _position = value; }
    }

    ///The Unique ID of the node. One is created only if requested.
    public string UID
    {
        get
        {
            if (string.IsNullOrEmpty(_UID))
            {
                _UID = System.Guid.NewGuid().ToString();
            }
            return _UID;
        }
    }

    //The custom title name of the node if any.
    private string customName
    {
        get { return _name; }
        set { _name = value; }
    }

    ///The node tag. Useful for finding nodes through code.
    public string tag
    {
        get { return _tag; }
        set { _tag = value; }
    }

    ///The comments of the node if any.
    public string nodeComment
    {
        get { return _comment; }
        set { _comment = value; }
    }

    ///The title name of the node shown in the window if editor is not in Icon Mode. This is a property so title name may change instance wise
    virtual public string name
    {
        get
        {
            if (!string.IsNullOrEmpty(customName))
            {
                return customName;
            }

            if (string.IsNullOrEmpty(_nodeName))
            {
                var nameAtt = this.GetType().RTGetAttribute<NameAttribute>(true);
                _nodeName = nameAtt != null ? nameAtt.name : GetType().FriendlyName().SplitCamelCase();
            }
            return _nodeName;
        }
        set { customName = value; }
    }

    ///The description info of the node
    virtual public string description
    {
        get
        {
            if (string.IsNullOrEmpty(_nodeDescription))
            {
                var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(true);
                _nodeDescription = descAtt != null ? descAtt.description : "No Description";
            }
            return _nodeDescription;
        }
    }


    ///The numer of possible inputs. -1 for infinite.
    abstract public int maxInConnections { get; }
    ///The numer of possible outputs. -1 for infinite.
    abstract public int maxOutConnections { get; }
    ///The output connection Type this node has.
    abstract public System.Type outConnectionType { get; }
    ///Can this node be set as prime (Start)?
    abstract public bool allowAsPrime { get; }
    ///Alignment of the comments when shown.
    abstract public Alignment2x2 commentsAlignment { get; }
    ///The placement of the icon. By default it replace the title text
    abstract public Alignment2x2 iconAlignment { get; }


    ///The current status of the node
    public Status status
    {
        get { return _status; }
        protected set { _status = value; }
    }

    ///The current agent. Taken from the graph this node belongs to
    public Component graphAgent
    {
        get { return graph != null ? graph.agent : null; }
    }

    //Used to check recursion
    private bool isChecked { get; set; }

    /////////////////////
    /////////////////////
    /////////////////////

    //required
    public Node() { }

    public static Node Create(Graph targetGraph, BevNode bevNode)
    {
        var nodeType = TypeInfo.GetType(bevNode.Type);
        var newNode = Create(targetGraph, nodeType, bevNode.Position.ToVector2(), true);
        newNode._UID = bevNode.Uid;
        var messageInstance = (IMessage)Utility.CreateIMessage(bevNode.TaskType);
        if (messageInstance != null)
        {
            var taskProperty = nodeType.GetProperty("task");
            if (taskProperty != null)
            {
                messageInstance.MergeFrom(bevNode.Task.ToByteArray());
                var messageField = taskProperty.PropertyType.GetField("message");
                var task = taskProperty.GetValue(newNode);
                if (task == null)
                {
                    task = Activator.CreateInstance(bevNode.TaskType.StartsWith("Action") ? typeof(ActionTask) : typeof(ConditionTask));
                    taskProperty.SetValue(newNode, task);
                }
                messageField.SetValue(task, messageInstance);
            }
        }
        return newNode;
    }
    ///Create a new Node of type and assigned to the provided graph. Use this for constructor
    public static Node Create(Graph targetGraph, System.Type nodeType, Vector2 pos, bool onSerialization = false)
    {

        if (targetGraph == null)
        {
            Debug.LogError("Can't Create a Node without providing a Target Graph");
            return null;
        }

        var newNode = (Node)System.Activator.CreateInstance(nodeType);
        if (!onSerialization)
        {
            targetGraph.RecordUndo("Create Node");
        }


        newNode.graph = targetGraph;
        newNode.nodePosition = pos;

        newNode.OnValidate(targetGraph);
        newNode.OnCreate(targetGraph);
        return newNode;
    }

    ///Duplicate node alone assigned to the provided graph
    public Node Duplicate(Graph targetGraph)
    {

        if (targetGraph == null)
        {
            Debug.LogError("Can't duplicate a Node without providing a Target Graph");
            return null;
        }

        //deep clone
        var newNode = Clone();

        if (targetGraph != null)
        {
            targetGraph.RecordUndo("Duplicate Node");
        }

        targetGraph.allNodes.Add(newNode);
        newNode._UID = null;
        newNode.inConnections.Clear();
        newNode.outConnections.Clear();

        if (targetGraph == this.graph)
        {
            newNode.nodePosition += new Vector2(50, 50);
        }

        newNode.graph = targetGraph;

        var assignable = this as ITaskAssignable;
        if (assignable != null && assignable.task != null)
        {
            (newNode as ITaskAssignable).task = assignable.task.Duplicate(targetGraph);
        }

        newNode.OnValidate(targetGraph);
        return newNode;
    }

    ///Called once the first time node is created.
    virtual public void OnCreate(Graph assignedGraph) { }
    ///Called when the Node is created, duplicated or otherwise needs validation.
    virtual public void OnValidate(Graph assignedGraph) { }
    ///Called when the Node is removed from the graph (always through graph.RemoveNode)
    virtual public void OnDestroy() { }


    ///The main execution function of the node. Execute the node for the agent and blackboard provided. Default = graphAgent and graphBlackboard
    public Status Execute(Component agent)
    {

        if (isChecked)
        {
            return Error("Infinite Loop. Please check for other errors that may have caused this in the log before this.");
        }

        isChecked = true;
        status = OnExecute(agent);
        isChecked = false;

        return status;
    }

    ///A little helper function to log errors easier
    protected Status Error(string error)
    {
        Debug.LogError(string.Format("{0} | On Node '{1}' | ID '{2}' | Graph '{3}'", error, name, ID, graph.name));
        status = Status.Error;
        return Status.Error;
    }

    ///A little helper function to log errors easier
    public Status Fail(System.Exception e)
    {
        Debug.LogError(e);
        status = Status.Failure;
        return Status.Failure;
    }

    ///A little helper function to log errors easier
    public Status Fail(string error)
    {
        Debug.LogError(string.Format("{0} | On Node '{1}' | ID '{2}' | Graph '{3}'", error, name, ID, graph.name));
        status = Status.Failure;
        return Status.Failure;
    }

    ///Set the Status of the node directly. Not recomended if you don't know why!
    public void SetStatus(Status status)
    {
        this.status = status;
    }

    ///Recursively reset the node and child nodes if it's not Resting already
    public void Reset(bool recursively = true)
    {

        if (status == Status.Resting || isChecked)
        {
            return;
        }

        OnReset();
        status = Status.Resting;

        isChecked = true;
        for (var i = 0; i < outConnections.Count; i++)
        {
            outConnections[i].Reset(recursively);
        }
        isChecked = false;
    }
    ///Returns if a new input connection should be allowed.
    public bool IsNewConnectionAllowed() { return IsNewConnectionAllowed(null); }
    ///Returns if a new input connection should be allowed from the source node.
    public bool IsNewConnectionAllowed(Node sourceNode)
    {

        if (sourceNode != null)
        {
            if (this == sourceNode)
            {
                Debug.LogWarning("Node can't connect to itself");
                return false;
            }

            if (sourceNode.outConnections.Count >= sourceNode.maxOutConnections && sourceNode.maxOutConnections != -1)
            {
                Debug.LogWarning("Source node can have no more out connections.");
                return false;
            }
        }

        if (this == graph.primeNode && maxInConnections == 1)
        {
            Debug.LogWarning("Target node can have no more connections");
            return false;
        }

        if (maxInConnections <= inConnections.Count && maxInConnections != -1)
        {
            Debug.LogWarning("Target node can have no more connections");
            return false;
        }

        return true;
    }

    //Updates the node ID in it's current graph. This is called in the editor GUI for convenience, as well as whenever a change is made in the node graph and from the node graph.
    public int AssignIDToGraph(int lastID)
    {

        if (isChecked)
        {
            return lastID;
        }

        isChecked = true;
        lastID++;
        ID = lastID;

        for (var i = 0; i < outConnections.Count; i++)
        {
            lastID = outConnections[i].targetNode.AssignIDToGraph(lastID);
        }

        return lastID;
    }

    public void ResetRecursion()
    {

        if (!isChecked)
        {
            return;
        }

        isChecked = false;
        for (var i = 0; i < outConnections.Count; i++)
        {
            outConnections[i].targetNode.ResetRecursion();
        }
    }

    ///Returns all parent nodes in case node can have many parents like in FSM and Dialogue Trees
    public List<Node> GetParentNodes()
    {
        if (inConnections.Count != 0)
        {
            return inConnections.Select(c => c.sourceNode).ToList();
        }
        return new List<Node>();
    }

    ///Get all childs of this node, on the first depth level
    public List<Node> GetChildNodes()
    {
        if (outConnections.Count != 0)
        {
            return outConnections.Select(c => c.targetNode).ToList();
        }
        return new List<Node>();
    }

    ///Override to define node functionality. The Agent and Blackboard used to start the Graph are propagated
    virtual protected Status OnExecute(Component agent) { return status; }

    ///Called when the node gets reseted. e.g. OnGraphStart, after a tree traversal, when interrupted, OnGraphEnd etc...
    virtual protected void OnReset() { }

    ///Called when an input connection is connected
    virtual public void OnParentConnected(int connectionIndex) { }

    ///Called when an input connection is disconnected but before it actually does
    virtual public void OnParentDisconnected(int connectionIndex) { }

    ///Called when an output connection is connected
    virtual public void OnChildConnected(int connectionIndex) { }

    ///Called when an output connection is disconnected but before it actually does
    virtual public void OnChildDisconnected(int connectionIndex) { }

    ///Called when the parent graph is started. Use to init values or otherwise.
    virtual public void OnGraphStarted() { }

    ///Called when the parent graph is stopped.
    virtual public void OnGraphStoped() { }

    ///Called when the parent graph is paused.
    virtual public void OnGraphPaused() { }

    ///Called when the parent graph is unpaused.
    virtual public void OnGraphUnpaused() { }

    sealed public override string ToString()
    {
        return string.Format("{0} ({1})", name, tag);
    }

    public void OnDrawGizmos()
    {
        if (this is ITaskAssignable && (this as ITaskAssignable).task != null)
        {
            (this as ITaskAssignable).task.OnDrawGizmos();
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (this is ITaskAssignable && (this as ITaskAssignable).task != null)
        {
            (this as ITaskAssignable).task.OnDrawGizmosSelected();
        }
    }
    public Node Clone()
    {
        return JsonUtility.FromJson<Node>(JsonUtility.ToJson(this));
    }
    public virtual BevNode ToBevNode()
    {
        var bevNode = new BevNode();

        bevNode.Uid = UID;
        bevNode.Id = ID;
        bevNode.Name = name;
        bevNode.Tag = string.IsNullOrEmpty(tag) ? "" : tag;
        bevNode.Comment = string.IsNullOrEmpty(nodeComment) ? "" : nodeComment;
        bevNode.Type = GetType().Name;
        bevNode.InConnectionsUid.AddRange(inConnections.Select(c => c.UID));
        bevNode.OutConnectionsUid.AddRange(outConnections.Select(c => c.UID));
        bevNode.Position = nodePosition.ToVector2Proto();

        return bevNode;
    }
}