using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class PointerEnterHandler : EventHandler, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);
        }
    }
}
