using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GameScripts
{
    [RequireComponent(typeof(UIDocument))]
    public class StartMenu : MonoBehaviour
    {
        public AssetReference combat;
        VisualElement m_root;
        UIDocument m_mainMenuDocument;
        private GameObject m_runningCombat;
        public UIDocument mainMenuDocument => m_mainMenuDocument;

        private void Awake()
        {
            m_mainMenuDocument = GetComponent<UIDocument>();
            if (m_mainMenuDocument == null)
            {
                Debug.LogWarning("MenuScreen StartMenu: missing UIDocument. Check Script Execution Order.");
                return;
            }

            // get a reference to the root VisualElement 
            if (m_mainMenuDocument != null)
                m_root = m_mainMenuDocument.rootVisualElement;
            ShowStartMenu(null);
            RegisterButtonCallbacks();
        }

        protected virtual void RegisterButtonCallbacks()
        {
            m_root.Q("StartGame").RegisterCallback<ClickEvent>(StartCombat);
            m_root.Q("Quit").RegisterCallback<ClickEvent>(Quit);
        }

        public void ShowStartMenu(ClickEvent evt)
        {
            if (m_runningCombat != null)
            {
                DestroyImmediate(m_runningCombat);
            }

            var startMenu = m_root.Q<VisualElement>("StartMenu");
            ShowVisualElement(startMenu, true);
            var gameEnd = m_root.Q<VisualElement>("GameEnd");
            ShowVisualElement(gameEnd, false);
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
            m_runningCombat = combat.InstantiateAsync().WaitForCompletion();
            var startMenu = m_root.Q<VisualElement>("StartMenu");
            ShowVisualElement(startMenu, false);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                OnReload();
            }
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