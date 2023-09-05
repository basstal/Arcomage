using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// IEventSystemHandler used to mark Graphic on the same Game Object as raycast target.
namespace NOAH.UI
{
    public class EventHandler : MonoBehaviour, IEventSystemHandler
    {
        [NonSerialized]
        public UnityAction<BaseEventData> Callback;
        public string PassEventTag = "";

        static List<RaycastResult> results = new List<RaycastResult>();
        public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
        {
            results.Clear();
            EventSystem.current.RaycastAll(data, results);
            var current = data.pointerCurrentRaycast.gameObject;
            foreach (var result in results)
            {
                if (current != result.gameObject)
                {
                    var eventCaptor = result.gameObject.GetComponent<EventCaptor>();
                    if (eventCaptor && eventCaptor.PassEventTag == PassEventTag)
                    {
                        ExecuteEvents.Execute(result.gameObject, data, function);
                    }
                }
            }
        }
    }
}
