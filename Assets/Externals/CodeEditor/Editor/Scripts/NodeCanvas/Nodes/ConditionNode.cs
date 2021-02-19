using UnityEngine;

[Name("Condition")]
[Description("Check a condition and return Success or Failure")]
[Icon("Condition")]
public class ConditionNode : BTNode, ITaskAssignable<ConditionTask>
{

    [SerializeField]
    private ConditionTask _condition;

    public Task task
    {
        get { return condition; }
        set { condition = (ConditionTask)value; }
    }

    public ConditionTask condition
    {
        get { return _condition; }
        set { _condition = value; }
    }

    public override string name
    {
        get { return base.name.ToUpper(); }
    }

    protected override Status OnExecute(Component agent)
    {
        if (condition != null)
            return condition.CheckCondition(agent) ? Status.Success : Status.Failure;
        return Status.Failure;
    }
#if UNITY_EDITOR
    public override BevNode ToBevNode()
    {
        var bevNode = base.ToBevNode();
        if (task?.message != null)
        {
            bevNode.Task = Google.Protobuf.ByteString.CopyFrom(task.message.ToData());
            bevNode.TaskType = task.message.GetType().Name;
        }
        return bevNode;
    }
#endif
}