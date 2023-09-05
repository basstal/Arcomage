using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class DeselectHandler : EventHandler, IDeselectHandler
    {
        public void OnDeselect(BaseEventData eventData)
        {
            Callback?.Invoke(eventData);
        }
    }
}
