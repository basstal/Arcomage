using Localization;
using TMPro;
using UnityEngine;

namespace Arcomage.GameScripts.UI.Runtime
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocaledText : MonoBehaviour
    {
        [SerializeField] private string key;
        private string _text;

        public string text
        {
            get => _text;
            set
            {
                _text = value;
                var textMeshProUGUI = GetComponent<TextMeshProUGUI>();
                textMeshProUGUI.text = _text;
            }
        }

        private void Start()
        {
            text = LocalizationManager.Instance.GetString(key);
        }
    }
}