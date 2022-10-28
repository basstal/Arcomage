using Unity.VisualScripting;
using UnityEngine;

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
        public bool disallowDrop;
        public ScriptGraphAsset logic;
        [Multiline(20)] public string luaSource;

        [Multiline(5)] public string describe_en;
        [Multiline(5)] public string describe_cn;

        public Sprite sprite;
    }
}