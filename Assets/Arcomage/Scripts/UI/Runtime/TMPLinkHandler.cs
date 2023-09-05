// using NOAH.Asset;
// using NOAH.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        public class TMPEvent:UnityEvent<TMP_LinkInfo>{}
        public TMPEvent onPointerClick = new TMPEvent();
        private TextMeshProUGUI mTmp;
        void Awake()
        {
            mTmp = GetComponent<TextMeshProUGUI>();
            if(mTmp.raycastTarget == false)
                mTmp.raycastTarget =true;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            int _linkIndex = TMP_TextUtilities.FindIntersectingLink(mTmp, eventData.position, eventData.enterEventCamera);
            if(_linkIndex != -1)
            {
                TMP_LinkInfo _linkInfo = mTmp.textInfo.linkInfo[_linkIndex];
                onPointerClick.Invoke(_linkInfo);
                // DataBinding.TriggerEvent("HyperLinkClicked", mTmp, _linkInfo.GetLinkID(), _linkInfo.GetLinkText());
            }
        }
    }
}
