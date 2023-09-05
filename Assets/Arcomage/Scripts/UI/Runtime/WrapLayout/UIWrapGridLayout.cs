// using NOAH.Utility;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace NOAH.UI
{
    public class UIWrapGridLayout : UIWrapLayoutBase
    {
        // Start is called before the first frame update

        struct Section
        {
            public int start;
            public int end;
        }
        Dictionary<int, int> m_mapStartIndex = new Dictionary<int, int>();
        Dictionary<int, Section> m_mapSection = new Dictionary<int, Section>();
        // [SerializeField, SetProperty("ChildLen")]
        protected int _childRealLen = 0;

        // [SerializeField, SetProperty("DisplayMode")]
        protected bool _displayMode = false;
        override public bool DisplayMode
        {

            set
            {
                m_displayMode = value;
                gDisplayMode = m_displayMode;
                if (m_displayMode)
                {
                    CallFindTemplateFun = (index) => 0;
                    LoadTemplateCell();
                    SetChildLen(_childRealLen);
                }
                else
                {
                    ResetEditorScrollView();
                }
            }
        }
        override public int ChildLen
        {
            get { return _childRealLen; }
            set
            {
                SetChildLen(value, true);
            }
        }

        override public void SetChildLen(int len, bool bForceUpdate = true)
        {
            if (len == _childRealLen && !bForceUpdate)
            {
                return;
            }
            _childRealLen = len;
            UpdateChildByLen(_childRealLen, bForceUpdate);
        }
        [ReadOnly]
        public Direction _axis = UIWrapLayoutBase.Direction.VERTICAL;

        // [SerializeField, SetProperty("Fill_Direction")]
        protected FillDirection _fill = FillDirection.TOP_BOTTOM;
        override public FillDirection Fill_Direction
        {
            get
            {
                return _fill;
            }
            set
            {
                _fill = value;
                base.Fill_Direction = value;
            }
        }
        Dictionary<int, UIWrapGridCell.CellRowInfo> m_mapRowCellPosition = new Dictionary<int, UIWrapGridCell.CellRowInfo>();
        override protected void Start()
        {
            m_axis = _axis;
            m_fillDirection = _fill;
            base.Start();
        }

        UIWrapGridCell.CellRowInfo GetRowCellPosition(int index)
        {
            UIWrapGridCell.CellRowInfo positions;
            if (!m_mapRowCellPosition.TryGetValue(index, out positions))
            {
                positions = new UIWrapGridCell.CellRowInfo();
                m_mapRowCellPosition.Add(index, positions);
            }
            return positions;
        }
        override protected void OnRectTransformDimensionsChange()
        {
            if (IsDisplayMode() || m_layoutInited)
            {
                ReloadData(false);
            }
            base.OnRectTransformDimensionsChange();
        }



        [ContextMenu("Reset ScrollView")]
        override public void ResetEditorScrollView()
        {
#if UNITY_EDITOR
            m_displayMode = false;
            CallFindTemplateFun = null;

            base.ResetEditorScrollView();
            int cnt = transform.childCount;
            List<GameObject> objects = new List<GameObject>();
            for (int i = 0; i < cnt; i++)
            {
                GameObject ob = rectTransform.GetChild(i).gameObject;
                UIWrapGridCell gridCell = ob.GetComponent<UIWrapGridCell>();
                if (gridCell)
                {
                    gridCell.ClearClone();
                }
            }
#endif
        }
        int GetTemplateCnt()
        {
            int childLen = 0;
            UIWrapGridCell cell = GetComponentInChildren<UIWrapGridCell>();
            if (cell)
            {
                childLen = cell.cellPool.TemplateCells.Count;
            }
            return childLen;
        }
        void UpdateChildByLen(int len, bool bForceUpdate = false)
        {
            bool bDirty = false;
            int rowLen = CalculateChildLen(len, out bDirty);
            if (bDirty || rowLen != base.ChildLen || bForceUpdate)
            {
                base.SetChildLen(rowLen, bForceUpdate);
            }
        }

        protected override void ResetCell(UIWrapCell cell)
        {
            RectTransform r = cell.rectTransform;
            if (m_axis == Direction.HORIZONTAL)
            {

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

        override public void ReloadData(bool bReset = false)
        {
            int childLen = _childRealLen;
            if (!IsDisplayMode())
            {
                childLen = GetTemplateCnt();
            }
            m_bInitContent = !bReset;
            UpdateChildByLen(childLen);

        }

        void SetAllDirty()
        {
            UnityEngine.Debug.Log("SetAllDirty");
            cellPool.SetAllDirty();
        }

        override protected void Awake()
        {
            m_displayMode = _displayMode;
            base.Awake();
            if (Application.IsPlaying(this))
            {
                _childRealLen = 0;
            }
        }

        override protected UIWrapCell UpdateCellAtIndex(int index, bool bNew)
        {

            UIWrapCell cell = _UpdateCellAtIndex(index, bNew);
            UIWrapGridCell cellEx = cell as UIWrapGridCell;
            if (cellEx != null)
            {
                cellEx.StartIndex = m_mapStartIndex[index];
                cellEx.UpdateChildLayout(GetRowCellPosition(index));
            }
            else
            {
                CallUpdateCellDataFun?.Invoke(cell, index);
            }

            return cell;
        }

        public override void CalculateLayoutInputVertical()
        {
            AlignGridCell();
        }

        public void AlignGridCell()
        {
            //Debug.Log("AlignGridCell");
            if (!IsDisplayMode())
            {
                bool bDirty = false;
                CalculateChildLen(GetTemplateCnt(), out bDirty);
                foreach (var item in cellPool.TemplateCells)
                {
                    UIWrapGridCell gridcell = item.Value as UIWrapGridCell;
                    if (gridcell)
                    {
                        gridcell.UpdateChildLayout(GetRowCellPosition(gridcell.Row));
                    }
                }
            }
        }
        int CalculateChildLen(int realLen, out bool bNeedDirty)
        {
            // Debug.Log("CalculateChildLen");
            int next = 0;
            int rowLen = 0;
            //cellPool.ClearDirtyCell();
            m_mapStartIndex.Clear();
            m_mapSection.Clear();
            m_mapStartIndex.Add(0, 0);
            bNeedDirty = false;
            while (next < realLen)
            {
                UIWrapCell cell = cellPool.GetTemplateCellByTemplateIndex(0);
                if (cell)
                {
                    UIWrapGridCell gridCell = cell as UIWrapGridCell;
                    if (gridCell)
                    {
                        gridCell.SetParentLayout(this);
                        if (gridCell.TemplateCellCnt == 0)
                        {
                            gridCell.LoadTemplateCell();
                        }
                        gridCell.Row = rowLen;
                        UIWrapGridCell.CellRowInfo cellPosition = GetRowCellPosition(rowLen);
                        bool bDirty = false;
                        int start = next;
                        next = gridCell.UpdateCellPosition(next, realLen, ref cellPosition, out bDirty);
                        if (next == -1)
                        {
                            break;
                        }
                        if (bDirty)
                        {
                            bNeedDirty = true;
                            InsertDirtyCellIndex(rowLen);
                        }
                        Section s = new Section();
                        s.start = start;
                        s.end = next;
                        m_mapSection.Add(rowLen, s);
                        rowLen += 1;
                        m_mapStartIndex.Add(rowLen, next);
                    }
                }
                else
                {
                    break;
                }
            }
            return rowLen;
        }

        override public void ScrollIndex(int cellIndex)
        {
            base.ScrollIndex(GetRowIndex(cellIndex));
        }
        int GetRowIndex(int cellIndex)
        {
            int rowLen = 0;
            foreach (var item in m_mapSection)
            {
                if (item.Value.start <= cellIndex && cellIndex < item.Value.end)
                {
                    rowLen = item.Key;
                    break;
                }
            }
            return rowLen;
        }
        override public void JumpIndex(int cellIndex)
        {
            base.JumpIndex(GetRowIndex(cellIndex));
        }

        override protected void LoadTemplateCell()
        {
            base.LoadTemplateCell();
            if (!IsDisplayMode())//编辑器模式用
            {
                foreach (var item in cellPool.TemplateCells)
                {
                    UIWrapGridCell cell = item.Value as UIWrapGridCell;
                    if (cell)
                    {
                        cell.LoadTemplateCell();
                    }
                }
            }
        }
        [ContextMenu("Execute")]
        public override void UpdateLayout()
        {
            SetChildLen(_childRealLen);
        }

    }
}
