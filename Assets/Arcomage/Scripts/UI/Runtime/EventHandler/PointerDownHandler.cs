using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NOAH.UI
{
    public class PointerDownHandler : EventHandler, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            bool findTween = false;
            if (TryGetComponent<DG.Tweening.DOTweenAnimation>(out var a))
            {
                var tweens = GetComponents<DG.Tweening.DOTweenAnimation>();
                foreach (var tween in tweens)
                {
                    if (tween.id == "press")
                    {
                        findTween = true;
                        if (tween.onComplete == null)
                            tween.onComplete = new UnityEvent();
                        tween.onComplete.RemoveAllListeners();
                        tween.onComplete.AddListener(() => OnPointerDownImp(eventData));
                        tween.DORestartById("press");
                        break;
                    }
                }
                
            }

            if (!findTween)
            {
                OnPointerDownImp(eventData); 
            }
        }
        void OnPointerDownImp(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);

            if (!string.IsNullOrEmpty(PassEventTag))
            {
                PassEvent(eventData, ExecuteEvents.pointerDownHandler);
            }
        }
    }
}
