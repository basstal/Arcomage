using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 临时的，等需求出来再完善
namespace NOAH.UI
{
    public class TextNumberChange : MonoBehaviour
    {
        public TMP_Text TextComponent;
        [HorizontalGroup("Number",LabelWidth = 70),LabelText("开始值")]
        public float StartNumber;
        [HorizontalGroup("Number",LabelWidth = 70),LabelText("结束值")]
        public float EndNumber;
        [LabelText("数值变化曲线")]
        public AnimationCurve CurveNumberChange;
        [LabelText("保留位数")]
        public int KeepDecimalPlaces = 0;
        
        [LabelText("是否是固定时间长度"),Space]
        public bool IsFixDuration = true;
        [ShowIf("@this.IsFixDuration")]
        public float Duration;
        
        [ShowIf("@!this.IsFixDuration"),HorizontalGroup("MinDuration",LabelWidth = 70),LabelText("最短时间")]
        public float MinDuration;
        [ShowIf("@!this.IsFixDuration"),HorizontalGroup("MinDuration",LabelWidth = 70),LabelText("对应变化值")]
        public float MinDurationChangeValue;
        [ShowIf("@!this.IsFixDuration"),HorizontalGroup("MaxDuration",LabelWidth = 70),LabelText("最长时间")]
        public float MaxDuration;
        [ShowIf("@!this.IsFixDuration"),HorizontalGroup("MaxDuration",LabelWidth = 70),LabelText("对应变化值")]
        public float MaxDurationChangeValue;
        [ShowIf("@!this.IsFixDuration"),LabelText("时间变化曲线")]
        public AnimationCurve CurveDuration;
    
        [LabelText("变化间隔时间值")]
        public float ChangeInterval = 0.1f;

        private float m_passTime = -1;
        private float m_lastChangeTime = 0f;
        private float m_curDuration = -1;

        void Update()
        {
            if (m_passTime < 0 || TextComponent == null) return;
            // m_passTime += GameTime.deltaTime;
            // m_lastChangeTime += GameTime.deltaTime;
            if (m_lastChangeTime < ChangeInterval) return;

            m_lastChangeTime -= ChangeInterval;
            var curveKey = Math.Min(1, m_passTime / m_curDuration);
            var ratio = CurveNumberChange.Evaluate(curveKey);
            var curNumber = Mathf.Lerp(StartNumber, EndNumber, ratio);
            curNumber = FormatNumber(curNumber);

            TextComponent.text = "" + curNumber;
            if (m_passTime > m_curDuration)
            {
                m_passTime = -1;
            }
        }

        float FormatNumber(float num)
        {
            var formatVal = (int)Math.Pow(10, KeepDecimalPlaces);
            return Mathf.Floor(num * formatVal) / formatVal;
        }
        
        public void Play(float start,float finish,float duration = -1)
        {
            StartNumber = start;
            EndNumber = finish;
            if (duration >= 0)
            {
                IsFixDuration = true;
                Duration = duration;
            }

            Play();
        }

        [Button("播放")]
        public void Play()
        {
            if (TextComponent == null) return;
            var noChange = Mathf.Approximately(StartNumber, EndNumber);
            if (noChange)
            {
                TextComponent.text = "" + FormatNumber(EndNumber);
                return;
            }

            TextComponent.text = "" + FormatNumber(StartNumber);
            if (IsFixDuration)
            {
                m_curDuration = Duration;
            }
            else
            {
                var change = Math.Abs(StartNumber - EndNumber);
                var curveKey = (change - MinDurationChangeValue) / (MaxDurationChangeValue - MinDurationChangeValue);
                curveKey = Math.Min(1,Math.Max(0,curveKey));
                var ratio = CurveDuration.Evaluate(curveKey);
                m_curDuration = Mathf.Lerp(MinDuration, MaxDuration, ratio);
            }

            m_lastChangeTime = 0;
            m_passTime = 0;
        }

        public void Stop(bool setEndNum)
        {
            if (TextComponent == null) return;
            m_passTime = -1;
            if (setEndNum)
            {
                TextComponent.text = "" + FormatNumber(EndNumber);
            }
        }
    }
}
