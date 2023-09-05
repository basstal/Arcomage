using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class SelectHandler : EventHandler, ISelectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            Callback?.Invoke(eventData);
        }
    }
}
