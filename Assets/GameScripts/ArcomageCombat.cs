using System;
using GameScripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

namespace GameScripts
{
    public class ArcomageCombat : MonoBehaviour
    {
        public static ArcomageDatabase Database;

        public AssetReference databaseRef;
        public int firstPlayer = 1;
        public GamePlayer m_player1;
        public GamePlayer m_player2;
        public GameCardCache gameCardCache;
        public RectTransform handCardLayout;
        public RectTransform cardDisappearPoint;
        public RectTransform handCardBlocking;

        [NonSerialized] public GamePlayer currentPlayer;
        [NonSerialized] public bool blockAction;

        private int m_round = 0;
        private Action m_currentStage;
        private bool m_playerSwitched;

        public GamePlayer FindEnemyById(int id)
        {
            return id == 1 ? m_player2 : m_player1;
        }

        private void Awake()
        {
            ArcomageCombat.Database = databaseRef.LoadAssetAsync<ArcomageDatabase>().WaitForCompletion();
            Assert.IsNotNull(Database);
        }

        private void Update()
        {
            if (m_currentStage != null)
            {
                m_currentStage.Invoke();
            }
        }

        private void Start()
        {
            GameReset();
        }

        public void GameReset()
        {
            if (!m_player1.trainingMode && !m_player2.trainingMode)
            {
                Log.LogInfo("游戏重置", "游戏重置");
            }

            m_round = 0;
            currentPlayer = firstPlayer == 1 ? m_player1 : m_player2;
            m_player1.ResetPlayer();
            m_player2.ResetPlayer();
            m_playerSwitched = false;
            m_currentStage = DisplayHandCards;
        }

        public void DisplayHandCards()
        {
            Assert.IsNotNull(currentPlayer);
            if (currentPlayer.handCards.Count == 0)
            {
                currentPlayer.OnGenHandCards(5);
            }

            Assert.IsTrue(currentPlayer.handCards.Count > 0);

            foreach (var handCard in currentPlayer.handCards)
            {
                handCard.OnDisplay();
                if (!currentPlayer.trainingMode)
                {
                    handCard.PlayDisplayingCardAnim();
                }
            }

            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(handCardLayout);
            handCardBlocking.gameObject.SetActive(false);
            m_currentStage = WaitCardUse;
        }

        public void WaitCardUse()
        {
            if (currentPlayer.trainingMode ||
                (currentPlayer.isAIControlling && !currentPlayer.isAIWaitAnimation))
            {
                currentPlayer.RequestDecision();
                currentPlayer.isAIWaitAnimation = true;
            }

            if (currentPlayer.usingCard != null)
            {
                var usingCard = currentPlayer.usingCard;
                currentPlayer.RemoveFromHandCard(usingCard);
                if (!currentPlayer.isDropping)
                {
                    usingCard.Apply();
                }

                currentPlayer.isDropping = false;
                currentPlayer.usingCard = null;
                // ** trainingMode have no animation
                if (currentPlayer.trainingMode)
                {
                    gameCardCache.TurnBack(usingCard);
                    m_currentStage = IsNextPlayer;
                }
                else
                {
                    usingCard.PlayUsingCardAnim(() =>
                    {
                        currentPlayer.isAIWaitAnimation = false;
                        gameCardCache.TurnBack(usingCard);
                        m_currentStage = IsNextPlayer;
                    });
                }
            }
        }

        public void IsNextPlayer()
        {
            if (currentPlayer.isPlayAgain)
            {
                handCardBlocking.gameObject.SetActive(false);
                currentPlayer.isPlayAgain = false;
                // ** 刷新一遍展示效果，因为有新抽的卡以及不能扔的卡
                foreach (var handCard in currentPlayer.handCards)
                {
                    handCard.OnDisplay();
                }

                m_currentStage = WaitCardUse;
            }
            else
            {
                if (m_playerSwitched)
                {
                    m_round++;
                }
                else
                {
                    m_playerSwitched = true;
                }

                SharedLogics.PlayerResourceGrowthAll(currentPlayer);
                currentPlayer.WithdrawAnimation();
                if (currentPlayer.trainingMode)
                {
                    currentPlayer.CalculateReward();
                }

                currentPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
                m_currentStage = DisplayHandCards;
            }
        }
    }
}