using System;
using Localization;
using TMPro;
using UnityEngine;

namespace Arcomage.GameScripts.UI.Runtime
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocaledText : MonoBehaviour
    {
        public string key;
        public Func<object[]> resolveParameters;

        private TextMeshProUGUI textMeshProUGUI;

        private void Start()
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            Refresh();
        }

        public void Refresh()
        {
            if (resolveParameters != null)
            {
                SetTextFormat(key, resolveParameters);
            }
            else
            {
                SetText(key);
            }
        }


        public void SetText(string inKey)
        {
            key = inKey;
            if (key != null)
            {
                textMeshProUGUI.text = LocalizationManager.Instance.GetString(key);
            }
        }


        public void SetTextFormat(string inKey, Func<object[]> inResolveParameters)
        {
            key = inKey;
            resolveParameters = inResolveParameters;
            if (key != null)
            {
                textMeshProUGUI.text = LocalizationManager.Instance.GetStringFormat(key, resolveParameters?.Invoke());
            }
        }
    }
}