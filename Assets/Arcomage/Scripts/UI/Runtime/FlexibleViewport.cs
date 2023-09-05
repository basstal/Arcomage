using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
// using NOAH.Debug;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    //挂在scroll rect的content上，和scroll rect一起使用，让超viewport一点点的文字框显示完，不用滚动
    public class FlexibleViewport : UIBehaviour
    {
        public bool HorizontalFlexible;
        [ShowIf("@this.HorizontalFlexible"), LabelText("最大扩展宽度")]
        public float MaxHorizontalOffset;
        public bool VerticalFlexible;
        [ShowIf("@this.VerticalFlexible"), LabelText("最大扩展高度")]
        public float MaxVerticalOffset;

        private Vector2 m_originalSize;
        private Vector2 m_flexibleSize;
        private Vector2 m_prevSelfSize;
        private RectTransform m_viewPortRect;
        private RectTransform m_selfRect;
        private bool m_curVerticalState;
        private bool m_curHorizontalState;
        [SerializeField] private ScrollRect m_scrollRect;

        protected override void Awake()
        {
            if (!m_scrollRect)
            {
                // LogTool.LogError("FlexibleViewport", "No ScrollRect found");
                return;
            }
            if(m_scrollRect.content != transform)
            {
                // LogTool.LogError("FlexibleViewport", "scroll rect's content is not this object");
                return;
            }
            m_selfRect = transform as RectTransform;
        }

        protected override void OnEnable()
        {
            m_curVerticalState = false;
            m_curHorizontalState = false;
            m_viewPortRect = m_scrollRect.viewport;
            if (!m_viewPortRect)
            {
                // LogTool.LogError("FlexibleViewport", $"viewport not found");
                return;
            }
            m_originalSize = m_viewPortRect.rect.size;
            m_flexibleSize = m_viewPortRect.rect.size;
            m_prevSelfSize = m_selfRect.rect.size;
            UpdateSize();
        }

        protected override void OnDisable()
        {
            if (m_viewPortRect)
            {
                // m_viewPortRect.SetWidth(m_originalSize.x);
                // m_viewPortRect.SetHeight(m_originalSize.y);
            }
            m_curHorizontalState = false;
            m_curVerticalState = false;
        }

        void Update()
        {
            if (m_flexibleSize!=m_viewPortRect.rect.size)
            {
                if (m_flexibleSize!=m_originalSize)
                {
                    if (HorizontalFlexible)
                    {
                        // m_viewPortRect.SetWidth(m_viewPortRect.rect.width - (m_flexibleSize.x - m_originalSize.x));
                        m_curHorizontalState = false;
                    }
                    if (VerticalFlexible)
                    {
                        // m_viewPortRect.SetHeight(m_viewPortRect.rect.height - (m_flexibleSize.y - m_originalSize.y));
                        m_curVerticalState = false;
                    }
                }
                //LogTool.LogInfo("FlexibleViewport", $"viewport size changed! from {m_flexibleSize} to {m_viewPortRect.rect.size}");
                m_originalSize = m_viewPortRect.rect.size;
                UpdateSize();
            }
            else if (m_prevSelfSize != m_selfRect.rect.size)
            {
                m_prevSelfSize = m_selfRect.rect.size;
                UpdateSize();
            }
        }

        /*protected override void OnRectTransformDimensionsChange()
        {
            UpdateSize();
        }*/

        private void UpdateSize()
        {
            UpdateVertical();
            UpdateHorizontal();
        }

        private void UpdateVertical()
        {
            if (!m_selfRect || !m_viewPortRect) return;
            if (VerticalFlexible)
            {
                if (m_selfRect.rect.height < m_originalSize.y ||
                    m_selfRect.rect.height > m_originalSize.y + MaxVerticalOffset)
                {
                    if (m_curVerticalState == true)
                    {
                        // m_viewPortRect.SetHeight(m_originalSize.y);
                        m_curVerticalState = false;
                    }
                }
                else if (m_curVerticalState == false)
                {
                    // m_viewPortRect.SetHeight(m_originalSize.y + MaxVerticalOffset);
                    m_curVerticalState = true;
                }
                m_flexibleSize.y = m_viewPortRect.rect.height;
            }
        }

        private void UpdateHorizontal()
        {
            if (!m_selfRect || !m_viewPortRect) return;
            if (HorizontalFlexible)
            {
                if (m_selfRect.rect.width < m_originalSize.x ||
                    m_selfRect.rect.width > m_originalSize.x + MaxHorizontalOffset)
                {
                    if (m_curHorizontalState == true)
                    {
                        // m_viewPortRect.SetWidth(m_originalSize.x);
                        m_curHorizontalState = false;
                    }
                }
                else if (m_curHorizontalState == false)
                {
                    // m_viewPortRect.SetWidth(m_originalSize.x + MaxHorizontalOffset);
                    m_curHorizontalState = true;
                }
                m_flexibleSize.x = m_viewPortRect.rect.width;
            }
        }

        protected void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!m_scrollRect || (!VerticalFlexible && !HorizontalFlexible)) return;
            RectTransform rect = m_scrollRect.viewport;
            if (!rect) return;
            Gizmos.color = Color.green;
            if (VerticalFlexible)
            {
                var gizmoSize = new Vector2(rect.rect.width, MaxVerticalOffset) * rect.lossyScale;
                var centerPos = rect.TransformPoint(new Vector2(rect.rect.center.x,
                    rect.rect.center.y - (rect.rect.height + MaxVerticalOffset) / 2f));
                Gizmos.DrawWireCube(new Vector3(centerPos.x, centerPos.y, 0f), gizmoSize);
            }
            if (HorizontalFlexible)
            {
                var gizmoSize = new Vector2(MaxHorizontalOffset, rect.rect.height) * rect.lossyScale;
                var centerPos = rect.TransformPoint(new Vector2(rect.rect.center.x + (rect.rect.width + MaxHorizontalOffset) / 2f,
                    rect.rect.center.y));
                Gizmos.DrawWireCube(new Vector3(centerPos.x, centerPos.y, 0f), gizmoSize);
            }
#endif
        }


        /*Lazy方式不支持动态加载，如果viewport被其它逻辑更改，无法反映出来
        protected override void OnEnable()
        {
            StartCoroutine(DelayedInitialize());
        }

        protected override void OnDisable()
        {
            if (m_viewPortRect)
            {
                m_viewPortRect.SetWidth(m_originalSize.x);
                m_viewPortRect.SetHeight(m_originalSize.y);
            }
        }

        protected override void OnDestroy()
        {
            
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if(this.IsActive())
                StartCoroutine(DelayedUpdateViewportRect());
        }

        private IEnumerator DelayedInitialize()
        {
            yield return null;
            m_viewPortRect = m_scrollRect.viewport;
            if (!m_viewPortRect)
            {
                LogTool.LogError("FlexibleViewport", $"viewport not found");
                yield break;
            }
            m_originalSize = m_viewPortRect.rect.size;
            yield return DelayedUpdateViewportRect();
            //LogTool.LogInfo("FlexibleViewport", $"original size {m_originalSize}");
        }

        private IEnumerator DelayedUpdateViewportRect()
        {
            //if(m_viewPortRect)
            //    LogTool.LogInfo("FlexibleViewport", $"original size {m_viewPortRect.rect.size}");
            yield return null;
            UpdateHorizontal();
            UpdateVertical();
        }*/
    }
}
