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
        public int id => m_data.id;

        [NonSerialized] private ArcomageCard m_data;
        [NonSerialized] public GamePlayer owner;

        public Button useCardButton;
        public Image cardImage;
        public Image costTypeImage;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI cardDescribeText;
        public TextMeshProUGUI cardCostText;
        public Image cardExtentImage;

        private bool m_isTweenDisappearCreated;

        public DOTweenAnimation disappearTweenAnimation;
        public DOTweenAnimation drawTweenAnimation;

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
            var scriptMachine = GetComponent<ScriptMachine>();
            scriptMachine.nest.SwitchToMacro(m_data.logic);
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
            owner.usingCard = this;
        }

        public void OnShowUseCard()
        {
            if (!m_isTweenDisappearCreated)
            {
                m_isTweenDisappearCreated = true;
                disappearTweenAnimation.useTargetAsV3 = true;
                disappearTweenAnimation.endValueTransform = GameMain.Instance.cardDisappearPoint;
                disappearTweenAnimation.CreateTween();
                disappearTweenAnimation.onComplete.AddListener(Apply);
            }

            disappearTweenAnimation.DORestart();
        }

        public void Apply()
        {
            Assert.IsNotNull(owner);
            CustomEvent.Trigger(gameObject, "Apply", owner);
            Log.LogInfo("卡牌使用完成", "卡牌使用完成");
        }
    }
}