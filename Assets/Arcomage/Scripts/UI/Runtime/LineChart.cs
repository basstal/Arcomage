using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// using NOAH.Debug;
// using NOAH.Asset;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace NOAH.UI
{
    public class LineChart : MaskableGraphic
    {
        private bool isline = false;
        private float width = 0.02f;

        //所画多边形顶点的位置
        private List<Vector3> downPointPos = new List<Vector3>();
        private List<Vector3> upPointPos = new List<Vector3>();

        [ReadOnly] public string SpritePath;
        [ReadOnly] public string SpriteName;
        [SerializeField]public Sprite mSprite;

        public override Texture mainTexture
        {
            get
            {
                if (sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return sprite.texture;
            }
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (sprite)
            {
                // SpritePath = AssetBuilder.MakeAssetRefPath(sprite);
                SpriteName = sprite.name;
            }
            
        }
#endif

        public Sprite sprite
        {
            get
            {
                if (mSprite == null)
                {
                    // mSprite = AssetHelper.LoadSubAsset<Sprite>(SpritePath, SpriteName, this);
                }

                return mSprite;
            }
            set
            {
                if (mSprite == value)
                    return;

                mSprite = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        //画多边形
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (isline) AddLinePos(vh);
            else AddPolygonPos(vh);
            AddTriangle(vh);
        }

        private VertexHelper AddPolygonPos(VertexHelper vh)
        {
            var color32 = color;
            int downposcount = 0;
            int upposcount = 0;
            for (int i = 0; i < downPointPos.Count + upPointPos.Count; ++i)
            {
                if (i % 2 == 0)
                {
                    Vector2 uv = new Vector2(((float) sprite.textureRect.xMin + (float) sprite.textureRect.width * ((float) downposcount / downPointPos.Count)) / (float) sprite.texture.width,
                        ((float) sprite.textureRect.yMin + (float) sprite.textureRect.height * downPointPos[downposcount].y / rectTransform.rect.height) / (float) sprite.texture.height);
                    vh.AddVert(downPointPos[downposcount], color32, uv);
                    downposcount++;
                }
                else
                {
                    Vector2 uv = new Vector2(((float) sprite.textureRect.xMin + (float) sprite.textureRect.width * ((float) upposcount / upPointPos.Count)) / (float) sprite.texture.width,
                        ((float) sprite.textureRect.yMin + (float) sprite.textureRect.height * upPointPos[upposcount].y / rectTransform.rect.height) / (float) sprite.texture.height);
                    vh.AddVert(upPointPos[upposcount], color32, uv);
                    upposcount++;
                }
            }

            return vh;
        }

        private VertexHelper AddLinePos(VertexHelper vh)
        {
            SetPointPos();
            var color32 = color;
            int downposcount = 0;
            int upposcount = 0;

            //以uv的x坐标等分采样
            List<float> uvx = new List<float>();
            float dx = 1 / (float) (downPointPos.Count - 1);
            for (int i = 0; i < downPointPos.Count; ++i)
            {
                uvx.Add(dx * i);
            }

            //添加点的坐标
            for (int i = 0; i < downPointPos.Count + upPointPos.Count; ++i)
            {
                if (i % 2 == 0)
                {
                    vh.AddVert(downPointPos[downposcount], color32,
                        new Vector2(
                            ((float) sprite.textureRect.xMin + (float) sprite.textureRect.width * uvx[downposcount]) / (float) sprite.texture.width,
                            ((float) sprite.textureRect.yMin) / (float) sprite.texture.height));
                    downposcount++;
                }
                else
                {
                    vh.AddVert(upPointPos[upposcount], color32,
                        new Vector2(
                            ((float) sprite.textureRect.xMin + (float) sprite.textureRect.width * uvx[upposcount]) / (float) sprite.texture.width,
                            ((float) sprite.textureRect.yMin + (float) sprite.textureRect.height) / (float) sprite.texture.height));
                    upposcount++;
                }
            }

            return vh;
        }

        private VertexHelper AddTriangle(VertexHelper vh)
        {
            for (int i = 0; i < upPointPos.Count + downPointPos.Count - 2; ++i)
            {
                vh.AddTriangle(i, i + 1, i + 2);
            }

            return vh;
        }

        public void SetPolygonVertexPos(float[] upposlist, float[] downposlist)
        {
            this.isline = false;
            upPointPos.Clear();
            downPointPos.Clear();
            for (int i = 0; i < upposlist.Length; ++i)
            {
                upPointPos.Add(new Vector3(i * rectTransform.rect.width / upposlist.Length,
                    rectTransform.rect.height * upposlist[i], 0.0f));
                downPointPos.Add(new Vector3(i * rectTransform.rect.width / downposlist.Length,
                    rectTransform.rect.height * downposlist[i], 0.0f));
            }

            SetVerticesDirty();
        }

        public void SetLineVertexPos(float[] downposlist, float width)
        {
            this.isline = true;
            this.width = width;
            upPointPos.Clear();
            downPointPos.Clear();
            for (int i = 0; i < downposlist.Length; ++i)
            {
                downPointPos.Add(new Vector3(rectTransform.rect.width * i / downposlist.Length, rectTransform.rect.height * downposlist[i], 0.0f));
            }

            for (int i = 0; i < downposlist.Length - 2; ++i)
            {
                float k1 = (downPointPos[i + 1].y - downPointPos[i].y) / (downPointPos[i + 1].x - downPointPos[i].x);
                float k2 = (downPointPos[i + 2].y - downPointPos[i + 1].y) / (downPointPos[i + 2].x - downPointPos[i + 1].x);

                float b1 = (downPointPos[i + 1].y + downPointPos[i].y) / 2 - k1 * (downPointPos[i + 1].x + downPointPos[i].x) / 2;
                float b2 = (downPointPos[i + 2].y + downPointPos[i + 1].y) / 2 - k2 * (downPointPos[i + 2].x + downPointPos[i + 1].x) / 2;

                float dx1 = width / Mathf.Cos(Mathf.Atan(k1));
                float dx2 = width / Mathf.Cos(Mathf.Atan(k2));
                Vector3 lineAStart = new Vector3(downPointPos[i].x, k1 * downPointPos[i].x + dx1 + b1, 0f);
                Vector3 lineAEnd = new Vector3(downPointPos[i + 1].x, k1 * downPointPos[i + 1].x + dx1 + b1, 0f);
                Vector3 lineBStart = new Vector3(downPointPos[i + 1].x, k2 * downPointPos[i + 1].x + dx2 + b2, 0f);
                Vector3 lineBEnd = new Vector3(downPointPos[i + 2].x, k2 * downPointPos[i + 2].x + dx2 + b2, 0f);

                Vector3 crossoverpoint;
                if (k1 - k2 <= 0.01f) crossoverpoint = lineAEnd;
                else crossoverpoint = GetIntersection(lineAStart, lineAEnd, lineBStart, lineBEnd);
                if (i == 0)
                {
                    upPointPos.Add(lineAStart);
                    upPointPos.Add(crossoverpoint);
                }
                else if (i == downPointPos.Count - 3)
                {
                    upPointPos.Add(crossoverpoint);
                    upPointPos.Add(lineBEnd);
                }
                else
                {
                    upPointPos.Add(crossoverpoint);
                }
            }
        }

        //摆放点的位置
        private void SetPointPos()
        {
            for (int i = 0; i < upPointPos.Count; ++i)
            {
                // this.rectTransform.GetChild(i).GetComponent<RectTransform>().SetAnchoredPos(upPointPos[i].x - 0.5f, upPointPos[i].y - 0.5f);
            }
        }

        //测试用
        private void SetVertexPos(List<Vector3> upposlist, List<Vector3> downposlist)
        {
            this.upPointPos = upposlist;
            this.downPointPos = downposlist;
            SetVerticesDirty();
        }

        public Vector3 GetIntersection(Vector3 lineAStart, Vector3 lineAEnd, Vector3 lineBStart, Vector3 lineBEnd)
        {
            float x1 = lineAStart.x, y1 = lineAStart.y;
            float x2 = lineAEnd.x, y2 = lineAEnd.y;

            float x3 = lineBStart.x, y3 = lineBStart.y;
            float x4 = lineBEnd.x, y4 = lineBEnd.y;

            //两向量相互垂直，返回0
            if (x1 == x2 && x3 == x4 && x1 == x3) return Vector3.zero;
            //两向量相互平行。返回0
            if (y1 == y2 && y3 == y4 && y1 == y3) return Vector3.zero;
            //两向量相互垂直，返回0
            if (x1 == x2 && x3 == x4) return Vector3.zero;
            //两向量相互平行。返回0
            if (y1 == y2 && y3 == y4) return Vector3.zero;

            float x, y;
            if (x1 == x2)
            {
                float m2 = (y4 - y3) / (x4 - x3);
                float c2 = -m2 * x3 + y3;

                x = x1;
                y = c2 + m2 * x1;
            }
            else if (x3 == x4)
            {
                float m1 = (y2 - y1) / (x2 - x1);
                float c1 = -m1 * x1 + y1;

                x = x3;
                y = c1 + m1 * x3;
            }
            else
            {
                float m1 = (y2 - y1) / (x2 - x1);
                float c1 = -m1 * x1 + y1;
                float m2 = (y4 - y3) / (x4 - x3);
                float c2 = -m2 * x3 + y3;
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;
            }

            return new Vector3(x, y, 0);
        }
    }
}
