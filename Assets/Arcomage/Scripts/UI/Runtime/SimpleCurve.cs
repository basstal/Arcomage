using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NOAH.UI
{
    public class SimpleCurve  : MonoBehaviour,ISaveRunTime
    {
        public const float InvalidValue = -999999;
        [Serializable]
        public class Data
        {
            public string Tag;
            public AnimationCurve Curve;
            public float Duration;
        }
        
        public List<Data> Values = new List<Data>();
        Dictionary<string,Data> m_map;
        
        public Dictionary<string,Data> Map{
            get{
                if(m_map == null)
                {
                    m_map = new Dictionary<string,Data>();
                    foreach (var item in Values)
                    {
                        m_map[item.Tag] = item;
                    }
                }
                return m_map;
            }
        }

        public float GetValue(string tag, float timeRatio)
        {
            if(Map.TryGetValue(tag,out var data))
            {   
                return data.Curve.Evaluate(timeRatio);
            }
            return InvalidValue;
        }

        public float GetDuration(string tag)
        {
            if(Map.TryGetValue(tag,out var data))
            {
                return data.Duration;
            }
            return InvalidValue;
        }
        
        public void SaveInRunTime(Component c)
        {
#if UNITY_EDITOR
            var sc = c as SimpleCurve;
            SerializedObject sCmpt = new SerializedObject(sc);
            SerializedObject sCmptCur = new SerializedObject(this);
            SerializedProperty sp = sCmptCur.FindProperty("Values");
            sCmpt.CopyFromSerializedProperty(sp);
            sCmpt.ApplyModifiedProperties();
#endif
        }
    }
}
