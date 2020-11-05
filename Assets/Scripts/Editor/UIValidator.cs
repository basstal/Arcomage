using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[InitializeOnLoad]
public class UIValidator
{
    public static event System.Action<GameObject> OnPrefabUpdate;

    [DidReloadScripts]
    static void Init()
    {
        PrefabUtility.prefabInstanceUpdated = PrefabInstanceUpdated;
        var saveMethod = typeof(PrefabUtility).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.Name == "SaveAsPrefabAsset" && m.GetParameters().Length == 3);
        new MethodHooker(saveMethod,
                        typeof(UIValidator).GetMethod("PrefabAssetSaved", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static),
                        typeof(UIValidator).GetMethod("PrefabAssetSavedDelegate", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)).Install();
    }


    static void PrefabInstanceUpdated(GameObject prefab)
    {
        try
        {
            OnPrefabUpdate?.Invoke(prefab);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    static void PrefabAssetSaved(GameObject prefab, string assetPath, out bool success)
    {
        // ** 把所有没有EventSystemHandler的Graphic的raycastTarget字段给设置为false，以防止raycast遮挡
        Graphic[] graphics = prefab.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics)
        {
            graphic.raycastTarget = graphic.gameObject.GetComponent<IEventSystemHandler>() != null;
        }

        PrefabAssetSavedDelegate(prefab, assetPath, out success);
    }

    static void PrefabAssetSavedDelegate(GameObject prefab, string assetPath, out bool success)
    {
        success = false;
    }
}