using UnityEditor.AddressableAssets.Settings;

namespace GameEditorScripts
{
    public static class BuildAssetBundle
    {
        public static void BuildByAddressable()
        {
            AddressableAssetSettings.BuildPlayerContent();
        }
    }
}