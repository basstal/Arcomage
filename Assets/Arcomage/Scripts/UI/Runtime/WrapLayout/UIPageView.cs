using UnityEngine;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class UIPageView : UIWrapLayout
    {
    
        public delegate void OnPageEvent(int page);
        public OnPageEvent OnPageCall;

        [SerializeField]
        float m_needPageSpeed = 50.0f;
        float m_startBeginTime = 0.0f;
        float m_moveStart = 0.0f;
        int m_currPage = -1;
        public int Page{
            get{
                return m_currPage;
            }
            set
            {
                ScrollIndex(value);
            }
        }
    
        override public int ChildLen{
            get{
                return base.ChildLen;
            }
            set
            {
                SetChildLen(value,true);
            }
        }

        override public void SetChildLen(int len,bool bForceUpdate = false)
        {
            base.SetChildLen(len,bForceUpdate);
            if (Page == -1 && len > 0)
            {
                Page = 0;
            }
            else if(len <= 0)
            {
                m_currPage = -1;
            }
        }

        override public void ScrollIndex(int index)
        {
            m_currPage = Mathf.Min(index,GetCellPositionCount()); //最后会多填一个cell size来填充content size
            m_currPage = Mathf.Max(0,m_currPage);
            if (OnPageCall != null)
            {
                OnPageCall(m_currPage);
            }
            base.ScrollIndex(m_currPage);
        }

        override public void JumpIndex(int index)
        {
            m_currPage = Mathf.Min(index,GetCellPositionCount());
            m_currPage = Mathf.Max(0,m_currPage);
            if (OnPageCall != null)
            {
                OnPageCall(m_currPage);
            }
            base.JumpIndex(m_currPage);
        }

 

        bool IsSpeedEnough(float moveDelta)
        {
            float tempTime = Time.realtimeSinceStartup - m_startBeginTime;
            if (Mathf.Abs(tempTime) <= 0.0001f )
            {
                return false;
            }
            float temp = moveDelta / tempTime;
            return Mathf.Abs(temp) >= m_needPageSpeed;
        }

        void StartMagneticCenterScroll()
        {
            if (m_cellPosition.Count > 0)
            {
                RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
                Vector2 target = new Vector2(scrollRectTransform.rect.width / 2,scrollRectTransform.rect.height / 2);
                if (m_axis == Direction.HORIZONTAL)
                {
                    target.x -= rectTransform.anchoredPosition.x;
                }
                else
                {
                    target.y += m_fillDirection == FillDirection.TOP_BOTTOM?rectTransform.anchoredPosition.y:-rectTransform.anchoredPosition.y;
                }
                int targetIndex = GetClosestIndex(target);
                Page = targetIndex;
            }
        }

        float GetContentCenterPosition()
        {
            float result = 0.0f;
            RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            if (m_axis == Direction.HORIZONTAL)
            {
                result = scrollRectTransform.rect.width / 2 - rectTransform.anchoredPosition.x;
            }
            else
            {
                if (m_fillDirection == FillDirection.TOP_BOTTOM)
                {
                    result = scrollRectTransform.rect.height / 2 + rectTransform.anchoredPosition.y;
                }
                else
                {
                    result = scrollRectTransform.rect.height / 2 - rectTransform.anchoredPosition.y;
                }
            }
            return result;
        }

        float CalculateCellCenterPosition(int index)
        {
            Vector2 cellSize;
            float result = 0.0f;
            if (m_cellSize.TryGetValue(index,out cellSize) && index < m_cellPosition.Count)
            {
                if (m_axis == Direction.HORIZONTAL)
                {
                    result = m_cellPosition[index] + cellSize.x / 2;
                }
                else
                {
                    result = m_cellPosition[index] + cellSize.y / 2;
                }
            }
            return result;
        }
        int FindClosestIndex(int left,int right,Vector2 target)
        {
            if (left >= right)
            {
                return left;
            }
            if (left >= m_cellPosition.Count)
            {
                return -1;
            }
            if (right >= m_cellPosition.Count)
            {
                return -1;
            }
            float centerPosition = m_axis == Direction.HORIZONTAL?target.x:target.y;

            int middle     = (left + right) / 2;
            float leftPt   = CalculateCellCenterPosition(left);
            float rightPt  = CalculateCellCenterPosition(right);
            float middlePt = CalculateCellCenterPosition(middle);

            float leftDistance   = centerPosition - leftPt;
            float rightDistance  = rightPt - centerPosition;
            float middleDistance = Mathf.Abs(middlePt - centerPosition);
            if (right - left == 1)
            {
                return leftDistance < rightDistance?left:right;
            }
            if (leftDistance <= rightDistance)
            {
                return FindClosestIndex(left,middle,target);
            }
            else
            {
                return FindClosestIndex(middle,right,target);
            }
        }
        int GetClosestIndex(Vector2 target)
        {
            int left  = 0;
            int right = GetCellPositionCount();
            if (left >= right)
            {
                return left;
            }
            return FindClosestIndex(left,right,target);
        }

        override protected void OnEndDrag(PointerEventData eventData)
        {
            scrollCtrl.StopRectScroll();
            base.OnEndDrag(eventData);
            if (m_currPage == -1)
            {
                return;
            }
            float moveDelta = m_axis == Direction.HORIZONTAL?eventData.position.x - m_moveStart:eventData.position.y - m_moveStart;
            //Debug.Log(string.Format("offset:{0}",moveDelta));
            if (IsSpeedEnough(moveDelta))
            {
                float currDistance = CalculateCellCenterPosition(m_currPage);
                float contentPs = GetContentCenterPosition();
                float vector = contentPs - currDistance;
                if (vector * moveDelta < 0)
                {
                    if (moveDelta < 0)
                    {
                        m_currPage++;
                    }
                    else
                    {
                        m_currPage--;
                    }
                    m_currPage = Mathf.Min(m_currPage,m_cellSize.Count - 1);
                    m_currPage = Mathf.Max(0,m_currPage);
                    Page = m_currPage;
                }
                else
                {
                    StartMagneticCenterScroll();
                }
            }
            else
            {
                StartMagneticCenterScroll();
            }
        }
    
        override protected void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log(string.Format("offset:{0} {1}",eventData.delta.x,eventData.position.x));
            scrollCtrl.StopAllScroll();
            m_startBeginTime = Time.realtimeSinceStartup;
            m_moveStart = m_axis == Direction.HORIZONTAL?eventData.position.x:eventData.position.y;
        }
    
   
    }
}
