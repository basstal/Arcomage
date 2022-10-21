using System;
using System.Collections.Generic;
using GameScripts.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace GameScripts
{
    public delegate void GenHandCardsDelegate(int cardAmount, int fromIndex);

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

        public bool useAI = false;

        // ** control the fillAmount of tower sprite
        public const float TOWER_MAX_FILL_AMOUNT_SCORE = 100;

        public const float TOWER_MIN_FILL_AMOUNT = 0.05f;

        // ** control the fillAmount of wall sprite
        public const float WALL_MAX_FILL_AMOUNT_SCORE = 100;
        public const float WALL_MIN_FILL_AMOUNT = 0.05f;

        public static GenHandCardsDelegate GenHandCards;
        public static Action RecycleHandCards;

        [NonSerialized] public List<GameCard> handCards;

        private void Awake()
        {
            GenHandCards += OnGenHandCards;
            RecycleHandCards += OnRecycleHandCards;
            handCards = new List<GameCard>();
        }

        private void OnDestroy()
        {
            GenHandCards -= OnGenHandCards;
            RecycleHandCards -= OnRecycleHandCards;
        }

        public void OnRecycleHandCards()
        {
            foreach (GameCard handCard in handCards)
            {
                if (!handCard.isUsing)
                {
                    GameCardCache.Instance.TurnBack(handCard);
                }
            }

            handCards.Clear();
        }

        public void OnRecycleAll()
        {
            // for i = 1, #m_handCards do
            //     local handCard = m_handCards[i]
            // if not handCard.using then
            //     local ref = handCard.REF
            // local id = GamePlayerCS.playerID == 1 and "left2right" or "right2left"
            //     ref.DrawCard:DOPlayBackwardsById(id)
            //     ref.DrawCard:DOPlayBackwardsById("alpha")
            // end
            //     end
        }

        public void OnRefresh()
        {
            // local GetString = CS.LocaleManager.GetString
            // REF.BricksCountTMP.text = GetString("BrickCount", GamePlayerCS.brick)
            // REF.GemsCountTMP.text = GetString("GemsCount", GamePlayerCS.gem)
            // REF.RecruitsCountTMP.text = GetString("RecruitsCount", GamePlayerCS.recruit)
            // local tower = GamePlayerCS.tower
            // REF.TowerScoreTMP.text = GetString("SingleScore", tower)
            // local wall = GamePlayerCS.wall
            // REF.WallScoreTMP.text = GetString("SingleScore", wall)
            // REF.BricksIncRateTMP.text = GetString("IncRate", GamePlayerCS.brickIncRate)
            // REF.GemsIncRateTMP.text = GetString("IncRate", GamePlayerCS.gemIncRate)
            // REF.RecruitsIncRateTMP.text = GetString("IncRate", GamePlayerCS.recruitIncRate)
            // REF.PlayerNameTMP.text = GamePlayerCS.playerName
            //
            // local hasTower = tower > 0
            // REF.TowerRoof:SetActive(hasTower)
            // if hasTower then
            // REF.TowerBodyImage.fillAmount =
            //     math.max(tower / CS.GamePlayer.TOWER_MAX_FILL_AMOUNT_SCORE, CS.GamePlayer.TOWER_MIN_FILL_AMOUNT)
            // RepositionRoof(REF.TowerRoof.transform, REF.TowerBodyImage)
            // else
            // REF.TowerBodyImage.fillAmount = 0
            // end
            //
            // local hasWall = wall > 0
            // REF.WallRoof:SetActive(hasWall)
            // if hasWall then
            // REF.WallBodyImage.fillAmount =
            //     math.max(wall / CS.GamePlayer.WALL_MAX_FILL_AMOUNT_SCORE, CS.GamePlayer.WALL_MIN_FILL_AMOUNT)
            // RepositionRoof(REF.WallRoof.transform, REF.WallBodyImage)
            // else
            // REF.WallBodyImage.fillAmount = 0
            // end
        }

        public int UseHandCard(GameCard inCard)
        {
            // local gameMain = DB.GetData("Main")
            // local gameMainCS = gameMain.GameMainCS
            // local cardObjCacheRoot = gameMainCS.CardObjCacheRoot
            for (int i = handCards.Count - 1; i >= 0; --i)
            {
                if (handCards[i] == inCard)
                {
                    handCards.RemoveAt(i);
                    GameCardCache.Instance.TurnBack(inCard);
                    return i;
                }
            }

            return -1;
        }

        public void OnGenHandCards(int cardAmount, int fromIndex)
        {
            Log.LogInfo("生成玩家手牌", $"玩家：{playerName}\n预定生成：{cardAmount}张手牌\n玩家详情:");
            // local dataSize = #Data
            // local gameMain = DB.GetData("Main")
            // local gameMainCS = gameMain.GameMainCS
            // local ID2Sprite = gameMainCS.ID2Sprite
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
                if (!handCards.Find(card => card.id == template.id))
                {
                    handCards.Add(GameCardCache.Instance.Acquire(this, template));
                    cardAmount--;
                }
            }

            Log.LogInfo("玩家手牌", $"玩家：{playerID}\n总手牌：{handCards.Count}\n详情：");
            // DB.SetData(string.format("Player%s/HandCards", GamePlayerCS.playerID), m_handCards)
        }
    }
}