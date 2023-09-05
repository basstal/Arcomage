using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NOAH.UI
{
    [ExecuteAlways]
    public class UIWrapGridCell : UIWrapCell
    {
        // Start is called before the first frame update
        public class CellRowInfo
        {
            public CellRowInfo()
            {
                positions = new List<float>();
                startOffset = 0.0f;
            }
            public List<float> positions ;
            public float startOffset;
        }
        protected DrivenRectTransformTracker m_CellTracker;
    
    
        UIWrapLayoutBase m_layoutBase;
        // [SerializeField,SetProperty("Space")]
        private float m_space = 0.0f;
        public float Space {
            get { return m_space; }
            set 
            { 
                m_space = value;
#if UNITY_EDITOR
                UpdateView();
#endif
            }
        }



        [HideInInspector]
        public int Row = 0; //编辑器模式使用
        public bool m_isPreloadCache = false;
        [SerializeField,HideInInspector]
        protected UICellPool m_cellPool = null;
        public UICellPool cellPool{
            get{
                if (m_cellPool == null)
                {
                    m_cellPool = GetComponent<UICellPool>();
                    if (m_cellPool == null)
                    {
                        m_cellPool = gameObject.AddComponent<UICellPool>();
                    }
                }
                return m_cellPool;
            }
        }

        public int TemplateCellCnt{
            get{
                return cellPool.TemplateCells.Count;
            }
        }

    
    
        int m_startIndex = 0;
        public int StartIndex
        {
            get{
                return m_startIndex;
            }
            set{
                m_startIndex = value;
            }
        }
        UIWrapGridLayout m_grid = null;
        public UIWrapGridLayout GridLayout{
            get{
                if (m_grid == null)
                {
                    m_grid = GetComponentsInParent<UIWrapGridLayout>(true)[0];
                }
                return m_grid;
            }
        }
   

        // [SerializeField,SetProperty("AlignCenter")]
        bool m_isAlignCenter = false;
        public bool AlignCenter{
            get{
                return m_isAlignCenter;
            }
            set{
                // if(m_isAlignCenter == value)
                // {
                //     return;
                // }
                // m_isAlignCenter = value;
                m_isAlignRight = false;
                UpdateByParent();
            }
        }

        // [SerializeField,SetProperty("AlignRight")]
        bool m_isAlignRight = false;
        public bool AlignRight{
            get{
                return m_isAlignRight;
            }
            set{
                // if(m_isAlignRight == value)
                // {
                //     return;
                // }
                // m_isAlignRight = value;
                m_isAlignCenter = false;
                UpdateByParent();
            }
        }
        public void SetParentLayout(UIWrapLayoutBase ob)
        {
            m_layoutBase = ob;
        }
        protected override void Awake()
        {
            m_grid=GetComponentInParent<UIWrapGridLayout>();
            base.Awake();
            if (IsDisplayMode())
            {
                LoadTemplateCell();
                if (m_isPreloadCache)
                {
                    PreLoadCacheCell();
                }
            }
        }

        IEnumerator DelayedUpdate()
        {
            yield return new WaitForSeconds(0.3f);
            UIWrapGridLayout l = GetComponentInParent<UIWrapGridLayout>();
            if (l)
            {
                l.ResetEditorScrollView();
            }
        }
        // void LateUpdate()
        // {
        //     if(m_bNeedUpdate)
        //     {
        //         m_bNeedUpdate = false;
        //         UIWrapGridLayout l = GetComponentInParent<UIWrapGridLayout>();
        //         if (l)
        //         {
        //             l.ResetEditorScrollView();
        //         }
        //     }
        // }
        void UpdateByParent()
        {
            UIWrapGridLayout l = GetComponentInParent<UIWrapGridLayout>();
            if (l)
            {
                l.ResetEditorScrollView();
            }
            //StartCoroutine(DelayedUpdate());
            //m_bNeedUpdate = true;
        }
        public void ClearAlignFlag()
        {
            m_isAlignCenter = false;
            m_isAlignRight = false;
            SetDirty();
        }

        int CacheNum(int templateIndex)
        {
            UIWrapCell cell = cellPool.GetTemplateCellByTemplateIndex(templateIndex);
            if (cell != null)
            {
                return Mathf.CeilToInt(rectTransform.rect.width / cell.GetSize().y) + 1;
            }
            else
            {
                return 0;
            }
        }

        void PreLoadCacheCell()
        {
            foreach (var item in cellPool.TemplateCells)
            {
                int templateIndex = item.Value.TemplateIndex;
                int cacheNum = CacheNum(templateIndex);
                cellPool.PreLoadCacheCell(cacheNum);
            }
        }

        void DisableCellUselessFunc(UIWrapCell cell)
        {
// #if UNITY_EDITOR
//     m_CellTracker.Add(this,cell.rectTransform,DrivenTransformProperties.Pivot | 
//     DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Rotation | DrivenTransformProperties.Scale);
// #endif
        }

        void InitCell(UIWrapCell cell,int index,CellRowInfo cellPosition)
        {
            if (cellPosition.positions.Count == 0)
            {
                return;
            }
            Vector2 vec = new Vector2(cellPosition.startOffset + cellPosition.positions[index],0);
            RectTransform r = cell.rectTransform;
            r.pivot = new Vector2(0.0f,1.0f);
            r.anchorMax = new Vector2(0,r.anchorMax.y);
            r.anchorMin = new Vector2(0,r.anchorMin.y);
            r.localPosition = Vector3.zero;
            cell.gameObject.SetActive(true);
            // cell.transform.localScale = Vector3.one;
            r.localScale = Vector3.one;
            cell.Index = index;
//        Debug.Log(string.Format("cell index:{0} SetPosition:{0}",index,vec.x));
            cell.SetPosition(vec);
            DisableCellUselessFunc(cell);
        }

        void AddToUsedCell(UIWrapCell cell)
        {
            cellPool.AddToUsedCell(cell);
        }

        public void ClearClone()
        {
            int cnt = transform.childCount;
            List<GameObject> objects= new List<GameObject>();
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
        }
        void UpdateCellAtIndex(int index,int tmpIdx,CellRowInfo cellPosition)
        {
            UIWrapCell cell = cellPool.PopCellFromCache(tmpIdx);
            if (cell == null)
            {
                return;
            }
            int realIndex = m_startIndex + index;
            InitCell(cell,index,cellPosition);
            AddToUsedCell(cell);
            if (GridLayout.CallUpdateCellDataFun != null)
            {
                GridLayout.CallUpdateCellDataFun(cell,realIndex);
            }
        }
    
        private bool IsDisplayMode()
        {
            return UIWrapLayoutBase.gDisplayMode || Application.IsPlaying(this);
        }

        public void UpdateChildLayout(CellRowInfo cellPosition)
        {
//        Debug.Log("UpdateChildLayout");
            if (cellPosition == null)
            {
                return;
            }
            m_CellTracker.Clear();
            int len = 0;
            if (IsDisplayMode())
            {
                RecycleAll();
                if (cellPool.TemplateCells.Count == 0)
                {
                    LoadTemplateCell();
                }
                len = cellPosition.positions.Count - 1; //最后一个是所有cell size之和
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        int temIndex = cellPool.GetTemplateIndex(m_startIndex + i,GridLayout.CallFindTemplateFun);
                        UpdateCellAtIndex(i,temIndex,cellPosition);
                    }
                }
            }
            else
            {
                len = cellPool.TemplateCells.Count;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        UIWrapCell cell = cellPool.TemplateCells[i];
                        InitCell(cell,i,cellPosition);
                    }
                }
            }
        }

        void RecycleAll()
        {
            while(cellPool.UsedCells.Count > 0)
            {
                UIWrapCell cell = cellPool.UsedCells[0];
                RecycleCell(cell);
            }
        }
        void RecycleCell(UIWrapCell cell)
        {
            if (cell == null || cell.IsRecycle)
            {
                return;
            }
            if (cell != null && GridLayout.CallWillRecycleCellFun != null && !cell.IsRecycle)
            {
                GridLayout.CallWillRecycleCellFun(cell);
            }
            cellPool.RecycleCell(cell);
        }


        public virtual void LoadTemplateCell()
        {
            cellPool.Clear();
            int cnt = transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject ob = rectTransform.GetChild(i).gameObject;
                UIWrapCell cell = ob.GetComponent<UIWrapCell>();
                cellPool.AddTemplateCell(cell);
                if (!IsDisplayMode())
                {
                    ob.SetActive(true);
                }
            }
        }
    
        public int UpdateCellPosition(int start,int end,ref CellRowInfo cellPosition,out bool bDirty)
        {
            UIWrapGridLayout l = GridLayout;
            int next = start;
            List<float> tempCellPosition = new List<float>();
            bDirty = false;
            int count = 0;
            if (l.Axis == UIWrapLayout.Direction.VERTICAL)
            {
                float size = rectTransform.rect.width;
                float tempSize = 0;
                float currPosition = 0.0f;
                if (!IsDisplayMode())
                {
                    List<int> tempIndex = new List<int>();
                    foreach (var item in cellPool.TemplateCells)
                    {
                        UIWrapCell cell = item.Value;
                        tempIndex.Add(cell.TemplateIndex);
                    }

                    l.CallFindTemplateFun = new UICellPool.CallFindTemplate((index)=>
                    {
                        if (index >= tempIndex.Count)
                        {
                            return tempIndex[0];
                        }
                        return tempIndex[index];
                    });
                }
                while (next < end)
                {
                    int templateIndex = cellPool.GetTemplateIndex(next,l.CallFindTemplateFun);
                    UIWrapCell cell = cellPool.GetTemplateCellByTemplateIndex(templateIndex);
                    if (cell)
                    {
                        Vector2 vec = cell.GetSize();
                        tempSize += vec.x;
                        if (vec.x > size)
                        {
                            UnityEngine.Debug.LogError("error:The cell is larger than parent");
                            next = -1;
                            break;
                        }
                        if (tempSize <= size)
                        {
                            tempSize += Space;
                            tempCellPosition.Add(currPosition);
                            currPosition = tempSize;
                            if (count < cellPosition.positions.Count && cellPosition.positions[count] != tempCellPosition[count])
                            {
                                bDirty = true;
                            }
                            count += 1;
                            next += 1;
                        }
                        else
                        {
                            tempSize -= vec.x;
                            break;
                        }
                    }
                    else
                    {
                        next = -1;
                        UnityEngine.Debug.LogError(string.Format("error:no template {0}",templateIndex));
                        break;
                    }
                }
                if (!IsDisplayMode())
                {
                    l.CallFindTemplateFun = null;
                }
                tempSize -= Space;
                tempCellPosition.Add(tempSize);
                if (tempCellPosition.Count > 0)
                {
                    float width = rectTransform.rect.width;
//                Debug.Log(string.Format("content width:{0}",width));
                    float space = width - tempCellPosition[tempCellPosition.Count - 1];
                    space = Mathf.Max(0,space);
                    float startOffset = 0.0f;
                    if (m_isAlignCenter)
                    {
                        startOffset = space / 2;
                    }
                    else if (m_isAlignRight)
                    {
                        startOffset = space;
                    }
                    if (cellPosition.startOffset != startOffset)
                    {
                        bDirty = true;
                    }
                    // Debug.Log(string.Format("startOffset:{0}",startOffset));
                    cellPosition.startOffset = startOffset;
                }
                if (cellPosition.positions.Count != tempCellPosition.Count)
                {
                    bDirty = true;
                }
                if (bDirty)
                {
                    cellPosition.positions.Clear();
                    int len = tempCellPosition.Count;
                    for (int i = 0; i < len; i++)
                    {
                        cellPosition.positions.Add(tempCellPosition[i]);
                    }
                }
            }
            else
            {
                next = -1;
            }
            return next;
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif
        protected void UpdateView()
        {
            UIWrapGridLayout l = GetComponentInParent<UIWrapGridLayout>();
            if (l)
            {
                l.UpdateLayout();
            }
        }
    }
}

