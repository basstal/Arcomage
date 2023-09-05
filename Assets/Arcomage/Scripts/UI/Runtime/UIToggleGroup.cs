
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace NOAH.UI
{

    public class UIToggleGroup : MonoBehaviour
    {
        [Serializable]
        public class UIToggleGroupChangeEvent : UnityEvent { }

        [SerializeField]
        UIToggleGroupChangeEvent m_onChange = new UIToggleGroupChangeEvent();
        public UIToggleGroupChangeEvent OnChange
        {
            get => m_onChange;
            set => m_onChange = value;
        }
        
        Toggle m_curToggle = null;
        List<Toggle> m_toggles = new List<Toggle>();
        public int Count  => m_toggles.Count;
        public Toggle this[int i]
        {
            get
            {
                if (i < 0 || i >= m_toggles.Count)
                    return null;
                return m_toggles[i];
            }
        }
        public int CurSelectionIndex => m_toggles.IndexOf(m_curToggle); 
        public Toggle CurSelection  => m_curToggle;

        bool m_silent = false;
        // bool m_sendEvent = true;

        void Awake()
        {
            InitToggles();
        }
        
        void Start()
        {
        }

        public void InitToggles()
        {
            m_silent = true;
            m_toggles.Clear();
            foreach (Transform t in transform)
            {
                if(!t.gameObject.activeSelf) continue;
                if(!t.TryGetComponent<Toggle>(out var toggle)) continue;
                m_toggles.Add(toggle);
                toggle.isOn = false;
                toggle.enabled = true;
                BindToggleEvent(toggle);
            }

            m_silent = false;
        }
    
        void BindToggleEvent(Toggle t)
        {
            var eventCount = t.onValueChanged.GetPersistentEventCount();
            for (var i = eventCount - 1; i >= 0; i--)
            {
                if (t.onValueChanged.GetPersistentTarget(i) == this) return;
            }
            t.onValueChanged.AddListener(delegate { OnChildToggleChange(t); });
        }

        void OnChildToggleChange(Toggle t)
        {
            if (m_silent) return;
            if (t == null) return;
            if (m_curToggle == t)
            {
                if (t.isOn == false)
                {
                    m_curToggle.isOn = true;
                }
                return;
            }
            if(!m_toggles.Contains(t)) return;
            if (!t.isOn) return;
            
            m_curToggle = t;
            foreach (var childT in m_toggles)
            {
                
                if ((childT != t) && childT.isOn)
                {
                    childT.isOn = false;
                    childT.enabled = true;
                }
            }
            t.enabled = false;
            OnChange.Invoke();
        }

        public void SetSelection(Toggle t, bool force)
        {
            if (t != m_curToggle)
            {
                t.isOn = true;
                return;
            }
            if (force)
            {
                OnChange.Invoke();
            }
        }
        
        public void SetSelectionIndex(int index, bool force)
        {
            SetSelection(this[index], force);
        }
        
    }

}