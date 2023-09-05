using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace NOAH.UI
{
public class UISetScale : UIBehaviour
{
    [Serializable]
    public struct ScaleInfo
    {
        public Vector2Int resolution;
        public Vector3 scale;
    }
    public List<ScaleInfo> m_designScale;
    public List<Transform> m_applyToList;

    protected RectTransform m_rectTransform;

    protected override void OnRectTransformDimensionsChange()
    {
        if (m_rectTransform == null)
        {
            m_rectTransform = transform.GetComponent<RectTransform>();
         
            m_designScale.Sort( ( x, y ) => (x.resolution.x + x.resolution.y).CompareTo(y.resolution.x + y.resolution.y) );
        }

        int designScaleListCount = m_designScale.Count;
        int minIndex = designScaleListCount - 1;
        int maxIndex = designScaleListCount - 1;
        for (int i = 0; i < designScaleListCount; i++)
        {
            if (m_rectTransform.rect.width + m_rectTransform.rect.height <= m_designScale[i].resolution.x + m_designScale[i].resolution.y)
            {
                minIndex = Mathf.Max(i-1, 0);
                maxIndex = i;
                break;
            }
        }

        bool isLandscape = m_designScale[0].resolution.y == m_designScale[1].resolution.y;
        float t = 0;
        if (isLandscape)
        {
            t = (m_rectTransform.rect.width - m_designScale[minIndex].resolution.x)/(m_designScale[maxIndex].resolution.x - m_designScale[minIndex].resolution.x);
        }
        else
        {
            t = (m_rectTransform.rect.height - m_designScale[minIndex].resolution.y)/(m_designScale[maxIndex].resolution.y - m_designScale[minIndex].resolution.y);
        }
        Vector3 scale = Vector3.Lerp(m_designScale[minIndex].scale, m_designScale[maxIndex].scale, t);

        for (int i = 0; i < m_applyToList.Count; i++)
        {
            m_applyToList[i].localScale = scale;
        }
    }
}
}