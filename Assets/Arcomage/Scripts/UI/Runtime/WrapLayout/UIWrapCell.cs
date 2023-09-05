using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// using NOAH.Utility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NOAH.UI
{
    public class UIWrapCell : UIBehaviour
    {
    
        // cell 所展示的 item 在 datalist 中的 index，从 0 开始
        [ReadOnly]public int Index = 0;
        public int TemplateIndex = 0;
        // [HideInInspector]public float m_width = 0;
        // [HideInInspector]public float m_height = 0;

        private bool m_isRecycle = false;
        public bool IsRecycle{
            get{
                return m_isRecycle;
            }
            set
            {
                m_isRecycle = value;
            }
        }

        private RectTransform m_rect;
        private RectTransform m_scrollRectTransform;
        public bool m_widthMatchScrollRect = false;
        public RectTransform rectTransform
        {
            get
            {
                if (m_rect == null)
                {
                    m_rect = GetComponent<RectTransform>();
                }
                return m_rect;
            }
        }

        public RectTransform scrollRectTransform{
            get 
            { 
                if(m_scrollRectTransform == null)
                {
                    // m_scrollRectTransform = transform.parent.GetComponent<RectTransform>();
                    ScrollRectControl scrollRect = GetComponentInParent<ScrollRectControl>();
                    m_scrollRectTransform = scrollRect.scrollRectTransform;
                }

                return m_scrollRectTransform;
            }
        }
    
        bool m_isDirty = false;
        public bool IsDirty{
            get{
                return m_isDirty;
            }
            set{
                m_isDirty = value;
            }
        }

        public GameObject GetFirstChild
        {
            get
            {
                Transform t = gameObject.transform.GetChild(0);
                return t ? t.gameObject : null;
            }
        }
        
        public void SetPosition(Vector2 pt)
        {
            rectTransform.anchoredPosition = pt;
        }

        public Vector2 GetSize()
        {
            if(m_widthMatchScrollRect)
            {
                return new Vector2(scrollRectTransform.rect.width, rectTransform.rect.height);
            }
            return new Vector2(rectTransform.rect.width,rectTransform.rect.height);
        }

        // public Vector2 GetInitSize()
        // {
        // #if UNITY_EDITOR
        //     if(m_width > 0 && m_height > 0)
        //     {
        //         float screenWidth = Screen.width;
        //         float screenHeight = Screen.height;
        //         //水平strech
        //         if(rectTransform.anchorMax.x - rectTransform.anchorMin.x == 1)
        //         {
        //             return new Vector2(m_width * screenWidth / 1920 ,m_height);
        //         }
        //         if(rectTransform.anchorMax.y - rectTransform.anchorMin.y == 1)
        //         {
        //             return new Vector2(m_width * screenWidth , m_height * screenHeight / 1080);
        //         }
        //         return new Vector2(m_width, m_height);
        //     }else
        // #endif
        //     {
        //         return new Vector2(rectTransform.rect.width,rectTransform.rect.height);
        //     }
        // }

   
        public void Init(UIWrapLayoutBase.Direction axis,UIWrapLayoutBase.FillDirection fill = UIWrapLayoutBase.FillDirection.TOP_BOTTOM)
        {
            RectTransform r = rectTransform;
            if (axis == UIWrapLayoutBase.Direction.HORIZONTAL)
            {
                r.pivot = new Vector2(0.0f,0.0f);
                r.anchorMax = new Vector2(0,r.anchorMax.y);
                r.anchorMin = new Vector2(0,r.anchorMin.y);
            }
            else
            {
                if (fill == UIWrapLayoutBase.FillDirection.BOTTOM_TOP)
                {
                    r.pivot = new Vector2(0.0f,0.0f);
                    r.anchorMax = new Vector2(r.anchorMax.x,0.0f);
                    r.anchorMin = new Vector2(r.anchorMin.x,0.0f);
                }
                else
                {
                    r.pivot = new Vector2(0.0f,1.0f);
                    r.anchorMax = new Vector2(r.anchorMax.x,1.0f);
                    r.anchorMin = new Vector2(r.anchorMin.x,1.0f);
                }
            }
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x,rectTransform.localPosition.y,0);
            gameObject.SetActive(true);
            rectTransform.localScale = Vector3.one;
            // transform.localScale = Vector3.one;
        }

        public void SetSize(Vector2 size)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y);
        }
        protected void SetDirty()
        {
            if (!gameObject.activeSelf)
                return;
        
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            else
                StartCoroutine(DelayedSetDirty(rectTransform));
        }

        IEnumerator DelayedSetDirty(RectTransform rectTransform)
        {
            //Debug.Log("SetDirty1");
            yield return null;
            //Debug.Log("SetDirty2");
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}