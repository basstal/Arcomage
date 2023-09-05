using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class DynamicContainer : UIBehaviour, ILayoutGroup, ILayoutElement
    {
        [ShowIf("@widthMonitoreds.Length > 0")]
        public float initialWidth;

        [ShowIf("@widthMonitoreds.Length > 0")]
        public float widthPadding = 40;

        [ShowIf("@heightMonitoreds.Length > 0")]
        public float initialHeight;

        [ShowIf("@heightMonitoreds.Length > 0")]
        public float heightPadding = 40;

        [SerializeField] private RectTransform[] widthMonitoreds;
        [SerializeField] private RectTransform[] heightMonitoreds;

        private RectTransform rectTransform;
        private DynamicGrid dynamicGrid;
        private float m_preferWidth;
        private float m_preferHeight;

        private float Width
        {
            get
            {
                return m_preferWidth;
            }
            set
            {
                if (Math.Abs(value - m_preferWidth) > 0.1)
                {
                    m_preferWidth = value;
                    dynamicGrid?.SetDirty();
                }
            }
        }

        private float Height
        {
            get
            {
                return m_preferHeight;
            }
            set
            {
                if (Math.Abs(value - m_preferHeight) > 0.1)
                {
                    m_preferHeight = value;
                    dynamicGrid?.SetDirty();
                }
            }
        }

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            dynamicGrid = GetComponentInParent<DynamicGrid>();
        }

        public void CalculateLayoutInputHorizontal()
        {
            float width = 0;
            foreach (var monitored in widthMonitoreds)
            {
                width += LayoutUtility.GetPreferredWidth(monitored);
            }

            float prefer = width + widthPadding;

            Width = prefer > initialWidth ? prefer : initialWidth;
        }

        public void CalculateLayoutInputVertical()
        {
            float height = 0;
            foreach (var monitored in heightMonitoreds)
            {
                height += LayoutUtility.GetPreferredHeight(monitored);
            }

            float prefer = height + heightPadding;

            Height = prefer > initialHeight ? prefer : initialHeight;
        }

        public void SetLayoutHorizontal()
        {
            foreach (var monitored in widthMonitoreds)
            {
                float prefer = LayoutUtility.GetPreferredWidth(monitored);
                // monitored.SetWidth(prefer);
            }
        }

        public void SetLayoutVertical()
        {
            foreach (var monitored in heightMonitoreds)
            {
                float prefer = LayoutUtility.GetPreferredHeight(monitored);
                // monitored.SetHeight(prefer);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            rectTransform = GetComponent<RectTransform>();
        }
#endif

        public virtual float minWidth { get { return 0; } }

        public virtual float preferredWidth
        {
            get
            {
                return widthMonitoreds.Length > 0 ? Width : rectTransform.rect.width;
            }
        }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight { get { return 0; } }

        public virtual float preferredHeight
        {
            get
            {
                return heightMonitoreds.Length > 0 ? Height : rectTransform.rect.height;
            }
        }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return 0; } }
    }
}
