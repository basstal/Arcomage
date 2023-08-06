using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcomage.GameScripts
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

        private float GetParticleCurveMax(ParticleSystem.MinMaxCurve curve)
        {
            float result;
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    result = curve.constant;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    result = curve.constantMax;
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                case ParticleSystemCurveMode.Curve:
                    result = curve.curveMultiplier;
                    break;
                default:
                    throw new Exception($"GetParticleCurveMax didn't handle mode {curve.mode}.");
            }

            return result;
        }

        private float CalculateParticleDuration(ParticleSystem particle)
        {
            var main = particle.main;
            var emission = particle.emission;
            float baseTime = 0;
            if (GetParticleCurveMax(emission.rateOverTime) != 0 || emission.rateOverTimeMultiplier != 0)
            {
                baseTime = main.duration;
            }

            if (emission.burstCount > 0)
            {
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
                emission.GetBursts(bursts);
                if (bursts.Length > 0)
                {
                    foreach (var burst in bursts)
                    {
                        if (burst.cycleCount == 0)
                        {
                            baseTime = main.duration;
                            break;
                        }

                        baseTime = Mathf.Clamp((burst.cycleCount - 1) * burst.repeatInterval + burst.time, baseTime, main.duration);
                    }
                }
            }

            return baseTime + GetParticleCurveMax(main.startLifetime) + GetParticleCurveMax(main.startDelay);
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
                maxDuration = Mathf.Max(totalTime, maxDuration);
            }

            var uiParticles = GetComponentsInChildren<UIParticle>();
            foreach (var uiParticle in uiParticles)
            {
                float maxDurationUIParticle = 0;
                foreach (var particle in uiParticle.particles)
                {
                    var time = CalculateParticleDuration(particle);
                    maxDurationUIParticle = Mathf.Max(maxDurationUIParticle, time);
                }

                uiParticle.Play();
                maxDuration = Mathf.Max(maxDuration, maxDurationUIParticle);
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