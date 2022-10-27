using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameScripts
{
    public enum Localization
    {
        EN,
        CN
    }

    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    [CreateAssetMenu(fileName = "ArcomageDatabase", menuName = "ScriptableObjects/ArcomageDatabase", order = 1)]
    public class ArcomageDatabase : ScriptableObject
    {
        public Localization localization;
        public AssetReference player1PrefabAssetRef;
        public AssetReference player2PrefabAssetRef;
        public AssetReference cardPrefabAssetRef;
        public AssetReferenceSprite brickAssetRef;
        public AssetReferenceSprite gemAssetRef;
        public AssetReferenceSprite recruitAssetRef;
        [SerializeReference] public List<AssetReference> cardsAssetRef;
        [SerializeReference] public List<AssetReference> difficultyAssetRef;

        public static T RetrieveObject<T>(AssetReference assetReference) where T : UnityEngine.Object
        {
            if (!assetReference.IsValid())
            {
                return assetReference.LoadAssetAsync<T>().WaitForCompletion();
            }

            return (T)assetReference.OperationHandle.Result;
        }
    }
}