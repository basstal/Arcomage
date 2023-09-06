using Localization;
using UnityEngine;

namespace Arcomage.GameScripts
{
    public class LocalizationSettings : ILocaleSettings
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Initialize()
        {
            LocalizationManager.Initialize(new LocalizationSettings());
        }

        public byte[] RawData { get; } = new byte[0];
        public string BudouDatabase { get; } = "";
        public string Language { get; } = "chs";
    }
}