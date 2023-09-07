using System.Diagnostics;
using System.IO;
using PackageManagerExtension;
using Sirenix.OdinInspector;
using UnityEngine;
using WhitericeEditor;

namespace Arcomage.GameEditorScripts
{
    [CreateAssetMenu(menuName = "ScriptableObjects/WhitericeConfig")]
    public class WhitericeConfig : WhitericeConfigBase
    {
        public override void GenerateConfig()
        {
            GenerateConfigJsonWithParameters(false, Path.Combine(Directory.GetCurrentDirectory(), "Whiterice/generate_config.py"));
        }

        [Button, FoldoutGroup("Config")]
        public void OpenConfigFile()
        {
            // 使用 Process 打开文件
            Process.Start(PackageManagerSettings.IDELocation, configJsonPath);
        }
    }
}