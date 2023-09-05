using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NOAH.UI
{
    public class Prompter : Selectable
    {
        private string m_json;
        private int m_type;
        private UnityAction<string> m_onEnter;
        private UnityAction<string> m_onExit;

        private bool m_isShow;
            
        public UnityAction<string> onEnter
        {
            get { return m_onEnter; }
            set { m_onEnter = value; }
        }

        public UnityAction<string> onExit
        {
            get { return m_onExit; }
            set { m_onExit = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            this.gameObject.BindSelectEvent(ShowPrompt);
            this.gameObject.BindDeselectEvent(ClosePrompt);
        }

        protected override void OnDisable()
        {
            if (m_isShow)
            {
                ClosePrompt(null);
            }
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnPointerEnter(eventData);
                return;
            }
    
            this.ShowPrompt(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnPointerExit(eventData);
                return;
            }
    
            this.ClosePrompt(eventData);
        }

        public void InitPrompter(int type, string json)
        {
            this.m_type = type;
            this.m_json = json;
        }

        public void ClearPrompter()
        {
            this.m_type = 0;
            this.m_json = null;
        }

        private void ShowPrompt(BaseEventData data)
        {
            if (this.m_json != null)
            {
                // UIManager.Instance.ShowPrompt(this.m_type, this.m_json, transform);
                m_isShow = true;
            }
        }
        
        private void ClosePrompt(BaseEventData data)
        {
            if (this.m_json != null)
            {
                // UIManager.Instance.ClosePrompt(this.m_type);
                m_isShow = false;
            }
        }
    }
}
