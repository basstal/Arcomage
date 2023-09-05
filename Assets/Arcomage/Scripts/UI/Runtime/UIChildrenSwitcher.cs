using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NOAH.UI
{
    /// <summary>
    /// Controls multiple children on and off by stateID
    /// </summary>
    public class UIChildrenSwitcher : MonoBehaviour
    {
        public enum SwitchType
        {
            active = 0, //GameObject的隐藏显示
            color = 1,  //UIWidget的color
            scale = 2,  //缩放
            pos = 3,    //pos
            rectSizeDelta = 4,   //宽高
        }

        public const int MaxStateNum = 8;   //不想做动态长度，暂定8，再大的话ui也装不下了

        [System.Serializable]
        public class ChildState
        {
            public string Path;
            public SwitchType SwitchType;
            public string[] SData = new string[MaxStateNum] { "", "", "", "", "", "", "", "" };

            [System.NonSerialized]
            public Transform ChildCached;
        }

        public List<ChildState> StateList = new List<ChildState>();

        public int StateCount = 2;

        //自动处理Toggle Event(与Toggle联动)
        public bool AutoProcToggleEvent = true;

        [Tooltip("避免重复设置state时，执行操作（降低GC）")]
        public bool SetStateOnlyDifferentWithLastState = false;
        

        [TextArea]
        public string Desc;


        //上次被选中的state
        public int LastState => m_LastState;

        int m_LastState = -1;

        private void Awake()
        {
            if (AutoProcToggleEvent)
            {
                var tg = GetComponent<Toggle>();
                if (tg)
                    tg.onValueChanged.AddListener(delegate { OnCheck(); });
            }
        }

        public void SetState(int targetState)
        {
            if (targetState < 0 || targetState >= StateCount)
                return;

            if (SetStateOnlyDifferentWithLastState)
            {
                if (LastState == targetState)
                {
                    return;
                }
            }

            m_LastState = targetState;

            for (int i = 0; i < StateList.Count; i++)
            {
                ChildState chs = StateList[i];
                switchOneChildSt(chs, targetState);
            }
            
        }

        void findChildCached(ChildState chs)
        {
            if (string.IsNullOrEmpty(chs.Path))
            {
                chs.ChildCached = null;
                return;
            }
            chs.ChildCached = transform.Find(chs.Path);
        }

        void switchOneChildSt(ChildState chs, int targetState)
        {
            if (Application.isPlaying == false)
            {
                chs.ChildCached = null;
            }


            if (!chs.ChildCached)
                findChildCached(chs);
            if (!chs.ChildCached)
                return;

            if (chs.SwitchType == SwitchType.active)
            {
                chs.ChildCached.gameObject.SetActive(chs.SData[targetState] != "0");    //默认是"1"
            }
            else if (chs.SwitchType == SwitchType.color)
            {
                Graphic wid = chs.ChildCached.GetComponent<Graphic>();
                if (wid)
                {
                    wid.color = String2Color(chs.SData[targetState]);
                }
            }
            else if (chs.SwitchType == SwitchType.scale)
            {
                bool succ = float.TryParse(chs.SData[targetState], NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture,  out float scale_out);
                if(succ)
                    chs.ChildCached.localScale = new Vector3(scale_out, scale_out, scale_out);
            }
            else if (chs.SwitchType == SwitchType.pos)
            {
                // var pos = global::Extension.String2Vector3(chs.SData[targetState]);
                // chs.ChildCached.localPosition = pos;
            }
            else if(chs.SwitchType == SwitchType.rectSizeDelta && chs.ChildCached is RectTransform)
            {
                // var rectSizeDelta = global::Extension.String2Vector2(chs.SData[targetState]);
                // (chs.ChildCached as RectTransform).sizeDelta = rectSizeDelta;
            }
        }

        public void OnCheck()
        {
            if (AutoProcToggleEvent)
            {
                Toggle tg = GetComponent<Toggle>();
                if (tg)
                    SetState(tg.isOn ? 1 : 0);
            }

        }

        public int ActiveTargetIndex(string nname)
        {
            var activeIndex = -1;
            var index = StateList.FindIndex((c) =>
            {
                if(c.SwitchType != SwitchType.active || !c.Path.Contains(nname)) return false;
                activeIndex = c.SData.ToList().FindIndex((sd) => sd == "1");
                return true;
            });
            return activeIndex;
        }

        public void SetDefaultChildState(ChildState chs)
        {
            Transform trans = transform.Find(chs.Path);
            for (int i=0; i<StateCount; i++)
            {
                switch (chs.SwitchType)
                {
                    case SwitchType.active:
                        chs.SData[i] = trans && trans.gameObject.activeSelf ? "1" : "0";
                        break;
                    case SwitchType.color:
                        chs.SData[i] = Color2String(Color.white);
                        break;
                    case SwitchType.scale:
                        chs.SData[i] = "1";
                        break;
                    case SwitchType.pos:
                        if (trans)
                        {
                            var pos = trans.localPosition;
                            chs.SData[i] = $"{pos.x},{pos.y},{pos.z}";
                        }
                        else
                            chs.SData[i] = "0, 0, 0";
                        break;
                    case SwitchType.rectSizeDelta:
                        if (trans && trans is RectTransform)
                        {
                            var sizeDelta = (trans as RectTransform).sizeDelta;
                            chs.SData[i] = $"{sizeDelta.x},{sizeDelta.y}";
                        }
                        else
                            chs.SData[i] = "0, 0";
                        break;
                    default:
                        break;
                }
            }
        }


        static public string Color2String(Color cl)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(cl);
        }
        
        static public Color String2Color(string s)
        {
            bool succ = ColorUtility.TryParseHtmlString(s, out Color cl);
            return succ ? cl : Color.white;
        }
        /*
        #if UNITY_EDITOR
                [XLuaPredef.BlackList]
                public void OnHierarchyBtnClick()
                {
                    UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
                    for (int i = 0; i < StateCount; i++)
                    {
                        int nOrder = i; //delegate不能直接用i，需要隔离
                        menu.AddItem(new GUIContent("切换->" + nOrder), false, () =>
                        {
                            SetState(nOrder);
                        });
                    }

                    menu.ShowAsContext();
                    GUI.FocusControl(null);

                }
        #endif*/
    }
}
