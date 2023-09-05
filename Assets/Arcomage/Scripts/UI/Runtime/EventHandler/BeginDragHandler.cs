using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class BeginDragHandler : EventHandler, IBeginDragHandler, IDragHandler
    {
        public void OnBeginDrag(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);

            if (!string.IsNullOrEmpty(PassEventTag))
            {
                PassEvent(eventData, ExecuteEvents.beginDragHandler);
            }
        }

        // 必须实现IDragHandler接口，不然OnBeginDrag无法触发
        public void OnDrag(PointerEventData eventData)
        {

        }
    }
}
