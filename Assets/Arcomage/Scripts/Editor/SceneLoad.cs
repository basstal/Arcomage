using System.IO;
using Arcomage.GameScripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Arcomage.GameEditorScripts
{
    [InitializeOnLoad]
    public class SceneLoad
    {
        public static string LastScene;
        public static string TransitionScene = "Assets/Arcomage/Scenes/Transition.unity";

        static SceneLoad()
        {
            // Debug.Log($"RegisterPlayModeStateChanged");
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && !string.IsNullOrEmpty(LastScene))
            {
                // Debug.Log($"EnteredEditMode");

                EditorSceneManager.OpenScene(LastScene);
                LastScene = null;
                EditorWindow sceneWindow = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.SceneView,UnityEditor"));
                if (sceneWindow != null)
                {
                    sceneWindow.Focus();
                }
            }

            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Debug.Log($"ExitingEditMode");

                var activeScene = SceneManager.GetActiveScene();
                if (!string.IsNullOrEmpty(activeScene.name))
                {
                    LastScene = activeScene.path;
                    if (activeScene.name != Path.GetFileNameWithoutExtension(TransitionScene))
                    {
                        Transition.NextScene = activeScene.name;
                    }
                }

                EditorSceneManager.OpenScene(TransitionScene);
                EditorWindow gameWindow = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView,UnityEditor"));
                if (gameWindow != null)
                {
                    gameWindow.Focus();
                }
            }
        }
    }
}