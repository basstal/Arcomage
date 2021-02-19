using System.Collections.Generic;
using UnityEngine;



[Category("Composites")]
[Description("Execute the child nodes in order or randonly until the first that returns Success and return Success as well. If none returns Success, then returns Failure.\nIf is Dynamic, then higher priority children Status are revaluated and if one returns Success the Selector will select that one and bail out immediately in Success too")]
[Icon("Selector")]
[Color("b3ff7f")]
public class Selector : BTComposite
{
    
    private int lastRunningNodeIndex = 0;

    public override string name
    {
        get { return base.name.ToUpper(); }
    }

    protected override Status OnExecute(Component agent)
    {

        for (var i = lastRunningNodeIndex; i < outConnections.Count; i++)
        {

            status = outConnections[i].Execute(agent);

            switch (status)
            {
                case Status.Running:
                    lastRunningNodeIndex = i;
                    return Status.Running;

                case Status.Success:
                    return Status.Success;
            }
        }

        return Status.Failure;
    }

    protected override void OnReset()
    {
        lastRunningNodeIndex = 0;
    }

    public override void OnChildDisconnected(int index)
    {
        if (index != 0 && index == lastRunningNodeIndex)
            lastRunningNodeIndex--;
    }

    public override void OnGraphStarted()
    {
        OnReset();
    }

    //Fisher-Yates shuffle algorithm
    private List<Connection> Shuffle(List<Connection> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = (int)Mathf.Floor(Random.value * (i + 1));
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        return list;
    }


    /////////////////////////////////////////
    /////////GUI AND EDITOR STUFF////////////
    /////////////////////////////////////////
#if UNITY_EDITOR

    protected override void OnNodeGUI()
    {
    }

#endif
}
