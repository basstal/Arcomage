using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NOAH.UI
{
    public class UICellPool : MonoBehaviour
    {
        //public int[] TemplateIndexs;
        // Start is called before the first frame update
        
        public delegate int CallFindTemplate(int index);
    
        protected Dictionary<int,List<UIWrapCell>> m_cachePool = new Dictionary<int, List<UIWrapCell>>();
  
        protected List<UIWrapCell> m_usedCell = new List<UIWrapCell>();

        protected HashSet<int> m_dirtyCell = new HashSet<int>();
        public HashSet<int> DirtyCells
        {
            get{
                return m_dirtyCell;
            }
        }
        public List<UIWrapCell> UsedCells
        {
            get
            {
                return m_usedCell;
            }
        }
        UIWrapLayoutBase.Direction m_direction;
        public UIWrapLayoutBase.Direction Direction{
            get{
                return m_direction;
            }
            set{
                m_direction = value;
            }
        }
        protected Dictionary<int,UIWrapCell> m_mapUsedCell = new Dictionary<int, UIWrapCell>();
        public Dictionary<int,UIWrapCell> MapUsedCell
        {
            get{
                return m_mapUsedCell;
            }
        }
        protected Dictionary<int,UIWrapCell> m_templateCell = new Dictionary<int,UIWrapCell>();
        public Dictionary<int,UIWrapCell> TemplateCells{
            get{
                return m_templateCell;
            }
        }
        public void Clear()
        {
            m_cachePool.Clear();
            m_templateCell.Clear();
        }
    
        public void AddTemplateCellStatic(UIWrapCell cell)
        {
            if (cell != null && !m_templateCell.ContainsKey(cell.TemplateIndex) /*&& cell.gameObject.activeSelf*/)
            {
                m_templateCell.Add(cell.TemplateIndex,cell);

                AddToUsedCell(cell);
            }
        }

        public void AddTemplateCell(UIWrapCell cell)
        {
            if (cell != null && !m_templateCell.ContainsKey(cell.TemplateIndex) /*&& cell.gameObject.activeSelf*/)
            {
                m_templateCell.Add(cell.TemplateIndex,cell);
                RecycleCell(cell);
            }
            else if (cell && m_templateCell.ContainsKey(cell.TemplateIndex))
            {
                RecycleCell(cell);
                // PrintLog("The Template Cell has same key!!");
            }
        }

        public bool HasUsedCell(int index)
        { 
            return m_mapUsedCell.ContainsKey(index);
        }

        public void SetAllDirty()
        {
            foreach (var item in m_mapUsedCell)
            {
                UIWrapCell cell = item.Value;
                InsertDirtyCellIndex(cell.Index);
            }
        }

        public bool AddToUsedCell(UIWrapCell cell)
        {
            if (!m_mapUsedCell.ContainsKey(cell.Index))
            {
                m_mapUsedCell.Add(cell.Index,cell);
                m_usedCell.Add(cell);
                cell.IsRecycle = false;
                cell.gameObject.SetActive(true);
//                var old = cell.rectTransform.localPosition;
//                old.z = 0;
//                cell.rectTransform.localPosition = old;
                // cell.transform.localScale = Vector3.one;
                // if (CanvasUpdateRegistry.IsRebuildingLayout())
                // {
                //     StartCoroutine(DelayActive(cell.gameObject,true));
                // }
                // else
                //{
                //   cell.gameObject.SetActive(true);
                //}
                return true;
            }
            return false;
        }

        public UIWrapCell GetTemplateCellByTemplateIndex(int templateIndex)
        {
            UIWrapCell cell = null;
            TemplateCells.TryGetValue(templateIndex,out cell);
            return cell;
        }

        public UIWrapCell GetTemplateCell(int index,CallFindTemplate call)
        {
            int templateIndex = GetTemplateIndex(index,call);
            UIWrapCell cell = null;
            TemplateCells.TryGetValue(templateIndex,out cell);
            return cell;
        }
        public virtual int GetTemplateIndex(int index,CallFindTemplate call)
        {
            int templateIndex = 0;
            if (m_templateCell.Count > 1 && call != null)
            {
                templateIndex = call(index);
            } 
            // else if (m_templateCell.Count > 1 && TemplateIndexs != null && index < TemplateIndexs.Length)
            // {
            //     templateIndex = TemplateIndexs[index];
            // }
            return templateIndex;
        }

        public void InsertDirtyCellIndex(int index)
        {
            m_dirtyCell.Add(index);
        }
    
        public void ClearDirtyCell()
        {
            m_dirtyCell.Clear();
        }
        public bool HasDirtyCell(int index)
        {
            return m_dirtyCell.Contains(index);
        }

        public void RemoveDirtyCell(int index)
        {
            m_dirtyCell.Remove(index);
        }
        public virtual UIWrapCell PopCellFromCache(int templateIndex)
        {
            UIWrapCell cell;
            bool bHas = false;
            List<UIWrapCell> cache;
            if (m_cachePool.TryGetValue(templateIndex,out cache))
            {
                if (cache.Count > 0)
                {
                    bHas = true;
                }
            }
            else
            {
                cache = new List<UIWrapCell>();
                m_cachePool.Add(templateIndex,cache);
            }
            if (bHas)
            {
                cell = cache[0];
                cache.Remove(cell);
                if(cell == null)
                {
                    cell = CreateCacheCell(templateIndex);
                    if (cell)
                    {
                        cell.Index = -1;
                    }
                }
            }
            else
            {
                cell = CreateCacheCell(templateIndex);
                if (cell)
                {
                    cell.Index = -1;
                }
            }
            return cell;
        }

        public virtual UIWrapCell PopCellFromCache(int index,CallFindTemplate call)
        {
            int templateIndex = GetTemplateIndex(index,call);
            return PopCellFromCache(templateIndex);
        }

        public UIWrapCell GetUsedCell(int index)
        {
            UIWrapCell cell;
            if (m_mapUsedCell.TryGetValue(index,out cell))
            {
                return cell;
            }
            return null;
        }


        public void PreLoadCacheCell(int cacheNum)
        {
            foreach (var item in m_templateCell)
            {
                int templateIndex = item.Value.TemplateIndex;
                List<UIWrapCell> cache = GetListPool(templateIndex);
                int cur = cache.Count;
                for (int i = cur; i < cacheNum; i++)
                {
                    GameObject ob = GameObject.Instantiate(m_templateCell[templateIndex].gameObject,transform);
                    UIWrapCell cell = ob.GetComponent<UIWrapCell>();
                    if (cell)
                    {
                        RecycleCell(cell);
                    }
                }
            }
        }
    
        protected virtual UIWrapCell CreateCacheCell(int templateIndex)
        {
            UIWrapCell srcCell;
            if (m_templateCell.Count > 0 && m_templateCell.TryGetValue(templateIndex,out srcCell))
            {
                GameObject ob = GameObject.Instantiate(srcCell.gameObject,transform);
                ob.name = string.Format("Cell Clone {0}",templateIndex);
                UIWrapCell newCell = ob.GetComponent<UIWrapCell>();
                return newCell;
            }
            return null;
        }
    
        public void SortCell()
        {
            m_usedCell.Sort((a,b)=>
            {
                UIWrapCell c1 = a.GetComponent<UIWrapCell>();
                UIWrapCell c2 = b.GetComponent<UIWrapCell>();
                if(c1.Index < c2.Index)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            });
        }

        public void RecycleCell(UIWrapCell cell)
        {
            if (cell == null || cell.IsRecycle)
            {
                return;
            }
    
            List<UIWrapCell> list = GetListPool(cell.TemplateIndex);
            list.Add(cell);
            cell.IsDirty = false;
            m_mapUsedCell.Remove(cell.Index);
            m_usedCell.Remove(cell);
            // var old = cell.rectTransform.anchoredPosition;  -- 不知道为啥曾哥用的是anchoredPosition，暂时改成localPosition 看有没有问题
            // cell.rectTransform.localPosition = new Vector3(old.x,old.y, -100000);  
            
//            var old = cell.rectTransform.localPosition;
//            old.z =  -100000;
//            cell.rectTransform.localPosition = old;
            // cell.transform.localScale = Vector3.zero;
            cell.gameObject.SetActive(false);
            cell.IsRecycle = true;
            // if(CanvasUpdateRegistry.IsRebuildingLayout())
            // {
            //     StartCoroutine(DelayActive(cell.gameObject,false));
            // }
            // else
            // {
            //     cell.gameObject.SetActive(false);
            // }
        
        }

        // IEnumerator DelayActive(GameObject ob,bool b)
        // {
        //     yield return null;
        //     if(ob)ob.SetActive(b);
        // }
        List<UIWrapCell> GetListPool(int templateIndex)
        {
            List<UIWrapCell> list;
            if (!m_cachePool.TryGetValue(templateIndex,out list))
            {
                list = new List<UIWrapCell>();
                m_cachePool.Add(templateIndex,list);
            }
            return list;
        }

        public void PrintLog(string str)
        {
            UnityEngine.Debug.Log(str);
        }
    }
}
