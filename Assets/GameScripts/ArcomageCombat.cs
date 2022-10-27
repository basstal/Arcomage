using System;
using System.Reflection;
using GameScripts.Utils;
using TMPro;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameScripts
{
    public class ArcomageCombat : MonoBehaviour
    {
        public AssetReference databaseRef;
        public int firstPlayer = 1;

        private int m_round = 0;

        public GamePlayer m_player1;
        public GamePlayer m_player2;
        public GameCardCache gameCardCache;

        [NonSerialized] public GamePlayer currentPlayer;

        public static ArcomageDatabase Database;

        // public Button startButton;
        // public Button reloadButton;
        public RectTransform handCardLayout;

        public RectTransform cardDisappearPoint;
        // public RectTransform gameEndBlock;

        private Action currentStage;

        private bool playerSwitched;

        // [NonSerialized] public bool m_isGameEnded;

        [NonSerialized] public bool blockAction;
        // private string m_gameEnd;

        public GamePlayer FindEnemyById(int id)
        {
            return id == 1 ? m_player2 : m_player1;
        }

        private void Awake()
        {
            ArcomageCombat.Database = databaseRef.LoadAssetAsync<ArcomageDatabase>().WaitForCompletion();
            Assert.IsNotNull(Database);
            // gameEndBlock.gameObject.SetActive(false);
            // startButton.BindButtonEvent(GameStart);
            // reloadButton.BindButtonEvent(OnReload);
            // gameEndBlock.GetComponent<Button>().BindButtonEvent(OnReload);
        }

        private void Update()
        {
            // if (m_isGameEnded)
            // {
            //     return;
            // }
            // if (m_gameEnd != null)
            // {
            //     // gameEndBlock.gameObject.SetActive(true);
            //     // gameEndBlock.GetComponentInChildren<TextMeshProUGUI>().text = m_gameEnd;
            //     currentStage = null;
            // }

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

            m_round = 0;
            currentPlayer = firstPlayer == 1 ? m_player1 : m_player2;
            m_player1.ResetPlayer();
            m_player2.ResetPlayer();
            playerSwitched = false;
            currentStage = DisplayHandCards;
        }

        // public bool IsPlayerWin(GamePlayer target)
        // {
        //     return target.tower > 50 || FindEnemyById(target.playerID).tower <= 0;
        // }

        // public void IsGameEnded()
        // {
        //     var player1Win = IsPlayerWin(m_player1);
        //     var player2Win = IsPlayerWin(m_player2);
        //     if (player1Win || player2Win)
        //     {
        //         // m_gameEnd = player1Win && player2Win ? $"Peace End" : (player1Win ? $"player1Win" : "player2Win");
        //         m_isGameEnded = true;
        //         return;
        //     }
        //
        //     currentStage = DisplayHandCards;
        //     m_isGameEnded = false;
        // }

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
                handCard.gameObject.SetActive(true);
                handCard.transform.SetParent(handCardLayout, false);
                handCard.GetComponentInChildren<CanvasGroup>().alpha = 1;
                handCard.Refresh();
            }

            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(handCardLayout);
            currentStage = WaitCardUse;
        }

        public void WaitCardUse()
        {
            if (currentPlayer.trainingMode)
            {
                currentPlayer.RequestDecision();
            }
            if (currentPlayer.usingCard != null)
            {
                var usingCard = currentPlayer.usingCard;
                currentPlayer.RemoveFromHandCard(usingCard);
                usingCard.Apply();
                gameCardCache.TurnBack(usingCard);
                currentPlayer.usingCard = null;
                currentStage = IsNextPlayer;
            }
        }

        public void IsNextPlayer()
        {
            if (playerSwitched)
            {
                m_round++;
            }
            else
            {
                playerSwitched = true;
            }

            SharedLogics.PlayerResourceGrowthAll(currentPlayer);
            currentPlayer.WithdrawAnimation();
            if (currentPlayer.trainingMode)
            {
                currentPlayer.CalculateReward();
            }

            currentPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
            currentStage = DisplayHandCards;
        }
    }
}