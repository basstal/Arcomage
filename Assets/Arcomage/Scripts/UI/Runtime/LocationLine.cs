using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// using NOAH.Debug;
// using NOAH.Asset;
// using NOAH.VFX;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace NOAH.UI
{
    public class LocationLine : MaskableGraphic
    {
        [SerializeField] Sprite m_sprite;

        private Canvas _canvas;
        private LineChartController _lineChartController;
        private RectTransform _dragDetail;

        [HideInInspector] public float dragOffset;
        [HideInInspector] public float width;
        public override Texture mainTexture
        {
            get
            {
                if (m_sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return m_sprite.texture;
            }
        }

        public Sprite texture
        {
            get { return m_sprite; }
            set
            {
                if (m_sprite == value)
                    return;

                m_sprite = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        //获取左下角的屏幕坐标
        private Vector2 _screenPos = Vector2.zero;

        private Vector2 screenPos
        {
            get
            {
                if (_screenPos == Vector2.zero)
                {
                    _screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, this.transform.position);
                    _screenPos = new Vector2(_screenPos.x - (rectTransform.rect.width / 2), _screenPos.y - (rectTransform.rect.height / 2));
                }

                return _screenPos;
            }
        }

        private int step;
        private float dw;
        private float lineX;
        private bool draw;

        public Action<int> OnPress;
        public Action<int> OnDrag;
        public Action<int> OnRelease;

        protected override void Start()
        {
            this.gameObject.BindPressEvent(OnPointerDown);
            this.gameObject.BindDragEvent(OnPointerDrag);
            this.gameObject.BindReleaseEvent(OnPointerUp);

            base.Start();
            draw = false;
            _canvas = this.GetComponentInParent<Canvas>();
            _lineChartController = this.GetComponentInParent<LineChartController>();
            _dragDetail = this.transform.GetChild(0).GetComponent<RectTransform>();
            _dragDetail.gameObject.SetActive(false);
            // dw = this.rectTransform.rect.width / global::Xlsx.Misc.All[0].BlessMisc.MaxBlessSeriesCount;
            step = -1;
        }

        //画多边形
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (draw)
            {
                AddPos(vh);
                AddTriangle(vh);
            }
        }

        private void AddPos(VertexHelper vh)
        {
            var color32 = color;
            Vector2 uvPos = new Vector2((m_sprite.textureRect.xMin+(m_sprite.textureRect.width/2))/m_sprite.texture.width, (m_sprite.textureRect.yMin+(m_sprite.textureRect.y/2))/m_sprite.texture.height);
            vh.AddVert(new Vector3(lineX-width/2 , -rectTransform.rect.height / 2 + 3, 0f), color32, uvPos);
            vh.AddVert(new Vector3(lineX+width/2, -rectTransform.rect.height / 2 + 3, 0f), color32, uvPos);
            vh.AddVert(new Vector3(lineX+width/2, rectTransform.rect.height / 2 + 3, 0f), color32, uvPos);
            vh.AddVert(new Vector3(lineX - width/2, rectTransform.rect.height / 2 + 3, 0f), color32, uvPos);
        }

        private void AddTriangle(VertexHelper vh)
        {
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }

        private Vector2 GetMousePos(PointerEventData eventData)
        {
            Vector2 temp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, eventData.position, eventData.pressEventCamera, out temp);
            return temp;
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            draw = true;
            Vector2 mousePos = GetMousePos(eventData as PointerEventData);
            _dragDetail.gameObject.SetActive(true);
            // _dragDetail.SetAnchoredPos(mousePos.x - rectTransform.rect.width / 2 + dragOffset, mousePos.y-dragOffset/2);
            step = (int) Mathf.Floor(mousePos.x / dw);
            OnPress?.Invoke(step);
            lineX = step * dw + dw / 2;
            SetVerticesDirty();
        }

        public void OnPointerDrag(BaseEventData eventData)
        {
            Vector2 mousePos = GetMousePos(eventData as PointerEventData);
            if (mousePos.x <= 0 || mousePos.x >= rectTransform.rect.width || mousePos.y <= -rectTransform.rect.height / 2 || mousePos.y >= rectTransform.rect.height / 2) return;
            int tmpStep = (int) Mathf.Floor(mousePos.x / dw);
            // _dragDetail.SetAnchoredPos(mousePos.x - rectTransform.rect.width / 2 + dragOffset, mousePos.y-dragOffset/2);
            if (tmpStep == step) return;
            draw = true;
            step = tmpStep;
            OnDrag?.Invoke(step);
            lineX = step * dw + dw / 2;
            SetVerticesDirty();
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            OnRelease?.Invoke(step);
            draw = false;
            _dragDetail.gameObject.SetActive(false);
            SetVerticesDirty();
        }
    }
}