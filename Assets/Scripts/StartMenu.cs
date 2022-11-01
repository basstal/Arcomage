using System.Reflection;
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

        // public bool IsPlayerWin(GamePlayer target)
        // {
        //     return target.tower > 50 || FindEnemyById(target.playerID).tower <= 0;
        // }

        // public void IsGameEnded()
        // {
        //     var player1Win = IsPlayerWin(m_player1);
        //     var player2Win = IsPlayerWin(m_player2);
        //     if (player1Win || player2Win)
        //     {
        //         // m_gameEnd = player1Win && player2Win ? $"Peace End" : (player1Win ? $"player1Win" : "player2Win");
        //         m_isGameEnded = true;
        //         return;
        //     }
        //
        //     currentStage = DisplayHandCards;
        //     m_isGameEnded = false;
        // }
    }


//     using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UIElements;
// using UnityEngine.Serialization;
// using System;

// namespace UIToolkitDemo
// {
//     // high-level manager for the various parts of the Main Menu UI. Here we use one master UXML and one UIDocument.
//     // We allow the individual parts of the user interface to have separate UIDocuments if needed (but not shown in this example).
//     
//     [RequireComponent(typeof(UIDocument))]
//     public class MainMenuUIManager : MonoBehaviour
//     {
//
//         [Header("Modal Menu Screens")]
//         [Tooltip("Only one modal interface can appear on-screen at a time.")]
//         [SerializeField] HomeScreen m_HomeModalScreen;
//         [SerializeField] CharScreen m_CharModalScreen;
//         [SerializeField] InfoScreen m_InfoModalScreen;
//         [SerializeField] ShopScreen m_ShopModalScreen;
//         [SerializeField] MailScreen m_MailModalScreen;
//
//         [Header("Toolbars")]
//         [Tooltip("Toolbars remain active at all times unless explicitly disabled.")]
//         [SerializeField] OptionsBar m_OptionsToolbar;
//         [SerializeField] MenuBar m_MenuToolbar;
//
//         [Header("Full-screen overlays")]
//         [Tooltip("Full-screen overlays block other controls until dismissed.")]
//         [SerializeField] MenuScreen m_InventoryScreen;
//         [SerializeField] SettingsScreen m_SettingsScreen;
//
//         List<MenuScreen> m_AllModalScreens = new List<MenuScreen>();
//
//         UIDocument m_MainMenuDocument;
//         public UIDocument MainMenuDocument => m_MainMenuDocument;
//
//         void OnEnable()
//         {
//             m_MainMenuDocument = GetComponent<UIDocument>();
//             SetupModalScreens();
//             ShowHomeScreen();
//         }
//
//         void Start()
//         {
//             Time.timeScale = 1f;
//         }
//
//         void SetupModalScreens()
//         {
//             if (m_HomeModalScreen != null)
//                 m_AllModalScreens.Add(m_HomeModalScreen);
//
//             if (m_CharModalScreen != null)
//                 m_AllModalScreens.Add(m_CharModalScreen);
//
//             if (m_InfoModalScreen != null)
//                 m_AllModalScreens.Add(m_InfoModalScreen);
//
//             if (m_ShopModalScreen != null)
//                 m_AllModalScreens.Add(m_ShopModalScreen);
//
//             if (m_MailModalScreen != null)
//                 m_AllModalScreens.Add(m_MailModalScreen);
//         }
//
//         // shows one screen at a time
//         void ShowModalScreen(MenuScreen modalScreen)
//         {
//             foreach (MenuScreen m in m_AllModalScreens)
//             {
//                 if (m == modalScreen)
//                 {
//                     m?.ShowScreen();
//                 }
//                 else
//                 {
//                     m?.HideScreen();
//                 }
//             }
//         }
//
//         // methods to toggle screens on/off
//
//         // modal screen methods 
//         public void ShowHomeScreen()
//         {
//             ShowModalScreen(m_HomeModalScreen);
//         }
//
//         // note: screens with tabbed menus default to showing the first tab
//         public void ShowCharScreen()
//         {
//             ShowModalScreen(m_CharModalScreen);
//         }
//
//         public void ShowInfoScreen()
//         {
//             ShowModalScreen(m_InfoModalScreen);
//         }
//
//         public void ShowShopScreen()
//         {
//             ShowModalScreen(m_ShopModalScreen);
//         }
//
//         // opens the Shop Screen directly to a specific tab (e.g. to gold or gem shop) from the Options Bar
//         public void ShowShopScreen(string tabName)
//         {
//             m_MenuToolbar?.ShowShopScreen();
//             m_ShopModalScreen?.SelectTab(tabName);
//         }
//
//         public void ShowMailScreen()
//         {
//             ShowModalScreen(m_MailModalScreen);
//         }
//
//         // overlay screen methods
//         public void ShowSettingsScreen()
//         {
//             m_SettingsScreen?.ShowScreen();
//         }
//
//         public void ShowInventoryScreen()
//         {
//             m_InventoryScreen?.ShowScreen();
//         }
//     }
// }
}