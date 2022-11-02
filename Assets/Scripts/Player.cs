using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameScripts.Utils;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameScripts
{
    public class Player : Agent
    {
        #region SerializedProperties

        [Tooltip("是否被AI接管")] public bool isAIControlling;
        public string playerName = "Default";
        public int playerID;
        public Difficulty difficulty = Difficulty.Easy;
        public TextMeshProUGUI bricksCountTMP;
        public TextMeshProUGUI gemsCountTMP;
        public TextMeshProUGUI recruitsCountTMP;
        public TextMeshProUGUI towerScoreTMP;
        public TextMeshProUGUI wallScoreTMP;
        public TextMeshProUGUI bricksIncRateTMP;
        public TextMeshProUGUI gemsIncRateTMP;
        public TextMeshProUGUI recruitsIncRateTMP;
        public TextMeshProUGUI playerNameTMP;
        public GameObject towerRoof;
        public GameObject wallRoof;
        public Image towerBodyImage;
        public Image wallBodyImage;
        public int[] debugInitCardIds;

        #endregion

        public const float RESOURCE_LIMITATION = 100;
        public const float RESOURCE_INCREASE_RATE_LIMITATION = 10;

        // ** control the fillAmount of tower sprite
        public const float TOWER_MAX_FILL_AMOUNT_SCORE = 100;

        public const float TOWER_MIN_FILL_AMOUNT = 0.05f;

        // ** control the fillAmount of wall sprite
        public const float WALL_MAX_FILL_AMOUNT_SCORE = 100;
        public const float WALL_MIN_FILL_AMOUNT = 0.05f;
        [NonSerialized] public bool isAIWaitAnimation;
        private int m_brick;


        private int m_gem;
        private int m_recruit;
        private int m_tower;
        private int m_wall;
        private List<Card> m_handCards;
        private int m_brickIncRate;
        private int m_gemIncRate;
        private int m_recruitIncRate;

        [NonSerialized] public Combat combat;
        [NonSerialized] public Card usingCard;
        [NonSerialized] private ArcomagePlayer snapshot;
        [NonSerialized] public bool isPlayAgain;
        [NonSerialized] public bool isDropping;
        [NonSerialized] public int lastRemovedIndex = -1;
        [NonSerialized] public Transform handCardsLocation;

        public List<Card> handCards => m_handCards ??= new List<Card>();

        public int brick
        {
            get => m_brick;
            set
            {
                OnNumberChanged(bricksCountTMP, m_brick, value);
                m_brick = value;
            }
        }

        public int brickIncRate
        {
            get => m_brickIncRate;
            set
            {
                OnIncRateChanged(bricksIncRateTMP, m_brickIncRate, value);
                m_brickIncRate = value;
            }
        }

        public int gem
        {
            get => m_gem;
            set
            {
                OnNumberChanged(gemsCountTMP, m_gem, value);
                m_gem = value;
            }
        }

        public int gemIncRate
        {
            get => m_gemIncRate;
            set
            {
                OnIncRateChanged(gemsIncRateTMP, m_gemIncRate, value);
                m_gemIncRate = value;
            }
        }

        public int recruit
        {
            get => m_recruit;
            set
            {
                OnNumberChanged(recruitsCountTMP, m_recruit, value);
                m_recruit = value;
            }
        }

        public int recruitIncRate
        {
            get => m_recruitIncRate;
            set
            {
                OnIncRateChanged(recruitsIncRateTMP, m_recruitIncRate, value);
                m_recruitIncRate = value;
            }
        }

        public int tower
        {
            get => m_tower;
            set
            {
                OnNumberChanged(towerScoreTMP, m_tower, value);
                if (value < m_tower)
                {
                    PlayDropEffect(towerScoreTMP);
                }

                m_tower = value;
            }
        }

        public int wall
        {
            get => m_wall;
            set
            {
                OnNumberChanged(wallScoreTMP, m_wall, value);
                if (value < m_wall)
                {
                    PlayDropEffect(wallScoreTMP);
                }

                m_wall = value;
            }
        }

        public bool trainingMode => Academy.Instance.IsCommunicatorOn;
        public bool allCardsDisabled => !handCards.Exists(card => !card.isDisabled);

        protected override void Awake()
        {
            base.Awake();
            handCardsLocation = transform.Find("HandCardsLocation");
            handCardsLocation.gameObject.SetActive(false);
            combat = transform.GetComponentInParent<Combat>();
        }

        public override void Initialize()
        {
            if (!trainingMode)
            {
                MaxStep = 0;
            }
        }

        /// <summary>
        /// 定义当一个新的 episode 开始时需要重置的游戏状态
        /// </summary>
        public override void OnEpisodeBegin()
        {
            combat.GameReset();
        }

        public bool PauseAction()
        {
            return combat.blockAction || handCards.Count == 0 || combat.currentPlayer != this || usingCard != null;
        }

        /// <summary>
        /// 接收到来自用户或 AI 的输入
        ///
        /// 这里先将输入记为手牌数组的下标序
        /// DiscreteActions[0] -- 当前打出的手牌的下标序
        /// </summary>
        /// <param name="actions"></param>
        public override void OnActionReceived(ActionBuffers actions)
        {
            int useCardId = actions.DiscreteActions[0];
            if (PauseAction() || useCardId == 0)
            {
                isAIWaitAnimation = false;
                return;
            }

            Assert.IsTrue(useCardId > 0 && useCardId <= 102);
            Card card = handCards.Find(card => card.id == useCardId);
            Assert.IsNotNull(card);
            card.UseCard();
        }

        /// <summary>
        /// 添加机器学习需要观测的运行时数据
        /// </summary>
        /// <param name="sensor"></param>
        public override void CollectObservations(VectorSensor sensor)
        {
            // 8 observations
            sensor.AddObservation(brick >= RESOURCE_LIMITATION ? 1.0f : brick / RESOURCE_LIMITATION);
            sensor.AddObservation(gem >= RESOURCE_LIMITATION ? 1.0f : gem / RESOURCE_LIMITATION);
            sensor.AddObservation(recruit >= RESOURCE_LIMITATION ? 1.0f : recruit / RESOURCE_LIMITATION);
            sensor.AddObservation(brickIncRate >= RESOURCE_INCREASE_RATE_LIMITATION ? 1.0f : brickIncRate / RESOURCE_INCREASE_RATE_LIMITATION);
            sensor.AddObservation(gemIncRate >= RESOURCE_INCREASE_RATE_LIMITATION ? 1.0f : gemIncRate / RESOURCE_INCREASE_RATE_LIMITATION);
            sensor.AddObservation(recruitIncRate >= RESOURCE_INCREASE_RATE_LIMITATION ? 1.0f : recruitIncRate / RESOURCE_INCREASE_RATE_LIMITATION);
            sensor.AddObservation(tower >= TOWER_MAX_FILL_AMOUNT_SCORE ? 1.0f : tower / TOWER_MAX_FILL_AMOUNT_SCORE);
            sensor.AddObservation(wall >= WALL_MAX_FILL_AMOUNT_SCORE ? 1.0f : tower / WALL_MAX_FILL_AMOUNT_SCORE);

            // 8 observations for enemy
            var enemy = this == combat.m_player1 ? combat.m_player2 : combat.m_player1;
            sensor.AddObservation(enemy.brick >= RESOURCE_LIMITATION ? 1.0f : enemy.brick / RESOURCE_LIMITATION);
            sensor.AddObservation(enemy.gem >= RESOURCE_LIMITATION ? 1.0f : enemy.gem / RESOURCE_LIMITATION);
            sensor.AddObservation(enemy.recruit >= RESOURCE_LIMITATION ? 1.0f : enemy.recruit / RESOURCE_LIMITATION);
            sensor.AddObservation(enemy.brickIncRate >= RESOURCE_INCREASE_RATE_LIMITATION ? 1.0f : enemy.brickIncRate / RESOURCE_INCREASE_RATE_LIMITATION);
            sensor.AddObservation(enemy.gemIncRate >= RESOURCE_INCREASE_RATE_LIMITATION ? 1.0f : enemy.gemIncRate / RESOURCE_INCREASE_RATE_LIMITATION);
            sensor.AddObservation(enemy.recruitIncRate >= RESOURCE_INCREASE_RATE_LIMITATION ? 1.0f : enemy.recruitIncRate / RESOURCE_INCREASE_RATE_LIMITATION);
            sensor.AddObservation(enemy.tower >= TOWER_MAX_FILL_AMOUNT_SCORE ? 1.0f : enemy.tower / TOWER_MAX_FILL_AMOUNT_SCORE);
            sensor.AddObservation(enemy.wall >= WALL_MAX_FILL_AMOUNT_SCORE ? 1.0f : enemy.tower / WALL_MAX_FILL_AMOUNT_SCORE);

            // 102 observations for card and its owner ?? 
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            var isPauseAction = PauseAction();
            // 0 means do nothing
            actionMask.SetActionEnabled(0, 0, true);
            for (int i = 1; i < Combat.Database.cardsAssetRef.Count + 1; ++i)
            {
                // only 1 branch, which is set to all discrete actions
                var findCard = handCards.Find(card => card.id == i && !card.isDisabled);
                var actionEnabled = !isPauseAction && findCard != null;
                if (isDropping)
                {
                    actionEnabled = actionEnabled && findCard.canDrop;
                }

                actionMask.SetActionEnabled(0, i, actionEnabled);
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // ** TODO:
            // if (Input.GetKey(KeyCode.Alpha1)) index = 0;
            // if (Input.GetKey(KeyCode.Alpha2)) index = 1;
            // if (Input.GetKey(KeyCode.Alpha3)) index = 2;
            // if (Input.GetKey(KeyCode.Alpha4)) index = 3;
            // if (Input.GetKey(KeyCode.Alpha5)) index = 4;
        }


        private void Start()
        {
            ResetPlayer();
        }

        public void WithdrawAnimation(TweenCallback callback)
        {
            var duration = 0.5f;
            for (int i = 0; i < handCards.Count; ++i)
            {
                var handCard = handCards[i];
                handCard.transform.DOMove(handCardsLocation.position, duration).SetDelay(i * 0.025f).OnComplete(i == handCards.Count - 1 ? callback : null);
                handCard.transform.DOScale(1.0f, duration);
            }
        }

        void RepositionRoof(Transform roofTrans, Image bodyImage)
        {
            var position = roofTrans.localPosition;
            var parentImage = roofTrans.parent.GetComponent<Image>();
            position.y = parentImage.preferredHeight / 2 + bodyImage.preferredHeight * bodyImage.fillAmount;
            roofTrans.localPosition = position;
        }

        public void ResetPlayer()
        {
            // ** 将手牌全部交回
            foreach (var handCard in handCards)
            {
                combat.cardCache.TurnBack(handCard);
            }

            handCards.Clear();
            // ** 清空正在使用的卡（是否要清理其他状态？）
            usingCard = null;
            lastRemovedIndex = -1;
            // ** 按难度重置基本数据
            ArcomagePlayer arcomagePlayer = ArcomageDatabase.RetrieveObject<ArcomagePlayer>(Combat.Database.difficultyAssetRef[(int)difficulty]);
            brick = arcomagePlayer.brick;
            gem = arcomagePlayer.gem;
            recruit = arcomagePlayer.recruit;
            brickIncRate = arcomagePlayer.brickIncRate;
            gemIncRate = arcomagePlayer.gemIncRate;
            recruitIncRate = arcomagePlayer.recruitIncRate;
            wall = arcomagePlayer.wall;
            tower = arcomagePlayer.tower;
            OnRefresh();
        }

        public void OnRefresh()
        {
            playerNameTMP.text = playerName;
            towerRoof.SetActive(tower > 0);
            if (tower > 0)
            {
                towerBodyImage.fillAmount =
                    Math.Max(tower / TOWER_MAX_FILL_AMOUNT_SCORE, TOWER_MIN_FILL_AMOUNT);
                RepositionRoof(towerRoof.transform, towerBodyImage);
            }
            else
            {
                towerBodyImage.fillAmount = 0;
            }

            wallRoof.SetActive(wall > 0);
            if (wall > 0)
            {
                wallBodyImage.fillAmount =
                    Math.Max(wall / WALL_MAX_FILL_AMOUNT_SCORE, WALL_MIN_FILL_AMOUNT);
                RepositionRoof(wallRoof.transform, wallBodyImage);
            }
            else
            {
                wallBodyImage.fillAmount = 0;
            }
        }

        public void RemoveFromHandCard(Card inCard)
        {
            for (int i = handCards.Count - 1; i >= 0; --i)
            {
                if (handCards[i] == inCard)
                {
                    handCards.RemoveAt(i);
                    lastRemovedIndex = i;
                    return;
                }
            }

            throw new Exception($"use card not in hand?");
        }

        Tweener AddOneCardToHand(ArcomageCard template)
        {
            Card genCard = combat.cardCache.Acquire(this, template);
            if (lastRemovedIndex == -1)
            {
                handCards.Add(genCard);
            }
            else
            {
                handCards.Insert(lastRemovedIndex, genCard);
            }

            Tweener tweener = null;
            if (!trainingMode)
            {
                tweener = genCard.PlayAcquireAnim(lastRemovedIndex == -1 ? handCards.Count - 1 : lastRemovedIndex);
            }

            lastRemovedIndex = -1;
            return tweener;
        }

        public void OnGenHandCards(int needGenCardAmount, TweenCallback callback)
        {
            if (!trainingMode)
            {
                Log.LogInfo("生成玩家手牌", $"玩家：{playerName}\n预定生成：{needGenCardAmount}张手牌\n玩家详情:");
            }

            while (needGenCardAmount > 0)
            {
                Tweener tweener;
#if USING_GMTOOL
                if (!trainingMode && debugInitCardIds != null)
                {
                    debugInitCardIds = debugInitCardIds.Distinct().ToArray();
                    for (int i = 0; i < Math.Min(debugInitCardIds.Length, 5); ++i)
                    {
                        var cardId = debugInitCardIds[i];
                        for (int ci = combat.cardBank.Count - 1; ci >= 0; --ci)
                        {
                            var card = combat.cardBank[ci];
                            if (card.id == cardId)
                            {
                                // ** remove init card from cardBank
                                combat.cardBank.RemoveAt(ci);
                                tweener = AddOneCardToHand(card);
                                needGenCardAmount--;
                                if (needGenCardAmount == 0)
                                {
                                    tweener.OnComplete(callback);
                                }

                                break;
                            }
                        }
                    }

                    // ** do init only once
                    debugInitCardIds = null;
                }
#endif
                var template = combat.cardBank[combat.cardBank.Count - 1];
                Assert.IsTrue(handCards.Find(card => card.id == template.id) == null);
                tweener = AddOneCardToHand(template);
                combat.cardBank.RemoveAt(combat.cardBank.Count - 1);
                needGenCardAmount--;
                if (!trainingMode && needGenCardAmount == 0)
                {
                    tweener.OnComplete(callback);
                }
            }

            if (!trainingMode)
            {
                Log.LogInfo("玩家手牌", $"玩家：{playerID}\n总手牌：{handCards.Count}\n详情：");
            }
        }

        /// <summary>
        /// MLAgent奖励函数结算
        /// </summary>
        public void CalculateReward()
        {
            if (Combat.Database.learningGoal == MLAgentLearningGoal.BuildTower)
            {
                if (snapshot == null)
                {
                    snapshot = ScriptableObject.CreateInstance<ArcomagePlayer>();
                    snapshot.TakeSnapshot(this);
                }

                int diffTower = tower - snapshot.tower;
                AddReward(diffTower > 0 ? Mathf.Clamp01(tower / TOWER_MAX_FILL_AMOUNT_SCORE) : 0);
                snapshot.TakeSnapshot(this);
            }

            if (Combat.Database.learningGoal == MLAgentLearningGoal.WinCombat)
            {
                // winner will set reward to 1.0
                SetReward(1.0f);
            }
        }


        void OnIncRateChanged(TextMeshProUGUI target, int oldNum, int num)
        {
            if (trainingMode)
            {
                target.text = $"{num}";
                return;
            }

            if (num != oldNum)
            {
                DOTween.To(() => oldNum, (_) => { target.text = $"{oldNum}->{num}"; }, num, 1f).OnComplete(() => { target.text = num.ToString(); });
            }
        }

        void OnNumberChanged(TextMeshProUGUI target, int oldNum, int num)
        {
            if (trainingMode)
            {
                target.text = $"{num}";
                return;
            }

            if (num != oldNum)
            {
                DOTween.To(() => oldNum, (lerpNum) => { target.text = lerpNum.ToString(); }, num, 1f);
                Color tweenColor = num > oldNum ? Color.red : Color.green;
                target.DOColor(tweenColor, 0.25f).SetLoops(8, LoopType.Yoyo).From(Color.white).SetEase(Ease.InOutElastic);
                if (!trainingMode && combat != null && combat.effectCache != null)
                {
                    EffectInstance effectInstance = combat.effectCache.CreateEffect(num > oldNum ? "IncNumber" : "DecNumber");
                    var textMeshProUGUI = effectInstance.GetComponent<TextMeshProUGUI>();
                    textMeshProUGUI.text = num > oldNum ? $"+{num - oldNum}" : $"-{oldNum - num}";
                    textMeshProUGUI.alpha = 1;
                    effectInstance.Play(target.transform.position + new Vector3(50f, 0, 0));
                }
            }
        }

        void PlayDropEffect(TextMeshProUGUI target)
        {
            if (trainingMode)
            {
                return;
            }

            EffectInstance effectInstance = combat.effectCache.CreateEffect("Drop");
            effectInstance.Play(target.transform.position + new Vector3(10f, 0, 0));
        }
    }
}