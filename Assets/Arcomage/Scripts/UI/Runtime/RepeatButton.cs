using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

using UnityEngine.Serialization;

namespace NOAH.UI
{

    public class RepeatButton : Button, IPointerDownHandler, IPointerUpHandler
    {
        public float RepeatInterval = 0.05f;

        public bool IsPressing { get => m_isPressing; }

        private bool m_isPressing;
        private float m_lastTriggerTime = 0;
        

        void Update()
        {
            if (m_isPressing)
            {
                if (Time.realtimeSinceStartup > m_lastTriggerTime + RepeatInterval)
                {
                    sendClick();
                    m_lastTriggerTime = Time.realtimeSinceStartup;
                }
            }
        }

        void sendClick()
        {
            if (onClick != null)
            {
                onClick.Invoke();
            }
        }


        new public void OnPointerDown(PointerEventData eventData)
        {
            m_isPressing = true;
            m_lastTriggerTime = Time.realtimeSinceStartup;
        }

        new public void OnPointerUp(PointerEventData eventData)
        {
            m_isPressing = false;
            m_lastTriggerTime = Time.realtimeSinceStartup;
        }

    }
}
