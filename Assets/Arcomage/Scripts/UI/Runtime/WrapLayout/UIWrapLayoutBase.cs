using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// using NOAH.Core;

namespace NOAH.UI
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UIWrapLayoutBase : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        //回调：cell回收前调用
        public delegate void CallWillRecycleCell(UIWrapCell cell);
        public CallWillRecycleCell CallWillRecycleCellFun { get; set; }

        //回调：更新cell时调用
        public delegate void CallUpdateCellData(UIWrapCell cell, int index);
        public CallUpdateCellData CallUpdateCellDataFun { get; set; }

        //回调：在获取cell size时调用，如果该回调为空,则默认取模板的cell size
        public delegate Vector2 CallGetCellSize(UIWrapCell cell, int idx);
        public CallGetCellSize CallGetCellSizeFun { get; set; }

        //回调：拖动结束后调用
        public delegate void CallOnDragEnd();
        public CallOnDragEnd CallOnDragEndFun { get; set; }

        //回调：滚动过程中持续调用
        public delegate void CallOnScrolling(GameObject g);
        public CallOnScrolling CallOnScrollingFun { get; set; }
        public float m_moveIndexDuration;

        //回调：获取cell模板时调用
        virtual public UICellPool.CallFindTemplate CallFindTemplateFun { get; set; }

        private RectTransform m_rectTransform;
        // wraplayout是否在重建中
        private bool m_isRebuilding;

        public bool isRebuilding
        {
            get => m_isRebuilding;
        }

        public RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null)
                    m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        [SerializeField, HideInInspector]
        protected UICellPool m_cellPool = null;
        public UICellPool cellPool
        {
            get
            {
                if (m_cellPool == null)
                {
                    m_cellPool = GetComponent<UICellPool>();
                    if (m_cellPool == null)
                    {
                        m_cellPool = gameObject.AddComponent<UICellPool>();
                        m_cellPool.Direction = m_axis;
                    }
                }
                return m_cellPool;
            }
        }

        public enum Direction
        {
            HORIZONTAL,
            VERTICAL
        }

        public enum FillDirection
        {
            TOP_BOTTOM,
            BOTTOM_TOP
        }

        //两个tracker是用于禁用rectTransform上的某些属性，确保它们不被改动，比如轴
        protected DrivenRectTransformTracker m_selfTracker;

        protected DrivenRectTransformTracker m_cellTracker;

        static public bool gDisplayMode = false;

        protected bool m_displayMode = false;

        virtual public bool DisplayMode
        {
            get
            {
                return IsDisplayMode();
            }
            set
            {
                m_displayMode = value;
                gDisplayMode = m_displayMode;
                if (m_displayMode)
                {
                    CallFindTemplateFun = (index) => 0;
                    SetChildLen(m_childLen);
                }
                else
                {
                    ResetEditorScrollView();
                }
            }
        }



        protected List<float> m_cellPosition = new List<float>();

        protected Dictionary<int, Vector2> m_cellSize = new Dictionary<int, Vector2>();



        [SerializeField, HideInInspector]
        protected Direction m_axis = Direction.HORIZONTAL;
        virtual public Direction Axis
        {
            get
            {
                return m_axis;
            }
            set
            {
                if (value == Direction.HORIZONTAL)
                {
                    scrollRect.horizontal = true;
                    scrollRect.vertical = false;
                }
                else
                {
                    scrollRect.horizontal = false;
                    scrollRect.vertical = true;
                }
                m_axis = value;
                cellPool.Direction = m_axis;
                m_bNeedAlign = true;
                RefreshRectTracker();
                LoadTemplateCell();
                ReloadData(true);
            }
        }

        [SerializeField, HideInInspector]
        protected FillDirection m_fillDirection = FillDirection.TOP_BOTTOM;
        virtual public FillDirection Fill_Direction
        {
            get
            {
                return m_fillDirection;
            }
            set
            {
                m_bNeedAlign = true;
                m_fillDirection = value;
                LoadTemplateCell();
                ReloadData(true);
            }
        }
        // [SerializeField, SetProperty("spacing")]
        protected float m_Spacing = 0;

        public float spacing
        {
            get
            {
                return m_Spacing;
            }
            set
            {
                SetProperty(ref m_Spacing, value);
#if UNITY_EDITOR
                UpdateLayout();
#endif
            }
        }

        // [SerializeField, SetProperty("padding")]
        protected float m_Padding = 0;

        public float padding
        {
            get
            {
                return m_Padding;
            }
            set
            {
                SetProperty(ref m_Padding, value);
#if UNITY_EDITOR
                if (!IsDisplayMode())
                {
                    UpdateLayout();
                }
#endif
            }
        }

        // public bool m_considerNotch = false;

        protected float m_jumpScrollTime = 0.1f;

        protected int m_childLen = 0;

        virtual public int ChildLen
        {
            get { return m_childLen; }
            set
            {
                SetChildLen(value, true);
            }
        }
        virtual public void SetChildLen(int len, bool bForceUpdate = true)
        {
            if (len == m_childLen && !bForceUpdate)
            {
                return;
            }
            // scrollCtrl.StopAllScroll();
            if (len == 0)
            {
                m_childLen = len;
                CleanAll();
            }
            else
            {
                if (bForceUpdate)
                {
                    SetAllCellDirty();
                } 
                m_childLen = len;
                UpdateCellPosition(len);
                SetDirty();
                //                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                ResetContent();
            }
        }

        private ScrollRectControl m_scrollCtrl;
        public ScrollRectControl scrollCtrl
        {
            get
            {
                if (scrollRect && m_scrollCtrl == null)
                {
                    m_scrollCtrl = scrollRect.GetComponent<ScrollRectControl>();
                    if (m_scrollCtrl == null)
                    {
                        m_scrollCtrl = scrollRect.gameObject.AddComponent<ScrollRectControl>();
                    }
                }
                return m_scrollCtrl;
            }
        }
        private ScrollRect m_scrollRect = null;
        public ScrollRect scrollRect
        {
            get
            {
                if (m_scrollRect == null)
                {

                    // m_scrollRect = gameObject.GetFirstComponentInParent<ScrollRect>();
                }
                return m_scrollRect;
            }
        }

        protected RectTransform ScrollTransForm => scrollCtrl.scrollRectTransform;

        private Vector2 m_scrollSize = Vector2.zero;
        protected Vector2 ScrollSize
        {
            get
            {
                if (m_scrollSize == Vector2.zero)
                {
                    UpdateScrollSize();
                }
                return m_scrollSize;
            }
        }

        public virtual float minWidth => rectTransform.rect.width;

        public virtual float preferredWidth => rectTransform.rect.width;

        public virtual float flexibleWidth => rectTransform.rect.width;

        public virtual float minHeight => rectTransform.rect.height;
        public virtual float preferredHeight => rectTransform.rect.height;

        public virtual float flexibleHeight => rectTransform.rect.height;
        public virtual int layoutPriority => 0;

        protected bool m_isUsedCellsDirty = true;
        protected bool m_bNeedAlign = false;

        protected bool m_isCenter;

        protected bool m_isPreloadCache = true;
        protected bool m_isStatic = false;

        protected bool m_bInitContent = false;
        protected bool m_layoutInited = false;

        protected Vector2 m_endOffset = Vector2.zero;

        // private bool m_rectReady = true;
        // private float m_screenWidth;
        // private float m_screenHeight;
        // private float m_initWrapSize;
        // private UIWrapCell m_tempCell;

        virtual protected void InitEvent()
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            scrollCtrl.OnDragEnd.AddListener(OnEndDrag);
            scrollCtrl.OnDragBegin.AddListener(OnBeginDrag);
        }

        override protected void Awake()
        {
            base.Awake();
            // m_tempCell = GetComponentInChildren<UIWrapCell>();
            InitEvent();
            LoadTemplateCell();
            m_rectTransform = GetComponent<RectTransform>();
            UpdateScrollSize();
            if (IsDisplayMode())
            {
                if (m_isPreloadCache)
                {
                    preLoadCacheCell();
                }
                
            }
        }
        override protected void Start()
        {
            base.Start();
            // m_screenWidth = Screen.width;
            // m_screenHeight = Screen.height;
            RefreshRectTracker();
            rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            if (IsDisplayMode())
            {
                ReloadData(true);
            }
            else
            {
                //延迟一帧,保证RectTransform就绪
                StartCoroutine(InitLayout());

            }
        }

        IEnumerator InitLayout()
        {
            yield return new WaitForEndOfFrame();
            if ((scrollRect.horizontal && scrollRect.vertical) || (!scrollRect.horizontal && !scrollRect.vertical))
            {
                UnityEngine.Debug.LogError("The ScrollRect must be Only One Direction!");
                yield break;
            }
            UpdateCellPosition(cellPool.TemplateCells.Count);
            ResetContent();
            AlignCell();
            m_layoutInited = true;
        }


        virtual protected void OnBeginDrag(PointerEventData eventData)
        {

        }
        virtual protected void OnEndDrag(PointerEventData eventData)
        {
            Vector2 offset = GetContentOffset();
            //Debug.Log(string.Format("x:{0},y{1}",eventData.scrollDelta.x,eventData.scrollDelta.y));

            float tempSize = 0.0f;
            bool bOnEnd = false;
            if (m_cellPosition.Count > 0)
            {
                if (m_childLen > 0)
                {
                    tempSize = Mathf.Abs(m_cellPosition[m_childLen]);
                }

                //Vector2 totalSize;
                if (m_axis == Direction.HORIZONTAL)
                {
                    tempSize = Mathf.Max(tempSize, m_scrollSize.x);
                    tempSize -= m_scrollSize.x;
                    float x = Mathf.Abs(offset.x);
                    float temp = x - tempSize;
                    if (offset.x < 0 && temp > 0)
                    {
                        bOnEnd = true;
                    }
                }
                else
                {
                    tempSize = Mathf.Max(tempSize, m_scrollSize.y);
                    tempSize -= m_scrollSize.y;
                    float y = Mathf.Abs(offset.y);
                    float temp = y - tempSize;
                    if (m_fillDirection == FillDirection.TOP_BOTTOM)
                    {
                        if (offset.y > 0 && temp > 0)
                        {
                            bOnEnd = true;
                        }
                    }
                    else
                    {
                        if (offset.y < 0 && temp > 0)
                        {
                            bOnEnd = true;
                        }
                    }
                }
            }
            if (bOnEnd)
            {
                CallOnDragEndFun?.Invoke();
            }
        }

        int CacheNum(int templateIndex)
        {
            UIWrapCell cell = cellPool.GetTemplateCellByTemplateIndex(templateIndex);
            if (cell != null)
            {
                if (scrollRect.horizontal)
                {
                    //考虑两边需要各多出一个用来放不完全显示的部分 所以要加2
                    return Mathf.CeilToInt(m_scrollSize.x / cell.GetSize().x) + 2;
                }
                else if (scrollRect.vertical)
                {
                    return Mathf.CeilToInt(m_scrollSize.y / cell.GetSize().y) + 2;
                }
            }
            return 0;
        }
        public Vector2 GetJumpOffsetFromIndex(int index)
        {
            Vector2 offset = OffsetFromIndex(index);
            if (m_axis == Direction.HORIZONTAL)
            {
                offset.x -= m_Padding;
                float maxMove = GetContentSize() - m_scrollSize.x;
                if (Mathf.Abs(offset.x) > maxMove)
                {
                    offset.x = maxMove;
                }
            }
            else
            {
                offset.y -= m_Padding;
                float maxMove = GetContentSize() - m_scrollSize.y;
                if (Mathf.Abs(offset.y) > maxMove)
                {
                    offset.y = m_fillDirection == FillDirection.BOTTOM_TOP ? maxMove : -maxMove;
                }
            }
            return offset;
        }
        virtual public void JumpIndex(int index)
        {
            scrollCtrl.StopAllScroll();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rectTransform);
            Vector2 offset = GetJumpOffsetFromIndex(index);

            // StartCoroutine(DelayJumpIndex(offset));
            if (m_axis == Direction.HORIZONTAL)
            {

                rectTransform.anchoredPosition = new Vector2(-offset.x, rectTransform.anchoredPosition.y);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -offset.y);
            }
        }

        virtual public void MoveIndex(int index, bool instant = true)
        {
            if (instant)
            {
                JumpIndex(index);
                
            }
            else
            {
                Vector2 offset = GetJumpOffsetFromIndex(index);
                Vector2 endvalue = ( m_axis == Direction.HORIZONTAL) ? new Vector2(-offset.x, rectTransform.anchoredPosition.y) : new Vector2(rectTransform.anchoredPosition.x, -offset.y);
                DOTween.To(()=>rectTransform.anchoredPosition,(value)=> rectTransform.anchoredPosition = value, endvalue, 0.5f);
            }
        }
        IEnumerator DelayJumpIndex(Vector2 offset)
        {
            yield return null;
            if (m_axis == Direction.HORIZONTAL)
            {

                rectTransform.anchoredPosition = new Vector2(-offset.x, rectTransform.anchoredPosition.y);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -offset.y);
            }
        }

        virtual public void ScrollIndex(int index)
        {
            scrollCtrl.StopAllScroll();
            Vector2 offset = GetJumpOffsetFromIndex(index);
            if (m_axis == Direction.HORIZONTAL)
            {
                offset.x = -offset.x;
                scrollCtrl.StartAutoScrollAbsolute(new Vector2(offset.x, 0), m_jumpScrollTime);
            }
            else
            {
                offset.y = -offset.y;
                scrollCtrl.StartAutoScrollAbsolute(new Vector2(0, offset.y), m_jumpScrollTime);
            }
        }
        public void OnScrollValueChanged(Vector2 vec)
        {
            if (IsDisplayMode())
            {
                UpdateScroll();
            }
        }
        void preLoadCacheCell()
        {

            foreach (var item in cellPool.TemplateCells)
            {
                int templateIndex = item.Value.TemplateIndex;
                int cacheNum = CacheNum(templateIndex);
                cellPool.PreLoadCacheCell(cacheNum);
            }
        }

        public virtual int GetWhichTempIndex(int index)
        {
            return cellPool.GetTemplateIndex(index, CallFindTemplateFun);
        }

        virtual protected int IndexFromOffset(Vector2 offset)
        {
            if (m_cellPosition.Count == 0)
            {
                return -1;
            }
            float search = 0.0f;
            if (m_axis == Direction.HORIZONTAL)
            {
                search = offset.x;
            }
            else
            {
                search = offset.y;
            }
            int low = 0;
            int high = m_childLen - 1;
            while (high >= low)
            {
                int temp = Mathf.FloorToInt((high - low) / 2);
                int index = low + temp;
                if (index >= m_cellPosition.Count)
                {
                    index = m_cellPosition.Count - 1;
                }
                index = Mathf.Max(index, 0);
                //PrintLog(string.Format("index:{0} count:{1}",index,m_cellPosition.Count));
                float start = m_cellPosition[index];
                temp = index + 1;
                if (temp >= m_cellPosition.Count)
                {
                    temp = m_cellPosition.Count - 1;
                }
                temp = Mathf.Max(temp, 0);
                //PrintLog(string.Format("temp:{0} count:{1}",temp,m_cellPosition.Count));
                float end = m_cellPosition[temp];
                if (m_fillDirection == FillDirection.TOP_BOTTOM)
                {
                    end = Mathf.Abs(end);
                    start = Mathf.Abs(start);
                }
                if (search >= start && search <= end)
                {
                    return index;
                }
                if (search < start)
                {
                    high = index - 1;
                }
                else
                {
                    low = index + 1;
                }
            }
            if (low <= 0)
            {
                return 0;
            }
            return -1;
        }



        void RefreshRectTracker()
        {
#if UNITY_EDITOR
            m_selfTracker.Clear();
            if (m_axis == Direction.HORIZONTAL)
            {
                m_selfTracker.Add(this, rectTransform, DrivenTransformProperties.Pivot
                         | DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchoredPositionX  | DrivenTransformProperties.Rotation );
            }
            else
            {
                m_selfTracker.Add(this, rectTransform, DrivenTransformProperties.Pivot
                         | DrivenTransformProperties.SizeDeltaY | DrivenTransformProperties.AnchorMaxY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.Rotation );
            }
#endif
        }



        virtual public void ResetEditorScrollView()
        {
#if UNITY_EDITOR
            m_displayMode = false;
            SetChildLen(0);
            int cnt = transform.childCount;
            List<GameObject> objects = new List<GameObject>();
            for (int i = 0; i < cnt; i++)
            {
                GameObject ob = rectTransform.GetChild(i).gameObject;
                int pos = ob.name.IndexOf("Clone");
                if (pos != -1)
                {
                    objects.Add(ob);
                }
            }
            foreach (var item in objects)
            {
                DestroyImmediate(item);
            }
            LoadTemplateCell();
            if ((scrollRect.horizontal && scrollRect.vertical) || (!scrollRect.horizontal && !scrollRect.vertical))
            {
                UnityEngine.Debug.LogError("The ScrollRect must be Only One Direction!");
                return;
            }
            m_bNeedAlign = true;
            ReloadData(true);
#endif
        }
        virtual protected void LoadTemplateCell()
        {
            cellPool.Clear();
            int cnt = transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject ob = rectTransform.GetChild(i).gameObject;
                UIWrapCell cell = ob.GetComponent<UIWrapCell>();
                cell.Init(m_axis, m_fillDirection);
                cell.IsRecycle = false;

                if (m_isStatic)
                {
                    cell.Index = cell.TemplateIndex;
                    cellPool.AddTemplateCellStatic(cell);
                }
                else
                {
                    cellPool.AddTemplateCell(cell);
                }
                if (!IsDisplayMode())
                {
                    ob.SetActive(true);
                }
            }
        }
        void UpdateScrollSize()
        {
            if (scrollRect)
            {
                RectTransform r = ScrollTransForm;
                m_scrollSize = new Vector2(r.rect.width, r.rect.height);
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }
        public bool IsDisplayMode()
        {
            return Application.IsPlaying(this) || m_displayMode;
        }
        protected override void OnDisable()
        {
            m_selfTracker.Clear();
            m_cellTracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        public virtual void UpdateWrapLayout()
        {
            if (!m_isRebuilding)
            {
                m_isRebuilding = true;
                UpdateLayoutHorizontal();
                UpdateLayoutVertical();
                m_isRebuilding = false;
            }
        }

        public virtual void ForceUpdateWrapLayout()
        {
            m_isRebuilding = false;
            UpdateWrapLayout();
        }
        public virtual void UpdateLayoutVertical()
        {
            if (m_axis == Direction.VERTICAL)
            {
                UpdateChildLayout();
            }
        }

        public virtual void UpdateLayoutHorizontal()
        { 
            if (m_axis == Direction.HORIZONTAL)
            {
                UpdateChildLayout();
            }
        }
        // 更新cell的回调可能会有触发layout重建的操作，这里不走layoutRebuilder的重建流程
        public virtual void SetLayoutVertical()
        {
//            if (m_axis == Direction.VERTICAL)
//            {
//                UpdateChildLayout();
//            }
        }
        
        

        public virtual void SetLayoutHorizontal()
        {
//            if (m_axis == Direction.HORIZONTAL)
//            {
//                UpdateChildLayout();
//            }
        }
        virtual public void ReloadData(bool bReset = false)
        {
            if (!IsDisplayMode())
            {
                List<int> tempIndex = new List<int>();
                foreach (var item in cellPool.TemplateCells)
                {
                    UIWrapCell cell = item.Value;
                    tempIndex.Add(cell.TemplateIndex);
                }

                CallFindTemplateFun = (index) => tempIndex[index];
                UpdateCellPosition(cellPool.TemplateCells.Count);
                CallFindTemplateFun = null;
            }
            else
            {
                UpdateCellPosition(m_childLen);
            }
            m_bInitContent = !bReset;
            SetDirty();
        }
        protected bool UpdateContent(bool bReset = false)
        {
            //PrintLog("UpdateContent");
            if (!m_bInitContent || bReset || !IsDisplayMode())
            {
                m_bInitContent = true;
                ResetContent(m_bNeedAlign);
                return true;
            }
            else
            {
                return UpdateContentSize();
            }
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
            //     //处理编辑模式下Cell Strech的问题,只处理了常规的strech
            // #if UNITY_EDITOR
            //     if(!Application.IsPlaying(this) && m_rectReady && m_tempCell != null)
            //     {
            //         bool StrechX = m_tempCell.rectTransform.anchorMax.x - m_tempCell.rectTransform.anchorMin.x == 1;
            //         bool StrechY = m_tempCell.rectTransform.anchorMax.y - m_tempCell.rectTransform.anchorMin.y == 1;

            //         Vector2 curSize = rectTransform.rect.size;
            //         List<int> updateKeyList = new List<int>();
            //         foreach (var kv in m_cellSize)
            //         {
            //             updateKeyList.Add(kv.Key);
            //         }
            //         foreach (var key in updateKeyList)
            //         {
            //             var size = m_cellSize[key];
            //             if(StrechX)
            //             {
            //                 size.x = curSize.x - m_tempCell.rectTransform.sizeDelta.x;
            //             }
            //             if(StrechY)
            //             {
            //                 size.y = curSize.y - m_tempCell.rectTransform.sizeDelta.y;
            //             }
            //             m_cellSize[key] = size;
            //         }
            //     }

            // #endif
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        private bool isRootLayoutGroup
        {
            get
            {
                Transform parent = transform.parent;
                if (parent == null)
                    return true;
                return transform.parent.GetComponent(typeof(ILayoutGroup)) == null;
            }
        }

        protected float GetChildSizes(RectTransform child, int axis)
        {
            return child.sizeDelta[axis];
        }
        protected override void OnRectTransformDimensionsChange()
        {
            if (IsDisplayMode() || m_layoutInited)
                UpdateScrollSize();
            base.OnRectTransformDimensionsChange();
            if (isRootLayoutGroup)
                SetDirty();
        }

        public UIWrapCell GetTemplateCell(int idx)
        {
            return cellPool.GetTemplateCell(idx, CallFindTemplateFun);
        }

        public float CalculateContentSize(int len = -1)
        {
            if (len == -1)
            {
                len = m_childLen;
            }
            float tempContentSize = 0.0f;
            for (int i = 0; i < len; i++)
            {
                Vector2 cellSize = Vector2.zero;
                UIWrapCell cell = GetTemplateCell(i);
                if (cell)
                {
                    if (CallGetCellSizeFun != null)
                    {
                        cellSize = CallGetCellSizeFun(cell, i);
                    }
                    else
                    {
                        cellSize = cell.GetSize();
                    }
                }
                if (m_axis == Direction.HORIZONTAL)
                {
                    tempContentSize += cellSize.x + m_Spacing;
                }
                else
                {
                    tempContentSize += cellSize.y + m_Spacing;
                }
                if (i == 0 || i == len - 1)
                {
                    tempContentSize += m_Padding;
                }
            }
            return tempContentSize;
        }
        protected virtual void UpdateCellPosition(int childLen)
        {
            m_cellPosition.Clear();
            m_cellSize.Clear();
            if (cellPool.TemplateCells.Count > 0)
            {
                Vector2 cellSize = cellPool.TemplateCells[0].GetSize();
                float currPosition = 0.0f;
                currPosition += m_Padding;
                if (m_isCenter && m_axis == Direction.HORIZONTAL)
                {
                    float tempContentSize = CalculateContentSize(childLen);
                    if (tempContentSize < m_scrollSize.x)
                    {
                        currPosition += (m_scrollSize.x - tempContentSize) / 2;
                    }
                }

                for (int i = 0; i < childLen; i++)
                {
                    m_cellPosition.Add(currPosition);
                    UIWrapCell cell = GetTemplateCell(i);
                    if (cell)
                    {
                        if (CallGetCellSizeFun != null)
                        {
                            cellSize = CallGetCellSizeFun(cell, i);
                        }
                        else
                        {
                            // cellSize = m_init ? cell.GetInitSize() : cell.GetSize();
                            cellSize = cell.GetSize();
                        }
                    }
                    // if((i == childLen -1) && m_axis == Direction.HORIZONTAL && m_considerNotch)
                    // {
                    //     DeviceNotch deviceNotch = GameGlobal.GetDeviceNotch();
                    //     if(deviceNotch != null)
                    //     {
                    //         float notchSize = deviceNotch.NotchSize;
                    //         cellSize.x += notchSize;
                    //     }
                    // }
                    m_cellSize.Add(i, cellSize);
                    if (m_axis == Direction.HORIZONTAL)
                    {
                        currPosition += cellSize.x + m_Spacing;
                    }
                    else
                    {
                        if (m_fillDirection == FillDirection.TOP_BOTTOM)
                        {
                            currPosition += -cellSize.y - m_Spacing;
                        }
                        else
                        {
                            currPosition += cellSize.y + m_Spacing;
                        }

                    }
                }
                if (m_axis == Direction.HORIZONTAL)
                {
                    currPosition += -m_Spacing + m_Padding;
                    m_cellPosition.Add(currPosition) ;
                }
                else
                {
                    if (m_fillDirection == FillDirection.TOP_BOTTOM)
                    {
                        currPosition += m_Spacing - m_Padding;
                    }
                    else
                    {
                        currPosition += -m_Spacing + m_Padding;
                    }
                    m_cellPosition.Add(currPosition);
                }
            }
        }

        //bAlignment:对齐父节点
        public void ResetContent(bool bAlignment = false)
        {

            float tempSize = 0.0f;
            if (m_cellPosition.Count > 0)
            {
                tempSize = Mathf.Abs(m_cellPosition[m_cellPosition.Count - 1]);
            }

            Vector2 totalSize;
            if (m_axis == Direction.HORIZONTAL)
            {
                tempSize = Mathf.Max(tempSize, m_scrollSize.x);
                if (bAlignment)
                {
                    rectTransform.pivot = new Vector2(0.0f, 0.0f);
                    totalSize = new Vector2(tempSize, m_scrollSize.y);
                    rectTransform.anchorMax = new Vector2(0, 1.0f);
                    rectTransform.anchorMin = new Vector2(0, 0.0f);
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalSize.y);
                }
                else
                {
                    rectTransform.anchorMax = new Vector2(0, rectTransform.anchorMax.y);
                    rectTransform.anchorMin = new Vector2(0, rectTransform.anchorMin.y);
                    totalSize = new Vector2(tempSize, rectTransform.rect.height);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalSize.y);
                }
                rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, totalSize.x);

            }
            else
            {
                tempSize = Mathf.Max(tempSize, m_scrollSize.y);

                if (m_fillDirection == FillDirection.TOP_BOTTOM)
                {
                    rectTransform.pivot = new Vector2(0.0f, 1.0f);
                    if (bAlignment)
                    {
                        rectTransform.anchorMin = new Vector2(0.0f, 1);
                        rectTransform.anchorMax = new Vector2(1.0f, 1);
                    }
                    else
                    {
                        rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 1);
                        rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 1);
                    }
                }
                else if (m_fillDirection == FillDirection.BOTTOM_TOP)
                {
                    if (bAlignment)
                    {
                        rectTransform.anchorMax = new Vector2(1.0f, 0);
                        rectTransform.anchorMin = new Vector2(0.0f, 0);
                    }
                    else
                    {
                        rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0);
                        rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0);
                    }
                    rectTransform.pivot = new Vector2(0.0f, 0.0f);
                }
                if (bAlignment)
                {
                    totalSize = new Vector2(m_scrollSize.x, tempSize);
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalSize.x);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalSize.y);
                }
                else
                {
                    totalSize = new Vector2(rectTransform.rect.width, tempSize);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalSize.x);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalSize.y);
                }



            }
            
        }

        public void PrintLog(string str)
        {
            UnityEngine.Debug.Log(str);
        }
        virtual protected bool UpdateContentSize()
        {
            if (m_childLen >= m_cellPosition.Count)
            {
                return false;
            }
            float tempSize = 0.0f;
            if (m_childLen > 0)
            {
                tempSize = Mathf.Abs(m_cellPosition[m_childLen]);
            }

            //Vector2 totalSize;
            if (m_axis == Direction.HORIZONTAL)
            {
                tempSize = Mathf.Max(tempSize, m_scrollSize.x);
                //totalSize = new Vector2(tempSize,rectTransform.rect.height);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tempSize);
            }
            else
            {
                tempSize = Mathf.Max(tempSize, m_scrollSize.y);
                //totalSize = new Vector2(rectTransform.rect.width,tempSize);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tempSize);
            }
            return true;
        }

        protected float GetContentSize()
        {
            float tempSize = 0.0f;
            if (m_cellPosition.Count > 0)
            {
                tempSize = Mathf.Abs(m_cellPosition[m_childLen]);
            }

            if (m_axis == Direction.HORIZONTAL)
            {
                tempSize = Mathf.Max(tempSize, m_scrollSize.x);
            }
            else
            {
                tempSize = Mathf.Max(tempSize, m_scrollSize.y);
            }
            return tempSize;
        }

        protected void RecycleCell(UIWrapCell cell)
        {
            if (cell == null || cell.IsRecycle)
            {
                return;
            }
            if (cell != null && CallWillRecycleCellFun != null && !cell.IsRecycle)
            {
                CallWillRecycleCellFun(cell);
            }
            cellPool.RecycleCell(cell);
        }
        protected Vector2 GetContentOffset()
        {
            return rectTransform.anchoredPosition;
        }



        protected void CleanAll()
        {
            while (cellPool.UsedCells.Count > 0)
            {
                UIWrapCell cell = cellPool.UsedCells[0];
                RecycleCell(cell);
            }
        }
        virtual public void UpdateScroll()
        {
            if (m_childLen == 0)
            {
                return;
            }
            if (m_isUsedCellsDirty)
            {
                m_isUsedCellsDirty = false;
                cellPool.SortCell();
            }
            //get currentPos
            Vector2 offset = GetContentOffset();
            float y = offset.y;
            if (m_fillDirection == FillDirection.BOTTOM_TOP && m_axis == Direction.VERTICAL)
            {
                y = -y;
            }
            //offset = new Vector2(-offset.x,y);
            offset.x = -offset.x;
            int startIndex = 0, endIndex = 0;

            startIndex = IndexFromOffset(offset);
            if (m_axis == Direction.HORIZONTAL)
            {
                m_endOffset.x = offset.x + m_scrollSize.x;
                m_endOffset.y = y;
            }
            else
            {
                m_endOffset.x = offset.x;
                m_endOffset.y = y + m_scrollSize.y;
            }
            endIndex = IndexFromOffset(m_endOffset);
            if (endIndex == -1)
            {
                endIndex = m_childLen - 1;
            }
            var usedCells = cellPool.UsedCells;
            //clear front
            if (!m_isStatic && usedCells.Count > 0)
            {
                UIWrapCell cell = usedCells[0];
                int index = cell.Index;

                while (index < startIndex)
                {
                    RecycleCell(cell);
                    if (usedCells.Count > 0)
                    {
                        cell = usedCells[0];
                        index = cell.Index;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //clear back
            if (!m_isStatic && usedCells.Count > 0)
            {
                UIWrapCell cell = usedCells[usedCells.Count - 1];
                int index = cell.Index;
                while (index > endIndex)
                {
                    RecycleCell(cell);
                    if (usedCells.Count > 0)
                    {
                        cell = usedCells[usedCells.Count - 1];
                        index = cell.Index;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // PrintLog(string.Format("Show Start:{0} - End:{1}",startIndex,endIndex));
            if (startIndex >= 0 && endIndex >= 0)
            {
                for (int i = startIndex; i <= endIndex; i++)
                {
                    bool bNew = !cellPool.HasUsedCell(i);
                    if (bNew || cellPool.HasDirtyCell(i))
                    {
                        // PrintLog(string.Format("UpdateCellAtIndex:{0}",i));
                        UpdateCellAtIndex(i, bNew);
                    }
                }
            }

            CallOnScrollingFun?.Invoke(gameObject);
        }
        virtual protected Vector2 OffsetFromIndex(int index)
        {
            if (m_cellPosition.Count <= 0)
                return Vector2.zero;
            if (index >= m_cellPosition.Count)
            {
                index = m_cellPosition.Count - 1;
            }
            Vector2 offset = Vector2.zero;
            if (m_axis == Direction.HORIZONTAL)
            {
                offset.x = m_cellPosition[index];
            }
            else
            {
                offset.y = m_cellPosition[index];
            }
            return offset;
        }


        void RefreshCellTracker(UIWrapCell cell)
        {
#if UNITY_EDITOR
            if (m_axis == Direction.HORIZONTAL)
            {
                m_cellTracker.Add(this, cell.rectTransform, DrivenTransformProperties.Pivot |
                DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.Rotation );
            }
            else
            {
                m_cellTracker.Add(this, cell.rectTransform, DrivenTransformProperties.Pivot |
                DrivenTransformProperties.AnchorMaxY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.Rotation );
            }
#endif
        }

        protected virtual void ResetCell(UIWrapCell cell)
        {
            RectTransform r = cell.rectTransform;
            if (m_axis == Direction.HORIZONTAL)
            {
                r.pivot = new Vector2(0.0f, 0.0f);
                r.anchorMax = new Vector2(0, r.anchorMax.y);
                r.anchorMin = new Vector2(0, r.anchorMin.y);
            }
            else
            {
                if (m_fillDirection == FillDirection.BOTTOM_TOP)
                {
                    r.pivot = new Vector2(0.0f, 0.0f);
                    r.anchorMax = new Vector2(r.anchorMax.x, 0.0f);
                    r.anchorMin = new Vector2(r.anchorMin.x, 0.0f);
                }
                else
                {
                    r.pivot = new Vector2(0.0f, 1.0f);
                    r.anchorMax = new Vector2(r.anchorMax.x, 1.0f);
                    r.anchorMin = new Vector2(r.anchorMin.x, 1.0f);
                }
            }
        }
        protected virtual void AlignCell()
        {
            int count = 0;
            m_cellTracker.Clear();
            foreach (var item in cellPool.TemplateCells)
            {
                UIWrapCell cell = item.Value;
                if (cell)
                {
                    bool bResetCell = false;
                    if (m_bNeedAlign)
                    {
                        ResetCell(cell);
                        bResetCell = true;
                    }
                    cell.Init(m_axis, m_fillDirection);
                    InitCell(cell, count, bResetCell);
                    count += 1;
                }
            }
            CallFindTemplateFun = null;
        }
        virtual protected void InitCell(UIWrapCell cell, int index, bool bReset = false)
        {
            Vector2 vec = OffsetFromIndex(index);
            cell.Index = index;
            if (!bReset)
            {
                if (m_axis == Direction.HORIZONTAL)
                {
                    vec.y = cell.rectTransform.anchoredPosition.y;
                }
                else
                {
                    vec.x = cell.rectTransform.anchoredPosition.x;
                }
            }
            cell.SetPosition(vec);
            Vector2 cellSize;
            if (m_cellSize.TryGetValue(index, out cellSize))
            {
                cell.SetSize(cellSize);
            }
            RefreshCellTracker(cell);
        }

        protected void AddToUsedCell(UIWrapCell cell)
        {
            if (cellPool.AddToUsedCell(cell))
            {
                m_isUsedCellsDirty = true;
            }
        }

        public void InsertDirtyCellIndex(int idx)
        {
            if (cellPool.HasUsedCell(idx))
            {
                cellPool.InsertDirtyCellIndex(idx);
            }
        }

        public void SetAllCellDirty()
        {
            var cells = cellPool.MapUsedCell;
            foreach (var item in cells)
            {
                UIWrapCell cell = item.Value;
                InsertDirtyCellIndex(cell.Index);
            }
        }

        // protected void LateUpdate() {
        //     if (cellPool.DirtyCells.Count > 0)
        //     {
        //         while (cellPool.DirtyCells.GetEnumerator().MoveNext())
        //         {
        //             var index = cellPool.DirtyCells.GetEnumerator().Current;
        //             UpdateCellAtIndex(index,false);
        //         }
        //     }

        // }
        protected int GetCellPositionCount()
        {
            return m_cellPosition.Count - 2;
        }
        protected UIWrapCell _UpdateCellAtIndex(int index, bool bNew)
        {
            UIWrapCell cell;
            if (bNew)
            {
                //这里会产生TransformChange 但不应该再次重建
                m_isRebuilding = true;
                cell = cellPool.PopCellFromCache(index, CallFindTemplateFun);
                if (cell == null)
                {
                    return null;
                }
                if (cell.Index == -1)
                {
                    cell.Init(m_axis, m_fillDirection);
                }
                m_isRebuilding = false;
            }
            else
            {
                cell = cellPool.GetUsedCell(index);
            }
            cellPool.RemoveDirtyCell(index);
            InitCell(cell, index);
            if (bNew)
            {
                AddToUsedCell(cell);
            }
            return cell;
        }

        virtual protected UIWrapCell UpdateCellAtIndex(int index, bool bNew)
        {
            UIWrapCell cell = _UpdateCellAtIndex(index, bNew);
            CallUpdateCellDataFun?.Invoke(cell, index);
            return cell;
        }

        protected virtual void UpdateChildLayout()
        {
            if (!IsDisplayMode())
            {
                UpdateContent(false);
                AlignCell();
                m_bNeedAlign = false;
            }
            else
            {
                if (UpdateContent(false))
                {
                    UpdateScroll();
                }
            }
        }

        protected virtual void OnTransformChildrenChanged()
        {
            UpdateScrollSize();
            SetDirty();
//            UpdateWrapLayout();
        }

        protected void SetProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
            SetDirty();
        }


        protected void SetDirty()
        {
            if (!IsActive())
                return;

            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            else
                StartCoroutine(DelayedSetDirty(rectTransform));
            // LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            UpdateWrapLayout();
        }

        IEnumerator DelayedCall(UIWrapCell cell, int idx)
        {
            yield return null;
            // cell.Display();
            CallUpdateCellDataFun?.Invoke(cell, idx);
        }

        IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            yield return null;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        IEnumerator DelayedReset(float sec)
        {
            yield return new WaitForSeconds(sec);
            UnityEngine.Debug.Log("DelayedReset");
            ResetEditorScrollView();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (scrollCtrl)
            {
                scrollCtrl.StopAllScroll();
            }
            SetDirty();
        }

#endif
        public virtual void UpdateLayout()
        {

        }
    }
}
