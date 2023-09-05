using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class PointerExitHandler : EventHandler, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);
        }
    }
}
