using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace NOAH.UI
{
    public class PointerUpHandler : EventHandler, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData)
        {
            bool findTween = false;
            if (TryGetComponent<DG.Tweening.DOTweenAnimation>(out var a))
            {
                var tweens = GetComponents<DG.Tweening.DOTweenAnimation>();
                foreach (var tween in tweens)
                {
                    if (tween.id == "release")
                    {
                        findTween = true;
                        if (tween.onComplete == null)
                            tween.onComplete = new UnityEvent();
                        tween.onComplete.RemoveAllListeners();
                        tween.onComplete.AddListener(() => OnPointerUpImp(eventData));
                        tween.DORestartById("release");
                        break;
                    }
                }
            }

            if (!findTween)
            {
                OnPointerUpImp(eventData); 
            }
        }

        void OnPointerUpImp(PointerEventData eventData)
        {
            Callback?.Invoke(eventData);
            if (!string.IsNullOrEmpty(PassEventTag))
            {
                PassEvent(eventData, ExecuteEvents.pointerUpHandler);
            }
        }
    }
}
