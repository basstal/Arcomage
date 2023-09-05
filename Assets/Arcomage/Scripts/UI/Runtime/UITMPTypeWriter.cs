using DG.Tweening;
// using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NOAH.UI
{
    //UI专用的，启动TextMeshPro的TypeWriter的组件
    //抄TLDialog
    public class UITMPTypeWriter : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI Target;

        [Sirenix.OdinInspector.LabelText("过渡范围 >1")]
        public float TypeWriterGradiantRange = 1;

        [Sirenix.OdinInspector.ReadOnly]
        [Sirenix.OdinInspector.LabelText("当前播放位置")]
        public float TypeWriterPosition = 1000;

        bool m_Inited = false;
        Material m_TempMaterial;


        float m_RealStartTime = 0;
        float m_durationCached = 1;

        private void OnEnable()
        {
            if (!m_Inited)
            {
                m_Inited = true;
                if(Target)
                {
                    m_TempMaterial = new Material(Target.fontSharedMaterial);
                    m_TempMaterial.EnableKeyword("TYPEWRITER_ON");
                    Target.fontSharedMaterial = m_TempMaterial;
                }
            }
        }

        private void OnDestroy()
        {
            if (m_TempMaterial)
            {
                Object.Destroy(m_TempMaterial);
                m_TempMaterial = null;
            }
        }

        private void Update()
        {
            if (Target.textInfo == null)
                return;

            float timeEllapsd = Time.realtimeSinceStartup - m_RealStartTime;
            if(timeEllapsd > m_durationCached)
            {
                //播放完成
                TypeWriterPosition = Target.typeWriterPosition = Target.textInfo.characterCount;
                enabled = false;
                return;
            }

            TypeWriterPosition = (timeEllapsd / m_durationCached) * Target.textInfo.characterCount;
            Target.typeWriterPosition = TypeWriterPosition;

        }


        public void Play(string content, float duration)
        {
            this.enabled = true;

            Target.SetBudouXText(content);
            Target.ForceMeshUpdate();
            
            Target.typeWriterRange = Mathf.Max(1, TypeWriterGradiantRange);

            m_RealStartTime = Time.realtimeSinceStartup;
            m_durationCached = duration;
            TypeWriterPosition = Target.typeWriterPosition = 0;
        }

    }
}
