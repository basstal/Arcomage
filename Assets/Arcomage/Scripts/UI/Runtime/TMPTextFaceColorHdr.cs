using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPTextFaceColorHdr : MonoBehaviour
    {
        [SerializeField][ColorUsage(true, true)]
        private Color m_hdrColor = Color.white;
        private TextMeshProUGUI m_text;

        void Awake()
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (m_text == null)
            {
                m_text = GetComponent<TextMeshProUGUI>();
            }

            if (m_text != null && m_hdrColor != null)
            {
                m_text.faceColor = m_hdrColor;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            UpdateColor();
        }
#endif
    }
}
