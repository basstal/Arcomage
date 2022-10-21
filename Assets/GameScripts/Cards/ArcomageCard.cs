using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameScripts
{
    [CreateAssetMenu(fileName = "ArcomageCard", menuName = "ScriptableObjects/ArcomageCard", order = 1)]
    public class ArcomageCard : ScriptableObject
    {
        public string cardName;
        public string cardName_cn;
        public int id;
        public CostType costType;
        public int cost;
        public ScriptGraphAsset logic;
        [Multiline(20)] public string luaSource;

        public string describe_en;
        public string describe_cn;

        public Sprite sprite;
    }
}