// using GamePlay;

using Arcomage.GameScripts.Utils;
using UnityEngine;
// using NOAH.Core;
using Sirenix.OdinInspector;
using UnityEngine.Animations;
using NOAH;

[ExecuteAlways]
public class Follow : MonoBehaviour
{
    [HideInInspector]
    public ParentConstraint Constraint;

    [SerializeField] Transform m_target;
    public Transform Target
    {
        get => m_target;
        set
        {
            m_target = value;
            if (!m_target) return;
            Prepare();
        }
    }
    RectTransform m_targetRT;
    Transform m_self;
    RectTransform m_selfRT;

    public enum FollowType
    {
        None, // 不跟随
        Keep, // 每一帧
        OneTime, // 一次性    
    }

    public enum AutoBindTarget
    {
        None,
        Player,
        FightCamera,
        MapElement,
    }
    
    public enum SnapToGroundType
    {
        None,
        Ground,
        GroundAndPlatform,
    }
    
    // [FoldoutGroup("自动绑定目标"),LabelText("目标")] 
    // public GameUtility.SpecialTargetType BindTarget;
    
    [FoldoutGroup("自动绑定目标"),ShowIf("@this.BindTarget==GameUtility.SpecialTargetType.MapElement"),LabelText("名字")]
    public string BindTargetName = "";
    [FoldoutGroup("自动绑定目标"),LabelText("贴地")] 
    public SnapToGroundType SnapGroundType;
    // [FoldoutGroup("自动绑定目标"),LabelText("过滤贴地对象")] 
    // public CollisionFilter CollisionFilter;

    [EnumToggleButtons, PropertySpace] 
    public Axis Position = Axis.X | Axis.Y | Axis.Z;
    [LabelText("位置偏移"),OnValueChanged("Refresh")]
    public Vector3 PosOffset;
    
    [EnumToggleButtons, PropertySpace] 
    public Axis Rotation = Axis.X | Axis.Y | Axis.Z;
    [LabelText("位置偏移"),OnValueChanged("Refresh")]
    public Vector3 RotOffset;
    
    [EnumToggleButtons,PropertySpace]
    public FollowType Scale;
    [EnumToggleButtons]
    public FollowType LocalScale;
    [EnumToggleButtons]
    public FollowType Size;
    public Vector2 SizeAdd = Vector2.zero;
    public bool SizeRequireKeepSelfPivot = false;

    Vector3 m_lastScale;
    Vector3 m_lastLocalScale;
    Vector2 m_lastSize;

    void Awake()
    {
        m_self = transform;
        m_selfRT =  m_self as RectTransform;
        m_lastScale = m_self.lossyScale;
        m_lastLocalScale = m_self.localScale;
        if (m_selfRT)
        {
            m_lastSize = m_selfRT.rect.size;
        }
        if(!Constraint)
            Constraint = gameObject.GetOrAddComponent<ParentConstraint>();
        Constraint.locked = true;
        var modifyPosition = Position;
        if (SnapGroundType != SnapToGroundType.None)
        { // 因为要根据碰撞调整Y的位置，因此只能手动控制Y，而不能使用ParentConstraint
            modifyPosition = Position &~Axis.Y;
        }
        Constraint.translationAxis = modifyPosition;
        Constraint.rotationAxis = Rotation;
        Constraint.constraintActive = true;

        // if (BindTarget != GameUtility.SpecialTargetType.None)
        // {
        //     m_target = null;
        // }
        
        AutoBind(true);
    }

    void Update()
    {
        AutoBind();
    }
    
    void LateUpdate()
    {
        if ((!m_target) || (!m_self)) return;
        UpdateOutParentConstraint(FollowType.Keep);
    }

    public void AutoBind(bool force = false)
    {
        if (Target && (!force)) return;
        var newTarget = m_target;
        // if (BindTarget != GameUtility.SpecialTargetType.None)
        // {
        //     var go =  GameUtility.GetSpecialTarget(BindTarget, BindTargetName);
        //     newTarget = go ? go.transform : null;
        // }
        Target = newTarget;
    }
    
    void Prepare()
    {
        m_targetRT = m_target as RectTransform;
        if (!Constraint) return;
        var s = new ConstraintSource() {sourceTransform = m_target, weight = 1};
        if (Constraint.sourceCount == 0)
        {
            Constraint.AddSource(s);
            Constraint.SetTranslationOffset(0,PosOffset);
            Constraint.SetRotationOffset(0,RotOffset);
        }
        else
        {
            Constraint.SetSource(0,s);
        }
        UpdateOutParentConstraint(FollowType.OneTime);
    }
    
    void UpdateOutParentConstraint(FollowType followType)
    {
        if (LocalScale == followType)
        {
            UpdateLocalScale();
        }
        if (Scale == followType)
        {
            UpdateScale();
        }

        if (Size == followType)
        {
            if(!SizeRequireKeepSelfPivot)
                UpdatePivot();
            UpdateSize();
        }

        if (Position.HasFlag(Axis.Y) && (SnapGroundType != SnapToGroundType.None))
        {
            UpdateOutY();
        }
    }
    
    void UpdateScale()
    {
        var scale = m_target.lossyScale;
        if (scale == m_lastScale) return;
        var scaleParent = m_self.parent.lossyScale;
        scale.x /= scaleParent.x; //每个轴除法（假设没有什么父节点旋转之类的）
        scale.y /= scaleParent.y; 
        scale.z = 1;
        m_self.localScale = scale;
        m_lastScale = scale;
    }

    void UpdateLocalScale()
    {
        var scale = m_target.localScale;
        if (scale == m_lastLocalScale) return;
        m_self.localScale = scale;
        m_lastLocalScale = scale;
    }

    void UpdateSize()
    {
        if ((!m_targetRT) || (!m_selfRT)) return;
        var rectSize = m_targetRT.rect.size;
        if (rectSize == m_lastSize) return;
        // m_selfRT.SetSizeDelta(rectSize.x + SizeAdd.x, rectSize.y + SizeAdd.y);
        m_lastSize = rectSize;
    }

    //rect transform跟随，需要同步pivot
    void UpdatePivot()
    {
        if ((!m_targetRT) || (!m_selfRT)) return;
        m_selfRT.pivot = m_targetRT.pivot;
    }

    void UpdateOutY()
    {
        if (!m_target) return;
        var pos = m_target.position + PosOffset;
        // var rc2D = BattleUtils.RayCastGround(pos, Vector2.down, 100,
        //     ActorDefine.CollisionFilterToLayerMask(CollisionFilter) );
        // if (rc2D)
        // {
        //     pos.y = rc2D.point.y + PosOffset.y + ActorDefine.ActorGroudMargin;
        // }
        m_self.position = pos;
    }
    
    void Destroy()
    {
        m_target = null;
        m_targetRT = null;
    }

#if UNITY_EDITOR
    [Button]
    void Refresh()
    {
        AutoBind(true);
        var modifyPosition = Position;
        if (SnapGroundType != SnapToGroundType.None)
        { // 因为要根据碰撞调整Y的位置，因此只能手动控制Y，而不能使用ParentConstraint
            modifyPosition = Position &~Axis.Y;
        }
        Constraint.translationAxis = modifyPosition;
        Constraint.rotationAxis = Rotation;
        Constraint.constraintActive = true;
        
        var s = new ConstraintSource() {sourceTransform = m_target, weight = 1};
        if (Constraint.sourceCount == 0)
        {
            Constraint.AddSource(s);
        }
        else
        {
            Constraint.SetSource(0,s);
        }
        Constraint.SetTranslationOffset(0,PosOffset);
        Constraint.SetRotationOffset(0,RotOffset);
        UpdateOutParentConstraint(FollowType.OneTime);
    }
#endif
    // public static bool Modify(GameObject go)
    // {
    //     var follow = go.GetOrAddComponent<Follow>();
    //     // if (go.TryGetComponent<SpawnRegion>(out var s))
    //     // {
    //     //     UnityEngine.Debug.LogWarning("Find it " + go.transform.root.name);
    //     // }
    //
    //     if (!follow) return false;
    //     // follow.Constraint = go.GetOrAddComponent<ParentConstraint>();
    //     // if (!follow.Constraint) return false;
    //     // follow.Constraint.translationAtRest = Vector3.zero;
    //     // follow.Constraint.rotationAtRest = Vector3.one;
    //     follow.BindTarget = (GameUtility.SpecialTargetType) follow.BindTargetType;
    //     // 
    //     return true;
    // }
    //
    // public static bool Convert(GameObject go)
    // {
    //      var old = go.GetComponent<TransformSync>();
    //      if (old == null)
    //      {
    //          return false;
    //      }
    //
    //      var cp = go.GetOrAddComponent<ParentConstraint>();
    //      var follow = go.GetOrAddComponent<Follow>();
    //      if (old.Target)
    //      {
    //          follow.Target = old.Target.transform;
    //      }
    //      
    //      follow.Position = Axis.None;
    //      if (old.SyncType.HasFlag(PositionSyncType.PositionX))
    //      {
    //          follow.Position |= Axis.X;
    //      }
    //      
    //      if (old.SyncType.HasFlag(PositionSyncType.PositionY))
    //      {
    //          follow.Position |= Axis.Y;
    //      }
    //      
    //      if (old.SyncType.HasFlag(PositionSyncType.PositionZ))
    //      {
    //          follow.Position |= Axis.Z;
    //      }
    //
    //      follow.PosOffset = old.Offset;
    //
    //      follow.Rotation = Axis.None;
    //      if (old.SyncType.HasFlag(PositionSyncType.Rotation))
    //      {
    //          follow.Rotation = Axis.X | Axis.Y | Axis.Z;
    //      }
    //      
    //      follow.LocalScale = FollowType.None;
    //      if (old.SyncType.HasFlag(PositionSyncType.LocalScale))
    //      {
    //          follow.LocalScale = FollowType.Keep;
    //      }
    //      follow.Scale = FollowType.None;
    //      follow.Size = FollowType.None;
    //
    //      var oldForStage = old as TransformSyncForStage;
    //      if (oldForStage)
    //      {
    //          follow.BindTargetType = (AutoBindTarget)oldForStage.AutoBindTarget;
    //          follow.BindTargetName = oldForStage.AutoBindTargetName;
    //          follow.SnapGroundType = (SnapToGroundType)oldForStage.SnapToGround;
    //          follow.CollisionFilter = oldForStage.CollisionFilter;
    //      }
    //      Util.Destroy(old);
    //      return true;
    // }
}
