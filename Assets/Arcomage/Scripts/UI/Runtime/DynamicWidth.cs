using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    public class DynamicWidth : UIBehaviour, ILayoutGroup, ILayoutElement
    {
        public float maxWidth;
        public float initialWidth;
        private float m_preferWidth;
        const float padding = 40;

        [SerializeField] [HideInInspector] private RectTransform monitored;
        [SerializeField] [HideInInspector] private RectTransform rectTransform;

        protected override void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            var child = GetComponentInChildren<TextMeshProUGUI>();
            monitored = child.GetComponent<RectTransform>();
        }

        public void CalculateLayoutInputHorizontal()
        {
            float childWidth = LayoutUtility.GetPreferredWidth(monitored);
            float prefer = childWidth + padding;

            if (prefer > initialWidth)
            {
                m_preferWidth = Mathf.Min(maxWidth, prefer);
            }
            else
            {
                m_preferWidth = initialWidth;
            }
        }

        public void CalculateLayoutInputVertical()
        {
        }

        public void SetLayoutHorizontal()
        {
            float childWidth = LayoutUtility.GetPreferredWidth(monitored);
            bool autoSize = false;
            if (childWidth + padding > maxWidth)
            {
                childWidth = maxWidth - padding;
                autoSize = true;
            }

            monitored.GetComponent<TextMeshProUGUI>().enableAutoSizing = autoSize;
            // monitored.SetWidth(childWidth);
        }

        public void SetLayoutVertical()
        {
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            rectTransform = GetComponent<RectTransform>();
            var child = GetComponentInChildren<TextMeshProUGUI>();
            monitored = child.GetComponent<RectTransform>();
        }
#endif

        public virtual float minWidth { get { return 0; } }

        public virtual float preferredWidth
        {
            get
            {
                return m_preferWidth;
            }
        }

        public virtual float flexibleWidth { get { return -1; } }

        public virtual float minHeight { get { return 0; } }

        public virtual float preferredHeight
        {
            get
            {
                return rectTransform.rect.height;
            }
        }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return 0; } }
    }
}
