using System.Collections.Generic;
using UnityEngine;



[Category("Composites")]
[Icon("RandomSelector")]
[Color("b3ff7f")]
public class RandomSelector : BTComposite
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
