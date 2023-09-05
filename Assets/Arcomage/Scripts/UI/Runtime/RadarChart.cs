using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Collections;
// using NOAH.Utility;


namespace NOAH.UI
{
    public class RadarChart : MaskableGraphic
    {
        //所画多边形顶点的位置
        private List<Vector3> PointPos=new List<Vector3>();
        //判断是否可以画多边形
        private bool CanDraw => PointPos != null && PointPos.Count >= 2;
        //获取内接圆的半径
        private float R => (this.rectTransform.rect.height >= this.rectTransform.rect.width ? this.rectTransform.rect.width : this.rectTransform.rect.height)/2;

        [SerializeField] Texture m_Texture;

        public override Texture mainTexture
        {
            get
            {
                if (m_Texture == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Texture;
            }
        }

        public Texture texture
        {
            get
            {
                return m_Texture;
            }
            set
            {
                if (m_Texture == value)
                    return;

                m_Texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        //画多边形
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            AddPos(vh);
            AddTriangle(vh);
        }
        private VertexHelper AddPos(VertexHelper vh)
        {
            if (!CanDraw) return null;
            var color32 = color;
            Vector2 uvPos = new Vector2(0f, 0f);
            for (int i = 0; i < PointPos.Count; ++i)
            {
                vh.AddVert(PointPos[i], color32, uvPos);
            }
            return vh;
        }
        private VertexHelper AddTriangle(VertexHelper vh)
        {
            if (!CanDraw) return null;
            for (int i = 1; i < PointPos.Count - 1; ++i)
            {
                vh.AddTriangle(0, i, i + 1);
            }
            return vh;
        }

        public void SetVertexPos(List<float> NorLenghList)
        {
            if (NorLenghList.Count <= 2 || NorLenghList == null) return;
            PointPos.Clear();
            int dAngle = 360 / NorLenghList.Count;//每个点的偏移量
            int oriAngle = 90;//以y轴为起点
            for (int i = 0; i < NorLenghList.Count; ++i)
            {
                int tmpAngle = oriAngle + dAngle * i;
                PointPos.Add(new Vector3(R*NorLenghList[i]*Mathf.Cos(tmpAngle*Mathf.Deg2Rad), R * NorLenghList[i] * Mathf.Sin(tmpAngle * Mathf.Deg2Rad), 0));
            }
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        //测试用，外部调用显示多边形
        [ContextMenu("ShowPolygon")]
        public void ShowAttrGraphic()
        {        
            List<float> Lengh=new List<float>();
            for (int i = 0; i < 5; ++i)
            {
                Lengh.Add(1f);
            }
            SetVertexPos(Lengh);
        }
#endif
    }
}
