using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameScripts
{
    public class EffectInstance : MonoBehaviour
    {
        [NonSerialized] public float duration;
        [NonSerialized] public EffectCache cacheInstance;
        private bool m_isPlaying;

        float GetTotalTime(DOTweenAnimation doTweenAnimation)
        {
            return doTweenAnimation.duration * doTweenAnimation.loops + doTweenAnimation.delay;
        }

        public void Play(Vector3 position)
        {
            transform.position = position;
            var doTweenAnimations = GetComponentsInChildren<DOTweenAnimation>();
            float maxDuration = -1f;
            foreach (var doTweenAnimation in doTweenAnimations)
            {
                doTweenAnimation.duration = 2f;
                doTweenAnimation.RecreateTweenAndPlay();
                float totalTime = GetTotalTime(doTweenAnimation);
                maxDuration = maxDuration < totalTime ? totalTime : maxDuration;
            }

            Assert.IsTrue(maxDuration > 0);
            duration = maxDuration;
            m_isPlaying = true;
        }

        public void Update()
        {
            if (m_isPlaying)
            {
                duration -= Time.deltaTime;
                if (duration <= 0)
                {
                    m_isPlaying = false;
                    cacheInstance.Recycle(this);
                }
            }
        }
    }
}