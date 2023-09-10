using System.Collections;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Whiterice;

namespace Arcomage.GameScripts
{
    public class Transition : MonoBehaviour
    {
        public static string NextScene;

        private IEnumerator Start()
        {
            CommunicatorFactory.Enabled = false;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
            yield return AssetManager.Initialize();
            yield return AssetManager.SwitchMode(true);
            var reporter = AssetManager.Instance.InstantiatePrefab("Debug_Reporter");
            DontDestroyOnLoad(reporter);
#if UNITY_EDITOR
            yield return AssetManager.Instance.LoadSceneAsync(NextScene ?? "GamePlay");
#else
            yield return AssetManager.Instance.LoadSceneAsync("StartMenu");
#endif
        }

        private void Update()
        {
            // if (EventSystem.current.currentSelectedGameObject)
            // {
            //     Debug.Log("Currently selected object: " + EventSystem.current.currentSelectedGameObject.name);
            // }

            // // 在每次鼠标点击时检查
            // if (Input.GetMouseButtonDown(0))
            // {
            //     PointerEventData pointerData = new PointerEventData(EventSystem.current)
            //     {
            //         position = Input.mousePosition
            //     };
            //
            //     // 存储射线投射结果的列表
            //     System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
            //     EventSystem.current.RaycastAll(pointerData, results);
            //
            //     // 打印所有命中的对象
            //     foreach (RaycastResult result in results)
            //     {
            //         Debug.Log("Hit: " + result.gameObject.name, result.gameObject);
            //     }
            // }
        }
    }
}