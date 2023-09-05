using System.Collections.Generic;
using UnityEngine;
// using NOAH.Render;
// using NOAH.Proto;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NOAH.UI
{
    [ExecuteAlways]
    [ExecuteInEditMode]
    public class UISafeAreaFitter : MonoBehaviour
    {
        // [SerializeField,TitleGroup("边适配")] private bool m_fitLeft = true;
        // [SerializeField,TitleGroup("边适配")] private bool m_fitRight = true;
        // [SerializeField,TitleGroup("边适配")] private bool m_fitTop = true;
        // [SerializeField,TitleGroup("边适配")] private bool m_fitBottom = true;
        [SerializeField,TitleGroup("通过Z轴进行缩放")] private bool m_zScale = false;
        [SerializeField,TitleGroup("通过Z轴进行缩放"),EnableIf("m_zScale")] private float m_z2aspect = 1;
        [SerializeField,TitleGroup("通过Z轴进行缩放"),EnableIf("m_zScale")] private float m_zZeroWidth = 2400f;
        [SerializeField,TitleGroup("通过Z轴进行缩放"),EnableIf("m_zScale")] private float m_zZeroHeight = 1080f;




        void OnEnable()
        {
            // if (Render.RenderManager.Instance == null) return;
            // Render.RenderManager.Instance.OnDeviceLayoutChanged += OnDeviceLayoutChanged;
            var rectTransform = GetComponent<RectTransform>();
            UpdateZ(rectTransform);
        }

        void OnDisable()
        {
            // if (Render.RenderManager.Instance == null) return;
            // Render.RenderManager.Instance.OnDeviceLayoutChanged -= OnDeviceLayoutChanged;
        }

        // called when attached or device layout changed
        // void OnDeviceLayoutChanged(DeviceLayout deviceLayout)
        // {
        //     var rectTransform = GetComponent<RectTransform>();
        //     var left = deviceLayout?.MarginLeft ?? 0;
        //     var bottom = deviceLayout?.MarginBottom ?? 0;
        //     rectTransform.offsetMin = Vector2.right * (m_fitLeft ? left : 0) + Vector2.up * (m_fitBottom ? bottom : 0);
        //
        //     var right = deviceLayout?.MarginRight ?? 0;
        //     var top = deviceLayout?.MarginTop ?? 0;
        //     rectTransform.offsetMax = Vector2.left * (m_fitRight ? right : 0) + Vector2.down * (m_fitTop ? top : 0);
        //
        //     UpdateZ(rectTransform);
        //     
        //     
        // }

        void UpdateZ(RectTransform rectTransform)
        {
            if(!m_zScale)
            {
                return;
            }
            float z = 0;
            var w = rectTransform.rect.width;
            var h = rectTransform.rect.height;
            if(h!= 0)
            {
                var aspect = 1.0 * w/h;
                var zZeroAspect = m_zZeroWidth/m_zZeroHeight;
                if(aspect < zZeroAspect)
                {
                    z = (float)(zZeroAspect - aspect) * m_z2aspect;
                }
            }
            var pos = transform.localPosition;
            pos.z = z;
            transform.localPosition = pos;
        }

#if UNITY_EDITOR
        bool changing = false;

        protected void OnValidate()
        {
            changing = true;

            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    // OnDeviceLayoutChanged(RenderManager.Instance ? RenderManager.Instance.DeviceLayout : null);
                }
            };

            changing = false;
        }

        protected void OnRectTransformDimensionsChange()
        {
            if (!Application.isPlaying && !changing)
            {
                changing = true;

                // OnDeviceLayoutChanged(RenderManager.Instance ? RenderManager.Instance.DeviceLayout : null);

                changing = false;
            }
        }
#endif
    }
}