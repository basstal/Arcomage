using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using System;
using TMPro;
using Whiterice;

// using NOAH.Native;

namespace NOAH.UI
{
public class UISetPositionByKeyboardHeight : MonoBehaviour
{
    public RectTransform m_target;
    private Vector3 m_targetLocalPosition;
    public float m_offset;
    private TMP_InputField m_inputField;
    private TouchScreenKeyboard.Status m_status;
    private RectTransform m_cavasRootRectTransform;
    
    void Awake()
    {
        m_inputField = GetComponent<TMP_InputField>();
        m_status = TouchScreenKeyboard.Status.Done;
        m_inputField.onTouchScreenKeyboardStatusChanged.AddListener(OnTouchScreenKeyboardStatusChanged);
        // m_cavasRootRectTransform = UIManager.Instance.CanvasRoot.gameObject.GetComponent<RectTransform>();
        m_targetLocalPosition = m_target.localPosition;
    }

    void LateUpdate()
    {
        if (m_status == TouchScreenKeyboard.Status.Visible)
        {
            float keyboardHeight = NativeInterface.External_GetKeyboardHeight();
            // NOAH.Debug.LogTool.LogWarning("UISetPositionByKeyboardHeight", "keyboardHeight: " + keyboardHeight + ", " + Display.main.systemHeight + ", " + Screen.height);
            keyboardHeight = keyboardHeight / Screen.height * m_cavasRootRectTransform.rect.height;
            // NOAH.Debug.LogTool.LogWarning("UISetPositionByKeyboardHeight", "2 keyboardHeight: " + keyboardHeight);

            m_target.localPosition = m_targetLocalPosition;
            Vector3 inputFieldPosition = m_cavasRootRectTransform.InverseTransformPoint(transform.position);
            float inputFieldHeight = inputFieldPosition.y - m_offset + m_cavasRootRectTransform.rect.height * 0.5f;
            if (keyboardHeight > inputFieldHeight)
            {
                m_target.localPosition = m_target.localPosition + new Vector3(0, keyboardHeight - inputFieldHeight, 0);
                // NOAH.Debug.LogTool.LogWarning("UISetPositionByKeyboardHeight", "m_target.localPosition: " + m_target.localPosition);
            }
        }
    }

    void OnTouchScreenKeyboardStatusChanged(TouchScreenKeyboard.Status status)
    {
        m_status = status;

        if (m_status != TouchScreenKeyboard.Status.Visible)
        {
            m_target.localPosition = m_targetLocalPosition;
        }
        else
        {
            LateUpdate();
        }
    }

    void OnDestroy()
    {
        m_inputField.onTouchScreenKeyboardStatusChanged.RemoveListener(OnTouchScreenKeyboardStatusChanged);
    }
}
}