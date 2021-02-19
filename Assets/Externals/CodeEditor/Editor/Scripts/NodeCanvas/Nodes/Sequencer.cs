using System.Collections.Generic;
using UnityEngine;


[Category("Composites")]
[Description("Execute the child nodes in order or randonly and return Success if all children return Success, else return Failure\nIf is Dynamic, higher priority child status is revaluated. If a child returns Failure the Sequencer will bail out immediately in Failure too.")]
[Icon("Sequencer")]
[Color("bf7fff")]
public class Sequencer : BTComposite
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

                case Status.Failure:


                    return Status.Failure;
            }
        }

        return Status.Success;
    }

    protected override void OnReset()
    {
        lastRunningNodeIndex = 0;
    }

    public override void OnChildDisconnected(int index)
    {
        if (index != 0 && index == lastRunningNodeIndex)
        {
            lastRunningNodeIndex--;
        }
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