using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

using UnityEngine.Serialization;

public class ButtonWithImagesChildren : Button
{
    [SerializeField]
    private bool m_playDefaultUiClickSound = true;
    public bool PlayDefaultUIClickSound
    {
        get { return m_playDefaultUiClickSound; }
    }

    private bool m_handleLongPress = false;
    public bool HandleLongPress
    {
        set { m_handleLongPress = value; }
    }
    private bool m_isPressStarted = false;
    private float m_pointDownTime = 0f;
    
     [Tooltip("LongPress logic enables once LongPress event is bound (UI.BindButtonLongPressEvent)")]
    public float m_longPressTimeThreshold = 0.6f;
    private bool m_isLongPress = false;

    [Serializable]
    public class ButtonLongPressEvent : UnityEvent { }
    [SerializeField]
    private ButtonLongPressEvent m_onLongPress = new ButtonLongPressEvent();
    public ButtonLongPressEvent onLongPress
    {
        get { return m_onLongPress; }
        set { m_onLongPress = value; }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        var targetColor =
            state == SelectionState.Disabled ? colors.disabledColor :
            state == SelectionState.Highlighted ? colors.highlightedColor :
            state == SelectionState.Normal ? colors.normalColor :
            state == SelectionState.Pressed ? colors.pressedColor :
            state == SelectionState.Selected ? colors.selectedColor : Color.white;
 
        foreach (var graphic in GetComponentsInChildren<Graphic>())
        {
            graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        }
    }

    void Update()
    {
        if (m_handleLongPress)
        {
            CheckLongPress();
        }
    }

    void CheckLongPress()
    {
        if (m_handleLongPress && m_isPressStarted && !m_isLongPress)
        {
            if (Time.unscaledTime > m_pointDownTime + m_longPressTimeThreshold)
            {
                m_isLongPress = true;
                m_isPressStarted = false;
                if (m_onLongPress != null)
                {
                    m_onLongPress.Invoke();
                }
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (m_handleLongPress)
        {
            m_pointDownTime = Time.unscaledTime;
            m_isPressStarted = true;
            m_isLongPress = false;
        }
    }
 
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (m_handleLongPress)
        {
            m_isPressStarted = false;   
        }
    }
 
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (m_handleLongPress)
        {
            m_isPressStarted = false;
        }
    }  

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!m_handleLongPress || !m_isLongPress)
        {
            base.OnPointerClick(eventData);
        }
    }
}