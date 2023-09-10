using System;
using DG.Tweening;
using Arcomage.GameScripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using Whiterice;

namespace Arcomage.GameScripts
{
    /// <summary>
    /// 游戏中卡牌实例的逻辑
    /// </summary>
    public class Card : MonoBehaviour
    {
        public int id => m_data.id;
        public bool canDrop => !m_data.disallowDrop;

        [NonSerialized] private ArcomageCard m_data;
        [NonSerialized] public Player owner;
        [NonSerialized] public bool isDisabled;

        public Button useCardButton;
        public Image cardImage;
        public Image costTypeImage;
        public TextMeshProUGUI cardNameText;
        public TextMeshProUGUI cardDescribeText;
        public TextMeshProUGUI cardCostText;
        public Image cardExtentImage;
        public GameObject cardDroppingNode;

        private Transform m_debug;

        // public GameObject acquireCardAnim;
        // public GameObject displayCardAnim;
        public CardAnimation CardAnimation;

        private void Awake()
        {
            useCardButton.BindButtonEvent(UseCard);
            CardAnimation.Init();
#if UNITY_EDITOR && USING_GMTOOL
            m_debug = transform.Find("Debug");
#endif
        }

        public void SetData(ArcomageCard inData)
        {
            Assert.IsNotNull(inData);
            m_data = inData;
            Assert.IsNotNull(m_data.sprite);
            cardImage.sprite = m_data.sprite;
            cardImage.SetNativeSize();
            costTypeImage.sprite = m_data.costType == CostType.Brick
                ? AssetManager.Instance.LoadAsset<Sprite>("UI_others[brick]", this)
                : (m_data.costType == CostType.Gem
                    ? AssetManager.Instance.LoadAsset<Sprite>("UI_others[gem]", this)
                    : AssetManager.Instance.LoadAsset<Sprite>("UI_others[recruit]", this));

            Assert.IsNotNull(costTypeImage.sprite);

            cardCostText.text = m_data.cost.ToString();
            var scriptMachine = GetComponent<ScriptMachine>();
            scriptMachine.nest.SwitchToMacro(m_data.logic);
#if UNITY_EDITOR && USING_GMTOOL
            if (m_debug != null)
            {
                m_debug.GetComponent<TextMeshProUGUI>().text = $"id : {id}";
            }
#endif
        }

        public void SetOwner(Player inOwner)
        {
            Assert.IsNotNull(inOwner);
            owner = inOwner;
            cardDroppingNode.SetActive(false);
            var localization = owner.combat.Database.localization;
            cardNameText.text = localization == Localization.CN ? m_data.cardName_cn : m_data.cardName;
            cardDescribeText.text = localization == Localization.CN ? m_data.describe_cn : m_data.describe_en;
        }

        public void OnDisplay()
        {
            cardExtentImage.gameObject.SetActive(true);
            gameObject.SetActive(true);
            isDisabled = SharedLogics.HandleCost(owner, m_data.costType, m_data.cost) < 0;
            if (owner.isDropping)
            {
                isDisabled = !canDrop;
            }

            cardDroppingNode.SetActive(owner.isDropping && !isDisabled);
            cardExtentImage.color = isDisabled ? Color.red : Color.white;
            useCardButton.interactable = !isDisabled;
            cardCostText.color = isDisabled ? new Color32(149, 26, 26, 255) : Color.black;
        }

        public void UseCard()
        {
            if (owner.usingCard == null && owner.combat.currentStage == owner.combat.WaitCardUse)
            {
                Assert.IsNotNull(m_data);
                if (!owner.isDropping)
                {
                    SharedLogics.ResChange(owner, m_data.costType, -m_data.cost);
                }

                owner.usingCard = this;
                owner.combat.cardBank.Insert(0, m_data);
                owner.combat.handCardBlocking.gameObject.SetActive(true);
            }
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