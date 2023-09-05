using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class UIKeyResponser : MonoBehaviour
    {
        public KeyCode m_keyCode = KeyCode.Escape;

        void OnEnable()
        {
            // TODO: 
            // if (UIManager.Instance != null) UIManager.Instance.RegisterKeyResponse(this);
        }

        void OnDisable()
        {
            // TODO:
            // if (UIManager.Instance != null) UIManager.Instance.UnregisterKeyResponse(this);
        }

        public void Trigger()
        {
            SendMessage("OnKey", m_keyCode, SendMessageOptions.DontRequireReceiver);
            ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        public void SetKeyCode(KeyCode kc)
        {
            m_keyCode = kc;
        }
    }
}
