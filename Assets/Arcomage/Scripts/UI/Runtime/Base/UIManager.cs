using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// using NOAH.Asset;
// using NOAH.Utility;
// using NOAH.Debug;
using UnityEngine;
// using NOAH.Render;
using System;
// using NOAH.VFX;
// using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Text;
// using GamePlay;
// using NOAH.Core;
// using NOAH.Criware;
// using NOAH.Input;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.VFX;
using Whiterice;

#if UNITY_EDITOR
#endif

namespace NOAH.UI
{
    public class UIManager : MonoBehaviour//SingletonBehaviour<UIManager>
    {
        public event Action<UIWindow> OnWindowUninited;
        public event Action<UIWindow> OnWindowInited;
        public event Action OnDestroyCachedWindows;
        public event Action<UIWindow> OnTopWindowChange;

        public event Action<string, Action> OnDialogOk;

        public event Action<int, string, Transform> OnShowPrompt;
        public event Action<int> OnClosePrompt;

        public event Action<string, float> OnShowTips;

        // public event Action<string,string,string,Action<string>> OnDialogYesOk;

        [SerializeField] private Camera m_camera = null;

        [SerializeField] private Canvas m_canvasRoot = null;

        // transform that holds all the normal windows on stage
        [SerializeField] private Transform m_windowNormal = null;

        // transform that holds all the overlay windows on stage
        [SerializeField] private Transform m_windowOverlay = null;

        public bool IsPortrait;

        public Transform WindowNormal => m_windowNormal;

        public Transform WindowOverlay => m_windowOverlay;

        // transform that holds all the windows off stage
        [SerializeField] private Transform m_windowCache = null;

        // [SerializeField] private Js.JsStatusBar m_statusBar = null;
        // [SerializeField] private CustomCamera m_customCamera = null;
        // [SerializeField] private UniversalAdditionalCameraData m_additionalCameraData = null;
        [SerializeField] private Material m_defaultMaterial = null;
        [SerializeField] private RectTransform m_cursorCanvas = null;
        [SerializeField] private CursorEffect m_cursorEffect = null;
        [SerializeField] private RectTransform m_touchCanvas = null;
        [SerializeField] private RectTransform m_canvasFill = null;
        // [SerializeField] private VFXEffectPlayer m_touchEffect = null;
        // [SerializeField] private int m_touchEffectCount = 1;
        // private UniversalAdditionalCameraData m_baseAdditionalCameraData;
        private Vector2 m_touchPos = Vector2.zero;
        private Vector2 m_stdAnchor = new Vector2(0.5f, 0.5f);
        // private List<VFXEffectPlayer> m_touchEffectList;
        private List<RectTransform> m_effectTransformList;
        private int m_touchEffectIndex = 0;
        public bool m_showTochEffect = false;

        [Tooltip("worked for URP camera stack")]
        //public Camera m_baseUICamera;
        public Transform m_overlayEffectRoot;
        //public UniversalAdditionalCameraData baseAdditionalData
        //{
        //    get
        //    {
        //        if(m_baseAdditionalCameraData == null)
        //        {
        //            m_baseAdditionalCameraData = m_baseUICamera.GetComponent<UniversalAdditionalCameraData>();
        //        }
        //        return m_baseAdditionalCameraData;
        //    }
        //}

        [SerializeField]
        private UIStyleSheet uiStyleSheet;

        public UIStyleSheet UIStyleSheet => uiStyleSheet;

        public Camera MainCamera { get { return m_camera; } }

        public Canvas CanvasRoot { get { return m_canvasRoot; } }

        // private List<Transform> m_statusBarRootStack = new List<Transform>();

        // hold the window that is creating in async mode
        private HashSet<string> m_pendingAsyncWindows = new HashSet<string>();

        private const int MAX_CANVAS_LAYERS = 20;

        private class SortingLayerContext
        {
            public int sortingLayerId;
            public int sortingLayer;
        }

        private SortingLayerContext[] m_canvasLayerContexts;

        protected void InitSingleton()
        {
            if (m_defaultMaterial != null)
            {
                var defaultMaterial = Canvas.GetDefaultCanvasMaterial();
                defaultMaterial.shader = m_defaultMaterial.shader;
                defaultMaterial.CopyPropertiesFromMaterial(m_defaultMaterial);
            }

            m_canvasLayerContexts = new SortingLayerContext[MAX_CANVAS_LAYERS];
            for (int i = 0; i < m_canvasLayerContexts.Length; i++)
            {
                var sortingLayerName = $"CanvasLayer_{i}";
                m_canvasLayerContexts[i] = new SortingLayerContext() { sortingLayerId = SortingLayer.NameToID(sortingLayerName), sortingLayer = SortingLayer.GetLayerValueFromName(sortingLayerName), };
            }

            // if (m_touchEffect != null)
            // {
            //     m_touchEffectList = new List<VFXEffectPlayer>();
            //     m_effectTransformList = new List<RectTransform>();
            //
            //     m_touchEffectList.Add(m_touchEffect);
            //     m_effectTransformList.Add(m_touchEffect.GetComponent<RectTransform>());
            //     if (m_touchEffectCount < 1)
            //         m_touchEffectCount = 1;
            //
            //     for (int i = 1; i < m_touchEffectCount; i++)
            //     {
            //         GameObject ob = GameObject.Instantiate(m_touchEffect.gameObject, m_touchCanvas);
            //         m_touchEffectList.Add(ob.GetComponent<VFXEffectPlayer>());
            //         m_effectTransformList.Add(ob.GetComponent<RectTransform>());
            //     }
            //
            //     m_touchEffectIndex = 0;
            // }

            // UniversalRenderPipelineAsset rpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            //if (rpAsset != null && m_baseUICamera)
            //{
            //    m_baseUICamera.gameObject.SetActive(true);
            //    if(!baseAdditionalData.cameraStack.Contains(m_camera))
            //    {
            //        baseAdditionalData.cameraStack.Add(m_camera);
            //    }
            //}
            // if (rpAsset != null)
            // {
            //     m_additionalCameraData.renderType = CameraRenderType.Base;
            // }

            // #if UNITY_EDITOR
            //             UpdatePreview();
            // #endif

            // Js.JsUtil.AddJsBehaviourWithScript(gameObject, "Manager/TsUIManager");
            ReplaceTMPSettings();
            UIStyleSheet.Init();
            InitScreenLog();
        }

        // protected void Awake()
        // {
        //     base.Awake();
        // }

        void ReplaceTMPSettings()
        {
            TMP_Settings settings = null;
#if !UNITY_EDITOR
            settings = AssetManager.Instance.LoadAsset<TMP_Settings>("UI/Font/RuntimeSetting", this);
#endif
            if (settings)
            {
                var f = typeof(TMP_Settings).GetField("s_Instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                var oldSetting = f.GetValue(null) as TMP_Settings;
                if (oldSetting != null)
                {
                    Resources.UnloadAsset(oldSetting);
                }

                f.SetValue(null, settings);
                // Debug.LogTool.LogInfo("Debug", $"Settings Replace succeed {settings.name}");
            }
        }

        protected void UninitSingleton()
        {
            // m_statusBar = null;

            DisableUIBase();
            DestroyAllWindows(true);
        }

        public void DisableUIBase(bool clearCamera = false)
        {
            //TODO: m_additionalCameraData.renderType = CameraRenderType.Overlay;
        }

        public void EnableUIBase()
        {
            //TODO: m_additionalCameraData.renderType = CameraRenderType.Base;
        }

        public void ReleaseCache()
        {
            DestroyAllWindows();
        }

        public void ToggleWindowNormal(bool visible)
        {
            foreach (Transform trans in m_windowNormal.transform)
            {
                if(trans.gameObject.name == "FloatingUI") continue;
                trans.gameObject.ToggleRendering(visible);
            }
        }

        // public void ToggleStatusBar(bool visible)
        // {
        //     m_statusBar.gameObject.SetActive(visible);
        // }

        // public bool IsStatusBarOn()
        // {
        //     return m_statusBar.gameObject.activeSelf;
        // }

        public bool DestroyCachedWindows()
        {
            bool purged = false;

            for (int i = m_windowCache.childCount - 1; i >= 0; --i)
            {
                var window = m_windowCache.GetChild(i).GetComponent<UIWindow>();
                if (!window.Config.DontDestory)
                {
                    DestroyWindowImpl(window);
                    purged = true;
                }
            }

            OnDestroyCachedWindows?.Invoke();
            return purged;
        }

        public void DestroyAllWindows(bool forcePurgeAll = false)
        {
            OnTopWindowChange?.Invoke(null);

            List<Transform> transformList = new List<Transform>();

            transformList.AddRange(m_windowCache.Cast<Transform>());
            transformList.AddRange(m_windowOverlay.Cast<Transform>().Reverse());
            transformList.AddRange(m_windowNormal.Cast<Transform>().Reverse());

            foreach (var winTransform in transformList)
            {
                var window = winTransform.GetComponent<UIWindow>();
                if (window != null)
                {
                    if (!window.Config.DontDestory || forcePurgeAll)
                    {
                        DestroyWindow(window, true, true);
                    }
                }
                else
                {
                    DestroyImmediate(winTransform.gameObject);
                }
            }

            OnDestroyCachedWindows?.Invoke();
        }

        // this function is dedicated to reduce the overdraw for overlapped windows
        private void UpdateWindowRendering(UIWindow window, bool visible)
        {
            if (window.Config.FullScreen && window.Config.SortingOrder == 0)
            {
                var startIndex = window.transform.GetSiblingIndex();
                bool dirty = true;
                for (int i = startIndex + 1; i < m_windowNormal.childCount; ++i)
                {
                    var targetWindow = m_windowNormal.GetChild(i).GetComponent<UIWindow>();
                    if (targetWindow != null && targetWindow.Config.FullScreen)
                    {
                        // there is full-screen window on top of current, skip the update
                        dirty = false;
                    }
                }

                if (dirty)
                {
                    for (int i = startIndex - 1; i >= 0; --i)
                    {
                        var targetWindow = m_windowNormal.GetChild(i).GetComponent<UIWindow>();

                        // it might be a shadow
                        if (targetWindow != null)
                        {
                            targetWindow.gameObject.ToggleRendering(!visible);

                            targetWindow.Focus(!visible);

                            if (targetWindow.Config.FullScreen)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void OnNavigation(string target)
        {
            if (m_windowNormal.childCount > 0)
            {
                if (string.IsNullOrEmpty(target))
                {
                    var window = TopWindow();
                    window.Back();
                    RecycleWindow(window);
                }
                else
                {
                    var targetWindow = FindWindow(target);
                    if (targetWindow != null)
                    {
                        var window = TopWindow();
                        while (window != targetWindow)
                        {
                            window.Back();
                            RecycleWindow(window, true);
                            window = TopWindow();
                        }
                    }
                    else
                    {
                        // LogTool.LogError("UI", "Can not navigate to window [" + target + "] since it's not on stage.");
                    }
                }
            }
            else
            {
                // LogTool.LogError("UI", "Window stack is empty while trying to navigate.");
            }
        }

        private UIWindow AcquireWindowFromCache(string winName)
        {
            UIWindow window = null;

            if (!string.IsNullOrEmpty(winName))
            {
                var winTransform = m_windowCache.Find(winName) as RectTransform;
                if (winTransform != null)
                {
                    var canvasRootTrans = CanvasRoot.transform as RectTransform;
                    winTransform.sizeDelta = canvasRootTrans.sizeDelta;
                    window = winTransform.GetComponent<UIWindow>();
                    window.gameObject.SetActive(true);
                    winTransform.SetParent(window.Config.SortingOrder == 0 ? m_windowNormal : m_windowOverlay);
                    // TODO: 可能需要重新考虑的设计
                    // winTransform.SetDefault();

                    SortWindow(window);
                }
            }

            return window;
        }

        private UIWindow AcquireWindowFromStage(string winName)
        {
            UIWindow window = null;

            if (!string.IsNullOrEmpty(winName))
            {
                window = FindWindow(winName);
                if (window != null)
                {
                    window.Recycling = false;

                    // window.OnSaveContext();

                    if (window.Config.SortingOrder == 0)
                    {
                        var shadowTransform = new GameObject(window.name + "_shadow").transform;
                        shadowTransform.SetParent(m_windowNormal);
                        // TODO: 可能需要重新考虑的设计
                        // shadowTransform.SetDefault();
                        shadowTransform.SetSiblingIndex(window.transform.GetSiblingIndex());
                        window.transform.SetAsLastSibling();
                    }

                    UpdateCanvasSortingLayer();
                }
            }

            return window;
        }

        private void OnWindowAcquired(UIWindow window, bool silence = false, AcquireWindowCallback callback = null)
        {
            if (window == null) return;
            InitWindow(window);
            window.OnAcquire();
            if (!silence)
            {
                // window.PlayWindowBgm();
            }

            // InputManager.Instance.SetLockFlag(LockFlag.AsyncLoading, false);
            //GameInputManager.Instance.SetUIActionLock(InputLockFlag.WindowRequire,false);
            try
            {
                callback?.Invoke(window);
            }
            catch (Exception)
            {
                // LogTool.LogError("UI", e);
            }
        }

        private void OnWindowCreated(GameObject windowGo)
        {
            windowGo.name = windowGo.name.Replace("(Clone)", "");

            //GameGlobal.Instance.AssignCameraRenderTexture(windowGo.GetComponentInChildren<Camera>());

            var window = windowGo.GetComponent<UIWindow>();

            AttachWindow(windowGo.transform, window.Config.SortingOrder == 0 ? m_windowNormal : m_windowOverlay);

            SortWindow(window);

            // window.OnSetup();

            // GameDebug.Log("create new [" + window.name + "]");
        }

        private void SortWindow(UIWindow window)
        {
            if (window.Config.SortingOrder != 0)
            {
                foreach (Transform child in m_windowOverlay)
                {
                    var target = child.GetComponent<UIWindow>();
                    if (window.Config.SortingOrder < target.Config.SortingOrder)
                    {
                        // rearrange window with new sibling index
                        window.transform.SetSiblingIndex(child.GetSiblingIndex());
                        break;
                    }
                }
            }

            UpdateCanvasSortingLayer();
        }

        private void InitWindow(UIWindow window)
        {
            // if (window.Config.StatusBarStyle > 0 && m_statusBar != null)
            // {
            //     m_statusBarRootStack.Add(m_statusBar.transform.parent);
            //     m_statusBar.SetParent(window.transform);
            // }

            UpdateWindowRendering(window, true);
            window.gameObject.ToggleRendering(true);

            window.PlayAnimation("Open");
            // CriwareAudioManager.Instance.PlaySoundUI(window.Config.OpenSE);
            window.Init();

            window.Focus(true);
            OnWindowInited?.Invoke(window);

            if (window.Config.SortingOrder == 0)
            {
                OnTopWindowChange?.Invoke(window);
            }
        }

        private void UninitWindow(UIWindow window)
        {
            if (window.ReferenceCount > 0)
            {
                if (window.Config.SortingOrder == 0)
                {
                    var shadowTransform = m_windowNormal.Cast<Transform>().Last(trans => trans.name == window.name + "_shadow");
                    if (shadowTransform != null)
                    {
                        var targetIndex = shadowTransform.GetSiblingIndex();
                        window.transform.SetSiblingIndex(targetIndex);
                        shadowTransform.SetParent(null);
                        DestroyImmediate(shadowTransform.gameObject);

                        bool visible = true;
                        for (int i = targetIndex + 1; i < m_windowNormal.childCount; ++i)
                        {
                            var _window = m_windowNormal.GetChild(i).GetComponent<UIWindow>();
                            if (_window != null && _window.Config.FullScreen)
                            {
                                visible = false;
                                break;
                            }
                        }

                        window.gameObject.ToggleRendering(visible);

                        UpdateCanvasSortingLayer();
                    }
                }

                // window.OnLoadContext();
            }
            else
            {
                window.Focus(false);
                window.Uninit();
                window.gameObject.SetActive(false);
                window.transform.SetParent(m_windowCache);
                OnWindowUninited?.Invoke(window);
            }

            window.Recycling = false;

            // if (window.Config.StatusBarStyle > 0 && m_statusBar != null)
            // {
            //     if (m_statusBar.transform.parent == window.StatusBarRoot)
            //     {
            //         var prev = m_statusBarRootStack.Last();
            //         m_statusBar.SetParent(prev);
            //         m_statusBarRootStack.Remove(prev);
            //     }
            //     else
            //     {
            //         m_statusBarRootStack.Remove(window.StatusBarRoot);
            //     }
            // }

            if (window.Config.SortingOrder == 0)
            {
                OnTopWindowChange?.Invoke(TopWindow());
            }
        }

        public delegate bool DelegateFindWindow(UIWindow window);

        private UIWindow FindWindowIfImpl(Transform root, DelegateFindWindow predicate)
        {
            UIWindow result = null;
            foreach (var window in root.GetComponentsInChildren<UIWindow>())
            {
                if (predicate(window))
                {
                    result = window;
                    break;
                }
            }

            return result;
        }

        public UIWindow FindWindowIf(DelegateFindWindow predicate, bool includeCache = false)
        {
            UIWindow window = null;

            if (predicate != null)
            {
                if (window == null) window = FindWindowIfImpl(m_windowNormal, predicate);
                if (window == null) window = FindWindowIfImpl(m_windowOverlay, predicate);
                if (window == null && includeCache) window = FindWindowIfImpl(m_windowCache, predicate);
            }

            return window;
        }

        public UIWindow FindWindow(string winName, bool includeCache = false)
        {
            UIWindow window = null;

            if (!string.IsNullOrEmpty(winName))
            {
                Transform winTransform = null;
                if (winTransform == null) winTransform = m_windowNormal.Find(winName);
                if (winTransform == null) winTransform = m_windowOverlay.Find(winName);
                if (winTransform == null && includeCache) winTransform = m_windowCache.Find(winName);

                if (winTransform != null)
                {
                    window = winTransform.GetComponent<UIWindow>();
                }
            }

            return window;
        }

        public UIWindow AcquireWindow(string winName, bool silence = false, AcquireWindowCallback callback = null)
        {
            UIWindow window = null;

            if (!string.IsNullOrEmpty(winName))
            {
                window = AcquireWindowFromCache(winName) ?? AcquireWindowFromStage(winName);
                if (window == null)
                {
                    var windowGo = CreateWindow(winName);
                    if (windowGo != null)
                    {
                        OnWindowCreated(windowGo);
                        window = windowGo.GetComponent<UIWindow>();
                    }
                }

                OnWindowAcquired(window, silence, callback);
            }

            return window;
        }


        public delegate void AcquireWindowCallback(UIWindow window);

        public void AcquireWindowAsync(string winName, AcquireWindowCallback callback = null)
        {
            UIWindow window = null;

            if (!string.IsNullOrEmpty(winName))
            {
                // InputManager.Instance.SetLockFlag(LockFlag.AsyncLoading, true);
                window = AcquireWindowFromCache(winName) ?? AcquireWindowFromStage(winName);
                if (window == null)
                {
                    if (!m_pendingAsyncWindows.Contains(winName))
                    {
                        m_pendingAsyncWindows.Add(winName);
                        CreateWindowAsync(winName, (windowGo) =>
                        {
                            if (windowGo != null)
                            {
                                OnWindowCreated(windowGo);

                                window = windowGo.GetComponent<UIWindow>();
                                OnWindowAcquired(window, false, callback);
                                m_pendingAsyncWindows.Remove(winName);
                            }
                            else
                            {
                                m_pendingAsyncWindows.Remove(winName);
                            }
                        });
                    }
                    else if(callback != null)
                    {
                        callback(window);
                        // LogTool.LogError("UI", "Try to acquire window [" + winName + "] which is loading asynchronously.");
                    }
                }
                else
                {
                    OnWindowAcquired(window, false, callback);
                }
            }
            // return window;
        }

        public UIWindow PreloadWindow(string winName)
        {
            UIWindow window = null;

            var winTransform = m_windowCache.Find(winName);
            if (winTransform != null)
            {
                window = winTransform.GetComponent<UIWindow>();
            }
            else
            {
                if (!FindWindow(winName))
                {
                    var windowGo = CreateWindow(winName);
                    if (windowGo != null)
                    {
                        OnWindowCreated(windowGo);

                        window = windowGo.GetComponent<UIWindow>();
                        OnWindowAcquired(window, true);

                        // foreach (var adapter in windowGo.GetComponentsInChildren<AssetAdapterBase>(false))
                        // {
                        //     adapter.Start();
                        // }

                        RecycleWindow(winName);
                    }
                }
            }

            return window;
        }

        public void RecycleWindow(string winName)
        {
            if (!string.IsNullOrEmpty(winName))
            {
                var window = FindWindow(winName);
                if (window != null)
                {
                    RecycleWindow(window);
                }
                else
                {
                    // LogTool.LogError("UI", "Failed to recycle window [" + winName + "] since it's not on stage.");
                }
            }
        }

        public void RecycleWindow(Component component, bool immediate = false, bool changeLevel = false)
        {
            var window = component.GetComponent<UIWindow>();
            if (window != null && !window.Recycling)
            {
                window.Recycling = true;

                window.OnRecycle(changeLevel);

                UpdateWindowRendering(window, false);

                if (immediate)
                {
                    UninitWindow(window);
                }
                else
                {
                    window.PlayAnimation("Close", () => UninitWindow(window));
                    // CriwareAudioManager.Instance.PlaySoundUI(window.Config.CloseSE);
                }
            }
        }

        private GameObject CreateWindow(string winName)
        {
            return AssetManager.Instance.InstantiatePrefab("UI/Prefab/Window/" + winName + "/" + winName);
        }

        private void CreateWindowAsync(string winName, System.Action<GameObject> callback = null)
        {
            AssetManager.Instance.InstantiatePrefabAsync("UI/Prefab/Window/" + winName + "/" + winName, this, callback);
        }

        public GameObject CreateWidget(string widgetPath)
        {
            return AssetManager.Instance.InstantiatePrefab("UI/Prefab/Window/" + widgetPath);
        }

        public void CreateWidgetAsync(string widgetPath, System.Action<GameObject> callback = null)
        {
            AssetManager.Instance.InstantiatePrefabAsync("UI/Prefab/Window/" + widgetPath, this, callback);
        }

        public UIWindow TopWindow()
        {
            UIWindow window = null;

            if (m_windowNormal.childCount > 0)
            {
                window = m_windowNormal.GetChild(m_windowNormal.childCount - 1).GetComponent<UIWindow>();
            }

            return window;
        }

        public void DestroyWindow(string winName, bool immediate = false)
        {
            if (!string.IsNullOrEmpty(winName))
            {
                UIWindow window = FindWindow(winName, true);
                if (window != null)
                {
                    DestroyWindow(window, immediate);
                }
            }
        }

        public void DestroyWindow(Component component, bool immediate = false, bool changeLevel = false)
        {
            var window = component.GetComponent<UIWindow>();

            if (window.ReferenceCount > 1)
            {
                // LogTool.LogError("UI", "Try to destroy window [" + window.name + "] with more than one references");
            }

            if (window.ReferenceCount > 0)
            {
                RecycleWindow(window, immediate, changeLevel);
            }

            DestroyWindowImpl(window);
        }

        private void DestroyWindowImpl(UIWindow window)
        {
            if (window != null)
            {
                window.transform.SetParent(null);
                m_pendingAsyncWindows.Remove(window.name);
                DestroyImmediate(window.gameObject);
            }
        }

        private void AttachWindow(Transform panel, Transform parent)
        {
            // set anchor of the window
            var rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            // RuntimeUtilitly.SetParent(panel, parent);

            // reset size delta to align window to the root canvas
            rectTransform.sizeDelta = Vector2.zero;
        }

        public int CachedWindowCount()
        {
            return m_windowCache.childCount;
        }

        private void Update()
        {
            // if (UnityEngine.Input.GetMouseButtonDown(0) && m_touchEffect && m_showTochEffect)
            {
// #if UNITY_EDITOR
//                 if (GameGlobal.g_EditMode)
//                     return;
// #endif
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_touchCanvas, UnityEngine.Input.mousePosition, m_camera, out m_touchPos);
                m_effectTransformList[m_touchEffectIndex].anchoredPosition = m_touchPos;
                // if (m_touchEffectList[m_touchEffectIndex].Effect == null)
                // {
                //     m_touchEffectList[m_touchEffectIndex].RestartEffect();
                // }
                // else
                // {
                //     m_touchEffectList[m_touchEffectIndex].Effect.Reactivate();
                // }

                // m_touchEffectIndex++;
                // if (m_touchEffectIndex >= m_touchEffectList.Count)
                //     m_touchEffectIndex = 0;
            }

#if DEVELOPMENT_BUILD
            UnityEngine.Debug.developerConsoleVisible = false;
#endif
        }

        public void SetMultiTouch(bool val)
        {
            //        mUICamera.allowMultiTouch = val;
        }

        public void SetOrthographic(bool orthographic)
        {
            m_camera.orthographic = orthographic;
        }

        private IEnumerable<UIWindow> ActiveWindows()
        {
            for (int i = 0; i < m_windowNormal.childCount; i++)
            {
                var window = m_windowNormal.GetChild(i).GetComponent<UIWindow>();
                if (window != null) yield return window;
            }

            for (int i = 0; i < m_windowOverlay.childCount; i++)
            {
                var window = m_windowOverlay.GetChild(i).GetComponent<UIWindow>();
                if (window != null) yield return window;
            }
        }

        private IEnumerable<Canvas> SplitableCanvases()
        {
            foreach (var window in ActiveWindows())
            {
                yield return window.GetComponent<Canvas>();
                foreach (var splitter in window.GetComponentsInChildren<UICanvasLayerSplitter>(true))
                {
                    yield return splitter.GetComponent<Canvas>();
                }
            }
        }

        public void UpdateCanvasSortingLayer()
        {
            int layer = 0;
            foreach (var canvas in SplitableCanvases())
            {
                canvas.overrideSorting = true;
                var context = m_canvasLayerContexts[layer++];
                // if (LogTool.Assert(context.sortingLayer != 0, $"Invalid sorting layer for {canvas}"))
                // {
                //     canvas.sortingLayerID = context.sortingLayerId;
                //     LogTool.LogDebug("UI", $"Set sorting layer to {context.sortingLayerId} for {canvas}");
                // }
            }

            if (layer > 0)
            {
                // if (m_customCamera != null) m_customCamera.Parameter.SetSortingLayerRange(m_canvasLayerContexts[0].sortingLayer, m_canvasLayerContexts[layer - 1].sortingLayer);
                // if (m_additionalCameraData != null)
                // {
                //     //m_additionalCameraData.m_SortingLayerMin = m_canvasLayerContexts[0].sortingLayer;
                //     //m_additionalCameraData.m_SortingLayerMax = m_canvasLayerContexts[layer - 1].sortingLayer;
                // }
            }

            // if (m_touchEffect && m_touchCanvas)
            // {
            //     m_touchCanvas.GetComponent<Canvas>().sortingLayerID = m_canvasLayerContexts[layer - 1].sortingLayerId;
            // }

            if (m_canvasFill != null)
            {
                m_canvasFill.GetComponent<Canvas>().sortingLayerID = m_canvasLayerContexts[layer - 1].sortingLayerId;
            }

            if (m_cursorCanvas && m_cursorEffect)
            {
                m_cursorCanvas.GetComponent<Canvas>().sortingLayerID = m_canvasLayerContexts[layer - 1].sortingLayerId;
            }
        }

        public void ToggleTouchEffect(bool flag)
        {
            // if (m_touchEffect && m_touchCanvas)
            // {
            //     m_showTochEffect = flag;
            // }
        }

        // public VFXEffectHub PlayFullScreenEffect(string effectName)
        // {
        //     EffectPlayParam param = new EffectPlayParam
        //     {
        //         name = effectName,
        //         localPosition = Vector3.zero,
        //         localRotation = Vector3.one,
        //         localScale = Vector3.one,
        //         target = m_overlayEffectRoot
        //     };
        //
        //     return VFXManager.Instance.PlayEffect(param);
        // }

        public void ShowDialogOk(string message, Action callback)
        {
            OnDialogOk?.Invoke(message, callback);
        }

        public void ShowPrompt(int type, string json, Transform trans)
        {
            OnShowPrompt?.Invoke(type, json, trans);
        }

        public void ClosePrompt(int type)
        {
            OnClosePrompt?.Invoke(type);
        }

        public void ShowTips(string tips, float duration)
        {
            OnShowTips?.Invoke(tips, duration);
        }

        public void StartCursorEffect(float time, bool fightCamera, float delayShow)
        {
            if (m_cursorEffect != null)
            {
                m_cursorEffect.Play(time, fightCamera, delayShow);
            }
        }

        public void StopCursorEffect()
        {
            if (m_cursorEffect != null)
            {
                m_cursorEffect.Stop();
            }
        }

        void InitScreenLog()
        {
#if !RELEASE_BUILD
            // LogViewer.Instance.OnLog += OnLog;
#endif
        }
#if !RELEASE_BUILD

        private string[] blackList = new string[] { "WindowNormal", "FloatingUI", "TouchInput", "TouchInputEvo", "InputLock", "Blocker" };


        void OnLog(StringBuilder screenlog)
        {
            var isFirst = true;
            foreach (Transform window in WindowNormal)
            {
                if (!window.gameObject.activeInHierarchy) { continue; }

                if (blackList.Contains(window.name)) { continue; }

                if (isFirst) { isFirst = false; }
                else
                {
                    screenlog.Append(">");
                }

                screenlog.Append(window.name);
            }

            isFirst = true;
            foreach (Transform window in WindowOverlay)
            {
                if (!window.gameObject.activeInHierarchy) { continue; }

                if (blackList.Contains(window.name)) { continue; }

                if (isFirst)
                {
                    isFirst = false;
                    screenlog.Append("|");
                }
                else
                {
                    screenlog.Append(">");
                }

                screenlog.Append(window.name);
            }

            // screenlog.Append($" LockFlags: [{InputManager.Instance.LockFlags}] UILockFlag: [{GameInputManager.Instance.UIActionFlag}] ActorLockFlag: [{GameInputManager.Instance.ActorActionFlag}]");
            // screenlog.Append($" LockFlags [Base:{InputManager.Instance.LockFlags}] [UI:{GameInputManager.Instance.UIActionFlag}] [Actor:{GameInputManager.Instance.ActorActionFlag}]");

            var inputModule = CanvasRoot.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (inputModule)
            {
                var tp = typeof(UnityEngine.EventSystems.StandaloneInputModule);
                var fld = tp.GetField("m_InputPointerEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fld != null)
                {
                    var ret = fld.GetValue(inputModule);
                    var pev = ret as PointerEventData;
                    if (pev != null && pev.pointerPressRaycast.isValid)
                    {
                        screenlog.Append($" pressing: {pev.pointerPressRaycast.gameObject}");
                    }
                }
            }

            screenlog.Append("\n");
        }
#endif
        // #if UNITY_EDITOR
        //         public void UpdatePreview()
        //         {
        //             var canvasPreviewTrans = transform.Find("CanvasPreview");
        //
        //             var previewName = EditorPrefs.GetString("UIPreview");
        //             if (!string.IsNullOrEmpty(previewName) && previewName != "Design")
        //             {
        //                 if (canvasPreviewTrans == null)
        //                 {
        //                     var canvasPreviewAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NOAH/Modules/UI/EditorRuntime/Prefabs/CanvasPreview.prefab");
        //                     var canvasPreview = Instantiate(canvasPreviewAsset);
        //                     canvasPreview.name = "CanvasPreview";
        //                     canvasPreviewTrans = canvasPreview.transform;
        //                     canvasPreviewTrans.SetParent(transform);
        //                     var statusBarTrans = canvasPreviewTrans.Find("StatusBar");
        //                     if (statusBarTrans != null)
        //                     {
        //                         statusBarTrans.gameObject.SetActive(false);
        //                     }
        //                 }
        //                 else
        //                 {
        //                     canvasPreviewTrans.gameObject.SetActive(true);
        //                 }
        //
        //                 var devices = canvasPreviewTrans.Find("Devices");
        //                 foreach (Transform device in devices)
        //                 {
        //                     device.gameObject.SetActive(device.name == previewName);
        //                 }
        //             }
        //             else
        //             {
        //                 if (canvasPreviewTrans != null)
        //                 {
        //                     canvasPreviewTrans.gameObject.SetActive(false);
        //                 }
        //             }
        //         }
        // #endif
    }
}
