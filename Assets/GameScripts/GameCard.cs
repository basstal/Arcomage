using System;
using DG.Tweening;
using GameScripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameScripts
{
    public enum CostType
    {
        None,
        Brick,
        Recruit,
        Gem,
    }

    public enum BuildingType
    {
        None,
        Wall,
        Tower,
        Castle,
    }

    public class GameCard : MonoBehaviour
    {
#if UNITY_EDITOR
        public TextMeshProUGUI debug;
#endif
        // private int m_id;

        public int id => m_data.id;

        // public CostType costType = CostType.None;
        // public int cost = 0;
        [NonSerialized] private ArcomageCard m_data;
        [NonSerialized] public GamePlayer owner;

        public Button useCardButton;
        public Image cardImage;
        public Image costTypeImage;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI cardDescribeText;
        public TextMeshProUGUI cardCostText;
        public Image cardExtentImage;

        private bool m_isUsing;
        private bool m_isTweenDisappearCreated;

        public DOTweenAnimation disappearTweenAnimation;

        private void Awake()
        {
            useCardButton.BindButtonEvent(UseCard);
        }

        public void SetData(ArcomageCard inData)
        {
            Assert.IsNotNull(inData);
            m_data = inData;
            // local image = REF.CardImage
            Assert.IsNotNull(m_data.sprite);
            cardImage.sprite = m_data.sprite;
            cardImage.SetNativeSize();
            // local costData = cardData.cost
            AssetReferenceSprite spriteRef = m_data.costType == CostType.Brick
                ? GameMain.Database.brickAssetRef
                : (m_data.costType == CostType.Gem ? GameMain.Database.gemAssetRef : GameMain.Database.recruitAssetRef);
            if (spriteRef.IsValid())
            {
                costTypeImage.sprite = (Sprite)spriteRef.OperationHandle.Result;
            }
            else
            {
                costTypeImage.sprite = spriteRef.LoadAssetAsync<Sprite>().WaitForCompletion();
            }

            Assert.IsNotNull(costTypeImage.sprite);
            // local GetString = CS.LocaleManager.GetString
            var localization = GameMain.Database.localization;
            cardNameText.text = localization == Localization.CN ? m_data.cardName_cn : m_data.cardName;
            cardDescribeText.text = localization == Localization.CN ? m_data.describe_cn : m_data.describe_en;
            cardCostText.text = m_data.cost.ToString();
#if UNITY_EDITOR
            debug.gameObject.SetActive(true);
#endif
            var trans = disappearTweenAnimation.transform;
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
        }

        public void SetOwner(GamePlayer inOwner)
        {
            Assert.IsNotNull(inOwner);
            owner = inOwner;
            var left = SharedLogics.HandleCost(owner, m_data.costType, m_data.cost);
            cardExtentImage.color = left >= 0 ? Color.white : Color.red;
        }

        public void UseCard()
        {
            Assert.IsNotNull(m_data);
            SharedLogics.ResChange(owner, m_data.costType, -m_data.cost);
            m_isUsing = true;
            OnShowUseCard();
        }

        public void OnShowUseCard()
        {
            if (!m_isTweenDisappearCreated)
            {
                m_isTweenDisappearCreated = true;
                disappearTweenAnimation.useTargetAsV3 = true;
                disappearTweenAnimation.endValueTransform = GameMain.Instance.cardDisappearPoint;
                disappearTweenAnimation.CreateTween();
                disappearTweenAnimation.onComplete.AddListener(OnUseCardComplete);
            }

            disappearTweenAnimation.DORestart();
            GamePlayer.RecycleHandCards.Invoke();
            // DB.TriggerEvent(string.format("Player%s/RecycleAll", currentPlayer))
        }

        public void OnUseCardComplete()
        {
            // var macro = flow.GetValue<TMacro>(graphInput);
            // var targetValue = flow.GetValue(target, targetType);
            //
            // if (targetValue is GameObject go)
            // {
            //     go.GetComponent<TMachine>().nest.SwitchToMacro(macro);
            // }
            // else
            // {
            //     ((TMachine)targetValue).nest.SwitchToMacro(macro);
            // }
            //
            // flow.SetValue(graphOutput, macro);
            //
            // return exit;
            GetComponent<ScriptMachine>().nest.SwitchToMacro(m_data.logic);
            // local currentPlayer = DB.GetData("CurrentPlayer")
            // local player = DB.GetData(string.format("Player%s", currentPlayer))
            // m_useCardResult = m_cardData.func(player)
            Log.LogInfo("卡牌使用完成", "卡牌使用完成");
            GameMain.PlayerSwitch.Invoke(false, this);
            m_isUsing = false;
            // DB.TriggerEvent("Main/PlayerSwitch", m_useCardResult, m_cardData)
            // m_cardData.using = false
        }
    }
}