using System.Collections.Generic;
using GameScripts.Utils;
using UnityEngine;
using Whiterice;

namespace GameScripts
{
    /// <summary>
    /// 缓存卡牌模板实例
    /// </summary>
    public class CardCache : MonoBehaviour
    {
        /// <summary>
        /// 从牌堆（缓存）中获得一张卡牌实例并设置所属的玩家和卡牌的实际数据
        /// </summary>
        /// <param name="inOwner">卡牌所属玩家</param>
        /// <param name="template">卡牌的实际数据</param>
        /// <returns>设置好的卡牌</returns>
        public Card Acquire(Player inOwner, ArcomageCard template)
        {
            Card card;
            if (transform.childCount > 0)
            {
                card = transform.GetChild(0).GetComponent<Card>();
                card.transform.SetParent(inOwner.combat.transform, true);
            }
            else
            {
                // var cardGameObject = Combat.Database.cardPrefabAssetRef.InstantiateAsync(inOwner.combat.transform).WaitForCompletion();
                var cardGameObject = AssetManager.Instance.InstantiatePrefab("Card", this, parent: inOwner.combat.transform);
                card = cardGameObject.GetComponent<Card>();
            }

            card.SetData(template);
            card.SetOwner(inOwner);
            return card;
        }

        /// <summary>
        /// 将卡牌实例放回牌堆
        /// </summary>
        /// <param name="inCard">卡牌实例</param>
        public void TurnBack(Card inCard)
        {
            inCard.transform.SetParent(transform, true);
            inCard.owner = null;
        }

        /// <summary>
        /// 生成一个随机的牌堆
        /// </summary>
        /// <returns>生成的牌堆</returns>
        public List<ArcomageCard> GenCardBank()
        {
            List<ArcomageCard> result = AssetManager.Instance.LoadAssets<ArcomageCard>(new [] { "Cards" }, this);
            // for (int i = 0; i < Combat.Database.cardsAssetRef.Count; ++i)
            // {
            //     var cardAssetRef = Combat.Database.cardsAssetRef[i];
            //     Assert.IsNotNull(cardAssetRef);
            //     ArcomageCard template = ArcomageDatabase.RetrieveObject<ArcomageCard>(cardAssetRef);
            //     Assert.IsNotNull(template);
            //     result.Add(template);
            // }

            result.Shuffle();
            if (result.Count == 0)
            {
                Debug.LogError($"GenCardBank with no card found!!");
            }
            return result;
        }
    }
}