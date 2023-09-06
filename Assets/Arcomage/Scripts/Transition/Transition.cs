using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whiterice;

namespace Arcomage.GameScripts
{
    public class Transition : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return AssetManager.Initialize();
            yield return AssetManager.Instance.LoadSceneAsync("StartMenu");
        }

        [MenuItem("Tools/Scene/Launch Transition")]
        public static void LaunchTransition()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (EditorSceneManager.SaveOpenScenes())
            {
                EditorSceneManager.OpenScene("Assets/Arcomage/Scenes/Transition.unity");
                EditorApplication.EnterPlaymode();
            }
        }
    }
}