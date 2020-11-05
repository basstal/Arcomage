using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

public static class AssetUtility
{
    public enum AssetLoadFailedReason
    {
        NotFound,
        InvalidAssetBundle,
        AssetNotInBundle,
    }
    static void OnAssetLoadFailed(string refPath, string assetName, AssetLoadFailedReason reason, object retainer = null)
    {
        switch (reason)
        {
            case AssetLoadFailedReason.NotFound:
                LogUtility.LogWarning("Asset", $"Asset not found '{refPath}'", retainer as Object);
                break;
            case AssetLoadFailedReason.InvalidAssetBundle:
                LogUtility.LogWarning("Asset", $"Asset bundle load failed '{refPath}'", retainer as Object);
                break;
            case AssetLoadFailedReason.AssetNotInBundle:
                LogUtility.LogWarning("Asset", $"Asset bundle '{refPath}' was load but it doesn't contain asset '{assetName}'", retainer as Object);
                break;
        }
    }
    static GameObject DoInstantiate(GameObject prefab, System.Action<GameObject> callback = null, Vector3? position = default, Quaternion? rotation = default, Transform parent = null)
    {
        GameObject result = null;
        if (prefab != null)
        {
            result = GameObject.Instantiate(prefab);
            result.name = prefab.name;
            if (position != null)
            {
                result.transform.localPosition = position.Value;
            }
            if (rotation != null)
            {
                result.transform.localRotation = rotation.Value;
            }
            if (parent != null)
            {
                result.transform.SetParent(parent);
            }
        }
        callback?.Invoke(result);
        return result;
    }
    public static GameObject InstantiatePrefab(string refPath, System.Action<GameObject> callback = null, Vector3? position = default, Quaternion? rotation = default, Transform parent = null)
    {
#if UNITY_EDITOR
        refPath = "Assets/Resources/" + refPath;
        var prefab = LoadAsset<GameObject>(refPath + ".prefab", null);
#else
        refPath = refPath.Replace(".prefab", "");
        var prefab = LoadResource<GameObject>(refPath);
#endif
        return DoInstantiate(prefab, callback, position, rotation, parent);
    }

    public static Texture2D LoadTexture2D(string refPath)
    {
#if UNITY_EDITOR
        refPath = "Assets/Resources/" + refPath;
        Debug.Log("refPath : " + refPath);
        var texture = LoadAsset<Texture2D>(refPath + ".png", null);
#else
        refPath = refPath.Replace(".png", "");
        var texture = LoadResource<Texture2D>(refPath);
#endif
        return texture;
    }

    public static T LoadResource<T>(string resource) where T : Object
    {
        var asset = Resources.Load(resource) as T;
        return asset;
    }
#if UNITY_EDITOR
    public static T LoadAsset<T>(string refPath, string assetName) where T : Object
    {
        Object result = null;
        if (refPath != null)
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(refPath);
            if (allAssets != null)
            {
                if (assetName == null)
                {
                    result = allAssets.First(a => AssetDatabase.IsMainAsset(a));
                }
                else
                {
                    foreach (var asset in allAssets)
                    {
                        if (asset.name == assetName && asset is T)
                        {
                            result = asset;
                            break;
                        }
                    }
                }
            }
        }
        if (result == null)
        {
            OnAssetLoadFailed(refPath, assetName, AssetLoadFailedReason.NotFound);
        }

        return result as T;
    }
#endif

    // ** todo 这些是做本地化可能需要的 按本地化路径加载资源
    //     public List<string> GetCandidatePaths(string path)
    //     {
    //         List<string> result = null;

    //         if (path != null)
    //         {
    //             result = new List<string>();
    // #if UNITY_EDITOR_WIN
    //             var platformSubstitute =  "__windows";
    // #elif UNITY_EDITOR_OSX
    //             var platformSubstitute = "__mac";
    // #elif UNITY_ANDROID
    //             var platformSubstitute = "__android";
    // #elif UNITY_IOS
    //             var platformSubstitute = "__ios";
    // #else
    //             var platformSubstitute = null;
    // #endif

    // #if UNITY_EDITOR
    //             LogUtility.Assert(!path.Contains($"{platformSubstitute}/"), "Explicit platform specified resource path is not allowed: " + path);
    //             LogUtility.Assert(!path.Contains("i18n/"), "Explicit localed resource path is not allowed: " + path);
    // #endif
    //             var platformSubstitePath = Path.Combine(Path.GetDirectoryName(path), Path.Combine(platformSubstitute, Path.GetFileName(path)));
    // #if UNITY_EDITOR_WIN
    //             platformSubstitePath = platformSubstitePath.Replace("\\", "/");
    // #endif
    //             result.Add(GetSubstitutePath(platformSubstitePath));
    //             result.Add(platformSubstitePath);
    //             result.Add(GetSubstitutePath(path));
    //             result.Add(path);
    //         }

    //         return result;
    //     }

    //     public string GetSubstitutePath(string path)
    //     {
    //         string result = null;

    //         if (!string.IsNullOrEmpty(path))
    //         {
    //             result = Path.Combine(Path.GetDirectoryName(path), Path.Combine(LocaleUtility.i18nReplacement, Path.GetFileName(path)));
    // #if UNITY_EDITOR_WIN
    //             result = result.Replace("\\", "/");
    // #endif
    //         }
    //         return result;
    //     }
}