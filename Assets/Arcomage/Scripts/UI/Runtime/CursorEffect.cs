using System;
// using GamePlay;
// using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace NOAH.UI
{
    public class CursorEffect : MonoBehaviour
    {
        public Slider HoldSlider;
        public float HoldTime;
        public bool HoldLeft = true;
        public bool FightCamera = false;
        public Vector3 OffsetPos = Vector3.zero;
        public bool Auto = false;

        public float DelayShow = 0.5f;
        // private bool m_press = false;
        private float m_pressPassTime = -1;

        private void OnEnable()
        {
            HoldSlider.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Auto)
            {
                // var controller = ReInput.controllers.GetLastActiveController();
                // if (controller.type != ControllerType.Mouse)
                // {
                //     Stop();
                //     return;
                // }

                // if (HoldLeft && controller.GetButtonDown(0))
                // {
                //     if (!m_press)
                //     {
                //         Play();
                //     }
                //
                //     return;
                // }
                //
                // if (HoldLeft && controller.GetButtonUp(0))
                // {
                //     Stop();
                //     return;
                // }
            }

            if (m_pressPassTime >= 0)
            {
                var pos = GetCursorWorldPos();
                HoldSlider.transform.position = pos + OffsetPos;
                // m_pressPassTime += GameTime.deltaTime;
                HoldSlider.value = Math.Min(1, m_pressPassTime / HoldTime);
                if (m_pressPassTime >= DelayShow)
                {
                    HoldSlider.gameObject.SetActive(true);
                }
            }

            if (m_pressPassTime >= HoldTime)
            {
                m_pressPassTime = -1;
                HoldSlider.gameObject.SetActive(false);
            }
        }

        Vector3 GetCursorWorldPos()
        {
            throw new NotImplementedException();
            // Vector3 mousePosition = ReInput.controllers.Mouse.screenPosition;
            // var cam = FightCamera ? CameraDirector.Cur.MainCamera : UIManager.Instance.MainCamera;
            // var z = transform.position.z;
            // mousePosition.z = z - cam.transform.position.z;
            // mousePosition = cam.ScreenToWorldPoint(mousePosition);
            // return new Vector3(mousePosition.x, mousePosition.y, z);
        }

        public void Play(float holdTime, bool fightCamera, float delayShow)
        {
            HoldTime = holdTime;
            FightCamera = fightCamera;
            DelayShow = delayShow;

            Play();
        }

        public void Play()
        {
            var pos = GetCursorWorldPos();
            HoldSlider.transform.position = pos + OffsetPos;
            // HoldSlider.gameObject.SetActive(true);
            HoldSlider.value = 0;
            m_pressPassTime = 0;
            // m_press = true;
        }

        public void Stop()
        {
            // m_press = false;
            HoldSlider.gameObject.SetActive(false);
            m_pressPassTime = -1;
        }
    }
}
