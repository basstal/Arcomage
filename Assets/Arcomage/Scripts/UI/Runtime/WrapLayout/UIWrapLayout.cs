using UnityEngine;

namespace NOAH.UI
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UIWrapLayout : UIWrapLayoutBase
    {
    
        // [SerializeField,SetProperty("Axis")]
        protected Direction _axis = Direction.HORIZONTAL;
        override public Direction Axis{
            get{
                return _axis;
            }
            set
            {
                _axis = value;
                base.Axis = value;
            }
        }
        [SerializeField] protected float _jumpScrollTime = 0.1f;
        // [SerializeField,SetProperty("Fill_Direction")]
        protected FillDirection _fillDirection = FillDirection.TOP_BOTTOM;
        override public FillDirection Fill_Direction
        {
            get{
                return _fillDirection;
            }
            set
            {
                _fillDirection = value;
                base.Fill_Direction = value;
            }
        }
        // [SerializeField,SetProperty("ChildLen")]
        protected int _childLen = 0;
        override public int ChildLen
        {
            get{return base.ChildLen;}
            set{
                SetChildLen(value);
            }
        }
        // [SerializeField,SetProperty("DisplayMode")]
        protected bool _displayMode = false;
        [SerializeField]
        bool _isPreloadCache = true;
        public bool _isCenter = false;
        public bool _isStatic = false;

        override public void SetChildLen(int len,bool bForceUpdate = true)
        {
            _childLen = len;
            base.SetChildLen(len,bForceUpdate);
        }


     

        [ContextMenu("Reset ScrollView")]
        override public void ResetEditorScrollView()
        {
            CallFindTemplateFun = null;
            m_displayMode = false;
            base.ResetEditorScrollView();
        }

        override protected  void Awake() {
            if (Application.IsPlaying(this))
            {
                _childLen = 0;
                m_childLen = _childLen;
            }
            m_isCenter = _isCenter;
            m_isPreloadCache = _isPreloadCache;
            m_jumpScrollTime = _jumpScrollTime;
            m_displayMode = _displayMode;
            m_isStatic = _isStatic;
            base.Awake();
        }
    
        [ContextMenu("Execute")]
        public override void UpdateLayout()
        {
            SetChildLen(_childLen);
        }

    }
}
