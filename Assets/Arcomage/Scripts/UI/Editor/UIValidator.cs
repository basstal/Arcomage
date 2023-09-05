using DG.Tweening;
// using NOAH.Utility;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using Arcomage.GameScripts.Utils;
// using NOAH.Core;
using UnityEditor.SceneManagement;

namespace NOAH.UI
{
    [InitializeOnLoad]
    public class UIValidator
    {
        public static bool Enabled = true;

        static UIValidator()
        {
            SetupCallbacks();
        }

        [DidReloadScripts(10)]
        static void SetupCallbacks()
        {
            PrefabStage.prefabSaving -= Validate;
            PrefabStage.prefabSaving += Validate;
            
            PrefabUtility.prefabInstanceUpdated -= Validate;
            PrefabUtility.prefabInstanceUpdated += Validate;
        }

        static public void Validate(GameObject prefab)
        {
            if (Enabled)
            {
                CheckUIWrapLayout(prefab);

                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
                if (string.IsNullOrEmpty(path))
                {
                    path = PrefabStageUtility.GetPrefabStage(prefab).assetPath;
                }
                if (path.Contains("AssetBundle/UI/Prefab"))
                {
                    ValidateUI(prefab);
                }
            }
        }

        static void CheckUIWrapLayout(GameObject instance)
        {
            UIWrapLayoutBase temps = instance.GetComponentInChildren<UIWrapLayoutBase>();
            if (temps)
            {
                temps.DisplayMode = false;
            }
            // foreach (var item in temps)
            // {
            //     item.DisplayMode = false;
            // }
            // save size for init
            // UIWrapCell[] cells = instance.GetComponentsInChildren<UIWrapCell>();
            // float screenWidth = Screen.width;
            // float screenHeight = Screen.height;
            // foreach (UIWrapCell cell in cells)
            // {
            //     cell.m_width = cell.rectTransform.anchorMax.x - cell.rectTransform.anchorMin.x == 1 ? cell.rectTransform.rect.width * 1920 / screenWidth : screenWidth;
            //     cell.m_height = cell.rectTransform.anchorMax.y - cell.rectTransform.anchorMin.y == 1 ? cell.rectTransform.rect.height * 1080 / screenHeight : screenHeight;
            // }
        }

        public static bool ValidateUI(GameObject ui)
        {
            bool result = true;
            Vector2Int referenceResolution;

            // read validate ui reference resolution in set prefabUIEnvironment file
            SceneAsset uiEnv = UnityEditor.EditorSettings.prefabUIEnvironment;
            var scenePath = AssetDatabase.GetAssetPath(uiEnv);
            string fileContent = File.ReadAllText(scenePath);
            Regex rx = new Regex(@"m_ReferenceResolution: {x: (\d*), y: (\d*)}");

            Match match = rx.Match(fileContent);
            if (match.Groups.Count == 3)
            {
                string width = match.Groups[1].Value;
                string height = match.Groups[2].Value;
                referenceResolution = new Vector2Int(int.Parse(width), int.Parse(height));
            }

            else
            {
                UnityEngine.Debug.LogError("Valdate UI failed, because Reference Resolution doesn't find in using prefabUIEnvronment scene.");
                return false;
            }

            bool valid = true;

            UIWindow window = ui.GetComponent<UIWindow>();
            if (window != null)
            {
                var rectTransform = window.GetComponent<RectTransform>();
                if (rectTransform.rect.width != referenceResolution.x || rectTransform.rect.height != referenceResolution.y)
                {
                    valid = false;
                }

                if (rectTransform.localScale.x == 0 || rectTransform.localScale.y == 0 || rectTransform.localScale.z == 0)
                {
                    valid = false;
                }
            }

            if (valid)
            {
                // make sure canvas doesn't override sorting
                var canvasList = ui.GetComponentsInChildren<Canvas>(true);
                foreach (var canvas in canvasList)
                {
                    var canvasScaler = canvas.GetComponent<CanvasScaler>();
                    if (canvasScaler != null)
                    {
                        GameObject.DestroyImmediate(canvasScaler);
                    }

                    var parentCanvas = canvas.gameObject.GetFirstComponentInParent<Canvas>();
                    if (parentCanvas != null)
                    {
                        if (canvas.overrideSorting)
                        {
                            if (canvas.gameObject.activeInHierarchy && canvas.enabled)
                            {
                                canvas.sortingOrder = 0;
                                canvas.overrideSorting = false;
                            }
                            else
                            {
                                result = false;
                                UnityEngine.Debug.LogError("Failed to cancel override sorting for Canvas on GameObject [" + canvas.name + "], make sure it's active in hierarchy and canvas component is enabled or cancel override sorting manually in prefab.", canvas.gameObject);
                            }
                        }
                    }
                    else // root canvas within window or widget
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.sortingLayerName = "Default";
                        canvas.sortingOrder = 0;
                    }
                }

                //only BMFont need ugui text
                var textList = ui.GetComponentsInChildren<Text>(true);
                foreach (var text in textList)
                {
                    UnityEngine.Debug.LogWarning("Text is used on game object " + text.name + ", use Text - TextMeshPro instead.", text.gameObject);
                }

                Graphic[] graphics = ui.GetComponentsInChildren<Graphic>(true);
                foreach (var graphic in graphics)
                {
                    graphic.raycastTarget = graphic.gameObject.GetComponent<IEventSystemHandler>() != null;

                    if (graphic.raycastTarget)
                    {
                        Image image = graphic.GetComponent<Image>();
                        if (image && image.color.a == 0)
                        {
                            CanvasRenderer canvasRenderer = graphic.GetComponent<CanvasRenderer>();
                            if (!canvasRenderer.cullTransparentMesh)
                            {
                                canvasRenderer.cullTransparentMesh = true;
                            }
                        }
                    }
                }

                // slider handle append eventhandler
                Slider[] sliders = ui.GetComponentsInChildren<Slider>(true);
                foreach (var slider in sliders)
                {
                    var handle = slider.handleRect;
                    if (handle == null)
                        break;
                    var handleGo = handle.gameObject;
                    var eventHandlers = handleGo.GetComponentsInChildren<IEventSystemHandler>(true);
                    if ((eventHandlers == null) || (eventHandlers.Length == 0))
                    {
                        Graphic[] gs = handleGo.GetComponentsInChildren<Graphic>(true);
                        if ((gs == null) || (gs.Length == 0))
                        {
                            result = false;
                            UnityEngine.Debug.LogError("Slider Handle need graphic");
                            continue;
                        }

                        gs[0].gameObject.AddComponent<EventHandler>();
                        gs[0].raycastTarget = true;
                    }
                }

                foreach (var tweenAnimation in ui.GetComponentsInChildren<DOTweenAnimation>(true))
                {
                    tweenAnimation.isIndependentUpdate = true;
                }

                foreach (var gameObject in ui.GetComponentsInChildren<Transform>(true))
                {
                    if (gameObject.localPosition.z != 0)
                    {
                        UnityEngine.Debug.LogWarning(gameObject.name + "'s Local Position Z is not 0", gameObject);
                    }
                    // if (gameObject.localRotation != Quaternion.identity)
                    // {
                    //     UnityEngine.Debug.LogWarning(gameObject.name + "'s Local Rotation Z is not identity", gameObject);
                    // }
                }
            }

            else
            {
                UnityEngine.Debug.LogWarning($"Window should be edited and saved with resolution {referenceResolution.x}x{referenceResolution.y} and scale=1", ui);
            }

            return result;
        }
    }
}