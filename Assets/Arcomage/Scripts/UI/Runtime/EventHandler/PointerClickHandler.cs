using UnityEngine;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class PointerClickHandler : EventHandler, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);

            if (!string.IsNullOrEmpty(PassEventTag))
            {
                PassEvent(eventData, ExecuteEvents.pointerClickHandler);
            }
        }
    }
}
