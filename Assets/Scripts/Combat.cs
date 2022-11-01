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

        public const int MAX_HAND_CARDS = 5;
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

        private int m_round;
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

        /// <summary>
        /// 显示当前手牌
        /// </summary>
        public void DisplayHandCards()
        {
            var displayIndex = 0;
            foreach (var handCard in currentPlayer.handCards)
            {
                if (!currentPlayer.trainingMode && !currentPlayer.isPlayAgain)
                {
                    currentPlayer.isAIWaitAnimation = true;
                    displayIndex = displayIndex == currentPlayer.lastRemovedIndex ? ++displayIndex : displayIndex;
                    handCard.PlayDisplayingCardAnim(displayIndex++, () => { currentPlayer.isAIWaitAnimation = false; });
                }
            }

            // UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(handCardLayout);
            m_currentStage = GenHandCards;
        }

        public void GenHandCards()
        {
            Assert.IsNotNull(currentPlayer);
            currentPlayer.isPlayAgain = currentPlayer.isDropping;
            if (currentPlayer.trainingMode)
            {
                currentPlayer.OnGenHandCards(MAX_HAND_CARDS - currentPlayer.handCards.Count, null);
                m_currentStage = currentPlayer.allCardsDisabled ? IsNextPlayer : WaitCardUse;
                return;
            }

            if (currentPlayer.handCards.Count < MAX_HAND_CARDS)
            {
                m_currentStage = null;
                currentPlayer.OnGenHandCards(MAX_HAND_CARDS - currentPlayer.handCards.Count, () => { m_currentStage = currentPlayer.allCardsDisabled ? IsNextPlayer : WaitCardUse; });
            }
            else
            {
                m_currentStage = currentPlayer.allCardsDisabled ? IsNextPlayer : WaitCardUse;
            }
        }


        public void WaitCardUse()
        {
            if (currentPlayer.trainingMode ||
                (currentPlayer.isAIControlling && !currentPlayer.isAIWaitAnimation))
            {
                currentPlayer.RequestDecision();
                currentPlayer.isAIWaitAnimation = true;
            }
            else // Inactive the block area when player use card.
            {
                handCardBlocking.gameObject.SetActive(false);
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

                if (IsGameEnd())
                {
                    m_currentStage = null;
                    return;
                }

                if (currentPlayer.trainingMode)
                {
                    currentPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
                    m_currentStage = DisplayHandCards;
                }
                else
                {
                    m_currentStage = null;
                    currentPlayer.WithdrawAnimation(() =>
                    {
                        currentPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
                        m_currentStage = DisplayHandCards;
                    });
                }
            }
        }

        public bool IsGameEnd()
        {
            if (currentPlayer.trainingMode && Database.learningGoal == MLAgentLearningGoal.BuildTower)
            {
                currentPlayer.CalculateReward();
            }

            ArcomagePlayer winningCond = ArcomageDatabase.RetrieveObject<ArcomagePlayer>(Database.winningAssetRef);
            var enemyPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
            var gameEnd = false;
            if (winningCond.IsPlayerWin(currentPlayer) || winningCond.IsPlayerLose(enemyPlayer))
            {
                if (currentPlayer.trainingMode && Database.learningGoal == MLAgentLearningGoal.WinCombat)
                {
                    currentPlayer.CalculateReward();
                }

                gameEnd = true;
            }
            else if (winningCond.IsPlayerLose(currentPlayer) || winningCond.IsPlayerWin(enemyPlayer))
            {
                if (enemyPlayer.trainingMode && Database.learningGoal == MLAgentLearningGoal.WinCombat)
                {
                    enemyPlayer.CalculateReward();
                }

                gameEnd = true;
            }

            if (gameEnd)
            {
                if (currentPlayer.trainingMode)
                {
                    currentPlayer.EndEpisode();
                }

                if (enemyPlayer.trainingMode)
                {
                    enemyPlayer.EndEpisode();
                }

                StartMenu startMenu = GameObject.Find("StartMenu")?.GetComponent<StartMenu>();
                if (startMenu != null)
                {
                    var winner = currentPlayer == m_player1 ? "Player" : "Enemy";
                    startMenu.ShowGameEnd($"{winner} Win!!", winner == "Player" ? Color.red : Color.green);
                }

                return true;
            }

            return false;
        }
    }
}