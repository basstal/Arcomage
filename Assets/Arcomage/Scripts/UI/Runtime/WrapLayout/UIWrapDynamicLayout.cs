// using NOAH.Utility;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UIWrapDynamicLayout : UIWrapLayoutBase
    {

        [ReadOnly]
        protected Direction _axis = UIWrapLayoutBase.Direction.VERTICAL;
        override public Direction Axis
        {
            get
            {
                return _axis;
            }
            set
            {
                _axis = value;
                base.Axis = value;
            }
        }

        [ReadOnly]
        protected FillDirection _fillDirection = FillDirection.TOP_BOTTOM;
        override public FillDirection Fill_Direction
        {
            get
            {
                return _fillDirection;
            }
            set
            {
                _fillDirection = value;
                base.Fill_Direction = value;
            }
        }


        override public void SetChildLen(int len, bool bForceUpdate = true)
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
                UpdateCellPosition(len);
                m_childLen = len;
                SetDirty();
//                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
        }



        [ContextMenu("Reset ScrollView")]
        override public void ResetEditorScrollView()
        {
            CallFindTemplateFun = null;
            m_displayMode = false;
            base.ResetEditorScrollView();
        }

        // HashSet<int> m_setUpdateingCell = new HashSet<int>();
        Dictionary<int, float> m_mapCellPosition = new Dictionary<int, float>();

        //Vector2 m_boundsOffset = Vector2.zero;

        //List<int> m_queueFinishCellSize = new List<int>();
        //List<int> m_queueFixCellPosition = new List<int>();

        int m_startIndex = 0;
        int m_endIndex = 0;
        int m_maxIndex = 0;

        public float m_bounceSpeed = 0.1f;
        public float m_decelerationRate = 0.25f;
        Vector2 m_PrevPosition;
        float m_Velocity = 0.0f;
        bool m_bDragging = false;

        override public bool DisplayMode
        {
            get
            {
                return false;
            }
            set
            {
                m_displayMode = false;
            }
        }

        private bool m_HasRebuiltLayout = false;
        override protected void Start()
        {
            //有时序问题，SetChildLen可能在start之前就被lua侧调用到了，事实上不需要下面的重置
//            if (Application.IsPlaying(this))
//            {
//                m_childLen = 0;
//            }
            base.Start();
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        override protected bool UpdateContentSize()
        {
            rectTransform.pivot = new Vector2(0.5f, 1);
            return true;
        }

        override protected void InitEvent()
        {
            base.InitEvent();
            scrollCtrl.OnDraging.AddListener(OnDrag);
        }


        override protected void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;
            m_bDragging = true;

        }

        override protected int IndexFromOffset(Vector2 offset)
        {
            if (m_mapCellPosition.Count == 0)
            {
                return -1;
            }
            float search = offset.y;

            int low = m_startIndex;
            while (m_childLen > low && low >= 0)
            {
                float startPs = 0.0f;
                float nextPs = 0.0f;
                //PrintLog(string.Format("index:{0} count:{1}",index,m_cellPosition.Count));
                if (!m_mapCellPosition.TryGetValue(low, out startPs))
                {
                    startPs = CalculateCellPosition(low);
                    CalculateCellSize(low);
                }
                else if (m_cellSize.Count == 0)
                {
                    CalculateCellSize(low);
                }
                int next = low + 1;
                if (next < m_childLen)
                {
                    if (!m_mapCellPosition.TryGetValue(next, out nextPs))
                    {
                        nextPs = CalculateCellPosition(next);
                        CalculateCellSize(next);
                    }
                }
                else
                {
                    Vector2 cellSize;
                    if (m_cellSize.TryGetValue(low, out cellSize))
                    {
                        nextPs = startPs - cellSize.y;
                    }
                }

                float boundUp = startPs + search;
                float boundBottom = nextPs + search;
                if (boundUp >= 0 && boundBottom <= 0)
                {
                    return low;
                }
                if (boundUp >= 0 && boundBottom >= 0)
                {
                    low++;
                }
                else if (boundUp < 0 && boundBottom < 0)
                {
                    low--;
                }
            }
            if (low == m_childLen)
            {
                low = m_childLen - 1;
            }
            if (low <= 0)
            {
                return 0;
            }
            return low;
        }

        override public void JumpIndex(int index)
        {
            if (index < 0)
            {
                return;
            }
            if (index >= m_childLen - 1)
            {
                JumpBottom();
            }
            else
            {
                scrollCtrl.StopAllScroll();
                m_mapCellPosition.Clear();
                m_cellSize.Clear();
                CleanAll();
                // if (m_cellSize.ContainsKey(0))
                // {
                //     m_cellSize.Remove(0);
                // }
                SetContentPosition(Vector2.zero);
                float targetSize = 0.0f;
                int start = index;
                m_startIndex = index;
                Vector2 cellSize;
                while (targetSize < ScrollSize.y && index < m_childLen)
                {
                    if (!m_cellSize.TryGetValue(index, out cellSize))
                    {
                        cellSize = CalculateCellSize(index);
                    }
                    targetSize += cellSize.y;
                    index++;
                }
                m_endIndex = index - 1;
                m_endIndex = Mathf.Max(0, m_endIndex);
                AddCellPosition(index, 0.0f);
                for (int i = start; i < index; i++)
                {
                    if (!m_mapCellPosition.ContainsKey(i))
                    {
                        CalculateCellPosition(i);
                    }
                    InitCellPosition(i);
                }
                UpdateScroll();
            }
        }

        void InitCellPosition(int index)
        {
            UIWrapCell cell = cellPool.GetUsedCell(index);
            if (cell)
            {
                InitCell(cell, index);
            }
        }
        public void JumpBottom()
        {
            scrollCtrl.StopAllScroll();
            m_mapCellPosition.Clear();
            m_cellSize.Clear();
            CleanAll();
            // if (m_cellSize.ContainsKey(0))
            // {
            //     m_cellSize.Remove(0);
            // }
            SetContentPosition(Vector2.zero);
            int index = m_childLen - 1;
            float targetSize = 0.0f;
            Vector2 cellSize;
            while (targetSize < ScrollSize.y && index >= 0)
            {
                if (!m_cellSize.TryGetValue(index, out cellSize))
                {
                    cellSize = CalculateCellSize(index);
                }
                targetSize += cellSize.y + m_Spacing;
                index -= 1;
            }
            index = Mathf.Max(0, index);
            m_startIndex = index;
            m_endIndex = m_childLen - 1;
            if (targetSize < ScrollSize.y)
            {
                AddCellPosition(index, 0.0f);
                for (int i = index + 1; i < m_childLen; i++)
                {
                    if (!m_mapCellPosition.ContainsKey(i))
                    {
                        CalculateCellPosition(i);
                    }
                    InitCellPosition(i);
                }
                UpdateScroll();
            }
            else
            {
                float offsetSize = targetSize - ScrollSize.y;
                Vector2 lastCellSize;
                int lastIndex = m_childLen - 1;
                if (!m_cellSize.TryGetValue(lastIndex, out lastCellSize))
                {
                    return;
                }
                float btps = ScrollSize.y - lastCellSize.y;//offsetSize - lastCellSize.y;
                AddCellPosition(lastIndex, -btps);
                for (int i = lastIndex; i > index; i--)
                {
                    if (!m_mapCellPosition.ContainsKey(i))
                    {
                        CalculateCellPosition(i);
                    }
                    InitCellPosition(i);
                }
                UpdateScroll();
            }
        }

        override protected void OnEndDrag(PointerEventData eventData)
        {
            // base.OnEndDrag(eventData);
            m_bDragging = false;
        }

        override protected void InitCell(UIWrapCell cell, int index, bool bReset = false)
        {
            Vector2 vec = OffsetFromIndex(index);
            cell.Index = index;
            cell.SetPosition(vec);
            Vector2 cellSize;
            if (m_cellSize.TryGetValue(index, out cellSize))
            {
                cell.SetSize(cellSize);
            }
            //DisableCellUselessFunc(cell);
        }

        override protected Vector2 OffsetFromIndex(int index)
        {
            float y;
            Vector2 offset = Vector2.zero;
            if (m_mapCellPosition.TryGetValue(index, out y))
            {
                offset.y = y;
            }

            return offset;
        }

        float CalculateCellPosition(int idx)
        {
            float resutlPs = 0.0f;
            int preIdx = idx - 1;
            int nextIdx = idx + 1;

            float prePosition;
            float nextPosition;
            Vector2 cellSize;
            bool bOk = false;
            if (m_mapCellPosition.TryGetValue(preIdx, out prePosition) && m_cellSize.TryGetValue(preIdx, out cellSize))
            {
                resutlPs = prePosition - cellSize.y - m_Spacing;
                bOk = true;
            }
            else if (m_mapCellPosition.TryGetValue(nextIdx, out nextPosition) && m_cellSize.TryGetValue(idx, out cellSize))
            {
                resutlPs = nextPosition + cellSize.y + m_Spacing;
                bOk = true;
            }
            if (bOk)
            {
                AddCellPosition(idx, resutlPs);
            }

            return resutlPs;
        }

        Vector2 CalculateCellSize(int idx)
        {
            Vector2 cellSize = Vector2.zero;
            bool bNew = !cellPool.HasUsedCell(idx);
            UIWrapCell cell = UpdateCellAtIndex(idx, bNew);
            RectTransform rt = cell.GetFirstChild.gameObject.GetComponent<RectTransform>();
            if (CallGetCellSizeFun != null)
            {
                cellSize = CallGetCellSizeFun(cell, idx);

            }
            else
            {
                //靠layout自己计算大小
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
//                Canvas.ForceUpdateCanvases();
                cellSize = new Vector2(rt.rect.width, rt.rect.height);
//                UnityEngine.Debug.Log($"cellSize of idx:{idx}---size:{cellSize}");

            }
            if (m_cellSize.ContainsKey(idx))
            {
                m_cellSize.Remove(idx);
            }
            m_cellSize.Add(idx, cellSize);
            cell.SetSize(cellSize);
            return cellSize;
        }

        public void UpdateCellLayout(int index)
        {
            for (int i = index; i < m_childLen; i++)
            {
                m_mapCellPosition.Remove(i);
                m_cellSize.Remove(i);
            }
            UpdateScroll();
        }

        void AddCellPosition(int idx, float ps)
        {
            if (idx > m_maxIndex)
            {
                m_maxIndex = idx;
            }
            m_mapCellPosition.Add(idx, ps);
        }

        private float CalculateOffset()
        {
            //  Debug.LogFormat("start:{0} end:{1}",m_startIndex,m_endIndex);
            float offset = 0.0f;

            float minPs;
            float maxPs;
            int startIndex = 0;
            int endIndex = m_childLen - 1;
            float direction = rectTransform.anchoredPosition.y - m_PrevPosition.y;
            Vector2 rectPs = rectTransform.anchoredPosition;
            Vector2 maxSize;

            if (direction > 0)
            {
                if (!m_mapCellPosition.TryGetValue(endIndex, out maxPs))
                {
                    return offset;
                }

                if (!m_cellSize.TryGetValue(endIndex, out maxSize))
                {
                    return offset;
                }

                float boundMaxSize = (Mathf.Abs(maxPs) + maxSize.y);//;

                if (boundMaxSize < ScrollSize.y)
                {
                    offset = -rectPs.y;
                }
                else
                {
                    float bottomBoundsOffset = boundMaxSize - rectPs.y - ScrollSize.y;
                    if (bottomBoundsOffset < 0)
                    {
                        offset = bottomBoundsOffset;
                    }
                }
            }
            else
            {
                if (!m_mapCellPosition.TryGetValue(startIndex, out minPs))
                {
                    return offset;
                }
                minPs += rectPs.y;
                if (minPs < 0) //pivot 从1开始计算
                {
                    offset = -minPs;
                }
            }



            //float bottomBounds = -boundMaxSize + rectPs.y - ScrollSize.y;
            return offset;
        }

        private void LateUpdate()
        {
            if (!gameObject.activeSelf || m_bDragging || scrollRect.velocity == Vector2.zero)
            {
                return;
            }
            float deltaTime = Time.unscaledDeltaTime;
            float offset = CalculateOffset();
            if (!m_bDragging && (offset != 0.0f || m_Velocity != 0.0f))
            {
                Vector2 position = rectTransform.anchoredPosition;

                if (offset != 0)
                {
                    float speed = m_Velocity;
                    position.y = Mathf.SmoothDamp(position.y, position.y + offset, ref speed, m_bounceSpeed, Mathf.Infinity, deltaTime);
                    if (Mathf.Abs(speed) < 1)
                    {
                        speed = 0;
                    }
                    m_Velocity = speed;
                }
                else if (scrollRect.inertia)
                {
                    m_Velocity *= Mathf.Pow(scrollRect.decelerationRate, deltaTime);
                    if (Mathf.Abs(m_Velocity) < 1)
                        m_Velocity = 0;
                    position.y += m_Velocity * deltaTime;
                }
                else
                {
                    m_Velocity = 0;
                }

                SetContentPosition(position);
            }

            if (m_bDragging && scrollRect.inertia)
            {
                Vector2 newVelocity = (rectTransform.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Mathf.Lerp(m_Velocity, newVelocity.y, deltaTime * 10);
            }

            if (rectTransform.anchoredPosition != m_PrevPosition)
            {
                UpdatePrevData();
            }
        }

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.PostLayout)
            {
                UpdatePrevData();
                m_HasRebuiltLayout = true;
            }
        }

        protected override void OnDisable()
        {
            //   CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
            m_HasRebuiltLayout = false;
            base.OnDisable();
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        protected void UpdatePrevData()
        {
            m_PrevPosition = rectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!gameObject.activeSelf)
                return;

            Vector2 position = rectTransform.anchoredPosition;

            float offset = CalculateOffset();
            position.y += offset;

            //Debug.Log(string.Format("offset:{0} ",offset));
            if (offset != 0)
                position.y = position.y - RubberDelta(offset, ScrollSize.y);
            //Debug.Log(string.Format("target position:{0} ",position.x));
            SetContentPosition(position);

        }
        protected virtual void SetContentPosition(Vector2 position)
        {
            if (!scrollRect.horizontal)
                position.x = rectTransform.anchoredPosition.x;
            if (!scrollRect.vertical)
                position.y = rectTransform.anchoredPosition.y;

            if (position != rectTransform.anchoredPosition)
            {
                rectTransform.anchoredPosition = position;
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        override protected void Awake()
        {
            m_mapCellPosition.Add(0, 0);
            Axis = UIWrapLayoutBase.Direction.VERTICAL;
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            base.Awake();
        }


        // void PostCalculateCellPosition(int start,int len)
        // {
        //     int totalLen = start + len;
        //     for (int i = start; i < totalLen; i++)
        //     {
        //         PushUpdateCellPosition(i);
        //     }
        // }

        override public void UpdateScroll()
        {
            EnsureLayoutHasRebuilt();
            if (m_childLen == 0)
            {
                return;
            }
            if (m_isUsedCellsDirty)
            {
                m_isUsedCellsDirty = false;
                cellPool.SortCell();
            }
            Vector2 offset = GetContentOffset();
            float y = offset.y;


            int startIndex = 0, endIndex = 0;

            startIndex = IndexFromOffset(offset);
            m_endOffset.y = y + ScrollSize.y;

            endIndex = IndexFromOffset(m_endOffset);
            if (endIndex == -1)
            {
                endIndex = m_childLen - 1;
            }
            m_startIndex = startIndex;
            m_endIndex = endIndex;
            var usedCells = cellPool.UsedCells;
            //clear front
            if (usedCells.Count > 0)
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
            if (usedCells.Count > 0)
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

        override protected void UpdateCellPosition(int childLen)
        {
            if (!IsDisplayMode())
            {
                return;
            }
            if (cellPool.TemplateCells.Count > 0)
            {
                Vector2 cellSize = cellPool.TemplateCells[0].GetSize();
                float currPosition = 0.0f;
                Vector2 offset = GetContentOffset();

                for (int i = 0; i < childLen; i++)
                {
                    if (!m_cellSize.ContainsKey(i) || cellPool.HasDirtyCell(i))
                    {
                        //    PostCalculateCellPosition(i,1);
                        continue;
                    }


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
                    currPosition -= m_Spacing;
                    m_cellPosition.Add(currPosition);
                }
                else
                {
                    if (m_fillDirection == FillDirection.TOP_BOTTOM)
                    {
                        currPosition += m_Spacing;
                    }
                    else
                    {
                        currPosition -= m_Spacing;
                    }
                    m_cellPosition.Add(currPosition);
                }
            }
        }

    }
}
