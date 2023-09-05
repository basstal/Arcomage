using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class DragHandler : EventHandler, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);

            if (!string.IsNullOrEmpty(PassEventTag))
            {
                PassEvent(eventData, ExecuteEvents.dragHandler);
            }
        }
    }
}
