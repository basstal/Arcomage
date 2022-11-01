using UnityEngine;
using UnityEngine.Assertions;

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
            Card card = null;
            if (transform.childCount > 0)
            {
                card = transform.GetChild(0).GetComponent<Card>();
            }
            else
            {
                var cardGameObject = Combat.Database.cardPrefabAssetRef.InstantiateAsync(inOwner.combat.transform).WaitForCompletion();
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
            // Assert.IsTrue(inCard.transform.parent != this.transform);
            // inCard.transform.SetParent(transform, false);
            inCard.owner = null;
        }
    }
}