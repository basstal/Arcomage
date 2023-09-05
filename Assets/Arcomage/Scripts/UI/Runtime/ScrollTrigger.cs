using System;
// using GamePlay;
// using NOAH.Debug;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * 使用方法：在需要按键滚动的节点上添加该组件，目前仅支持一个维度的滑动
 */
namespace NOAH.UI
{
    public class ScrollTrigger : MonoBehaviour
    {
        [SerializeField] private ScrollRect m_scroll;
        [SerializeField] private int sensitivity = 15;

        // private AxisAction m_viewMove = null;

        private int m_repeatCount = 0;
        // private float m_lastMoveTime = 0.0f;
        private float m_moveVal;
        private float m_lastMoveVal;


        private ScrollTriggerWatcher m_watcher;

        private void Awake()
        {
            if (!m_scroll)
            {
                // LogTool.LogError("ScrollTrigger", "No ScrollRect found");
                return;
            }

            if (m_scroll.horizontal && m_scroll.vertical)
            {
                // LogTool.LogWarning("ScrollTrigger", "ScrollRect Support both direction scrolling, which is not supposed by ScrollTrigger");
            }

            m_watcher = GetComponentInParent<ScrollTriggerWatcher>();
            if (!m_watcher)
            {
                // LogTool.LogError("ScrollTrigger", "No ScrollTriggerWatcher found");
                return;
            }

            // m_viewMove = m_watcher.viewMove;
            // if (m_viewMove != null)
            // {
            //     m_viewMove.OnChange.AddListener(OnNavigationMove);
            // }
        }

        private void OnEnable()
        {
            m_watcher?.AddTrigger(this);
            NotifyWatcher();
        }

        private void OnDisable()
        {
            m_moveVal = 0.0f;
            m_lastMoveVal = 0.0f;
            // m_lastMoveTime = 0.0f;
            m_repeatCount = 0;

            m_watcher?.DeleteTrigger();
            NotifyWatcher();
        }

        private void Update()
        {
            ProcessMove();
        }

        private void OnNavigationMove(float val)
        {
            m_moveVal = val;
        }

        private void ProcessMove()
        {
            if (Math.Abs(m_moveVal) > 0.5)
            {
                if (m_moveVal * m_lastMoveVal > 0)
                {
                    var repeatInterval = m_repeatCount > 0 ? 0.1f : 0.5f;
                    // if (GameTime.unscaledTime - m_lastMoveTime >= repeatInterval)
                    {
                        TriggerScroll(m_moveVal);

                        // m_lastMoveTime = GameTime.unscaledTime;
                        m_repeatCount++;
                    }
                }
                else
                {
                    TriggerScroll(m_moveVal);

                    // m_lastMoveTime = GameTime.unscaledTime;
                    m_repeatCount = 0;
                }

                m_lastMoveVal = m_moveVal;
            }
            else
            {
                m_repeatCount = 0;
                m_lastMoveVal = 0.0f;
            }
        }

        private void TriggerScroll(float val)
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                if (!CheckCanScroll())
                {
                    return;
                }

                var eventData = new PointerEventData(EventSystem.current);
                eventData.Reset();
                float x = m_scroll.horizontal ? val * sensitivity : 0.0f;
                float y = m_scroll.vertical ? val * sensitivity : 0.0f;
                eventData.scrollDelta = new Vector2(x, y);

                if (m_scroll)
                {
                    ExecuteEvents.Execute(m_scroll.gameObject, eventData, ExecuteEvents.scrollHandler);
                }
                else
                {
                    // LogTool.LogError("ScrollTrigger", "No ScrollRect Attached to ScrollTrigger");
                }
            }
        }


        private void OnRectTransformDimensionsChange()
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                NotifyWatcher();
            }
        }

        private void NotifyWatcher()
        {
            if (m_watcher)
            {
                m_watcher.SetDirty();
            }
        }


        public bool CheckCanScroll(bool log = false)
        {
            if (log)
            {
                // LogTool.LogInfo("ScrollTrigger", "content" + m_scroll.content.rect + " viewport" + m_scroll.viewport.rect + " " + enabled + " " + gameObject.activeInHierarchy);
                
            }
            if (!enabled || !gameObject.activeInHierarchy)
            {
                return false;
            }

            if (m_scroll.horizontal)
            {
                return m_scroll.content.rect.width > m_scroll.viewport.rect.width;
            }

            if (m_scroll.vertical)
            {
                return m_scroll.content.rect.height > m_scroll.viewport.rect.height;
            }

            return false;
        }
    }
}
