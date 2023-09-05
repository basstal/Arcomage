// using NOAH.EditorUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NOAH.UI
{
    public class UIEditorUtil
    {
        [MenuItem("Assets/⛵NOAH/Refactor/Update Asset Adapter")]
        static void DoUpdate()
        {
            // EditorToolsBase.UpdateMultipleObjects(Selection.objects,
            //     (entry) =>
            //     {
            //         UIEditorUtil.UpdateUIPrefab(entry as GameObject, null);
            //     });
        }

        public static void UpdateUIPrefab(GameObject prefab, System.Func<GameObject, bool> processor, bool overrideOnly = true)
        {
            UIValidator.Enabled = false;

            var scenePath = AssetDatabase.GetAssetPath(EditorSettings.prefabUIEnvironment);
            var scene = EditorSceneManager.OpenScene(scenePath);
            var root = GameObject.Find("CanvasPreview");
            var instance = PrefabUtility.InstantiatePrefab(prefab, root.transform) as GameObject;
            bool processed = false;
            if (processor != null)
                processed = processor(instance);
            if (overrideOnly)
            {
                //transform 和RectTransform的部分属性不会被识别为override
                PrefabUtility.ApplyObjectOverride(instance, AssetDatabase.GetAssetPath(prefab),
                    InteractionMode.AutomatedAction);
            }
            else if (processed)
            {
                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            }
            UIValidator.Enabled = true;
        }

    }
}