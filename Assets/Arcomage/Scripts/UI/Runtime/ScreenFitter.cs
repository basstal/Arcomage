using UnityEngine;
// using NOAH.Render;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NOAH.UI
{
    [ExecuteAlways]
    [ExecuteInEditMode]
    public class ScreenFitter : MonoBehaviour
    {
        public float RequireWidth = 2400f;
        public float RequireHeight = 1080f;
        public float BaseScale = 1f;

        public Vector3 Point;
        public Vector3 BasePoint;
        
        void OnEnable()
        {
            
            // var w = ACERender.ACERenderPipeline.Current.PixelWidth;
            // var h = ACERender.ACERenderPipeline.Current.PixelHeight;
            var scale = 1.0f * Camera.main.aspect / (RequireWidth / RequireHeight);
            transform.localScale = Vector3.one * scale;

            var oldPos = transform.position;

            var x = (BasePoint.x - Point.x) * scale + Point.x;
            var y = (BasePoint.y - Point.y) * scale + Point.y;

            transform.position = new Vector3(x, y, 0);
        }
    }
}
