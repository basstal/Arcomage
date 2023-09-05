using UnityEngine;
using System;
using System.Collections.Generic;
using NOAH.UI;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleData : MonoBehaviour,ISaveRunTime
{
    public const float InvalidValue = -999999;
    [Serializable,TableList]
    public class Pair
    {
        public string tag;
        public float val;
    }

    public List<Pair> Values = new List<Pair>();
    Dictionary<string,float> m_map;
    public Dictionary<string,float> Map{
        get{
            if(m_map == null)
            {
                m_map = new Dictionary<string,float>();
                foreach (var item in Values)
                {
                    m_map[item.tag] = item.val;
                }
            }
            return m_map;
        }
    }

    public float GetValue(string tag)
    {
        if (Map.TryGetValue(tag, out var result))
            return result;
        return InvalidValue;
    }

    [ContextMenu("Refresh"),Button]
    public void Refresh()
    {
        m_map = null;
    }

    public void SaveInRunTime(Component cmpt)
    {
#if UNITY_EDITOR
        var sc = cmpt as SimpleData;
        SerializedObject sCmpt = new SerializedObject(sc);
        SerializedObject sCmptCur = new SerializedObject(this);
        SerializedProperty sp = sCmptCur.FindProperty("Values");
        sCmpt.CopyFromSerializedProperty(sp);
        sCmpt.ApplyModifiedProperties();
#endif
    }
}
