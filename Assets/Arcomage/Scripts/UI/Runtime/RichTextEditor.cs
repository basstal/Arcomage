#if UNITY_EDITOR
// using NOAH.Utility;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NOAH.UI
{
    [ExecuteInEditMode]
	public class RichTextEditor : MonoBehaviour
	{
	    public string text;
        public string comment;
        public string size;
        public bool bold;
        public bool italic;
        public Color color;
	
	    [SerializeField]
	    private TextMeshProUGUI m_preview;

        [SerializeField]
        [Multiline]
        private string output;
	
        class TextMetrics
        {
            public Vector2 size;
            public int charCount;
        }

	    public void Commit()
	    {
            output = BuildText();
            m_preview.text = output;
            m_preview.ForceMeshUpdate();
        }

        private string ApplyTags(string text)
        {
            if (bold) text = $"<b>{text}</b>";
            if (italic) text = $"<i>{text}</i>";
            if (!string.IsNullOrWhiteSpace(size)) text = $"<size={size}>{text}</size>";
            if (color != new Color(0, 0, 0, 0)) text = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>";
            return text;
        }

        public string BuildText()
        {
            string result;

            var taggedCommnet = ApplyTags($"<size=50%>{comment}</size>");
            var taggedText = ApplyTags(text);

            var commentMetrics = GetTextMetrics(taggedCommnet);
            var textMetrics = GetTextMetrics(taggedText);
            var commentCharCount = commentMetrics.charCount;
            var textCharCount = textMetrics.charCount;
            var commentWidth = commentMetrics.size.x;
            var textWidth = textMetrics.size.x;

            if (!string.IsNullOrEmpty(comment))
            {
                if (textWidth > commentWidth)
                {
                    if (commentCharCount > 1)
                    {
                        float cspace = (textWidth - commentWidth) / (commentCharCount - 1);
                        result = $"{taggedText}<cspace={cspace}><space=-{textWidth}><voffset=1em>{taggedCommnet}</voffset></cspace>";
                    }
                    else
                    {
                        float space = (textWidth - commentWidth) * 0.5f;
                        result = $"{taggedText}<space=-{space + commentWidth}><voffset=1em>{taggedCommnet}</voffset><space={space}></cspace>";
                    }
                }
                else
                {
                    if (textCharCount > 1)
                    {
                        float cspace = (commentWidth - textWidth) / (textCharCount - 1);
                        result = $"<cspace={cspace}>{taggedText}</cspace><space=-{commentWidth}><voffset=1em>{taggedCommnet}</voffset>";
                    }
                    else
                    {
                        float space = (commentWidth - textWidth) * 0.5f;
                        result = $"<space={space}>{taggedText}<space=-{commentWidth - space}><voffset=1em>{taggedCommnet}</voffset>";
                    }
                }

                if (commentCharCount > 1 || textCharCount > 1) result = $"<nobr>{result}</nobr>";
            }
            else
            {
                result = taggedText;
            }

            return result;
        }

        private TextMetrics GetTextMetrics(string text)
        {
            m_preview.text = text;
            m_preview.ForceMeshUpdate(true);

            return new TextMetrics
            {
                size = m_preview.GetRenderedValues(),
                charCount = m_preview.textInfo.characterCount
            };
	    }
	
	    private void OnValidate()
	    {
	        Commit();
	    }

        [MenuItem("⛵NOAH/🔧Tools/UI/Rich Text Editor")]
        public static void OpenEditor()
        {
            // EditorSceneManager.OpenScene($"{NOAH.Utility.Const.InstallPath}/Modules/UI/Editor/RichTextEditor.unity");
        }
	}
}
#endif