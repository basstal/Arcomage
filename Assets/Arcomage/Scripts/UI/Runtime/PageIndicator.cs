using UnityEngine;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class PageIndicator : UIBehaviour
    {
        [SerializeField] private GameObject m_normal;

        [SerializeField] private GameObject m_highlight;

        [SerializeField] private int m_pageCount = 0;

        [SerializeField] private int m_enableMinCount = 3;

        private int m_highlightIndex = 0;

        public int PageCount
        {
            get => m_pageCount;
            set
            {
                var validValue = Mathf.Max(0, value);
                if (m_pageCount != validValue)
                {
                    m_pageCount = validValue;
                    Refresh();
                }
            }
        }

        public int EnableMinCount
        {
            get => m_enableMinCount;
            set
            {
                var validValue = Mathf.Max(0, value);
                if (m_enableMinCount != validValue)
                {
                    m_enableMinCount = validValue;
                    Refresh();
                }
            }
        }

        public int HighlightIndex
        {
            get => m_highlightIndex;
            set
            {
                var clampedValue = Mathf.Clamp(value, 0, m_pageCount);
                if (m_highlightIndex != clampedValue)
                {
                    m_highlightIndex = clampedValue;
                    Refresh();
                }
            }
        }

        private void Refresh()
        {
            m_pageCount = Mathf.Max(0, m_pageCount);
            gameObject.SetActive(m_pageCount >= m_enableMinCount);

            if (m_pageCount >= m_enableMinCount)
            {
                transform.ReserveChildren(m_normal.transform, m_pageCount);

                m_highlightIndex = Mathf.Clamp(m_highlightIndex, 0, m_pageCount);
                m_highlight.transform.SetSiblingIndex(m_highlightIndex);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            Refresh();
        }
#endif
    }
}
