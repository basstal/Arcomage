using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    public class ScrollRectControl : MonoBehaviour,IEndDragHandler,IBeginDragHandler,IDragHandler
    {
        public class ScrollDragEvent : UnityEvent<PointerEventData> {}

        // Start is called before the first frame update
        public ScrollDragEvent OnDragEnd = new ScrollDragEvent(); //拖拽结束
        public ScrollDragEvent OnDragBegin = new ScrollDragEvent();//拖拽开始
        public ScrollDragEvent OnDraging = new ScrollDragEvent(); // 持续拖拽
        public UnityEvent OnMoveEnd = new UnityEvent(); //自动滚动结束

        private UIWrapLayoutBase m_wrapLayout;
        public UIWrapLayoutBase wrapLayout{
            get{
                if (m_wrapLayout == null)
                {
                    m_wrapLayout = GetComponentInChildren<UIWrapLayoutBase>();
                }
                return m_wrapLayout;
            }
        }
        private ScrollRect m_scrollRect;
        public ScrollRect scrollRect
        {
            get{
                if (m_scrollRect == null)
                {
                    m_scrollRect = transform.GetComponent<ScrollRect>();
                }
                return m_scrollRect;
            }
        }

        private RectTransform m_scrollRectTransform = null;
        public RectTransform scrollRectTransform
        {
            get{
                if (m_scrollRectTransform == null)
                {
                    m_scrollRectTransform = scrollRect.GetComponent<RectTransform>();
                }
                return m_scrollRectTransform;
            }
        }
    

        RectTransform m_contentRectTransform = null;
        public RectTransform contentRectTransform
        {
            get
            {
                if (m_contentRectTransform == null)
                {
                    m_contentRectTransform = scrollRect.content;
                }
                return m_contentRectTransform;
            }
        }
        Vector2 m_autoScrollDstPosition;
        Vector2 m_autoScrollStartPosition;

        float m_autoScrollTotalTime;
        float m_startDeltatime;
        private bool m_scrolling = false;
 

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            OnDragBegin.Invoke(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnd.Invoke(eventData);
        }


        public void StartAutoScroll(Vector2 vec,float costTime)
        {
            m_autoScrollTotalTime = costTime;
            m_autoScrollStartPosition = contentRectTransform.anchoredPosition;
            m_autoScrollDstPosition = m_autoScrollStartPosition + vec;
            m_startDeltatime = 0.0f;
            m_scrolling = true;
        }
        public void StartAutoScrollAbsolute(Vector2 vec,float costTime)
        {
            Vector2 distance = vec - contentRectTransform.anchoredPosition;
            StartAutoScroll(distance,costTime);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!gameObject.activeSelf)
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            OnDraging.Invoke(eventData);
        }
        public void StopAutoScroll()
        {
            m_scrolling = false;
        }

        public void StopAllScroll()
        {
            StopAutoScroll();
            StopRectScroll();
        }

        public void StopRectScroll()
        {
            scrollRect.StopMovement();
        }
        protected void LateUpdate() {
            if (m_scrolling)
            {
                ProcessAutoScrolling();
            }
        }
        Vector2 GetOutOfBoundary()
        {
            float width = contentRectTransform.rect.width - scrollRectTransform.rect.width + scrollRectTransform.rect.width;
            width = Mathf.Max(width,scrollRectTransform.rect.width);
            float height = contentRectTransform.rect.height - scrollRectTransform.rect.height + scrollRectTransform.rect.height;
            height = Mathf.Max(height,scrollRectTransform.rect.height);
            return new Vector2(width,height);
        }

        bool IsCanMove(Vector2 postion)
        {
            float offsetX = contentRectTransform.rect.width - scrollRectTransform.rect.width;
            offsetX = Mathf.Max(0,offsetX);
            float offsetY = contentRectTransform.rect.height - scrollRectTransform.rect.height;
            if (postion.x >= 0 && scrollRect.horizontal)
            {
                return false;
            }
            else if (postion.x < 0 && Mathf.Abs(postion.x) <= offsetX && scrollRect.horizontal)
            {
                return true;
            }
            if (scrollRect.vertical && wrapLayout)
            {
                if (wrapLayout.Fill_Direction == UIWrapLayoutBase.FillDirection.TOP_BOTTOM)
                {
                    if (postion.y <= 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (postion.y >= 0)
                    {
                        return false;
                    }
                }
                if (Mathf.Abs(postion.y) <= offsetY && scrollRect.vertical)
                {
                    return true;
                }
            }
        
            return false;
        }
        void ProcessAutoScrolling()
        {
            float deltaTime = Time.unscaledDeltaTime;
            m_startDeltatime += deltaTime;
            float per = Mathf.Min(1,m_startDeltatime / m_autoScrollTotalTime);
            Vector2 newPosition = Vector3.Lerp(m_autoScrollStartPosition,m_autoScrollDstPosition,per);
            bool bReached = false;
            if (per >= 1)
            {
                bReached = true;
            }
        
            if (!IsCanMove(newPosition))
            {
                bReached = true;
            }
            if (bReached)
            {
                OnMoveEnd.Invoke();
                m_scrolling = false;
            }
            contentRectTransform.anchoredPosition = newPosition;
        }
    }
}
