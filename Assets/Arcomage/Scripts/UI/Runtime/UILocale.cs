// using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NOAH.UI
{
    [DisallowMultipleComponent]
    public class UILocale : MonoBehaviour
    {
        public string m_localeKey = "";

        public bool m_WordWrapWithBudouX = false;
        
        //专门改到Awake的，为了避免uilocale对text的赋值覆盖了代码里的赋值
        void Awake()
        {
            string text = null;
            // if (!string.IsNullOrEmpty(m_localeKey))
            // {
            //     text = LocalizationManager.Instance.GetString(m_localeKey.Trim());
            // }

            if (text != null)
            {
                var uitext = GetComponent<Text>();
                if (uitext != null) uitext.text = text;

                var tmptext = GetComponent<TMP_Text>();
                if (tmptext != null)
                {
                    if (m_WordWrapWithBudouX)
                    {
                        tmptext.SetBudouXText(text);
                    }
                    else
                    {
                        // if (tmptext is RubyTextMeshPro tmp)
                        // {
                        //     text = tmp.ReplaceRubyTags(text);
                        // }
                        // else if (tmptext is RubyTextMeshProUGUI tmpUGUI)
                        // {
                        //     text = tmpUGUI.ReplaceRubyTags(text);
                        // }
                        
                        tmptext.text = text;
                    }
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                var uitext = GetComponent<Text>();
                if (uitext != null)
                {
#if CI_MODE
                    uitext.text = "";
#else
                    // uitext.text = m_localeKey;
#endif
                }

                var tmptext = GetComponent<TMP_Text>();
                if (tmptext != null)
                {
#if CI_MODE
                tmptext.text = "";
#else
                    // tmptext.text = m_localeKey;
#endif
                }
            }
        }
#endif
    }
}
