using System.Collections.Generic;
using System.Linq;
using Arcomage.GameScripts.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Arcomage.GameScripts
{
    public class CardAnimation : MonoBehaviour
    {
        public float delay = 0.075f;
        public float duration = 0.07f;
        public float baseDuration = 0.25f;
        public Card card;

        private bool _dirty = true;

        private int _offsetIndex;

        public int offsetIndex
        {
            set
            {
                _offsetIndex = value;
                _dirty = true;
            }
        }

        private Dictionary<string, DOTweenAnimation[]> _animations = new Dictionary<string, DOTweenAnimation[]>();
        private Dictionary<string, UnityAction> onCompleteCallbackById = new Dictionary<string, UnityAction>();

        // public float[] keyDurations = new[] { 0.35f, 0.5f, 1.01f, 1.09f, 1.16f };

        public void AssignOnCompleteCallbackById(string tweenId, UnityAction callback)
        {
            if (callback == null)
            {
                onCompleteCallbackById.Remove(tweenId);
                return;
            }

            onCompleteCallbackById[tweenId] = callback;

            _dirty = true;
        }

        public void Init()
        {
            AddAllChildAnimations(transform);
        }

        void AddAllChildAnimations(Transform inTransform)
        {
            for (int i = 0; i < inTransform.childCount; ++i)
            {
                var gameObj = inTransform.GetChild(i);
                var animations = gameObj.GetComponents<DOTweenAnimation>();
                _animations.Add(gameObj.name, animations);
                AddAllChildAnimations(gameObj.transform);
            }
        }


        public void ApplyOnCompleteCallback(DOTweenAnimation tweenAnimation, UnityAction callback)
        {
            tweenAnimation.hasOnComplete = true;
            if (tweenAnimation.onComplete == null)
            {
                tweenAnimation.onComplete = new UnityEvent();
            }

            tweenAnimation.onComplete.RemoveListener(callback);
            tweenAnimation.onComplete.AddListener(callback);
        }

        public void ApplyRules()
        {
            foreach (var animationGroup in _animations.Values)
            {
                foreach (var tweenAnimation in animationGroup)
                {
                    tweenAnimation.hasOnComplete = false;
                    var ids = tweenAnimation.id.Split(",");
                    foreach (var key in onCompleteCallbackById.Keys)
                    {
                        if (ids.Contains(key))
                        {
                            if (onCompleteCallbackById.TryGetValue(key, out var callback))
                            {
                                ApplyOnCompleteCallback(tweenAnimation, callback);
                            }
                        }
                    }

                    if (ids.Contains("UsingCardComplete"))
                    {
                        tweenAnimation.endValueTransform = card.owner.combat.cardCache.transform;
                    }

                    if (ids.Contains("UsingCard"))
                    {
                        tweenAnimation.endValueTransform = card.owner.combat.center;
                    }

                    if (ids.Contains("DisplayCardAnim"))
                    {
                        tweenAnimation.fromValueTransform = card.owner.handCardsLocation;
                        tweenAnimation.endValueTransform = card.owner.combat.handCardLayout.GetChild(_offsetIndex);
                        tweenAnimation.duration = _offsetIndex * duration + baseDuration;
                        tweenAnimation.delay = _offsetIndex * delay;
                    }

                    if (ids.Contains("AcquireCardAnim"))
                    {
                        tweenAnimation.fromValueTransform = card.owner.combat.cardCache.transform;
                        tweenAnimation.endValueTransform = card.owner.combat.handCardLayout.GetChild(_offsetIndex);
                        tweenAnimation.duration = duration * _offsetIndex + baseDuration;
                    }

                    if (ids.Contains("AcquireCardAnimFade"))
                    {
                        tweenAnimation.delay = delay * _offsetIndex;
                    }

                    if (ids.Contains("WithdrawAnim"))
                    {
                        tweenAnimation.endValueTransform = card.owner.handCardsLocation;
                        tweenAnimation.delay = delay * _offsetIndex;
                    }

                    if (ids.Contains("ChangeUsingCardLayer"))
                    {
                        ApplyOnCompleteCallback(tweenAnimation, () => { card.transform.SetParent(card.owner.combat.background.transform); });
                    }
                }
            }
        }

        public void Play(string gameObjName)
        {
            Log.LogInfo("[Animation]", $"Play CardAnimation {gameObjName}");
            if (_dirty)
            {
                ApplyRules();
                _dirty = false;
            }

            foreach (var tweenAnimation in _animations[gameObjName])
            {
                tweenAnimation.RewindThenRecreateTweenAndPlay();
            }
        }

// #if UNITY_EDITOR
//         [FoldoutGroup("DEBUG")] public GameObject DebugPlayTarget;
//
//         protected bool IsDebugging;
//         [FoldoutGroup("DEBUG")] public Combat DebugCombat;
//
//         [Button, ShowIf("ShowDebugPlay"), FoldoutGroup("DEBUG")]
//         public void DebugPlay()
//         {
//             combat = DebugCombat;
//             _animations.Clear();
//             Init(null);
//             ApplyRules();
//             IsDebugging = true;
//             DOTweenPreviewManager.PlayAllOnGameObject(DebugPlayTarget);
//         }
//
//         [Button, ShowIf("ShowDisablePreviewing"), FoldoutGroup("DEBUG")]
//         public void DisablePreviewing()
//         {
//             IsDebugging = false;
//             DOTweenPreviewManager.StopPreview(DebugPlayTarget);
//         }
//
//         public bool ShowDisablePreviewing()
//         {
//             return IsDebugging;
//         }
//
//         public bool ShowDebugPlay()
//         {
//             return DebugPlayTarget != null && !IsDebugging;
//         }
// #endif
    }
}