using System;
using System.Collections;
using UnityEngine;

///Base class for all Conditions. Conditions dont span multiple frames like actions and return true or false immediately on execution. Derive this to create your own.
///Generic version to define the AgentType instead of using the [AgentType] attribute. T is the agentType (Behaviour or Interface) required by the Condition.
abstract public class ConditionTask<T> : ConditionTask where T : class
{
    sealed public override Type agentType { get { return typeof(T); } }
    new public T agent { get { return base.agent as T; } }
}


// #if UNITY_EDITOR //handles missing types
// [fsObject(Processor = typeof(fsRecoveryProcessor<ConditionTask, MissingCondition>))]
// #endif

///Base class for all Conditions. Conditions dont span multiple frames like actions and return true or false immediately on execution. Derive this to create your own
public class ConditionTask : Task
{

    [SerializeField]
    private bool _invert;

    [NonSerialized]
    private int yieldReturn = -1;

    public bool invert
    {
        get { return _invert; }
        set { _invert = value; }
    }

    ///...
    public void Enable(Component agent)
    {
        if (Set(agent))
        {
            OnEnable();
        }
    }

    ///...
    public void Disable()
    {
        isActive = false;
        OnDisable();
    }

    ///Check the condition for the provided agent and with the provided blackboard
    public bool CheckCondition(Component agent)
    {

        if (!isActive)
        {
            return false;
        }

        if (!Set(agent))
        {
            return false;
        }

        if (yieldReturn != -1)
        {
            var b = invert ? !(yieldReturn == 1) : (yieldReturn == 1);
            yieldReturn = -1;
            return b;
        }

        return invert ? !OnCheck() : OnCheck();
    }

    virtual protected void OnEnable() { }
    virtual protected void OnDisable() { }
    ///Override in your own conditions and return whether the condition is true or false. The result will be inverted if Invert is checked.
    virtual protected bool OnCheck() { return true; }


    private int yields;
    IEnumerator Flip()
    {
        yields++;
        yield return null;
        yields--;
        if (yields == 0)
        {
            yieldReturn = -1;
        }
    }
}