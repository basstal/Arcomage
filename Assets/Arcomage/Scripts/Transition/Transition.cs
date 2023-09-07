using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
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
#if UNITY_EDITOR
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
#endif
    }
}