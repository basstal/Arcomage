using System;
using System.Collections;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Whiterice;

namespace GameScripts
{
    [RequireComponent(typeof(UIDocument))]
    public class StartMenu : MonoBehaviour
    {
        // public AssetReference combat;
        VisualElement m_root;
        UIDocument m_mainMenuDocument;
        private GameObject m_runningCombatRoot;
        public UIDocument mainMenuDocument => m_mainMenuDocument;

        private float checkUpdateTick;
        // private AsyncOperationHandle<List<string>> checkUpdate;

        private IEnumerator Start()
        {
            m_mainMenuDocument = GetComponent<UIDocument>();
            if (m_mainMenuDocument == null)
            {
                Debug.LogWarning("MenuScreen StartMenu: missing UIDocument. Check Script Execution Order.");
                yield break;
            }

            // get a reference to the root VisualElement 
            if (m_mainMenuDocument != null)
                m_root = m_mainMenuDocument.rootVisualElement;
            yield return AssetManager.Initialize();
            ShowStartMenu(null);
            RegisterButtonCallbacks();
        }

        protected virtual void RegisterButtonCallbacks()
        {
            m_root.Q("StartGame").RegisterCallback<ClickEvent>(StartCombat);
            m_root.Q("Quit").RegisterCallback<ClickEvent>(Quit);
            m_root.Q<VisualElement>("NewPatch").RegisterCallback<ClickEvent>(ShowStartMenu);
        }

        public void ShowStartMenu(ClickEvent evt)
        {
            if (m_runningCombatRoot != null)
            {
                DestroyImmediate(m_runningCombatRoot);
                m_runningCombatRoot = null;
            }

            AssetManager.Instance.CollectAssets();
            var startMenu = m_root.Q<VisualElement>("StartMenu");
            ShowVisualElement(startMenu, true);
            var gameEnd = m_root.Q<VisualElement>("GameEnd");
            ShowVisualElement(gameEnd, false);
            var newPatch = m_root.Q<VisualElement>("NewPatch");
            ShowVisualElement(newPatch, false);
        }

        public void ShowGameEnd(string inText, Color inColor)
        {
            var gameEndLabel = m_root.Q<Label>("GameEndLabel");
            gameEndLabel.text = inText;
            gameEndLabel.style.color = inColor;
            var gameEnd = m_root.Q<VisualElement>("GameEnd");
            ShowVisualElement(gameEnd, true);
            m_root.Q("Continue").RegisterCallback<ClickEvent>(ShowStartMenu);
        }

        // Toggle a UI on and off using the DisplayStyle. 
        public static void ShowVisualElement(VisualElement visualElement, bool state)
        {
            if (visualElement == null)
                return;

            visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void StartCombat(ClickEvent evt)
        {
            if (m_runningCombatRoot == null)
            {
                m_runningCombatRoot = new GameObject("m_runningCombatRoot");
            }

            AssetManager.Instance.InstantiatePrefab("Combat", m_runningCombatRoot, parent: m_runningCombatRoot.transform);
            // m_runningCombat = combat.InstantiateAsync().WaitForCompletion();
            var startMenu = m_root.Q<VisualElement>("StartMenu");
            ShowVisualElement(startMenu, false);
        }

        private void Update()
        {
            // if (Input.GetKey(KeyCode.R))
            // {
            //     OnReload();
            // }
            //
            // checkUpdateTick -= Time.unscaledDeltaTime;
            // if (checkUpdateTick <= 0 && !checkUpdate.IsValid())
            // {
            //     Debug.LogWarning($" check");
            //     checkUpdateTick = 10;
            //     if (checkUpdate.IsValid())
            //     {
            //         Addressables.Release(checkUpdate);
            //     }
            //
            //     checkUpdate = Addressables.CheckForCatalogUpdates(false);
            //     checkUpdate.Completed += handle =>
            //     {
            //         Debug.LogWarning($"checkUpdate completed.");
            //         if (handle.Result != null && handle.Result.Count > 0)
            //         {
            //             var newPatch = m_root.Q<VisualElement>("NewPatch");
            //             ShowVisualElement(newPatch, true);
            //         }
            //
            //         Addressables.Release(checkUpdate);
            //     };
            // }
        }

        public void Quit(ClickEvent evt)
        {
            Application.Quit();
        }

        public void OnReload()
        {
#if UNITY_EDITOR
            ClearLog();
#endif
            DOTween.Clear();
            SceneManager.LoadSceneAsync("GamePlay");
        }
#if UNITY_EDITOR
        static void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method?.Invoke(new object(), null);
        }
#endif
    }
}