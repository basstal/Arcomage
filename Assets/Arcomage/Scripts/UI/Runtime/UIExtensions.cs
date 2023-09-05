using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
// using NOAH.Core;
// using NOAH.Criware;
using System.Linq;
using Arcomage.GameScripts.Utils;
// using GamePlay;
// using NOAH.Debug;
// using Localization;
using TMPro;

namespace NOAH.UI
{
    public static class UIExtensions
    {
        public static string GetRoute(this Transform transform, string splitter = "/")
        {
            var result = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                result = $"{parent.name}{splitter}{result}";
                parent = parent.parent;
            }

            return result;
        }

        public static int GetCharCount(this TMPro.TMP_Text cmptText)
        {
            if (cmptText == null) return 0;
            return cmptText.textInfo.characterCount;
        }
        
         public static int GetWordCount(this TMPro.TMP_Text cmptText)
        {
            if (cmptText == null) return 0;
            return cmptText.textInfo.wordCount;
        }

        public static void SetGraphicRaycastTarget(this GameObject target, bool value)
        {
            if (target != null)
            {
                var graphic = target.GetComponent<Graphic>();
                if (graphic != null)
                {
                    graphic.raycastTarget = value;
                }
            }
        }

        public static void ClickButton(this GameObject button)
        {
            ExecuteEvents.Execute(button, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        public static void PointerDown(this GameObject button)
        {
            ExecuteEvents.Execute(button, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
        }

        public static void PointerUp(this GameObject button)
        {
            ExecuteEvents.Execute(button, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
        }

        public static void BindEventHandler<T>(this GameObject target, UnityAction<BaseEventData> callback, bool requireGraphic = true) where T : NOAH.UI.EventHandler
        {
            if (target != null)
            {
                var graphic = target.GetComponent<Graphic>();
                if (graphic != null)
                {
                    target.GetOrAddComponent<T>().Callback = callback;

                    graphic.raycastTarget = true;
                }
                else if (requireGraphic)
                {
                    // LogTool.LogWarning("UI", $"Try to BindEventHandler {typeof(T)} to {target.name} without any Graphic Component.", target);
                }
            }
        }

        // bind button event for ButtonAction
        private static void BindButtonActionEvent(this GameObject target, UnityAction callback)
        {
            if (target != null)
            {
                // void WrappedCallback()
                // {
                //     // if (target.TryGetComponent<CriwareAudioAssist>(out var audioData))
                //     // {
                //     //     audioData.PlaySound("Click");
                //     // }
                //
                //     callback();
                // }

                // var buttonAction = target.GetComponent<ButtonAction>();
                // if (buttonAction != null) UIUtility.SetEventHandler(buttonAction.OnClick, WrappedCallback);
                //
                // var buttonAny = target.GetComponent<ButtonAny>();
                // if (buttonAny != null) UIUtility.SetEventHandler(buttonAny.OnClick, WrappedCallback);
            }
        }

        // bind button event for UI.Button
        private static void BindButtonPointerEvent(this GameObject target, UnityAction callback)
        {
            if (target != null)
            {
                var graphic = target.GetComponent<Graphic>();
                if (graphic != null)
                {
                    graphic.raycastTarget = true;

                    Button button = target.GetOrAddComponent<Button>();

                    // if (target.GetComponent<NavigationConfig>() != null || button.navigation.mode != Navigation.Mode.None)
                    // {
                    //     target.GetOrAddComponent<SelectOnPress>();
                    // }

                    void WrappedCallback()
                    {
                        // if (target.TryGetComponent<CriwareAudioAssist>(out var audioData))
                        // {
                        //     audioData.PlaySound("Click");
                        // }

                        callback?.Invoke();
                    }

                    UIUtility.SetEventHandler(button.onClick, WrappedCallback);
                }
                else
                {
                    // if (target.GetComponent<ButtonAction>() == null && target.GetComponent<ButtonAny>() == null)
                    // {
                    //     LogTool.LogWarning("UI", $"Try to BindButtonEvent to {target.name} without any Graphic Component.", target);
                    // }
                }
            }
        }

        public static void BindButtonEvent(this GameObject target, UnityAction callback)
        {
            BindButtonActionEvent(target, callback);
            BindButtonPointerEvent(target, callback);
        }

        public static void BindButtonHoldEvent(this GameObject target, UnityAction callback)
        {
            // void PressCallback()
            // {
            //     // if (target.TryGetComponent<CriwareAudioAssist>(out var audioData))
            //     // {
            //     //     audioData.PlaySound("HoldStart");
            //     // }
            // }
            //
            // void ReleaseCallback()
            // {
            //     // if (target.TryGetComponent<CriwareAudioAssist>(out var audioData))
            //     // {
            //     //     audioData.StopSound("HoldStart");
            //     // }
            // }
            //
            // void HoldCallback()
            // {
            //     // if (target.TryGetComponent<CriwareAudioAssist>(out var audioData))
            //     // {
            //     //     audioData.StopSound("HoldStart");
            //     //     audioData.PlaySound("HoldEnd");
            //     // }
            //
            //     callback?.Invoke();
            // }

            // var buttonAction = target.GetComponent<ButtonAction>();
            // if (buttonAction != null)
            // {
            //     UIUtility.SetEventHandler(buttonAction.OnPress, PressCallback);
            //     UIUtility.SetEventHandler(buttonAction.OnRelease, ReleaseCallback);
            //     UIUtility.SetEventHandler(buttonAction.OnHold, HoldCallback);
            // }

            // var buttonAny = target.GetComponent<ButtonAny>();
            // if (buttonAny != null)
            // {
            //     UIUtility.SetEventHandler(buttonAny.OnPress, PressCallback);
            //     UIUtility.SetEventHandler(buttonAny.OnRelease, ReleaseCallback);
            //     UIUtility.SetEventHandler(buttonAny.OnHold, HoldCallback);
            // }
        }

        public static void BindBeginDragEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<BeginDragHandler>(callback);
        }

        public static void BindDragEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<DragHandler>(callback);
        }

        public static void BindEndDragEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<EndDragHandler>(callback);
        }

        public static void BindPressEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            // void WrappedCallback() => callback(new PointerEventData(EventSystem.current));

            // var buttonAction = target.GetComponent<ButtonAction>();
            // if (buttonAction != null) UIUtility.SetEventHandler(buttonAction.OnPress, WrappedCallback);
            //
            // var buttonAny = target.GetComponent<ButtonAny>();
            // if (buttonAny != null) UIUtility.SetEventHandler(buttonAny.OnPress, WrappedCallback);

            // target.BindEventHandler<PointerDownHandler>(callback, buttonAction == null && buttonAny == null);
        }

        public static void BindReleaseEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            // void WrappedCallback() => callback(new PointerEventData(EventSystem.current));

            // var buttonAction = target.GetComponent<ButtonAction>();
            // if (buttonAction != null) UIUtility.SetEventHandler(buttonAction.OnRelease, WrappedCallback);

            // var buttonAny = target.GetComponent<ButtonAny>();
            // if (buttonAny != null) UIUtility.SetEventHandler(buttonAny.OnRelease, WrappedCallback);

            // target.BindEventHandler<PointerUpHandler>(callback, buttonAction == null && buttonAny == null);
        }

        public static void BindClickEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            // void WrappedCallback() => callback(new PointerEventData(EventSystem.current));

            // var buttonAction = target.GetComponent<ButtonAction>();
            // if (buttonAction != null) UIUtility.SetEventHandler(buttonAction.OnClick, WrappedCallback);
            //
            // var buttonAny = target.GetComponent<ButtonAny>();
            // if (buttonAny != null) UIUtility.SetEventHandler(buttonAny.OnClick, WrappedCallback);

            // target.BindEventHandler<PointerClickHandler>(callback, buttonAction == null && buttonAny == null);
        }

        public static void BindSelectEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<SelectHandler>(callback);
        }

        public static void BindDeselectEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<DeselectHandler>(callback);
        }

        public static void BindPointerEnterEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<PointerEnterHandler>(callback);
        }

        public static void BindPointerExitEvent(this GameObject target, UnityAction<BaseEventData> callback)
        {
            target.BindEventHandler<PointerExitHandler>(callback);
        }

        public static void BindAxisEvent(this GameObject target, UnityAction<float> callback)
        {
            // var axisAction = target.GetComponent<AxisAction>();
            // if (axisAction != null)
            // {
            //     axisAction.OnChange.RemoveAllListeners();
            //     axisAction.OnChange.AddListener(callback);
            // }
        }

        public static void BindAxis2DEvent(this GameObject target, UnityAction<Vector2> callback)
        {
            // var axis2DAction = target.GetComponent<Axis2DAction>();
            // if (axis2DAction != null)
            // {
            //     axis2DAction.OnChange.RemoveAllListeners();
            //     axis2DAction.OnChange.AddListener(callback);
            // }
        }

        public static void BindSwitchEvent(this GameObject target, UnityAction<int> callback)
        {
            var switcher = target.GetComponent<Switcher>();
            if (switcher != null)
            {
                switcher.onValueChanged.RemoveAllListeners();
                switcher.onValueChanged.AddListener(callback);
            }
        }

        public static void BindKeyEvent(this GameObject target, UnityAction callback)
        {
            throw new System.NotImplementedException();
        }

        public static bool ToggleCanvasRendering(this GameObject go, bool on)
        {
            bool result = false;
            if (go != null)
            {
                var canvas = go.GetComponent<Canvas>();
                if (canvas != null)
                {
                    result = true;

                    foreach (var childCanvas in go.GetComponentsInChildren<Canvas>())
                    {
                        if (childCanvas != null)
                        {
                            // refer to https://unity3d.com/cn/learn/tutorials/topics/best-practices/other-ui-optimization-techniques-and-tips at section "Disabling Canvases" for more details
                            childCanvas.enabled = on;
                        }
                    }
                }
            }

            return result;
        }

        public static void SetEnabled<T>(this GameObject target, bool value) where T : EventHandler
        {
            T eventHandler = target.GetComponent<T>();
            if (eventHandler != null) eventHandler.enabled = value;
        }

        public static void SetInteractable<T>(this GameObject target, bool value) where T : Selectable
        {
            T selectable = target.GetComponent<T>();
            if (selectable != null) selectable.interactable = value;
        }

        public static void ToggleRendering(this GameObject go, bool on)
        {
            if (!ToggleCanvasRendering(go, on))
            {
                if (go != null)
                {
                    if (!go.activeSelf && on || go.activeSelf && !on)
                    {
                        go.SetActive(on);
                    }
                }
            }
        }

        public static bool IsRendering(this GameObject go)
        {
            var result = false;

            if (go.activeInHierarchy)
            {
                var canvas = go.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.enabled)
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool RectOverlaps(this RectTransform rectTrans1, RectTransform rectTrans2)
        {
            Vector3[] worldCorner1 = new Vector3[4];
            Vector3[] worldCorner2 = new Vector3[4];
            rectTrans1.GetWorldCorners(worldCorner1);
            rectTrans2.GetWorldCorners(worldCorner2);
            float minX1 = Mathf.Min(worldCorner1[0].x, worldCorner1[1].x, worldCorner1[2].x, worldCorner1[3].x);
            float minX2 = Mathf.Min(worldCorner2[0].x, worldCorner2[1].x, worldCorner2[2].x, worldCorner2[3].x);
            float maxX1 = Mathf.Max(worldCorner1[0].x, worldCorner1[1].x, worldCorner1[2].x, worldCorner1[3].x);
            float maxX2 = Mathf.Max(worldCorner2[0].x, worldCorner2[1].x, worldCorner2[2].x, worldCorner2[3].x);
            float minY1 = Mathf.Min(worldCorner1[0].y, worldCorner1[1].y, worldCorner1[2].y, worldCorner1[3].y);
            float minY2 = Mathf.Min(worldCorner2[0].y, worldCorner2[1].y, worldCorner2[2].y, worldCorner2[3].y);
            float maxY1 = Mathf.Max(worldCorner1[0].y, worldCorner1[1].y, worldCorner1[2].y, worldCorner1[3].y);
            float maxY2 = Mathf.Max(worldCorner2[0].y, worldCorner2[1].y, worldCorner2[2].y, worldCorner2[3].y);
            return minX1 < maxX2 && maxX1 > minX2 && minY2 < maxY1 && maxY2 > minY1;
        }

        public static Toggle[] GetActiveToggles(this ToggleGroup toggleGroup)
        {
            return toggleGroup.ActiveToggles().ToArray();
        }

        public static Toggle FirstActiveToggle(this ToggleGroup toggleGroup)
        {
            var activeToggles = toggleGroup.ActiveToggles().ToArray();
            return activeToggles.Length > 0 ? activeToggles[0] : null;
        }

        public static Material GetInstanceMaterial(this Graphic graphic)
        {
            var material = graphic.material;

            if (material != null && !material.HasProperty("__INSTANCE"))
            {
                material = new Material(material)
                {
#if UNITY_EDITOR
                    name = material.name + "(Instance)"
#endif
                };
                material.SetFloat("__INSTANCE", 0);
                graphic.material = material;
            }

            return material;
        }

        public static void ReserveChildren(this Transform trans, Transform childTemplate, int count, Action<Transform, int> callback = null)
        {
            if (trans.childCount > 0)
            {
                for (var i = trans.childCount; i < count; i++)
                {
                    GameObject.Instantiate(childTemplate != null ? childTemplate : trans.GetChild(0), trans);
                }

                for (var i = 0; i < trans.childCount; i++)
                {
                    trans.GetChild(i).gameObject.SetActive(i < count);
                }

                if (callback != null)
                {
                    for (int i = 0; i < count; i++)
                    {
                        callback(trans.GetChild(i), i);
                    }
                }
            }
        }

        public static void SetBudouXText(this TMP_Text textComp, string text, bool instant = false)
        {
            var budouText = text;

            // if (LocalizationManager.Instance != null)
            // {
            //     budouText = LocalizationManager.Instance.BudouXlize(text);
            // }

            var preFontSize = textComp.fontSize;
            // if (textComp is RubyTextMeshPro tmp)
            // {
            //     var rubyText = tmp.ReplaceRubyTags(budouText);
            //     if (rubyText != budouText && textComp.enableAutoSizing) // 对有注音的文本，如果Text组件开启了自适应字号
            //     {
            //         textComp.GetTextInfo(rubyText); // 将转换后的文本更新到文本组件，强制立即刷新，使字号自适应
            //         if (!Mathf.Approximately(preFontSize, textComp.fontSize)) // 如果字号发生了变化，需要重新计算注音文本
            //         {
            //             rubyText = tmp.ReplaceRubyTags(budouText);
            //         }
            //     }
            //
            //     budouText = rubyText;
            // }
            // else if (textComp is RubyTextMeshProUGUI tmpUGUI)
            // {
            //     var rubyText = tmpUGUI.ReplaceRubyTags(budouText);
            //     if (rubyText != budouText && textComp.enableAutoSizing)
            //     {
            //         textComp.GetTextInfo(rubyText);
            //         if (!Mathf.Approximately(preFontSize, textComp.fontSize))
            //         {
            //             rubyText = tmpUGUI.ReplaceRubyTags(budouText);
            //         }
            //     }
            //
            //     budouText = rubyText;
            // }

            if (instant)
            {
                textComp.GetTextInfo(budouText);
            }
            else
            {
                textComp.text = budouText;
            }
        }
    }
}
