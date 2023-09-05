using UnityEngine;

namespace NOAH.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Collider2D))]
    public class Collider2DRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        private Collider2D m_collider;
        private RectTransform m_rectTransform;

        void Awake()
        {
            m_collider = GetComponent<Collider2D>();
            m_rectTransform = GetComponent<RectTransform>();
        }

        public bool IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
        {
            var worldPoint = Vector3.zero;
            var isInside = RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rectTransform, screenPos, eventCamera, out worldPoint);
 
            return isInside && m_collider.OverlapPoint(worldPoint);
        }
    }
}