using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace NOAH.UI
{
    public class ClipMesh : RawImage
    {
        [SerializeField] List<Vector2> tmpVertices = new List<Vector2>();
        [SerializeField] List<Vector2> m_vertices = new List<Vector2>();
        [SerializeField] Vector2 m_editAddSpan = Vector2.zero;
        List<int> m_triangles = new List<int>();
        [SerializeField] bool m_editMode = false;
        [SerializeField] int m_realWidth = 100;
        [SerializeField] int m_realHeight = 100;
        [SerializeField] bool m_keepNativeSize = true;

        Vector2 m_offset = Vector2.zero;
        public bool Drawing;

        protected ClipMesh()
        {
            useLegacyMeshGeneration = false;
        }

        override protected void OnEnable()
        {
#if UNITY_EDITOR
            Drawing = false;
            UpdateShape();
#endif
            base.OnEnable();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
#if UNITY_EDITOR
            if ((!Application.isPlaying) && m_editMode)
            {
                OnPopulateMeshEditMode(vh);
                return;
            }
#endif
            vh.Clear();
            if (m_vertices == null) return;
            if (m_triangles.Count == 0) return;
            Texture tex = mainTexture;
            if (tex != null)
            {
                var realR = GetOriginalRect();
                var realStartP = new Vector2(realR.x, realR.y);
                var displayR = GetPixelAdjustedRect();
                var displayStartP = new Vector2(displayR.x, displayR.y);

                var scaleX = 1.0f * displayR.width / m_realWidth;
                var scaleY = 1.0f * displayR.height / m_realHeight;
                {
                    var color32 = color;
                    foreach (var vv in m_vertices)
                    {
                        Vector2 span = vv - realStartP;
                        Vector2 displayV = displayStartP + new Vector2(span.x * scaleX, span.y * scaleY);
                        vh.AddVert(displayV, color32, GetUVPosition(vv, realStartP, realR));
                    }

                    for (int i = 0; i < m_triangles.Count(); i = i + 3)
                    {
                        vh.AddTriangle(m_triangles[i], m_triangles[i + 1], m_triangles[i + 2]);
                    }
                }
            }
        }

        public void SetOffset(int x, int y)
        {
            m_offset = new Vector2(x, y);
            SetVerticesDirty();
        }

        public void SetOffset(Vector2 v)
        {
            m_offset = v;
            SetVerticesDirty();
        }

        public void SetRealSize()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_realWidth);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_realHeight);
        }

        Vector2 GetUVPosition(Vector2 p, Vector2 startP, Rect r)
        {
            Vector2 v = Vector2.zero;
            Vector2 span = p - (startP + m_offset);
            Texture tex = mainTexture;
            v.x = uvRect.xMin + uvRect.width / r.width * span.x;
            v.y = uvRect.yMin + uvRect.height / r.height * span.y;
            return v;
        }

        Vector2 GetUVPosition(Vector2 p, Vector2 startP, int width, int height)
        {
            Vector2 v = startP;
            Vector2 span = p - startP;
            Texture tex = mainTexture;
            v.x = uvRect.xMin + uvRect.width / width * span.x;
            v.y = uvRect.yMin + uvRect.height / height * span.y;
            return v;
        }

        Rect GetOriginalRect()
        {
            Rect r = new Rect();
            r.width = m_realWidth;
            r.height = m_realHeight;
            var pivot = rectTransform.pivot;
            r.x = 0 - pivot.x * m_realWidth;
            r.y = 0 - pivot.y * m_realHeight;
            return r;
        }


#if UNITY_EDITOR
        public void UpdateShape()
        {
            var array = m_vertices.ToArray();
            m_triangles = new Triangulator(array).Triangulate().ToList();
            SetVerticesDirty();
        }

        void OnPopulateMeshEditMode(VertexHelper vh)
        {
            Drawing = true;
            vh.Clear();
            Texture tex = mainTexture;
            if (tex == null) return;
            PopulateBgMesh(vh);
            if (m_vertices == null)
            {
                Drawing = false;
                return;
            }

            // var r = GetPixelAdjustedRect();
            var r = GetOriginalRect();
            var startP = new Vector2(r.x, r.y);
            var color32 = color;
            color32.a = 1f;
            foreach (var vv in m_vertices)
            {
                vh.AddVert(vv, color32, GetUVPosition(vv, startP, m_realWidth, m_realHeight));
            }

            int x = int.MinValue;
            for (int i = 0; i < m_triangles.Count(); i = i + 3)
            {
                if (x < m_triangles[i])
                    x = m_triangles[i];
                vh.AddTriangle(m_triangles[i] + 4, m_triangles[i + 1] + 4, m_triangles[i + 2] + 4);
            }

            Drawing = false;
        }

        void GetTextureOriginalSize(string path, out int width, out int height)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            object[] args = new object[2] {0, 0};
            var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);

            if (importer != null) method.Invoke(importer, args);

            width = (int) args[0];
            height = (int) args[1];
        }

        void PopulateBgMesh(VertexHelper vh)
        {
            // var r = GetPixelAdjustedRect(); //rectTransform rect;
            var r = GetOriginalRect();
            var startP = new Vector2(r.x, r.y);
            var centerP = new Vector2(startP.x + r.width * 0.5f, startP.y + r.height * 0.5f);
            GetTextureOriginalSize(AssetDatabase.GetAssetPath(texture), out var width, out var height);
            float halfW = 0.5f * width;
            float halfH = 0.5f * height;
            var color32 = color;
            color32.a = 0.3f;

            Vector2 v = centerP + new Vector2(-halfW, -halfH);
            vh.AddVert(v, color32, GetUVPosition(v, startP, r));
            v = centerP + new Vector2(-halfW, +halfH);
            vh.AddVert(v, color32, GetUVPosition(v, startP, r));
            v = centerP + new Vector2(+halfW, +halfH);
            vh.AddVert(v, color32, GetUVPosition(v, startP, r));
            v = centerP + new Vector2(+halfW, -halfH);
            vh.AddVert(v, color32, GetUVPosition(v, startP, r));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        public void EnterEditMode()
        {
            GetTextureOriginalSize(AssetDatabase.GetAssetPath(texture), out var width, out var height);

            for (int i = 0; i < m_vertices.Count; i++)
            {
                m_vertices[i] = m_vertices[i] + m_editAddSpan;
            }

            Rect rect = new Rect();
            rect.width = 1.0f * m_realWidth / width;
            rect.height = 1.0f * m_realHeight / height;
            rect.x = 0.5f * (width - m_realWidth) / width;
            rect.y = 0.5f * (height - m_realHeight) / height;
            uvRect = rect;
            m_editMode = true;
        }

        public void LeaveEditMode()
        {
            m_editMode = false;
            if (m_vertices.Count < 3)
            {
                //SetVerticesDirty();
                return;
            }

            float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;
            foreach (var v in m_vertices)
            {
                if (v.x > xMax)
                    xMax = v.x;
                if (v.x < xMin)
                    xMin = v.x;
                if (v.y > yMax)
                    yMax = v.y;
                if (v.y < yMin)
                    yMin = v.y;
            }

            if ((xMin == xMax) || (yMin == yMax))
                return;
            float w = xMax - xMin;
            float h = yMax - yMin;
            m_realWidth = Mathf.RoundToInt(w);
            m_realHeight = Mathf.RoundToInt(h);
            if (m_keepNativeSize)
            {
                SetRealSize();
            }

            GetTextureOriginalSize(AssetDatabase.GetAssetPath(texture), out var width, out var height);

            var r = GetOriginalRect();
            var startP = new Vector2(r.x, r.y);
            var centerP = new Vector2(startP.x + r.width * 0.5f, startP.y + r.height * 0.5f);
            float x = (xMax + xMin) / 2;
            float y = (yMax + yMin) / 2;
            var shapeCenter = new Vector2(x, y);
            m_editAddSpan = shapeCenter - centerP;
            for (int i = 0; i < m_vertices.Count; i++)
            {
                m_vertices[i] = m_vertices[i] - m_editAddSpan;
            }

            Rect rect = new Rect();
            rect.width = 1.0f * w / width;
            rect.height = 1.0f * h / height;
            rect.x = (0.5f * (width - m_realWidth) + m_editAddSpan.x) / width;
            rect.y = (0.5f * (height - m_realHeight) + m_editAddSpan.y) / height;
            uvRect = rect; // It will trigger SetVerticesDirty() 
        }


#endif
    }
}