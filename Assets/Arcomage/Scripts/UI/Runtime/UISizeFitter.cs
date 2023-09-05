using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// using XlsxReader;

namespace NOAH.UI
{
    //bug to fix: 嵌套layoutgroup时表现不正常，也许与时序有关
    [ExecuteAlways]
    public class UISizeFitter : UIBehaviour, ILayoutElement, ILayoutSelfController
    {
        RectTransform m_rectTransform;
        
        public enum SizeCalcMethod
        {
            PreferedSize = 0,
            
            // [EnumLabel("子节点包围盒")]
            ChildrenExtends = 1,    //为了在子节点中做Animation，并且保证transform盒子包含子节点
        }

        [Header("宽度适配子物体")] public bool m_fitHorizontal;

        // [EnumLabel("Width计算")]
        [Sirenix.OdinInspector.ShowIf("m_fitHorizontal")]
        public SizeCalcMethod WidthCalc = SizeCalcMethod.PreferedSize;
        
        [Sirenix.OdinInspector.ShowIf("m_fitHorizontal")]
        [SerializeField] private float m_maxWidth = 0;
    
        public float maxWidth
        {
            get => m_maxWidth;
            set => m_maxWidth = value;
        }

        [Sirenix.OdinInspector.ShowIf("m_fitHorizontal")]
        public float m_minWidth = 0;

        public float minWidth
        {
            get => m_minWidth;
            set => m_minWidth = value;
        }

        [Header("高度适配子物体")] public bool m_fitVertival;
        
        // [EnumLabel("Width计算")]
        [Sirenix.OdinInspector.ShowIf("m_fitVertival")]
        public SizeCalcMethod HeightCalc = SizeCalcMethod.PreferedSize;
        
        [Sirenix.OdinInspector.ShowIf("m_fitVertival")]
        [SerializeField] private float m_maxHeight = 0;

        public float maxHeight
        {
            get => m_maxHeight;
            set => m_maxHeight = value;
        }

        [Sirenix.OdinInspector.ShowIf("m_fitVertival")]
        public float m_minHeight = 0;

        public float minHeight
        {
            get => m_minHeight;
            set => m_minHeight = value;
        }

        private float m_preferredWidth;
        private float m_preferredHeight;

        public bool UpdateEveryFrame = false;
        

        public RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null)
                    m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        public void CalculateLayoutInputHorizontal()
        {
            if (!m_fitHorizontal) return;
            if(WidthCalc == SizeCalcMethod.PreferedSize)
                m_preferredWidth = LayoutUtility.GetPreferredSize(rectTransform, 0);
            else if(WidthCalc == SizeCalcMethod.ChildrenExtends)
            {
                var rect = getChildrenBounds();
                m_preferredWidth = rect.xMax;
            }
            
            if (m_maxWidth > 0)
            {
                m_preferredWidth = Mathf.Min(m_preferredWidth, m_maxWidth);
            }

            if (m_minWidth > 0)
            {
                m_preferredWidth = Mathf.Max(m_preferredWidth, m_minWidth);
            }

            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_preferredWidth);
        }

        public void CalculateLayoutInputVertical()
        {
            if (!m_fitVertival) return;
            if(HeightCalc == SizeCalcMethod.PreferedSize)
                m_preferredHeight = LayoutUtility.GetPreferredSize(rectTransform, 1);
            else if(HeightCalc == SizeCalcMethod.ChildrenExtends)
            {
                var rect = getChildrenBounds();
                m_preferredHeight = Mathf.Max(0, -rect.yMin);
            }
            
            if (m_maxHeight > 0)
            {
                m_preferredHeight = Mathf.Min(m_preferredHeight, m_maxHeight);
            }

            if (m_minHeight > 0)
            {
                m_preferredHeight = Mathf.Max(m_preferredHeight, m_minHeight);
            }

            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_preferredHeight);
        }

        [Button("SetDirty")]
        public void SetDirty()
        {
            if (!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        protected override void OnEnable()
        {
            SetDirty();
        }

        protected override void OnDisable()
        {
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void Update()
        {
            if (UpdateEveryFrame)
            {
                SetDirty();
            }
        }

        public void ForceSetDirty()
        {
            SetDirty();
        }

        public float preferredWidth
        {
            get;
        }

        public float flexibleWidth { get; }

        public float preferredHeight
        {
            get;
        }

        public float flexibleHeight { get; }
        public int layoutPriority { get; }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif

        public void SetLayoutHorizontal()
        {
        }

        public void SetLayoutVertical()
        {
        }

        Rect getChildrenBounds()
        {
            Rect ret = Rect.zero;   //从0开始
            for (int i = 0; i < rectTransform.childCount; i++)
            {
                var ch = rectTransform.GetChild(i) as RectTransform;
                if (ch && ch.gameObject.activeSelf)
                {
                    var localPos = ch.localPosition;
                    ret.yMin = Mathf.Min(ret.yMin, localPos.y + ch.rect.yMin);
                    ret.yMax = Mathf.Max(ret.yMax, localPos.y + ch.rect.yMax);
                    ret.xMin = Mathf.Min(ret.xMin, localPos.x + ch.rect.xMin);
                    ret.xMax = Mathf.Max(ret.xMax, localPos.x + ch.rect.xMax);
                }
            }
            return ret;
        }
    }
}
