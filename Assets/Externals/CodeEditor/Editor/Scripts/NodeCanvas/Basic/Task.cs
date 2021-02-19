#define CONVENIENCE_OVER_PERFORMANCE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



///*RECOVERY PROCESSOR IS INSTEAD APPLIED RESPECTIVELY IN ACTIONTASK - CONDITIONTASK*///

// [Serializable] [SpoofAOT]
///The base class for all Actions and Conditions. You dont actually use or derive this class. Instead derive from ActionTask and ConditionTask
abstract public partial class Task
{

    ///Designates that the task requires Unity eventMessages to be forwarded from the agent and to this task
    [AttributeUsage(AttributeTargets.Class)]
    protected class EventReceiverAttribute : Attribute
    {
        public string[] eventMessages;
        public EventReceiverAttribute(params string[] args)
        {
            this.eventMessages = args;
        }
    }

    ///If the field type this attribute is used, derives Component then it will be retrieved from the agent.
    ///The field is also considered Required for correct initialization.
    [AttributeUsage(AttributeTargets.Field)]
    protected class GetFromAgentAttribute : Attribute { }

    [HideInInspector]
    public Google.Protobuf.IMessage message;

    [SerializeField]
    private bool _isDisabled;
    [NonSerialized]
    private ITaskSystem _ownerSystem;

    //the current/last agent used
    [NonSerialized]
    private Component current;

    //info
    [NonSerialized]
    private bool _agentTypeInit;
    [NonSerialized]
    private Type _agentType;
    [NonSerialized]
    private string _taskName;
    [NonSerialized]
    private string _taskDescription;
    [NonSerialized]
    private string _obsoleteInfo;
    //


    //Required
    public Task() { }


    ///Create a new Task of type assigned to the target ITaskSystem
    public static T Create<T>(ITaskSystem newOwnerSystem) where T : Task
    {
        return (T)Create(typeof(T), newOwnerSystem);
    }
    public static Task Create(Type type, ITaskSystem newOwnerSystem)
    {
        var newTask = (Task)Activator.CreateInstance(type);
        newOwnerSystem.RecordUndo("New Task");
        newTask.SetOwnerSystem(newOwnerSystem);
        newTask.OnValidate(newOwnerSystem);
        newTask.OnCreate(newOwnerSystem);
        return newTask;
    }

    //Duplicate the task for the target ITaskSystem
    virtual public Task Duplicate(ITaskSystem newOwnerSystem)
    {
        //Deep clone
        var newTask = this.Clone();
        newOwnerSystem.RecordUndo("Duplicate Task");
        newTask.SetOwnerSystem(newOwnerSystem);
        newTask.OnValidate(newOwnerSystem);
        return newTask;
    }

    ///Called once the first time task is created
    virtual public void OnCreate(ITaskSystem ownerSystem) { }
    ///Called when the task is created, duplicated or otherwise needs validation.
    ///This is not the editor only Unity OnValidate call!
    virtual public void OnValidate(ITaskSystem ownerSystem) { }

    //Following are special so they are declared first
    //...
    ///Sets the system in which this task lives in and initialize BBVariables. Called on Initialization of the system.
    public void SetOwnerSystem(ITaskSystem newOwnerSystem)
    {

        if (newOwnerSystem == null)
        {
            Debug.LogError("ITaskSystem set in task is null!!");
            return;
        }

        ownerSystem = newOwnerSystem;
    }

    ///The system this task belongs to from which defaults are taken from.
    public ITaskSystem ownerSystem
    {
        get { return _ownerSystem; }
        private set { _ownerSystem = value; }
    }

    ///The owner system's assigned agent
    public Component ownerAgent
    {
        get { return ownerSystem != null ? ownerSystem.agent : null; }
    }

    ///The time in seconds that the owner system is running
    protected float ownerElapsedTime
    {
        get { return ownerSystem != null ? ownerSystem.elapsedTime : 0; }
    }
    ///


    ///Is the Task active?
    public bool isActive
    {
        get { return !_isDisabled; }
        set { _isDisabled = !value; }
    }

    ///Is the task obsolete? (marked by [Obsolete]). string.Empty: is not.
    public string obsolete
    {
        get
        {
            if (_obsoleteInfo == null)
            {
                var att = this.GetType().RTGetAttribute<ObsoleteAttribute>(true);
                _obsoleteInfo = att != null ? att.Message : string.Empty;
            }
            return _obsoleteInfo;
        }
    }

    ///The friendly task name. This can be overriden with the [Name] attribute
    public string name
    {
        get
        {
            if (_taskName == null)
            {
                _taskName = message.GetType().Name;
            }
            return _taskName;
        }
    }

    ///The help description of the task if it has any through [Description] attribute
    public string description
    {
        get
        {
            if (_taskDescription == null)
            {
                var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(true);
                _taskDescription = descAtt != null ? descAtt.description : string.Empty;
            }
            return _taskDescription;
        }
    }

    ///The type that the agent will be set to by getting component from itself on task initialize. Also defined by using the generic versions of Action and Condition Tasks.
    ///You can omit this to keep the agent propagated as is or if there is no need for a specific type anyway.
    virtual public Type agentType { get { return null; } }



    ///A short summary of what the task will finaly do.
    public string summaryInfo
    {
        get
        {
            if (this is ActionTask)
            {
                return info;
            }
            if (this is ConditionTask)
            {
                return ((this as ConditionTask).invert ? "If <b>!</b> " : "If ") + info;
            }
            return info;
        }
    }

    ///Override this and return the information of the task summary
    virtual protected string info
    {
        get { return name; }
    }

    ///Helper summary info to display final agent string within task info if needed
    public string agentInfo
    {
        get { return "<b>owner</b>"; }
    }

    ///The current or last executive agent of this task
    protected Component agent
    {
        get
        {
            if (current != null)
            {
                return current;
            }

            return TransformAgent(ownerAgent, agentType);
        }
    }

    ///This contains the first warning encountered relevant to task correct execution.
    ///This is mostly used in editor.
    public string firstWarningMessage { get; private set; }

    ///Override in Tasks. This is called after a NEW agent is set, after initialization and before execution
    ///Return null if everything is ok, or a string with the error if not.
    virtual protected string OnInit() { return null; }

    //Actions and Conditions call this before execution. Returns if the task was sucessfully initialized as well
    // protected bool Set(Component newAgent, IBlackboard newBB){
    protected bool Set(Component newAgent)
    {
        if (current != null && newAgent != null && current.gameObject == newAgent.gameObject)
        {
            return isActive = true;
        }

        return isActive = Initialize(newAgent);
    }

    //"Transform" the agent to the agentType
    static Component TransformAgent(Component currentAgent, Type type)
    {
        if (currentAgent != null && type != null && !type.RTIsAssignableFrom(currentAgent.GetType()))
        {
            if (type.RTIsSubclassOf(typeof(Component)) || type.RTIsInterface())
            {
                currentAgent = currentAgent.GetComponent(type);
            }
        }
        return currentAgent;
    }

    //Initialize whenever agent is set to a new value
    bool Initialize(Component newAgent)
    {
        //"Transform" the agent to the agentType
        newAgent = TransformAgent(newAgent, agentType);

        //Set as current even if null
        current = newAgent;

        //error if it's null but an agentType is required
        if (newAgent == null && agentType != null)
        {
            return Error("Failed to resolve Agent to requested type '" + agentType + "', or new Agent is NULL. Does the Agent has the requested Component?");
        }

        //Use the field attributes
        if (InitializeAttributes(newAgent) == false)
        {
            return false;
        }

        //let user make further adjustments and inform us if there was an error
        var error = OnInit();
        if (error != null)
        {
            return Error(error);
        }

        return true;
    }

    bool InitializeAttributes(Component newAgent)
    {

#if CONVENIENCE_OVER_PERFORMANCE

        //Usage of [RequiredField] and [GetFromAgent] attributes
        var fields = this.GetType().RTGetFields();
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];

#if UNITY_EDITOR

            var value = field.GetValue(this);

            if (field.RTIsDefined<RequiredFieldAttribute>(true))
            {

                if (value == null || value.Equals(null))
                {
                    return Error(string.Format("A required field named '{0}' is not set.", field.Name));
                }

                if (field.FieldType == typeof(string) && string.IsNullOrEmpty((string)value))
                {
                    return Error(string.Format("A required string field named '{0}' is not set.", field.Name));
                }
            }

#endif

            if (newAgent != null && typeof(Component).RTIsAssignableFrom(field.FieldType))
            {
                if (field.RTIsDefined<GetFromAgentAttribute>(true))
                {
                    var o = newAgent.GetComponent(field.FieldType);
                    field.SetValue(this, o);
                    if (ReferenceEquals(o, null))
                    {
                        return Error(string.Format("GetFromAgent Attribute failed to get the required Component of type '{0}' from '{1}'. Does it exist?", field.FieldType.Name, agent.gameObject.name));
                    }
                }
            }

        }

#endif

        return true;
    }

    //Utility function to log and return errors above (for runtime)
    protected bool Error(string error)
    {
        Debug.LogError(string.Format("{0} | {1}", error, ownerSystem != null ? ownerSystem.contextObject : null));
        return false;
    }

    //Gather warnings for user convernience. Basicaly used in the editor, but could be used in runtime as well.
    public string GetWarning()
    {
        firstWarningMessage = Internal_GetWarning();
        return firstWarningMessage;
    }

    string Internal_GetWarning()
    {

        if (obsolete != string.Empty)
        {
            return string.Format("Task is obsolete: '{0}'.", obsolete);
        }

        if (agent == null && agentType != null)
        {
            return string.Format("'{0}' target is currently null.", agentType.Name);
        }

        var fields = this.GetType().RTGetFields();
        for (var i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (field.RTIsDefined<RequiredFieldAttribute>(true))
            {
                var value = field.GetValue(this);
                if (value == null || value.Equals(null))
                {
                    return string.Format("Required field '{0}' is currently null.", field.Name.SplitCamelCase());
                }
                if (field.FieldType == typeof(string) && string.IsNullOrEmpty((string)value))
                {
                    return string.Format("Required string field '{0}' is currently null or empty.", field.Name.SplitCamelCase());
                }
            }
        }
        return null;
    }

    sealed public override string ToString()
    {
        return summaryInfo;
    }

    virtual public void OnDrawGizmos() { }
    virtual public void OnDrawGizmosSelected() { }
    public Task Clone()
    {
        return JsonUtility.FromJson<Task>(JsonUtility.ToJson(this));
    }
}