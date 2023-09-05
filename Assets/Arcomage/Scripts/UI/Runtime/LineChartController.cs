using System;
using System.Collections.Generic;
// using NOAH.Debug;
// using NOAH.VFX;
using UnityEngine;

namespace NOAH.UI
{
    public class LineChartController : MonoBehaviour
    {
        public float width;
        public float detailOffset;
        public float locationLineWidth;

        public List<Sprite> bgImg;
        public List<Sprite> lineImg;

        private LocationLine _locationLine;

        private LocationLine locationLine
        {
            get
            {
                if (_locationLine == null)
                {
                    _locationLine = this.GetComponentInChildren<LocationLine>();
                }

                return _locationLine;
            }
        }

        private List<float[]> datas = new List<float[]>();

        private GameObject bgList;
        private GameObject lineList;
        public void OnPress(Action<int> callback) => locationLine.OnPress += callback;
        public void OnDrag(Action<int> callback) => locationLine.OnDrag += callback;
        public void OnRelease(Action<int> callback) => locationLine.OnRelease += callback;

        private void Start()
        {
            locationLine.dragOffset = detailOffset;
            locationLine.width = locationLineWidth;
        }

        public int dataLenth => datas.Count;

        private void DrawLineChart()
        {
            this.gameObject.SetActive(false);
            bgList = this.transform.Find("BgList").gameObject;
            lineList = this.transform.Find("LineList").gameObject;
            int dChildCount = bgList.transform.childCount - datas.Count;

            for (int i = 0; i < dChildCount; ++i)
            {
                Instantiate(bgList.transform.GetChild(0), bgList.transform);
                Instantiate(lineList.transform.GetChild(0), lineList.transform);
            }

            for (int i = 0; i < datas.Count - 1; ++i)
            {
                LineChart tmpLineChart;

                tmpLineChart = bgList.transform.GetChild(i).GetComponent<LineChart>();
                tmpLineChart.mSprite = bgImg[i % bgImg.Count];
                tmpLineChart.SetPolygonVertexPos(datas[i], datas[i + 1]);

                tmpLineChart = lineList.transform.GetChild(i).GetComponent<LineChart>();
                tmpLineChart.mSprite = lineImg[i % lineImg.Count];
                tmpLineChart.SetLineVertexPos(datas[i + 1], width);
            }

            this.gameObject.SetActive(true);
        }

        public void SetData(float[] data1, float[] data2 = null)
        {
            float[] data0 = new float[data1.Length];
            for (int i = 0; i < data0.Length; ++i)
            {
                data0[i] = 0f;
            }

            datas.Clear();
            datas.Add(data0);
            datas.Add(data1);
            if (data2 != null) datas.Add(data2);
            else datas.Add(data0);
            DrawLineChart();
        }


#if UNITY_EDITOR
        //测试用，外部调用显示折线图
        [ContextMenu("ShowPolygon")]
        public void ShowLineChart()
        {
            datas.Clear();

            float[] data1 = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            float[] data2 = {0.1f, 0.4f, 0.3f, 0f, 0.5f, 0.35f, 0.6f, 0.7f, 0.5f, 0.2f};
            float[] data3 = {0.2f, 0.5f, 0.6f, 0f, 0.7f, 0.45f, 0.9f, 0.8f, 0.7f, 0.4f};

            datas.Add(data1);
            datas.Add(data2);
            datas.Add(data3);
            DrawLineChart();
        }

        protected void OnValidate()
        {
            this.transform.Find("BgList").gameObject.hideFlags = HideFlags.HideInHierarchy;
            this.transform.Find("LineList").gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
#endif
    }
}