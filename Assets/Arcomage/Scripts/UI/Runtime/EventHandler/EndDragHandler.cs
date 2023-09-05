using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class EndDragHandler : EventHandler, IEndDragHandler, IDragHandler
    {
        public void OnEndDrag(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);

            if (!string.IsNullOrEmpty(PassEventTag))
            {
                PassEvent(eventData, ExecuteEvents.endDragHandler);
            }
        }

        // 必须实现IDragHandler接口，不然OnEndDrag无法触发
        public void OnDrag(PointerEventData eventData)
        {

        }
    }
}
