using UnityEngine;


/// BehaviourTrees are used to create advanced AI and logic based on simple rules.
public class BehaviourTree : Graph
{
    ///Should the tree repeat forever?
    [SerializeField]
    public bool repeat = true;
    ///The frequency in seconds for the tree to repeat if set to repeat.
    [SerializeField]
    public float updateInterval = 0;

    ///This event is called when the root status of a behaviour is changed
    public static event System.Action<BehaviourTree, Status> onRootStatusChanged;

    private float intervalCounter = 0;
    private Status _rootStatus = Status.Resting;

    ///The last status of the root
    public Status rootStatus
    {
        get { return _rootStatus; }
        private set
        {
            if (_rootStatus != value)
            {
                _rootStatus = value;
                if (onRootStatusChanged != null)
                {
                    onRootStatusChanged(this, value);
                }
            }
        }
    }

    public override System.Type baseNodeType { get { return typeof(BTNode); } }
    public override bool requiresAgent { get { return true; } }
    public override bool requiresPrimeNode { get { return true; } }
    public override bool autoSort { get { return true; } }

    protected override void OnGraphStarted()
    {
        intervalCounter = updateInterval;
        rootStatus = primeNode.status;
    }

    protected override void OnGraphUpdate()
    {

        if (intervalCounter >= updateInterval)
        {
            intervalCounter = 0;
            if (Tick(agent) != Status.Running && !repeat)
            {
                Stop(rootStatus == Status.Success);
            }
        }

        if (updateInterval > 0)
        {
            intervalCounter += Time.deltaTime;
        }
    }

    ///Tick the tree once for the provided agent and with the provided blackboard
    public Status Tick(Component agent)
    {

        if (rootStatus != Status.Running)
        {
            primeNode.Reset();
        }

        rootStatus = primeNode.Execute(agent);
        return rootStatus;
    }

    ////////////////////////////////////////
    ///////////GUI AND EDITOR STUFF/////////
    ////////////////////////////////////////
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Create/Behaviour Tree Asset", false, 0)]
    public static void Editor_CreateGraph()
    {
        var newGraph = EditorTools.CreateAsset<BehaviourTree>(true);
        UnityEditor.Selection.activeObject = newGraph;
    }


    [UnityEditor.MenuItem("Assets/Create/Behaviour Tree Asset")]
    public static void Editor_CreateGraphFix()
    {
        var path = EditorTools.GetAssetUniquePath("BehaviourTree.asset");
        var newGraph = EditorTools.CreateAsset<BehaviourTree>(path);
        UnityEditor.Selection.activeObject = newGraph;
    }
#endif
}