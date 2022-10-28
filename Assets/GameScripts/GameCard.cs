using System;
using DG.Tweening;
using GameScripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameScripts
{
    public class GameCard : MonoBehaviour
    {
        public int id => m_data.id;
        public bool canDrop => !m_data.disallowDrop;

        [NonSerialized] private ArcomageCard m_data;
        [NonSerialized] public GamePlayer owner;
        [NonSerialized] public bool isDisabled;

        public Button useCardButton;
        public Image cardImage;
        public Image costTypeImage;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI cardDescribeText;
        public TextMeshProUGUI cardCostText;
        public Image cardExtentImage;
        public DOTweenAnimation cardUsingAnimation;
        public DOTweenAnimation cardDisplayingAnimation;

        private void Awake()
        {
            useCardButton.BindButtonEvent(UseCard);
        }

        public void SetData(ArcomageCard inData)
        {
            Assert.IsNotNull(inData);
            m_data = inData;
            Assert.IsNotNull(m_data.sprite);
            cardImage.sprite = m_data.sprite;
            cardImage.SetNativeSize();
            AssetReferenceSprite spriteRef = m_data.costType == CostType.Brick
                ? ArcomageCombat.Database.brickAssetRef
                : (m_data.costType == CostType.Gem ? ArcomageCombat.Database.gemAssetRef : ArcomageCombat.Database.recruitAssetRef);
            if (spriteRef.IsValid())
            {
                costTypeImage.sprite = (Sprite)spriteRef.OperationHandle.Result;
            }
            else
            {
                costTypeImage.sprite = spriteRef.LoadAssetAsync<Sprite>().WaitForCompletion();
            }

            Assert.IsNotNull(costTypeImage.sprite);
            var localization = ArcomageCombat.Database.localization;
            cardNameText.text = localization == Localization.CN ? m_data.cardName_cn : m_data.cardName;
            cardDescribeText.text = localization == Localization.CN ? m_data.describe_cn : m_data.describe_en;
            cardCostText.text = m_data.cost.ToString();
            var trans = cardUsingAnimation.transform;
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
            var scriptMachine = GetComponent<ScriptMachine>();
            scriptMachine.nest.SwitchToMacro(m_data.logic);
        }

        public void SetOwner(GamePlayer inOwner)
        {
            Assert.IsNotNull(inOwner);
            owner = inOwner;
        }

        public void OnDisplay()
        {
            gameObject.SetActive(true);
            transform.SetParent(owner.arcomageCombat.handCardLayout, false);
            isDisabled = SharedLogics.HandleCost(owner, m_data.costType, m_data.cost) < 0 || (owner.isDropping && !canDrop);
            cardExtentImage.color = isDisabled ? Color.red : Color.white;
            useCardButton.interactable = !isDisabled;
            // useCardButton.GetComponent<Image>().raycastTarget = !isDisabled;
        }

        public void UseCard()
        {
            Assert.IsNotNull(m_data);
            owner.usingCard = this;
            owner.arcomageCombat.handCardBlocking.gameObject.SetActive(true);
        }

        public void PlayDisplayingCardAnim()
        {
            cardDisplayingAnimation.DOPlayForwardAllById(owner.playerID == 1 ? "PlayerLeft" : "PlayerRight");
        }

        public void PlayUsingCardAnim(UnityAction callback)
        {
            var tweenAnimations = cardUsingAnimation.GetComponents<DOTweenAnimation>();
            foreach (var tweenAnimation in tweenAnimations)
            {
                if (tweenAnimation.id == "Final")
                {
                    tweenAnimation.onComplete.RemoveAllListeners();
                    tweenAnimation.onComplete.AddListener(callback);
                }

                if (tweenAnimation.id == "GoCenter")
                {
                    tweenAnimation.endValueV3 = owner.arcomageCombat.cardDisappearPoint.position;
                }
            }

            cardUsingAnimation.DORestart();
        }

        public void Apply()
        {
            Assert.IsNotNull(owner);

            CustomEvent.Trigger(gameObject, "Apply", owner);
            if (!owner.trainingMode)
            {
                Log.LogInfo("卡牌使用完成", $"卡牌使用完成 : {m_data.logic.name}");
            }
        }
    }
}