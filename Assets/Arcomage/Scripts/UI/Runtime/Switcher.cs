using System.Collections.Generic;
// using NOAH.Criware;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    public class Switcher : Selectable
    {
        public enum SwitchAxis
        {
            Horizontal = 0,
            Vertical = 1
        }

        [SerializeField] private SwitchAxis m_axis = SwitchAxis.Horizontal;

        [SerializeField] private TextMeshProUGUI m_text;

        [SerializeField] private int m_value = 0;

        [SerializeField] private List<string> m_options = new List<string>();

        [SerializeField] private Button m_previous;

        [SerializeField] private Button m_next;

        [SerializeField] private PageIndicator m_indicator;

        public class SwitchEvent : UnityEvent<int>
        {
        }

        [SerializeField] private SwitchEvent m_onValueChanged = new SwitchEvent();

        public SwitchEvent onValueChanged { get { return m_onValueChanged; } set { m_onValueChanged = value; } }

        public SwitchAxis Axis => m_axis;

        public int Value
        {
            get => m_value;
            set
            {
                Set(value);
            }
        }

        public List<string> Options
        {
            get => m_options;
            set
            {
                m_options = value;
                Refresh();
            }
        }

        protected override void Start()
        {
            base.Start();

            if (m_previous != null)
            {
                m_previous.gameObject.BindButtonEvent(() => Switch(-1));
                m_previous.gameObject.BindPressEvent((e) =>
                {
                    // 模拟一次按下和抬起，以触发选中
                    gameObject.PointerDown();
                    gameObject.PointerUp();
                });
            }

            if (m_next != null)
            {
                m_next.gameObject.BindButtonEvent(() => Switch(1));
                m_next.gameObject.BindPressEvent((e) =>
                {
                    // 模拟一次按下和抬起，以触发选中
                    gameObject.PointerDown();
                    gameObject.PointerUp();
                });
            }

            Refresh();
        }

        public void AddOptions(List<string> options)
        {
            m_options.AddRange(options);

            Refresh();
        }

        public void ClearOptions()
        {
            m_options.Clear();
            m_value = 0;

            Refresh();
        }

        public new bool interactable
        {
            get => base.interactable;
            set
            {
                m_previous.interactable = value;
                m_next.interactable = value;
                base.interactable = value;
            }
        }

        public void Switch(int offset)
        {
            if (!IsActive() || !IsInteractable()) return;

            // var criwareAudioAssist = GetComponent<CriwareAudioAssist>();
            // if (criwareAudioAssist != null)
            // {
            //     criwareAudioAssist.PlaySound("Switch");
            // }

            if (m_options.Count > 0)
            {
                Set((m_value + offset + m_options.Count) % m_options.Count);
            }
        }

        private void Set(int value, bool sendCallback = true)
        {
            if (Application.isPlaying && (value == m_value || Options.Count == 0))
                return;

            m_value = Mathf.Clamp(value, 0, Options.Count - 1);
            Refresh();

            if (sendCallback)
            {
                m_onValueChanged.Invoke(m_value);
            }
        }

        private void Refresh()
        {
            var clampedValue = Mathf.Clamp(m_value, 0, m_options.Count);
            var option = m_options.Count > 0 ? m_options[clampedValue] : null;

            m_text.text = option ?? string.Empty;

            if (m_indicator != null)
            {
                m_indicator.PageCount = m_options.Count;
                m_indicator.HighlightIndex = clampedValue;
            }
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnMove(eventData);
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (m_axis == SwitchAxis.Horizontal && FindSelectableOnLeft() == null)
                        Switch(-1);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (m_axis == SwitchAxis.Horizontal && FindSelectableOnRight() == null)
                        Switch(1);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (m_axis == SwitchAxis.Vertical && FindSelectableOnUp() == null)
                        Switch(-1);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (m_axis == SwitchAxis.Vertical && FindSelectableOnDown() == null)
                        Switch(1);
                    else
                        base.OnMove(eventData);
                    break;
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            // 避免通过Update每帧检测来判断interactable状态的变化，通过state变化来间接的获得interactable状态的变化
            if (m_previous != null) m_previous.interactable = state != SelectionState.Disabled;
            if (m_next != null) m_next.interactable = state != SelectionState.Disabled;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            Refresh();
        }
#endif
    }
}
