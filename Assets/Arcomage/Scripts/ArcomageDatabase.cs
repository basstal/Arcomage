using UnityEngine;

namespace Arcomage.GameScripts
{
    /// <summary>
    /// 一个数据库包含一场游戏所需的所有数据
    /// </summary>
    [CreateAssetMenu(fileName = "ArcomageDatabase", menuName = "ScriptableObjects/ArcomageDatabase", order = 1)]
    public class ArcomageDatabase : ScriptableObject
    {
        [Tooltip("本地化类型")] public Localization localization;
        [Tooltip("MLAgent学习目标")] public MLAgentLearningGoal learningGoal;
        // [Tooltip("卡牌模板")] public AssetReference cardPrefabAssetRef;
        // [Tooltip("砖块精灵图资源")] public AssetReferenceSprite brickAssetRef;
        // [Tooltip("宝石精灵图资源")] public AssetReferenceSprite gemAssetRef;
        // [Tooltip("怪兽精灵图资源")] public AssetReferenceSprite recruitAssetRef;

        // [Tooltip("索引到卡牌数据"), SerializeReference]
        // public List<AssetReference> cardsAssetRef;

        // [Tooltip("索引到难度数据"), SerializeReference]
        // public List<AssetReference> difficultyAssetRef;

        // [Tooltip("索引到游戏胜利条件数据"), SerializeReference]
        // public AssetReference winningAssetRef;

        // /// <summary>
        // /// 从 AssetReference 获得对应资源并转型后返回
        // /// </summary>
        // /// <param name="assetReference">Addressable资源引用</param>
        // /// <typeparam name="T">转型的目标类型</typeparam>
        // /// <returns><see cref="assetReference"/>对应资源的转型</returns>
        // public static T RetrieveObject<T>(AssetReference assetReference) where T : Object
        // {
        //     if (assetReference == null)
        //     {
        //         Debug.LogWarning($"ArcomageDatabase RetrieveObject with null assetReference ??");
        //         return null;
        //     }
        //
        //     if (!assetReference.IsValid())
        //     {
        //         return assetReference.LoadAssetAsync<T>().WaitForCompletion();
        //     }
        //
        //     return (T)assetReference.OperationHandle.Result;
        // }
    }
}