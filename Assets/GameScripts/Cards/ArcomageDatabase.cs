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
    }
}