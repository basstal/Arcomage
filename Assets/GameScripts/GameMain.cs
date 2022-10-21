using System;
using System.Reflection;
using DG.Tweening;
using GameScripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameScripts
{
    public delegate void PlayerSwitchDelegate(bool result, GameCard previousCard);

    public class GameMain : MonoBehaviour
    {
        public AssetReference databaseRef;
        public int firstPlayer = 1;

        private int m_round = 0;
        private const int CARD_CACHE_SIZE = 20;
        private GameObject m_player1Obj;
        private GameObject m_player2Obj;


        public static ArcomageDatabase Database;
        // public Transform CardObjCacheRoot { get; private set; }
        // public Dictionary<int, Sprite> ID2Sprite { get; private set; }

        [HideInInspector] public Sprite brick;
        [HideInInspector] public Sprite gem;
        [HideInInspector] public Sprite recruit;

        public Button startButton;
        public Button reloadButton;
        public RectTransform handCardLayout;
        public RectTransform cardDisappearPoint;

        public static PlayerSwitchDelegate PlayerSwitch;
        public static GameMain Instance;

        private Action currentStage;
        private bool playerSwitched;
        private int currentPlayerId;

        public static GamePlayer FindPlayerById(int id)
        {
            return (id == 1 ? Instance.m_player1Obj : Instance.m_player2Obj).GetComponent<GamePlayer>();
        }

        public static GamePlayer FindEnemyById(int id)
        {
            return (id == 1 ? Instance.m_player2Obj : Instance.m_player1Obj).GetComponent<GamePlayer>();
        }

        public void SetCurrentPlayer(int playerId)
        {
            // DataBinding.Instance.SetData("CurrentPlayer", playerId);
            currentPlayerId = playerId;
        }

        private void Awake()
        {
            UnityEngine.Assertions.Assert.IsNull(Instance);
            Instance = this;
            GameMain.Database = databaseRef.LoadAssetAsync<ArcomageDatabase>().WaitForCompletion();
            Assert.IsNotNull(Database);
            startButton.BindButtonEvent(GameStart);
            reloadButton.BindButtonEvent(OnReload);

            PlayerSwitch += OnPlayerSwitch;
        }

        private void OnDestroy()
        {
            PlayerSwitch -= OnPlayerSwitch;
        }

        private void Update()
        {
            if (currentStage != null)
            {
                currentStage.Invoke();
            }
        }

        public void GameStart()
        {
            startButton.gameObject.SetActive(false);
            m_round = 0;
            m_player1Obj = Database.player1PrefabAssetRef.InstantiateAsync(transform).WaitForCompletion();
            m_player2Obj = Database.player2PrefabAssetRef.InstantiateAsync(transform).WaitForCompletion();
            // Init();
            currentStage = TurnRound;


            SetCurrentPlayer(0);
            playerSwitched = false;
        }

        public void OnReload()
        {
            // GameObject singletonLoaderGameObject = GameObject.Find("/SingletonLoader");
            // SingletonLoader singletonLoader = singletonLoaderGameObject.GetComponent<SingletonLoader>();
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
        public void TurnRound()
        {
            m_round++;
            if (m_round == 1)
            {
                GamePlayer.GenHandCards.Invoke(5, 0);
                SetCurrentPlayer(firstPlayer);
            }
            else
            {
                GamePlayer.RecycleHandCards.Invoke();
                GamePlayer.GenHandCards.Invoke(5, 0);
                SetCurrentPlayer(currentPlayerId % 2 + 1);
                // local player = DB.GetData("Player{CurrentPlayer}")
                // local U = require("Utils")
                SharedLogics.PlayerResourceGrowthAll(FindPlayerById(currentPlayerId));
            }

            currentStage = PlayerRound;
        }

        public void PlayerRound()
        {
            // local playerId = DB.GetData("CurrentPlayer")
            // local handCards = DB.GetData(string.format("Player%s/HandCards", playerId))
            GamePlayer currentPlayer = FindPlayerById(currentPlayerId);

            // local parentTrans = REF.HandCardsLayout.transform
            // local cardObjCacheRoot = GameMainCS.CardObjCacheRoot
            foreach (var handCard in currentPlayer.handCards)
            {
                // **此卡已在手牌中
                if (handCard.transform.parent == handCardLayout)
                {
                    var tweenComponent = handCard.GetComponentInChildren<DOTweenAnimation>();
                    tweenComponent.DORestartById(currentPlayerId == 1 ? "left2right" : "right2left");
                    tweenComponent.DORestartById("alpha");
                }
                else
                {
                    handCard.transform.SetParent(handCardLayout, false);
                    handCard.GetComponentInChildren<CanvasGroup>().alpha = 1;
                }
            }

            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(handCardLayout);
            currentStage = null;
            // local player = DB.GetData("Player{CurrentPlayer}")
            // if player.GamePlayerCS.useAI then
            // player.bevTree:Reset()
            // player.bevTreeEnabled = true
            // end
        }

        public void OnPlayerSwitch(bool playAgain, GameCard previousCard)
        {
            // ** 再次出牌直接不改变当前玩家进入PlayerRound
            if (playAgain)
            {
                // local player = DB.GetData("Player{CurrentPlayer}")
                GamePlayer currentPlayer = FindPlayerById(currentPlayerId);
                int removedIndex = currentPlayer.UseHandCard(previousCard);
                currentPlayer.OnGenHandCards(1, removedIndex);
                currentStage = PlayerRound;
                return;
            }

            if (playerSwitched)
            {
                currentStage = TurnRound;
            }
            else
            {
                GamePlayer currentPlayer = FindPlayerById(currentPlayerId);
                // local player = DB.GetData("Player{CurrentPlayer}")
                currentPlayer.OnRecycleHandCards();

                SetCurrentPlayer(currentPlayerId % 2 + 1);
                playerSwitched = true;
                currentStage = PlayerRound;
            }
        }
    }
}