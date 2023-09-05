using System.Collections;
// using NOAH.Debug;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class DynamicGrid : UIBehaviour, ILayoutGroup
    {
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private Vector2 init;
        private Vector2 prefer;

        private bool layoutDirty;

        protected override void Awake()
        {
            base.Awake();
            // if (!grid) LogTool.LogError("DynamicGrid", "No Grid Attached");
            layoutDirty = false;
        }

        public void SetDirty()
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                if (!layoutDirty)
                {
                    layoutDirty = true;
                    StartCoroutine(Rebuild());
                }
            }
        }

        public void SetLayoutHorizontal()
        {
            float width = 0;
            foreach (RectTransform childTrans in grid.transform)
            {
                if (!childTrans) return;
                width = Mathf.Max(LayoutUtility.GetPreferredWidth(childTrans), width);
            }

            prefer.x = width > init.x ? width : init.x;
        }

        public void SetLayoutVertical()
        {
            float height = 0;
            foreach (RectTransform childTrans in grid.transform)
            {
                if (!childTrans) return;
                height = Mathf.Max(LayoutUtility.GetPreferredHeight(childTrans), height);
            }

            prefer.y = height > init.y ? height : init.y;
            //the auto layout system evaluates widths first and then heights afterwards, so height can depend on width
            if (prefer.x > grid.cellSize.x || prefer.y > grid.cellSize.y)
            {
                grid.cellSize = prefer;
            }
        }

        private IEnumerator Rebuild()
        {
            yield return null;
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
            layoutDirty = false;
        }
    }
}
