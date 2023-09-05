// using System.Linq;
// using NOAH.EditorExtends;
// using UnityEditor;
// using UnityEditor.Experimental.SceneManagement;
// using UnityEditor.SceneManagement;
// using UnityEngine;
//
//
// namespace NOAH.UI
// {
//     [InitializeOnLoad]
//     public class UIPreview
//     {
//         static UIPreview()
//         {
//             PrefabStage.prefabStageOpened += OnPrefabStageOpened;
//             EditorApplication.delayCall += () =>
//             {
//                 Menu.SetChecked("⛵NOAH/Preview/Status Bar", EditorPrefs.GetBool("UIPreview/ShowStatusBar", true));
//             };
//         }
//
//         [MenuItem("⛵NOAH/Preview/DesignLandscape")]
//         static void SwitchPreviewToDesignLandscape()
//         {
//             SwitchTo("DesignLandscape", 1920, 1080);
//         }
//
//         [MenuItem("⛵NOAH/Preview/DesignLandscape", true)]
//         static bool SwitchPreviewToDesignValidate()
//         {
//             GameViewSizeGroupType groupType = GameViewUtils.GetCurrentGroupType();
//             return GameViewUtils.FindSize(groupType, "DesignLandscape") != GameViewUtils.GetCurrentSizeIndex();
//         }
//
//         [MenuItem("⛵NOAH/Preview/iPhoneXLandscape")]
//         static void SwitchPreviewToiPhoneXLandscape()
//         {
//             SwitchTo("iPhoneXLandscape", 2436, 1125);
//         }
//
//         [MenuItem("⛵NOAH/Preview/iPhoneXLandscape", true)]
//         static bool SwitchPreviewToiPhoneXLandscapeValidate()
//         {
//             GameViewSizeGroupType groupType = GameViewUtils.GetCurrentGroupType();
//             return GameViewUtils.FindSize(groupType, "iPhoneXLandscape") != GameViewUtils.GetCurrentSizeIndex();
//         }
//
//         [MenuItem("⛵NOAH/Preview/2400Landscape")]
//         static void SwitchPreviewTo2400Landscape()
//         {
//             SwitchTo("2400Landscape", 2400, 1080);
//         }
//
//         [MenuItem("⛵NOAH/Preview/2400Landscape", true)]
//         static bool SwitchPreviewTo2400LandscapeValidate()
//         {
//             GameViewSizeGroupType groupType = GameViewUtils.GetCurrentGroupType();
//             return GameViewUtils.FindSize(groupType, "2400Landscape") != GameViewUtils.GetCurrentSizeIndex();
//         }
//
//         [MenuItem("⛵NOAH/Preview/DesignPortrait")]
//         static void SwitchPreviewToDesignPortrait()
//         {
//             SwitchTo("DesignPortrait", 1080, 1920);
//         }
//
//         [MenuItem("⛵NOAH/Preview/DesignPortrait", true)]
//         static bool SwitchPreviewToDesignPortraitValidate()
//         {
//             GameViewSizeGroupType groupType = GameViewUtils.GetCurrentGroupType();
//             return GameViewUtils.FindSize(groupType, "DesignPortrait") != GameViewUtils.GetCurrentSizeIndex();
//         }
//
//         [MenuItem("⛵NOAH/Preview/iPhoneXPortrait")]
//         static void SwitchPreviewToiPhoneXPortrait()
//         {
//             SwitchTo("iPhoneXPortrait", 1125, 2436);
//         }
//
//         [MenuItem("⛵NOAH/Preview/iPhoneXPortrait", true)]
//         static bool SwitchPreviewToiPhoneXPortraitValidate()
//         {
//             GameViewSizeGroupType groupType = GameViewUtils.GetCurrentGroupType();
//             return GameViewUtils.FindSize(groupType, "iPhoneXPortrait") != GameViewUtils.GetCurrentSizeIndex();
//         }
//
//         [MenuItem("⛵NOAH/Preview/Status Bar")]
//         static void TogglePreviewStatusBar()
//         {
//             bool show = !EditorPrefs.GetBool("UIPreview/ShowStatusBar", false);
//             EditorPrefs.SetBool("UIPreview/ShowStatusBar", show);
//             Menu.SetChecked("⛵NOAH/Preview/Status Bar", show);
//
//             UpdateStatusBar();
//
//             EditorApplication.RepaintHierarchyWindow();
//         }
//
//         public static void SwitchTo(string name, int width, int height)
//         {
//             GameViewSizeGroupType groupType = GameViewUtils.GetCurrentGroupType();
//             var index = GameViewUtils.FindSize(groupType, name);
//             if (index == -1)
//             {
//                 GameViewUtils.AddCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, groupType, width, height, name);
//                 index = GameViewUtils.FindSize(groupType, name);
//             }
//
//             if (index != -1)
//             {
//                 GameViewUtils.SetSize(index);
//                 EditorPrefs.SetString("UIPreview", name);
//             }
//
//             if (EditorApplication.isPlayingOrWillChangePlaymode)
//             {
//                 if (UIManager.Instance)
//                 {
//                     UIManager.Instance.UpdatePreview();
//
//                     UIManager.Instance.DelayInvokeInFrames(1, () => {
//                         var deviceLayout = NOAH.Render.RenderManager.GetDeviceLayoutDefault();
//                         NOAH.Render.RenderManager.Instance.DeviceLayout = deviceLayout;
//                     });
//                 }
//             }
//             else
//             {
//                 UpdateDevice();
//             }
//         }
//
//         static GameObject GetRootGameObjectInPrefabStage(string name)
//         {
//             PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
//             GameObject result = null;
//             if (stage != null)
//             {
//                 GameObject[] rootGameObjects = stage.scene.GetRootGameObjects();
//                 result = rootGameObjects?.FirstOrDefault((go) => go.name == name);
//             }
//
//             return result;
//         }
//
//         static void OnPrefabStageOpened(PrefabStage prefabStage)
//         {
//             var canvasEdit = GetRootGameObjectInPrefabStage("CanvasEdit (Environment)");
//             if (canvasEdit != null)
//             {
//                 prefabStage.prefabContentsRoot.transform.SetParent(canvasEdit.transform);
//
//                 prefabStage.prefabContentsRoot.transform.localScale = Vector3.one;
//
//                 UpdateStatusBar();
//
//                 UpdateDevice();
//             }
//         }
//
//         private static void UpdateStatusBar()
//         {
//             var canvasPreview = GetRootGameObjectInPrefabStage("CanvasPreview (Environment)");
//             if (canvasPreview != null)
//             {
//                 var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
//
//                 var statusBar = canvasPreview.transform.Find("StatusBar (Environment)");
//                 if (statusBar != null)
//                 {
//                     var showStatusBar = EditorPrefs.GetBool("UIPreview/ShowStatusBar", false);
//                     statusBar.gameObject.SetActive(prefabStage.prefabContentsRoot.name != "StatusBar" && showStatusBar);
//                 }
//             }
//         }
//
//         private static void UpdateDevice()
//         {
//             var canvasPreview = GetRootGameObjectInPrefabStage("CanvasPreview (Environment)");
//             if (canvasPreview != null)
//             {
//                 var name = EditorPrefs.GetString("UIPreview");
//                 var devices = canvasPreview.transform.Find("Devices (Environment)");
//                 if (devices != null)
//                 {
//                     foreach (Transform trans in devices.transform)
//                     {
//                         trans.gameObject.SetActive(trans.name.Contains(name));
//                     }
//                 }
//             }
//         }
//     }
// }