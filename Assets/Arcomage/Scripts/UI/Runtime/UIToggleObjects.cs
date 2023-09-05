using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using NOAH.Criware;
namespace NOAH.UI
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggleObjects : MonoBehaviour
    {
        public List<GameObject> m_activate;
        public List<GameObject> m_deactivate;
        public Color m_activeColor;
        public Color m_deactiveColor;
        public List<Graphic> coloredGraphics;

        Toggle m_toggle;

        void Awake()
        {
            m_toggle = GetComponent<Toggle>();
            m_toggle.onValueChanged.AddListener(Toggle);

            if (m_toggle.group == null)
            {
                var group = GetComponentInParent<ToggleGroup>();
                if (group != null)
                    m_toggle.group = group;
            }
        }

        void Start()
        {
            ToggleSilent(m_toggle.isOn);
        }

        void Toggle(bool val)
        {
            if (!enabled) return;

            // if (TryGetComponent<CriwareAudioAssist>(out var criAudioObject))
            // {
            //     criAudioObject.PlaySound("Click");
            // }
            ToggleImp(val);
        }

        void ToggleSilent(bool val)
        {
            if (!enabled) return;
            ToggleImp(val);
        }

        void ToggleImp(bool val)
        {
            foreach (var g in m_activate)
                g.SetActive(val);

            foreach (var g in m_deactivate)
                g.SetActive(!val);

            foreach (var g in coloredGraphics)
                g.color = val ? m_activeColor : m_deactiveColor;
        }

        public void Sync()
        {
            Toggle(m_toggle.isOn);
        }
    }
}
