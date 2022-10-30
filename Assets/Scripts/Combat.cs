using System;
using GameScripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

namespace GameScripts
{
    public class Combat : MonoBehaviour
    {
        public static ArcomageDatabase Database;

        public AssetReference databaseRef;
        public int firstPlayer = 1;
        public Player m_player1;
        public Player m_player2;
        public RectTransform handCardLayout;
        public RectTransform cardDisappearPoint;
        public RectTransform handCardBlocking;

        [NonSerialized] public CardCache cardCache;
        [NonSerialized] public Player currentPlayer;
        [NonSerialized] public bool blockAction;

        private int m_round = 0;
        private Action m_currentStage;
        private bool m_playerSwitched;

        public Player FindEnemyById(int id)
        {
            return id == 1 ? m_player2 : m_player1;
        }

        private void Awake()
        {
            Combat.Database = databaseRef.LoadAssetAsync<ArcomageDatabase>().WaitForCompletion();
            Assert.IsNotNull(Database);
            cardCache = GetComponentInChildren<CardCache>();
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
            if (currentPlayer.handCards.Count < 5)
            {
                currentPlayer.OnGenHandCards(5 - currentPlayer.handCards.Count);
            }

            Assert.IsTrue(currentPlayer.handCards.Count > 0);

            foreach (var handCard in currentPlayer.handCards)
            {
                handCard.OnDisplay();
                if (!currentPlayer.trainingMode && !currentPlayer.isPlayAgain)
                {
                    currentPlayer.isAIWaitAnimation = true;
                    handCard.PlayDisplayingCardAnim(() => { currentPlayer.isAIWaitAnimation = false; });
                }
            }

            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(handCardLayout);
            handCardBlocking.gameObject.SetActive(false);
            currentPlayer.isPlayAgain = currentPlayer.isDropping;
            m_currentStage = currentPlayer.allCardsDisabled ? IsNextPlayer : WaitCardUse;
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
                else
                {
                    currentPlayer.isDropping = false;
                }

                currentPlayer.usingCard = null;
                // ** trainingMode have no animation
                if (currentPlayer.trainingMode)
                {
                    cardCache.TurnBack(usingCard);
                    m_currentStage = IsNextPlayer;
                }
                else
                {
                    usingCard.PlayUsingCardAnim(() =>
                    {
                        currentPlayer.isAIWaitAnimation = false;
                        cardCache.TurnBack(usingCard);
                        m_currentStage = IsNextPlayer;
                    });
                }
            }
        }

        public void IsNextPlayer()
        {
            if (currentPlayer.isPlayAgain)
            {
                m_currentStage = DisplayHandCards;
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