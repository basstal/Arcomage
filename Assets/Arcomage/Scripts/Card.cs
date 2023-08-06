using System;
using DG.Tweening;
using Arcomage.GameScripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
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

        private void Awake()
        {
            useCardButton.BindButtonEvent(UseCard);
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
                ? AssetManager.Instance.LoadAsset<Sprite>("others[brick]", this)
                : (m_data.costType == CostType.Gem
                    ? AssetManager.Instance.LoadAsset<Sprite>("others[gem]", this)
                    : AssetManager.Instance.LoadAsset<Sprite>("others[recruit]", this));

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

        public Tweener PlayDisplayingCardAnim(int offsetIndex)
        {
            var handCardPos = GetHandCardAnimPos(offsetIndex);
            float duration = 0.45f;
            var tweener = transform.DOMove(handCardPos, duration).From(owner.handCardsLocation.position).SetEase(Ease.InCubic).SetDelay(offsetIndex * 0.075f);
            transform.DOScale(2.2f, duration).From(owner.handCardsLocation.lossyScale).SetEase(Ease.OutQuad).OnComplete(OnDisplay);
            return tweener;
        }

        public void PlayUsingCardAnim(TweenCallback callback)
        {
            transform.DOMove(owner.combat.cardDisappearPoint.position, 0.3f).OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(0, 0, -90f), 0.25f).SetDelay(1.5f)
                    .OnStart(() => { cardExtentImage.gameObject.SetActive(false); })
                    .OnComplete(() =>
                    {
                        transform.SetAsFirstSibling();
                        var duration = 0.3f;
                        transform.DOMove(owner.combat.cardCache.transform.parent.position, duration);
                        transform.DOLocalRotate(Vector3.zero, duration);
                        transform.DOScale(Vector3.one, duration);
                        transform.Find("BackImage").GetComponent<Image>().DOFade(1, duration).SetEase(Ease.OutQuart).OnComplete(callback);
                    });
            });
        }

        public Vector3 GetHandCardAnimPos(int offsetIndex)
        {
            float offset = GetComponent<RectTransform>().rect.size.x + 150f;
            var handCardsLayoutBegin = owner.combat.handCardLayout.position;
            handCardsLayoutBegin.x -= Combat.MAX_HAND_CARDS / 2 * offset;
            handCardsLayoutBegin.x += offsetIndex * offset;
            return handCardsLayoutBegin;
        }

        public Tweener PlayAcquireAnim(int offsetIndex)
        {
            cardExtentImage.gameObject.SetActive(false);
            var handCardPos = GetHandCardAnimPos(offsetIndex);
            float duration = 0.7f;
            var tweener = transform.DOMove(handCardPos, duration).From(owner.combat.cardCache.transform.position).SetEase(Ease.InOutCubic).SetDelay(offsetIndex * 0.075f);
            transform.DOScale(2.2f, duration).From(owner.combat.cardCache.transform.lossyScale).SetEase(Ease.InQuad).OnComplete(OnDisplay);
            float delay = 0.4f;
            transform.Find("BackImage").GetComponent<Image>().DOFade(0, duration - delay).From(1).SetDelay(delay).SetEase(Ease.InQuad);
            return tweener;
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