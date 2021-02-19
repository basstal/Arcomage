using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Name("SubTree")]
[Category("Nested")]
[Description("SubTree Node can be assigned an entire Sub BehaviorTree. The root node of that behaviour will be considered child node of this node and will return whatever it returns.\nThe target SubTree can also be set by using a Blackboard variable as normal.")]
[Icon("BT")]
public class SubTree : BTNode, IGraphAssignable
{

    private BehaviourTree _subTree = null;
    private Dictionary<BehaviourTree, BehaviourTree> instances = new Dictionary<BehaviourTree, BehaviourTree>();
    private BehaviourTree currentInstance = null;

    public override string name
    {
        get { return base.name.ToUpper(); }
    }

    public BehaviourTree subTree
    {
        get { return _subTree; }
        set { _subTree = value; }
    }

    Graph IGraphAssignable.nestedGraph
    {
        get { return subTree; }
        set { subTree = (BehaviourTree)value; }
    }

    Graph[] IGraphAssignable.GetInstances() { return instances.Values.ToArray(); }

    /////////
    /////////

    protected override Status OnExecute(Component agent)
    {

        if (subTree == null || subTree.primeNode == null)
        {
            return Status.Failure;
        }

        if (status == Status.Resting)
        {
            currentInstance = CheckInstance();
        }

        return currentInstance.Tick(agent);
    }

    protected override void OnReset()
    {
        if (currentInstance != null && currentInstance.primeNode != null)
        {
            currentInstance.primeNode.Reset();
        }
    }

    public override void OnGraphStoped()
    {
        if (currentInstance != null)
        {
            for (var i = 0; i < currentInstance.allNodes.Count; i++)
            {
                currentInstance.allNodes[i].OnGraphStoped();
            }
        }
    }

    public override void OnGraphPaused()
    {
        if (currentInstance != null)
        {
            for (var i = 0; i < currentInstance.allNodes.Count; i++)
            {
                currentInstance.allNodes[i].OnGraphPaused();
            }
        }
    }

    BehaviourTree CheckInstance()
    {

        if (subTree == currentInstance)
        {
            return currentInstance;
        }

        BehaviourTree instance = null;
        if (!instances.TryGetValue(subTree, out instance))
        {
            instance = Graph.Clone<BehaviourTree>(subTree);
            instances[subTree] = instance;
            for (var i = 0; i < instance.allNodes.Count; i++)
            {
                instance.allNodes[i].OnGraphStarted();
            }
        }

        instance.agent = graphAgent;
        instance.UpdateReferences();
        subTree = instance;
        return instance;
    }

    ////////////////////////////
    //////EDITOR AND GUI////////
    ////////////////////////////
#if UNITY_EDITOR

    protected override void OnNodeGUI()
    {
        GUILayout.Label(string.Format("SubTree\n{0}", _subTree));
        if (subTree == null)
        {
            if (!Application.isPlaying && GUILayout.Button("CREATE NEW"))
            {
                Node.CreateNested<BehaviourTree>(this);
            }
        }
    }

    protected override void OnNodeInspectorGUI()
    {
        if (subTree == this.graph)
        {
            Debug.LogWarning("You can't have a Graph nested to iteself! Please select another");
            subTree = null;
        }
    }

#endif
}