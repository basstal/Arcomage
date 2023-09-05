using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 临时的，等需求出来再完善
namespace NOAH.UI
{
    public enum TextAnimationType
    {
        Wave
    }

    public enum TextAnimState
    {
        None,Playing,Pause,Finish
    }
    
    [Serializable]
    public class TyperConfig
    {
        [NonSerialized] public TextAnimState State = TextAnimState.None; // -1 未开启，0 进行中，1 结束
        [LabelText("OnEnable时播放")]    
        public bool TypeOnEnable = true;
        [LabelText("打印单字后等待时间")]  
        public float TypeWaitTime = 0.05f;
        [LabelText("特殊符号等待时间倍数")]  
        public float SymbolWaitMul = 5;
        [LabelText("同时渐变字数")]
        public float FadeInRange = 2f; // 如果是0的话会直接出现，如果大于0，则会有一个过渡

        [LabelText("同时淡入字数")] public int FadeInCount = 3;
        

        [LabelText("打印完成后传递信息Key")] public string BroadCastKey = "";
        [LabelText("打印完成后传递信息Val")] public string BroadCastVal = "";
        
        public float Duration { get; set; }
        
            
        [NonSerialized]
        public HashSet<int> CharIndex = new HashSet<int>();
        [NonSerialized] public int CurIndex;
        [NonSerialized] public float TimePass;
        [NonSerialized] public float TimePassSinceLastCharIn;
            
        
        [NonSerialized] public int CharCount = 0;
        [NonSerialized] public List<float> ListInTimePoint = new List<float>();

        [NonSerialized] public float Speed = 1f; //倍速，实际的打印单字后等待时间为 TypeWaitTime / Speed;
        [NonSerialized] public float DurationAppoint = -1; //指定的打印时长
    }
    
    public class TextEffect : MonoBehaviour
    {
        public TMP_Text TextComponent;
        private bool m_hasTextChanged;
        //public bool PlayOnEnable;
        
        public TyperConfig Typer;
        
        
        private Coroutine m_animCoroutine;
        
        //Wave
        public float WaveSpeed = 1;
        public float WaveMax = 1;
        public float WaveWaitTime = 0.5f;
        private float m_baseWaveY = 0;
        private float m_invalideBaseWaveY = 999999;

        // private string m_broadcastKey = "";
        // private string m_broadcastVal = "";

        public class AnimConfig
        {
            public int StartIndex;
            public int EndIndex;
            public TextAnimationType AnimationType;
        }

        private List<AnimConfig> m_anims;

        public bool Finish
        {
            get
            {
                if(Typer == null) return true;
                return Typer.State == TextAnimState.Finish;
            }
        }
        

        private void OnEnable()
        {
            if (Typer.TypeOnEnable)
            {
                TyperPlay();
            }
            
            // TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
            // HandleContentInfo();
            // m_baseWaveY = m_invalideBaseWaveY;
            
            // if (!PlayOnEnable) return;
            //m_animCoroutine = StartCoroutine(DoAnimation());
        }

        void Update()
        {
            if (TextComponent.textInfo.characterCount <= 0) return;
            if (Typer.State == TextAnimState.Playing)
            {
                // TyperUpdate(GameTime.deltaTime);
            }
        }

        public void TyperPlayAppointDuration(string content, float duration)
        {
            TyperPlay(content);
            Typer.DurationAppoint = duration;
        }

        public void TyperPlay(string content = "")
        {
            if (!String.IsNullOrEmpty(content))
            {
                //为了快速初始化textInfo的信息（用于文字特效），必须使用此方法设置文本内容
                TextComponent.SetBudouXText(content, true);
            }
            TextComponent.typeWriterPosition = 0;
            TextComponent.maxVisibleCharacters = 0;
            Typer.State = TextAnimState.Playing;
            Typer.TimePass = 0;
            Typer.TimePassSinceLastCharIn = 0;
            Typer.Duration = -1;
            Typer.CharIndex.Clear();
            Typer.CurIndex = -1;
            Typer.DurationAppoint = -1;
            Typer.Speed = 1;
            Typer.ListInTimePoint.Clear();
            TextComponent.ToggleTypeWriter(Typer.FadeInRange > 0 );
            if (Typer.FadeInRange > 0)  //渐变的
            {
                TextComponent.typeWriterRange = Typer.FadeInRange;
                TextComponent.typeWriterPosition = -Typer.FadeInRange;
            }
            else
            {
                TextComponent.typeWriterPosition = TextComponent.textInfo.characterCount;
            }
        }

        void TyperUpdate(float delta)
        {
            if (Typer.Duration < 0)
            {
                TyperDurationInit();
            }

            if (Typer.TimePass > Typer.Duration)
            {
                Typer.State = TextAnimState.Finish;
                // if(!String.IsNullOrEmpty(Typer.BroadCastKey) && JsManager.Instance)
                //     JsManager.Instance.BroadCSEvent(Typer.BroadCastKey,Typer.BroadCastVal);
                return;
            }

            var realSingleWaitTime = Typer.TypeWaitTime / Typer.Speed;
            
            if (Typer.CurIndex == -1)
            {
                Typer.TimePass = realSingleWaitTime;
                Typer.TimePassSinceLastCharIn = realSingleWaitTime;
            }
            else
            {
                Typer.TimePass += delta;
                Typer.TimePassSinceLastCharIn += delta;
            }

            //字符
            while (true)
            {
                if (Typer.CurIndex < TextComponent.textInfo.characterCount - 1)
                {
                    var isSpecialChar = Typer.CharIndex.Contains(Typer.CurIndex);
                    var waitShowNext =  isSpecialChar ? realSingleWaitTime * Typer.SymbolWaitMul : realSingleWaitTime;

                    if (Typer.TimePassSinceLastCharIn >= waitShowNext)
                    {
                        TextComponent.maxVisibleCharacters++;
                        TextComponent.ForceMeshUpdate();
                        Typer.TimePassSinceLastCharIn = Typer.TimePassSinceLastCharIn - waitShowNext;
                        if (Typer.FadeInCount > 0)
                        {
                            if (Typer.ListInTimePoint.Count == Typer.FadeInCount + 1)
                            {
                                Typer.ListInTimePoint.Remove(0);
                            }
                            Typer.ListInTimePoint.Add(Typer.TimePass - Typer.TimePassSinceLastCharIn);
                        }
                        Typer.CurIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
                    
                    
                
            }
            
            if (Typer.FadeInRange > 0)
            {
                //渐变
                var oldP = TextComponent.typeWriterPosition;
                var newP = -Typer.FadeInRange + Typer.CurIndex + Typer.TimePassSinceLastCharIn / realSingleWaitTime; //+ 1
                TextComponent.typeWriterPosition = Mathf.Max(oldP, newP);
            }

            if (Typer.FadeInCount > 0)
            {
                var indexEffect = Typer.CurIndex;
                var during = realSingleWaitTime * Typer.FadeInCount;
                for (var i = Typer.ListInTimePoint.Count-1; i >= 0; i--)
                {
                    var alpha = (byte)(255 * Mathf.Lerp(0, 1, Mathf.Min(1, (Typer.TimePass - Typer.ListInTimePoint[i]) / during)));
                    SetCharAlpha(indexEffect, alpha);
                    indexEffect--;
                }
                TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }
        }

        void TyperDurationInit()
        {
            var stack = new Stack<int>();
            for (var i = 0; i < TextComponent.textInfo.characterCount; i++)
            {
                var c = TextComponent.textInfo.characterInfo[i];
                if (Char.IsPunctuation((c.character)))
                {
                    if (stack.Count > 0)
                    {
                        if (stack.Last() == i - 1)
                        {
                            stack.Pop();
                        }
                    }
                    stack.Push(i);
                }

                if (Typer.FadeInCount > 0)
                {
                    SetCharAlpha(i, 0);
                }
            }
            TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            while (stack.Count > 0)
            {
                Typer.CharIndex.Add(stack.Pop());
            }

            if (Typer.FadeInCount > 0)
            {
                Typer.Duration =
                    (TextComponent.textInfo.characterCount + Typer.CharIndex.Count * (Typer.SymbolWaitMul - 1) + Typer.FadeInCount) *
                    Typer.TypeWaitTime;
            }
            else
            {
                Typer.Duration =
                    (TextComponent.textInfo.characterCount + Typer.CharIndex.Count * (Typer.SymbolWaitMul - 1) + Mathf.Max(Typer.FadeInRange,Typer.FadeInCount)) *
                    Typer.TypeWaitTime;
            }

            if (Typer.DurationAppoint > 0)
            {
                Typer.Speed = Typer.Duration / Typer.DurationAppoint;
                Typer.Duration = Typer.DurationAppoint;
            }
            
        }

        public void TyperUpdateEditor(float passTime)
        {
#if UNITY_EDITOR
            TyperUpdate(passTime - Typer.TimePass);
#endif
        }
        
        public void TyperCompelete(bool showAll)
        {
            Typer.State = TextAnimState.Finish;
            Typer.Duration = -1;
            if (showAll)
            {
                var count = TextComponent.textInfo.characterCount;
                TextComponent.maxVisibleCharacters = count;
                TextComponent.typeWriterPosition = count;
                if (Typer.FadeInCount > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        SetCharAlpha(i, 255);
                    }
                    TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }
            }
        }

        void SetCharAlpha(int index, byte alpha)
        {
            var materialIndex = TextComponent.textInfo.characterInfo[index].materialReferenceIndex;
            var vertexIndex = TextComponent.textInfo.characterInfo[index].vertexIndex;
            var vertexColors = TextComponent.textInfo.meshInfo[materialIndex].colors32;

            // 零宽字符会导致下标越界
            if (vertexIndex + 3 < vertexColors.Length)
            {
                vertexColors[vertexIndex + 0].a = alpha;
                vertexColors[vertexIndex + 1].a = alpha;
                vertexColors[vertexIndex + 2].a = alpha;
                vertexColors[vertexIndex + 3].a = alpha;
            }
        }
        
        private void OnDisable()
        {
            TyperCompelete(true);
            // if(m_animCoroutine != null)
            //     StopCoroutine(m_animCoroutine);
            // m_animCoroutine = null;
            // TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }
        
        void ON_TEXT_CHANGED(UnityEngine.Object obj)
        {
            if (obj == TextComponent)
                m_hasTextChanged = true;
        }

        void HandleContentInfo()
        {
            m_anims = new List<AnimConfig>();
            var textInfo = TextComponent.textInfo;
            foreach (var link in textInfo.linkInfo)
            {
                var node = new AnimConfig();
                node.StartIndex = link.linkTextfirstCharacterIndex;
                node.EndIndex = link.linkTextfirstCharacterIndex + link.linkTextLength - 1;
                node.AnimationType = (TextAnimationType) Enum.Parse(typeof(TextAnimationType), link.GetLinkID());
                m_anims.Add(node);
            }

            m_anims.Sort((a, b) => a.StartIndex - b.StartIndex);
        }
        
        IEnumerator DoAnimation()
        {
            
            TextComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = TextComponent.textInfo;
            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
            
            int counter = 0;
            
            while (true)
            {
                if (m_hasTextChanged)
                {
                    cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                    m_hasTextChanged = false;
                }
                int charCount = textInfo.characterCount;
                
                int animIndex = 0;
                int animCount = m_anims.Count;
                AnimConfig animConfig = null;
                if (animIndex < animCount) animConfig = m_anims[0];
                for (int i = 0; i < charCount; i++)
                {
                    if(animConfig == null) break;
                    while(i > animConfig.EndIndex)
                    {
                        animIndex++;
                        if (animIndex == animCount)
                        {
                            animConfig = null;
                            break;
                        }
                        animConfig = m_anims[animIndex];
                    }
                    if(animConfig == null) break;
                    if(i < animConfig.StartIndex) continue;
                    
                    var charInfo = textInfo.characterInfo[i];
                    if(!charInfo.isVisible) continue;
                    
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Vector3[] svs = cachedMeshInfo[materialIndex].vertices;
                    Vector3[] dvs = textInfo.meshInfo[materialIndex].vertices;
                    
                    var y = (float)Math.Sin(counter*WaveSpeed + i) * WaveMax;
                    var appendV3 = new Vector3(0, y, 0);

                    dvs[vertexIndex + 0] = svs[vertexIndex + 0] + appendV3;
                    dvs[vertexIndex + 1] = svs[vertexIndex + 1] + appendV3;
                    dvs[vertexIndex + 2] = svs[vertexIndex + 2] + appendV3;
                    dvs[vertexIndex + 3] = svs[vertexIndex + 3] + appendV3;
                }

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }
                counter++;
                yield return new WaitForSeconds(WaveWaitTime);
            }
        }

        void UpdateBaseWaveBaseY(float y)
        {
            if (Mathf.Approximately(m_baseWaveY, m_invalideBaseWaveY))
            {
                m_baseWaveY = y;
            }
        }
    }
}
