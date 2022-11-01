using Unity.VisualScripting;
using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 卡牌数据，这个对象不应该在运行时被修改
    /// </summary>
    [CreateAssetMenu(fileName = "ArcomageCard", menuName = "ScriptableObjects/ArcomageCard", order = 1)]
    public class ArcomageCard : ScriptableObject
    {
        [Tooltip("卡牌英文名")] public string cardName;
        [Tooltip("卡牌中文名")] public string cardName_cn;
        [Tooltip("唯一id")] public int id;
        [Tooltip("卡牌所属的资源类型")] public CostType costType;
        [Tooltip("出牌消耗的资源数量")] public int cost;
        [Tooltip("是否不允许弃牌")] public bool disallowDrop;
        [Tooltip("卡牌的执行逻辑")] public ScriptGraphAsset logic;
        [Tooltip("卡牌英文描述"), Multiline(5)] public string describe_en;
        [Tooltip("卡牌中文描述"), Multiline(5)] public string describe_cn;
        [Tooltip("卡牌对应的精灵图")] public Sprite sprite;
    }
}