
using UnityEngine;
using System.Collections;
using UnityEngine.UI;



public class UIDrawRaycastGizmosBox : MonoBehaviour
{

    public Color GizmosColor = Color.cyan;

#if UNITY_EDITOR
    static Vector3[] fourCorners = new Vector3[4];
    void OnDrawGizmos()
    {
        foreach (MaskableGraphic g in gameObject.GetComponentsInChildren<MaskableGraphic>())
        {
            if (g.raycastTarget)
            {
                RectTransform rectTransform = g.transform as RectTransform;
                rectTransform.GetWorldCorners(fourCorners);
                Gizmos.color = GizmosColor;
                for (int i = 0; i < 4; i++)
                    Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);

            }
        }
    }
    
#endif
}