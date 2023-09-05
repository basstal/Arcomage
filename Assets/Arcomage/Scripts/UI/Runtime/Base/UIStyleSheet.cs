using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// using NOAH.Debug;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace NOAH.UI
{
    public class UIStyleSheet : MonoBehaviour
    {
        [Serializable]
        public class StyleTierConfig
        {
            public float dpiUpperBound;
            public int tier;
        }

        [Serializable]
        public class StyleTierConfigEntry
        {
            public List<string> languages;
            [TableList]
            public List<StyleTierConfig> styleTierConfig = new List<StyleTierConfig>();
        }
        
        private static readonly FieldInfo StyleListFiledInfo = typeof(TMP_StyleSheet).GetField("m_StyleList", BindingFlags.NonPublic | BindingFlags.Instance);

        [SerializeField] private List<TMP_StyleSheet> m_styleSheetList;

        [SerializeField] [TableList] private List<StyleTierConfigEntry> m_styleTierConfigEntries = new List<StyleTierConfigEntry>();

        private Dictionary<string, List<TMP_Style>> m_styleTiersDict = new Dictionary<string, List<TMP_Style>>();

        private List<TMP_Style> m_currentStyleList = null;

        public void Init()
        {
            InitStyleTiers();

            LoadStyleSettings();
        }

        private void InitStyleTiers()
        {
            foreach (var styleSheet in m_styleSheetList)
            {
                if (styleSheet == null)
                {
                    // LogTool.LogError("UI", $"Invalid style sheet encountered!");
                    continue;
                }
                
                var styleList = StyleListFiledInfo.GetValue(styleSheet) as List<TMP_Style>;
                if (styleList?.Count > 0)
                {
                    foreach (var style in styleList)
                    {
                        if (style.name == "Normal") continue;

                        if (!m_styleTiersDict.TryGetValue(style.name, out var styleTiers))
                        {
                            styleTiers = new List<TMP_Style>();
                            m_styleTiersDict.Add(style.name, styleTiers);
                        }

                        styleTiers.Add(style);
                    }
                }
            }
        }

        private void LoadStyleSettings()
        {
            // var entry = m_styleTierConfigEntries.Find(e => e.languages.Contains(SettingsManager.Instance.MiscSettings.Current.Language));
            // if (entry == null)
            // {
            //     entry = m_styleTierConfigEntries.Find(e => e.languages.Contains("default"));
            // }
            // var suggestedTierIndex = Screen.dpi > 0 ? entry.styleTierConfig.First(c => Screen.dpi <= c.dpiUpperBound).tier : 0;
            //
            // // LogTool.LogDebug("Debug", $"Screen DPI {Screen.dpi}, suggested tier {suggestedTierIndex}");
            //
            // bool settingsChanged = false;
            
            // var styleSettings = SettingsManager.Instance.MiscSettings.Current.StyleSettings;
            // foreach (var styleTiersPair in m_styleTiersDict)
            // {
            //     if (!styleSettings.TryGetValue(styleTiersPair.Key, out var tierIndex))
            //     {
            //         tierIndex = suggestedTierIndex;
            //         styleSettings.Add(styleTiersPair.Key, tierIndex);
            //         settingsChanged = true;
            //     }
            //
            //     if (!(0 <= tierIndex && tierIndex < styleTiersPair.Value.Count)) continue;
            //
            //     var style = styleTiersPair.Value[tierIndex];
            //
            //     // LogTool.LogDebug("Debug", $"Apply tier {tierIndex} to style {styleTiersPair.Key}");
            //     
            //     ApplyStyle(style);
            // }
            //
            // if (settingsChanged)
            // {
            //     SettingsManager.Instance.MiscSettings.Save();
            // }
        }

        public void ApplyStyle(TMP_Style style)
        {
            var styleList = StyleListFiledInfo.GetValue(TMP_Settings.defaultStyleSheet) as List<TMP_Style>;
            if (m_currentStyleList == null)
            {
                m_currentStyleList = new List<TMP_Style>(styleList);
            }

            var targetIndex = m_currentStyleList.FindIndex(s => s.name == style.name);

            if (targetIndex >= 0)
            {
                m_currentStyleList[targetIndex] = style;
                // 使用修改后的StyleList进行刷新
                StyleListFiledInfo.SetValue(TMP_Settings.defaultStyleSheet, m_currentStyleList);
                TMP_Settings.defaultStyleSheet.RefreshStyles();

                // 因为我们修改的是ScriptableObject，所以需要回滚StyleList
                StyleListFiledInfo.SetValue(TMP_Settings.defaultStyleSheet, styleList);

                // not working on release build.
                // Canvas.ForceUpdateCanvases();

                // ui flicker
                // var canvasScalerList = FindObjectsOfType<CanvasScaler>();
                // foreach (var canvasScaler in canvasScalerList)
                // {
                //     if (canvasScaler.gameObject.activeInHierarchy)
                //     {
                //         canvasScaler.gameObject.SetActive(false);
                //         canvasScaler.gameObject.SetActive(true);
                //     }
                // }
            }
        }

        public List<string> GetStyleNames()
        {
            return m_styleTiersDict.Keys.ToList();
        }

        public List<TMP_Style> GetStyleTiers(string styleName)
        {
            m_styleTiersDict.TryGetValue(styleName, out var result);

            return result;
        }
    }
}
