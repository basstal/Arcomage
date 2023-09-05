using System;
using UnityEngine;
using Sirenix.OdinInspector;
namespace NOAH.UI
{
    public class ScaleEventHandler : MonoBehaviour
    {
        public Transform Target;
        public Transform Parent;
        public float ScaleSpeed = 1;
        public bool LimitScaleRange = false;
        [ShowIf("@LimitScaleRange==true")]
        public float MaxScale;
        [ShowIf("@LimitScaleRange==true")]
        public float MinScale;

        public AnimationCurve SimulateEnlarge = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve SimulateLessen = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        public event Action OnScaling;
        public event Action OnScaleStart;
        public event Action OnScaleEnd;
        public event Action OnScaleReachLimit;
        
        private bool m_scaleOn;

        private Vector3 m_oriTouch1;
        private Vector3 m_oriTouch2;
        private Vector3 m_oriTargetPos;
        private Vector3 m_posToTouchCenter;
        private float m_oriScale;

        private Vector2 m_parentMin;
        private Vector2 m_parentMax;
        private Vector2 m_childSize = Vector2.zero;
        
        private bool m_simulating = false;
        private float m_simuFrom;
        private float m_simuTo;
        private float m_simuDuration;
        private float m_simuPass = -1;
        
        private void Start()
        {
            m_scaleOn = false;
            if (Parent)
            {
                var rect = (Parent as RectTransform).rect;
                var halfW = (float)rect.width * 0.5f;
                var halfH = (float)rect.height * 0.5f;

                Vector2 pos = Parent.position;
                m_parentMin = pos - new Vector2(halfW, halfH);
                m_parentMax= pos + new Vector2(halfW, halfH);
                m_childSize = (Target as RectTransform).rect.size;
                LimitScaleRange = true;

                var wScale = rect.width / m_childSize.x;
                var hScale = rect.height / m_childSize.y;

                var maxToFill = Math.Max(wScale, hScale);
                MinScale = Math.Max(maxToFill, MinScale);
            }
        }
        
        void Update()
        {
            UpdateSimulate();
            UpdateEvent();
        }
        
        void UpdateSimulate()
        {
            if (!m_simulating) return;
            var ratio = Math.Min(1,(float)m_simuPass / m_simuDuration);
            var curve = m_simuFrom < m_simuTo ? SimulateEnlarge : SimulateLessen;
            var curveValue = curve.Evaluate(ratio);
            var newScale = Mathf.Lerp(m_simuFrom, m_simuTo, curveValue);
            DoScale(newScale);
            if (m_simuPass > m_simuDuration)
            {
                m_simulating = false;
                OnScaleEnd?.Invoke();
                return;
            }
            // m_simuPass += GameTime.deltaTime;
        }

        void UpdateEvent()
        {
            if (m_simulating) return;
#if UNITY_EDITOR
            var dv = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if(Mathf.Abs(dv) < Mathf.Epsilon)
#else
            if (UnityEngine.Input.touchCount < 2)
#endif
            {

                if (m_scaleOn)
                {
                    m_scaleOn = false;
                    OnScaleEnd?.Invoke();
                }
                return;
            }
            
            if (!m_scaleOn)
            {
                m_scaleOn = true;
#if UNITY_EDITOR            
                CacheOriginalInfoEditor();
#else
                CacheOriginalInfo();
#endif
                OnScaleStart?.Invoke();
            }
            var newScale =
#if UNITY_EDITOR
                
                GetNewScaleEditor(dv);
#else
                GetNewScale();
#endif
            if (LimitScaleRange)
            {
                if (newScale < MinScale)
                {
                    newScale = MinScale;
                    OnScaleReachLimit?.Invoke();
                }

                if (newScale > MaxScale)
                {
                    newScale = MaxScale;
                    OnScaleReachLimit?.Invoke();
                }
            }
            DoScale(newScale);
        }
        void DoScale(float newScale)
        {
            var scaleDelta = newScale - m_oriScale;
            Target.localScale = Vector3.one * newScale;
            var oldZ = m_oriTargetPos.z;
            var newPos = m_oriTargetPos - scaleDelta * (m_posToTouchCenter - m_oriTargetPos)/m_oriScale;
            newPos.z = oldZ;
            if (!Parent)
            {
                Target.position = newPos;
            }
            else
            {
                MarginParent(newPos,newScale);
            }
            OnScaling?.Invoke();
        }
        

        void MarginParent(Vector3 pos,float scale)
        {
            var left = pos.x - m_childSize.x * 0.5 * scale;
            var right = pos.x + m_childSize.x * 0.5 * scale;
            var top = pos.y + m_childSize.y * 0.5 * scale;
            var bottom = pos.y - m_childSize.y * 0.5 * scale;

            var leftDeta = (float)(m_parentMin.x - left);  
            if (leftDeta < 0)
            {
                pos.x -= leftDeta;
            }
            var rightDeta = (float)(m_parentMax.x - right);
            if (rightDeta > 0)
            {
                pos.x += rightDeta;
            }
            
            var topDeta = (float)(m_parentMax.y - top);
            if (topDeta > 0) 
            {
                pos.y += topDeta;
            }
            var bottomDeta = (float)(m_parentMin.y - bottom);
            if (bottomDeta < 0)
            {
                pos.y -= bottomDeta;
            }
            Target.position = pos;
        }

        void CacheOriginalInfo()
        {
            m_oriTargetPos = Target.position;
            var t = UnityEngine.Input.GetTouch(0).position;
            var p = new Vector3(t.x, t.y, m_oriTargetPos.z);
            // m_oriTouch1 = UIManager.Instance.MainCamera.ScreenToWorldPoint(p);
            t = UnityEngine.Input.GetTouch(1).position;
            p = new Vector3(t.x, t.y, m_oriTargetPos.z);
            // m_oriTouch2 = UIManager.Instance.MainCamera.ScreenToWorldPoint(p);
            
            var touchCenterPos = (m_oriTouch1 + m_oriTouch2) / 2;
            m_posToTouchCenter = touchCenterPos - m_oriTargetPos;
            m_oriScale = Target.localScale.x;
        }

        float GetNewScale()
        {
            var t = UnityEngine.Input.GetTouch(0).position;
            var p = new Vector3(t.x, t.y, m_oriTargetPos.z);
            // var touch1 = UIManager.Instance.MainCamera.ScreenToWorldPoint(p);
            t = UnityEngine.Input.GetTouch(1).position;
            p = new Vector3(t.x, t.y, m_oriTargetPos.z);
            // var touch2 = UIManager.Instance.MainCamera.ScreenToWorldPoint(p);
            // var scale = Vector3.Distance(touch1, touch2) / Vector3.Distance(m_oriTouch1, m_oriTouch2);
            // var scaleDelta = (scale - 1) * ScaleSpeed;
            // return m_oriScale + scaleDelta;
            throw new NotImplementedException();
        }

        public void SimulateScale(Vector3 centerPos,float to,float duration)
        {
            if (m_simulating) return;
            m_oriScale = Target.localScale.x;
            m_oriTargetPos = Target.position;
            m_posToTouchCenter = centerPos;

            m_simuFrom = m_oriScale;
            m_simuTo = Math.Min(MaxScale,Math.Max(to,MinScale));
            m_simuPass = 0;
            m_simuDuration = duration;
            m_simulating = true;
            OnScaleStart?.Invoke();
        }
        
#if UNITY_EDITOR
        // private float m_scrollWheelDelta;
        void CacheOriginalInfoEditor()
        {
            m_oriTargetPos = Target.position;
            var pos = UnityEngine.Input.mousePosition;
            pos.z = m_oriTargetPos.z;
            // m_posToTouchCenter = UIManager.Instance.MainCamera.ScreenToWorldPoint(pos);
            m_oriScale = Target.localScale.x;
            // m_scrollWheelDelta = 0;
        }

        float GetNewScaleEditor(float dv)
        {
            // var dv = Input.GetAxis("Mouse ScrollWheel");
            return m_oriScale + dv * ScaleSpeed;
        }
#endif
    }
}
