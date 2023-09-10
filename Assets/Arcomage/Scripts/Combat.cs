using System;
using System.Collections.Generic;
using Arcomage.GameScripts.UI.Runtime;
using Arcomage.GameScripts.Utils;
using Localization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using Whiterice;

namespace Arcomage.GameScripts
{
    public class Combat : MonoBehaviour
    {
        public ArcomageDatabase Database;

        public int firstPlayer = 1;
        public Player m_player1;
        public Player m_player2;
        public RectTransform handCardLayout;
        public RectTransform handCardBlocking;
        public RectTransform center;
        public CardCache cardCache;

        [NonSerialized] public EffectCache effectCache;
        [NonSerialized] public Player currentPlayer;
        [NonSerialized] public bool blockAction;
        [NonSerialized] public List<ArcomageCard> cardBank;
        [NonSerialized] public GameObject background;
        public Action currentStage;

        public Player FindEnemy(Player player)
        {
            return player == m_player1 ? m_player2 : m_player1;
        }

        private void Awake()
        {
            background = transform.Find("Background").gameObject;
            Assert.IsNotNull(background);
            center.gameObject.SetActive(false);
            Database = AssetManager.Instance.LoadAsset<ArcomageDatabase>("GameSettings_ArcomageDatabase", this);
            Assert.IsNotNull(Database);
            cardCache = GetComponentInChildren<CardCache>(true);
            effectCache = GetComponentInChildren<EffectCache>(true);
        }

        private void Update()
        {
            if (currentStage != null)
            {
                currentStage.Invoke();
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

            cardBank = cardCache.GenCardBank();
            currentPlayer = firstPlayer == 1 ? m_player1 : m_player2;
            m_player1.ResetPlayer();
            m_player2.ResetPlayer();
            currentStage = DisplayCurrentPlayerHandCards;
        }

        public void DisplayCurrentPlayerHandCards()
        {
            currentPlayer.DisplayHandCards();
            currentStage = GenCurrentPlayerHandCards;
        }


        public void GenCurrentPlayerHandCards()
        {
            currentStage = null;
            bool finished = currentPlayer.GenHandCards();
            if (finished)
            {
                ChangeStage();
            }
        }


        public void ChangeStage()
        {
            currentStage = currentPlayer.allCardsDisabled ? IsNextPlayer : WaitCardUse;
        }

        public void WaitCardUse()
        {
            if (currentPlayer.WaitCardUse())
            {
                currentStage = IsNextPlayer;
            }
        }

        public void UsingCardComplete(Card turnBackCard)
        {
            // Debug.LogWarning($"UsingCardBackCallback called");
            currentPlayer.isAIWaitAnimation = false;
            cardCache.TurnBack(turnBackCard);
            currentStage = IsNextPlayer;
        }


        public void IsNextPlayer()
        {
            if (currentPlayer.isPlayAgain)
            {
                currentStage = DisplayCurrentPlayerHandCards;
            }
            else
            {
                SharedLogics.PlayerResourceGrowthAll(currentPlayer);

                currentStage = null;
                if (IsGameEnd())
                {
                    return;
                }

                currentPlayer.WithdrawCards();
            }
        }

        public void OnWithdrawComplete()
        {
            currentPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
            currentStage = DisplayCurrentPlayerHandCards;
        }

        public bool IsGameEnd()
        {
            if (currentPlayer.trainingMode && Database.learningGoal == MLAgentLearningGoal.BuildTower)
            {
                currentPlayer.CalculateReward();
            }

            ArcomagePlayer winningCond = AssetManager.Instance.LoadAsset<ArcomagePlayer>("GameSettings_WinningCond", this);
            var enemyPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
            var gameEnd = false;
            if (winningCond.IsPlayerWin(currentPlayer) || winningCond.IsPlayerLose(enemyPlayer))
            {
                if (currentPlayer.trainingMode && Database.learningGoal == MLAgentLearningGoal.WinCombat)
                {
                    currentPlayer.CalculateReward();
                }

                if (winningCond.IsPlayerWin(currentPlayer))
                {
                    Debug.LogWarning($"Game End, winner is {currentPlayer.name}");
                }

                gameEnd = true;
            }
            else if (winningCond.IsPlayerLose(currentPlayer) || winningCond.IsPlayerWin(enemyPlayer))
            {
                if (enemyPlayer.trainingMode && Database.learningGoal == MLAgentLearningGoal.WinCombat)
                {
                    enemyPlayer.CalculateReward();
                }

                if (winningCond.IsPlayerLose(currentPlayer))
                {
                    Debug.LogWarning($"Game End, winner is {enemyPlayer.name}");
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

                center.gameObject.SetActive(true);
                LocaledText title = center.Find("Title").GetOrAddComponent<LocaledText>();
                title.key = "PlayerWin";
                LocaledText text = center.Find("Text").GetOrAddComponent<LocaledText>();
                text.key = "PlayerWin_Text";
                text.resolveParameters = () => { return new object[] { currentPlayer == m_player1 ? "Player" : "Enemy" }; };

                return true;
            }

            return false;
        }
    }
}