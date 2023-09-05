// using NOAH.Asset;
// using NOAH.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
// using NOAH.Core;

namespace NOAH.UI
{
    [Serializable]
    public class TextureMetricsEntry
    {
        public string path;
        public int width;
        public int height;
    }

    public class TextureMetrics : ScriptableObject
    {
        [SerializeField] TextureMetricsEntry[] _entries;

        Dictionary<string, TextureMetricsEntry> _lookup;

        public TextureMetricsEntry GetTextureMetrics(string refPath)
        {
            TextureMetricsEntry result = null;
            if (_lookup == null)
            {
                _lookup = new Dictionary<string, TextureMetricsEntry>(StringComparer.OrdinalIgnoreCase);
                if (_entries != null)
                    foreach (var entry in _entries)
                    {
                        _lookup[entry.path] = entry;
                    }
            }

#if UNITY_EDITOR
            // var assetPath = AssetBuilder.GetAssetDatabasePath<Texture2D>(refPath);
            // var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            // if (importer != null)
            // {
            //     result = new TextureMetricsEntry();
            //     EditorUtils.EditorToolsBase.GetTextureOriginalSize(importer, out result.width, out result.height);
            //     result.path = refPath;
            // }
#else
            _lookup.TryGetValue(refPath, out result);
#endif
            return result;
        }

#if UNITY_EDITOR

        [Button]
        // [Build.OnBeforeBuildResource]
        static void Rebuild()
        {
            // var settings = EditorAsset.LoadAsset<AssetImportSettings>();
            // var textureMetric = EditorAsset.LoadOrCreateAsset<TextureMetrics>(settings.UITextureMetricsPath);
            // textureMetric._lookup = null;

            // List<TextureMetricsEntry> entries = new List<TextureMetricsEntry>();
            // foreach (var assetPath in Directory.GetFiles(settings.UITextureSearchPath, "*", SearchOption.AllDirectories))
            // {
            //     if (!assetPath.EndsWithOrdinal(".meta") && !assetPath.Contains("%"))
            //     {
            //         var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            //         if (importer != null)
            //         {
            //             var entry = new TextureMetricsEntry();
            //             EditorUtils.EditorToolsBase.GetTextureOriginalSize(importer, out entry.width, out entry.height);
            //             entry.path = AssetBuilder.AssetToBundlePath(assetPath);
            //             entries.Add(entry);
            //         }
            //     }
            // }
            //
            // entries.Sort((a, b) => a.path.CompareTo(b.path));
            // textureMetric._entries = entries.ToArray();
            // EditorUtility.SetDirty(textureMetric);
        }

        [Button]
        void CleanUp()
        {
            _entries = null;
            _lookup = null;
        }

#endif
    }
}