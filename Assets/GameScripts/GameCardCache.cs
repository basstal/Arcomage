using UnityEngine;
using UnityEngine.Assertions;

namespace GameScripts
{
    public class GameCardCache : MonoBehaviour
    {
        public static GameCardCache Instance;

        private void Awake()
        {
            Instance = this;
        }

        public GameCard Acquire(GamePlayer inOwner, ArcomageCard template)
        {
            GameCard gameCard = null;
            if (transform.childCount > 0)
            {
                gameCard = transform.GetChild(0).GetComponent<GameCard>();
            }
            else
            {
                var cardGameObject = GameMain.Database.cardPrefabAssetRef.InstantiateAsync().WaitForCompletion();
                gameCard = cardGameObject.GetComponent<GameCard>();
            }

            gameCard.SetData(template);
            gameCard.SetOwner(inOwner);

            return gameCard;
        }

        public void TurnBack(GameCard inCard)
        {
            Assert.IsTrue(inCard.transform.parent != this.transform);
            inCard.transform.SetParent(transform);
            inCard.owner = null;
        }
    }
}