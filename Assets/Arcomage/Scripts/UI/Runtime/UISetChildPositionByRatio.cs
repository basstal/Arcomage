using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace NOAH.UI
{
[ExecuteInEditMode]
public class UISetChildPositionByRatio : UIBehaviour
{
    public List<Vector2> m_childrenPositionRatio = new List<Vector2>();
    protected RectTransform m_rectTransform;

    public void Refresh() // need to call after SetPosition is invoked
    {
        OnRectTransformDimensionsChange();
    }

    protected override void OnRectTransformDimensionsChange()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            if (m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var localPosition = new Vector3(m_rectTransform.rect.width * m_childrenPositionRatio[i].x, m_rectTransform.rect.height * m_childrenPositionRatio[i].y, 0);
                transform.GetChild(i).localPosition = localPosition;
            }
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
        {
            if (m_rectTransform == null)
            {
                m_rectTransform = GetComponent<RectTransform>();
            }

            m_childrenPositionRatio = new List<Vector2>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var localPosition = transform.GetChild(i).localPosition;
                m_childrenPositionRatio.Add(new Vector2(localPosition.x / m_rectTransform.rect.width, localPosition.y / m_rectTransform.rect.height));
            }
        }
    }
#endif

}
}