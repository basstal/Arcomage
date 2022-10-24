using System;
using System.Collections.Generic;
using GameScripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameScripts
{
    public class GamePlayer : MonoBehaviour
    {
        public int brick = 0;
        public int gem = 0;
        public int recruit = 0;
        public int brickIncRate = 1;
        public int gemIncRate = 1;
        public int recruitIncRate = 1;

        public int tower = 0;
        public int wall = 0;
        public string playerName = "Default";
        public int playerID;

        // ** control the fillAmount of tower sprite
        public const float TOWER_MAX_FILL_AMOUNT_SCORE = 100;

        public const float TOWER_MIN_FILL_AMOUNT = 0.05f;

        // ** control the fillAmount of wall sprite
        public const float WALL_MAX_FILL_AMOUNT_SCORE = 100;
        public const float WALL_MIN_FILL_AMOUNT = 0.05f;

        [NonSerialized] public List<GameCard> handCards;
        [NonSerialized] public GameCard usingCard;

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

        public ParticleSystem bricksAddEffect;
        public ParticleSystem bricksDropEffect;
        public ParticleSystem gemsAddEffect;
        public ParticleSystem gemsDropEffect;
        public ParticleSystem recruitsAddEffect;
        public ParticleSystem recruitsDropEffect;
        public ParticleSystem towerAddEffect;
        public ParticleSystem towerDropEffect;
        public ParticleSystem wallAddEffect;
        public ParticleSystem wallDropEffect;

        private void Awake()
        {
            handCards = new List<GameCard>();
        }

        private void Start()
        {
            OnRefresh();
        }

        public void WithdrawAnimation()
        {
            foreach (var handcard in handCards)
            {
                handcard.gameObject.SetActive(false);
            }
        }

        void RepositionRoof(Transform roofTrans, Image bodyImage)
        {
            var position = roofTrans.localPosition;
            var parentImage = roofTrans.parent.GetComponent<Image>();
            position.y = parentImage.preferredHeight / 2 + bodyImage.preferredHeight * bodyImage.fillAmount;
            roofTrans.localPosition = position;
        }

        public void OnRefresh()
        {
            bricksCountTMP.text = $"{brick} <size=80%>bricks</size>";
            gemsCountTMP.text = $"{gem} <size=80%>gem</size>";
            recruitsCountTMP.text = $"{recruit} <size=80%>recruit</size>";
            towerScoreTMP.text = $"{tower}";
            wallScoreTMP.text = $"{wall}";
            bricksIncRateTMP.text = $"+ {brickIncRate}";
            gemsIncRateTMP.text = $"+ {gemIncRate}";
            recruitsIncRateTMP.text = $"+ {recruitIncRate}";
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

        public void RemoveFromHandCard(GameCard inCard)
        {
            for (int i = handCards.Count - 1; i >= 0; --i)
            {
                if (handCards[i] == inCard)
                {
                    handCards.RemoveAt(i);
                    OnGenHandCards(1);
                    return;
                }
            }

            throw new Exception($"use card not in hand?");
        }

        public void OnGenHandCards(int cardAmount)
        {
            Log.LogInfo("生成玩家手牌", $"玩家：{playerName}\n预定生成：{cardAmount}张手牌\n玩家详情:");
            while (cardAmount > 0)
            {
                int index = Random.Range(0, GameMain.Database.cardsAssetRef.Count);
                var cardAssetRef = GameMain.Database.cardsAssetRef[index];
                Assert.IsNotNull(cardAssetRef);
                ArcomageCard template = null;
                if (!cardAssetRef.IsValid())
                {
                    template = cardAssetRef.LoadAssetAsync<ArcomageCard>().WaitForCompletion();
                }
                else
                {
                    template = (ArcomageCard)cardAssetRef.OperationHandle.Result;
                }

                Assert.IsNotNull(template);
                if (handCards.Find(card => card.id == template.id) == null)
                {
                    GameCard genCard = GameCardCache.Instance.Acquire(this, template);
                    genCard.gameObject.SetActive(false);
                    handCards.Add(genCard);
                    cardAmount--;
                }
            }

            Log.LogInfo("玩家手牌", $"玩家：{playerID}\n总手牌：{handCards.Count}\n详情：");
        }
    }
}