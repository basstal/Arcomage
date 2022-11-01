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
            AssetReferenceSprite spriteRef = m_data.costType == CostType.Brick
                ? Combat.Database.brickAssetRef
                : (m_data.costType == CostType.Gem ? Combat.Database.gemAssetRef : Combat.Database.recruitAssetRef);
            if (spriteRef.IsValid())
            {
                costTypeImage.sprite = (Sprite)spriteRef.OperationHandle.Result;
            }
            else
            {
                costTypeImage.sprite = spriteRef.LoadAssetAsync<Sprite>().WaitForCompletion();
            }

            Assert.IsNotNull(costTypeImage.sprite);
            var localization = Combat.Database.localization;
            cardNameText.text = localization == Localization.CN ? m_data.cardName_cn : m_data.cardName;
            cardDescribeText.text = localization == Localization.CN ? m_data.describe_cn : m_data.describe_en;
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
        }

        public void OnDisplay()
        {
            gameObject.SetActive(true);
            // transform.SetParent(owner.transform);
            isDisabled = SharedLogics.HandleCost(owner, m_data.costType, m_data.cost) < 0;
            if (owner.isDropping)
            {
                isDisabled = !canDrop;
            }

            cardExtentImage.color = isDisabled ? Color.red : Color.white;
            useCardButton.interactable = !isDisabled;
            // useCardButton.GetComponent<Image>().raycastTarget = !isDisabled;
        }

        public void UseCard()
        {
            Assert.IsNotNull(m_data);
            if (!owner.isDropping)
            {
                SharedLogics.ResChange(owner, m_data.costType, -m_data.cost);
            }

            owner.usingCard = this;
            owner.combat.handCardBlocking.gameObject.SetActive(true);
        }

        public void PlayDisplayingCardAnim(int offsetIndex, TweenCallback callback)
        {
            var handCardPos = GetHandCardAnimPos(offsetIndex);
            float duration = 0.45f;
            transform.DOMove(handCardPos, duration).From(owner.handCardsLocation.position).SetEase(Ease.InCubic).SetDelay(offsetIndex * 0.075f).OnComplete(offsetIndex == Combat.MAX_HAND_CARDS - 1 ? callback : null);
            transform.DOScale(2.2f, duration).From(owner.handCardsLocation.lossyScale).SetEase(Ease.OutQuad).OnComplete(OnDisplay);
        }

        public void PlayUsingCardAnim(TweenCallback callback)
        {
            transform.DOMove(owner.combat.cardDisappearPoint.position, 0.5f).OnComplete(() =>
            {
                transform.DOLocalRotate(new Vector3(0, 0, -90f), 0.25f).SetDelay(1.5f).OnComplete(() =>
                {
                    transform.SetAsFirstSibling();
                    var duration = 1f;
                    transform.DOMove(owner.combat.cardCache.transform.parent.position, duration);
                    transform.DOLocalRotate(Vector3.zero, duration);
                    transform.DOScale(Vector3.one, duration);
                    transform.Find("BackImage").GetComponent<Image>().DOFade(1, duration).SetEase(Ease.OutQuart).OnComplete(callback);
                });
            });
        }

        public Vector3 GetHandCardAnimPos(int offsetIndex)
        {
            float offset = GetComponent<RectTransform>().rect.size.x + 50f;
            var handCardsLayoutBegin = owner.combat.handCardLayout.position;
            handCardsLayoutBegin.x -= Combat.MAX_HAND_CARDS / 2 * offset;
            handCardsLayoutBegin.x += offsetIndex * offset;
            return handCardsLayoutBegin;
        }

        public Tweener PlayAcquireAnim(int offsetIndex)
        {
            var handCardPos = GetHandCardAnimPos(offsetIndex);
            float duration = 0.7f;
            var tweener = transform.DOMove(handCardPos, duration).From(owner.combat.cardCache.transform.position).SetEase(Ease.InCubic).SetDelay(offsetIndex * 0.075f);
            transform.DOScale(2.2f, duration).From(owner.combat.cardCache.transform.lossyScale).SetEase(Ease.OutQuad).OnComplete(OnDisplay);
            transform.Find("BackImage").GetComponent<Image>().DOFade(0, duration).SetEase(Ease.InQuart);
            return tweener;
            // transform.DOLocalRotate(Vector3.zero, duration).From(new Vector3(0, -90f, 0)).SetEase(Ease.InQuart);
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