using UnityEngine;
using UnityEngine.UI;

namespace NOAH.UI
{
    class UIMaterialInstancier : MonoBehaviour
    {
        public Graphic m_target;

        private void OnEnable()
        {
            var material = m_target != null ? m_target.material : null;
            if (material != null && ((material.hideFlags & HideFlags.DontSave) == 0))
            {
                material = new Material(material);
#if UNITY_EDITOR
                material.name += "(Instance)";
#endif
                material.hideFlags = HideFlags.DontSave;
                m_target.material = material;
            }
        }

        private void OnValidate()
        {
            if (m_target == null) m_target = GetComponent<Graphic>();
        }
    }
}
