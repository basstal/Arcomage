using System;
using System.Reflection;
using GameScripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameScripts
{
    public class GameMain : MonoBehaviour
    {
        public AssetReference databaseRef;
        public int firstPlayer = 1;

        private int m_round = 0;

        private GamePlayer m_player1;
        private GamePlayer m_player2;

        [NonSerialized] public GamePlayer currentPlayer;

        public static ArcomageDatabase Database;

        public Button startButton;
        public Button reloadButton;
        public RectTransform handCardLayout;
        public RectTransform cardDisappearPoint;
        public RectTransform gameEndBlock;

        public static GameMain Instance;

        private Action currentStage;

        private bool playerSwitched;

        private string m_gameEnd;

        public static GamePlayer FindEnemyById(int id)
        {
            return id == 1 ? Instance.m_player2 : Instance.m_player1;
        }

        private void Awake()
        {
            UnityEngine.Assertions.Assert.IsNull(Instance);
            Instance = this;
            GameMain.Database = databaseRef.LoadAssetAsync<ArcomageDatabase>().WaitForCompletion();
            Assert.IsNotNull(Database);
            gameEndBlock.gameObject.SetActive(false);
            startButton.BindButtonEvent(GameStart);
            reloadButton.BindButtonEvent(OnReload);
            gameEndBlock.GetComponent<Button>().BindButtonEvent(OnReload);
        }

        private void Update()
        {
            if (m_gameEnd != null)
            {
                gameEndBlock.gameObject.SetActive(true);
                gameEndBlock.GetComponentInChildren<TextMeshProUGUI>().text = m_gameEnd;
                currentStage = null;
            }

            if (currentStage != null)
            {
                currentStage.Invoke();
            }
        }

        public void GameStart()
        {
            startButton.gameObject.SetActive(false);
            m_round = 0;
            m_player1 = Database.player1PrefabAssetRef.InstantiateAsync(transform).WaitForCompletion().GetComponent<GamePlayer>();
            m_player2 = Database.player2PrefabAssetRef.InstantiateAsync(transform).WaitForCompletion().GetComponent<GamePlayer>();
            currentPlayer = firstPlayer == 1 ? m_player1 : m_player2;
            playerSwitched = false;
            currentStage = IsGameEnded;
        }

        public bool IsPlayerWin(GamePlayer target)
        {
            return target.tower > 50 || FindEnemyById(target.playerID).tower <= 0;
        }

        public void IsGameEnded()
        {
            var player1Win = IsPlayerWin(m_player1);
            var player2Win = IsPlayerWin(m_player2);
            if (player1Win || player2Win)
            {
                m_gameEnd = player1Win && player2Win ? $"Peace End" : (player1Win ? $"player1Win" : "player2Win");
            }

            currentStage = DisplayHandCards;
        }

        public void OnReload()
        {
#if UNITY_EDITOR
            ClearLog();
#endif
            SceneManager.LoadSceneAsync("GamePlay");
        }
#if UNITY_EDITOR
        static void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method?.Invoke(new object(), null);
        }
#endif
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
            }

            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(handCardLayout);
            currentStage = WaitCardUse;
        }

        public void WaitCardUse()
        {
            if (currentPlayer.usingCard != null)
            {
                var usingCard = currentPlayer.usingCard;
                currentPlayer.RemoveFromHandCard(usingCard);
                usingCard.Apply();
                GameCardCache.Instance.TurnBack(usingCard);
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
            currentPlayer = currentPlayer == m_player1 ? m_player2 : m_player1;
            currentStage = IsGameEnded;
        }
    }
}