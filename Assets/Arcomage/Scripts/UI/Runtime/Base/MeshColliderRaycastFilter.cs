using UnityEngine;
// using NOAH.Debug;

namespace NOAH.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(MeshCollider))]
    public class MeshColliderRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        private MeshCollider m_collider;

        void Start()
        {
            m_collider = GetComponent<MeshCollider>();
        }

        public bool IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
        {
            return m_collider.Raycast(eventCamera.ScreenPointToRay(screenPos), out var hit, Mathf.Infinity);
        }
    }
}