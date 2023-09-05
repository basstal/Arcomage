
using UnityEngine;
using System.Collections.Generic;

namespace NOAH.UI
{

    public class UIList : MonoBehaviour
    {
        [Sirenix.OdinInspector.LabelText("复制对象")]
        public GameObject DuplicateFrom;

        [Tooltip("如果需要手动添加就改这里，并保证DuplicateFrom=null")]
        public List<GameObject> ListObj = new List<GameObject>();


        [Sirenix.OdinInspector.LabelText("项目GameObject名字格式")]
        public string GameObjectNameFormat = "{0}{1:D3}";

        [Sirenix.OdinInspector.LabelText("装饰性的尾部占位节点")]
        public GameObject TailObject;

        //public bool bAutoCallReposition = false;



        int m_ActiveCount = 0;

        private void Awake()
        {
            if (ListObj.Count == 0 && DuplicateFrom == null)
            {
                //当ui制作者忘记指定GameObject时，帮他填一个
                if (transform.childCount > 0)
                {
                    DuplicateFrom = transform.GetChild(0).gameObject;
                }
            }
            else if (ListObj.Count > 0 && DuplicateFrom == null)
            {
                //预先已经指定了一些SubItem
                m_ActiveCount = ListObj.Count;
            }

            if (Application.isPlaying)
            {
                if (DuplicateFrom)
                {
                    DuplicateFrom.SetActive(false);
                }

            }
        }


        public void Duplicate(int count)
        {
            bool bChanged = (count != m_ActiveCount);

            if (count > m_ActiveCount)
            {
                for (int nIndex = m_ActiveCount; nIndex < count; nIndex++)
                {
                    duplicateOne(DuplicateFrom);
                }
            }
            else if (count < m_ActiveCount)
            {
                for (int i = m_ActiveCount - 1; i >= count; i--)
                {
                    if (i >= ListObj.Count)
                        continue;
                    ListObj[i].SetActive(false);
                }
                m_ActiveCount = count;
            }

            resetTailNodePos();

            /*
                    if (bChanged && bAutoCallReposition && gameObject.activeInHierarchy)
                    {
                        gameObject.SendMessage("Reposition", SendMessageOptions.DontRequireReceiver);
                    }*/
        }

        public GameObject DuplicateAppendOne()
        {
            Duplicate(Count + 1);
            return Last;
        }

        GameObject duplicateOne(GameObject itSrc)
        {
            if (m_ActiveCount < ListObj.Count)
            {
                //如果在缓冲以内，直接返回缓冲的go
                m_ActiveCount++;
                ListObj[m_ActiveCount - 1].SetActive(true);
                return ListObj[m_ActiveCount - 1];
            }
            else if (itSrc)
            {
                //如果在缓冲以外，新建	
                GameObject goNew = GameObject.Instantiate(itSrc) as GameObject;
                goNew.SetActive(true);
                goNew.name = string.Format(GameObjectNameFormat, itSrc.name, m_ActiveCount);
                goNew.transform.SetParent(transform, false);
                goNew.transform.localPosition = itSrc.transform.localPosition;
                goNew.transform.localRotation = itSrc.transform.localRotation;
                goNew.transform.localScale = itSrc.transform.localScale;
                
                ListObj.Add(goNew);
                m_ActiveCount = ListObj.Count;
                return goNew;
            }
            return null;
        }


        public int Count => m_ActiveCount;
        public GameObject this[int index]
        {
            get
            {
                if (index < 0 || index >= m_ActiveCount)
                    return null;

                return ListObj[index];
            }
        }

        public GameObject Last
        {
            get
            {
                return (m_ActiveCount > 0) ? this[m_ActiveCount - 1] : null;
            }
        }

        /// <summary>
        /// 查询传入参数是第几个
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int IndexOf(UnityEngine.Object obj)
        {
            if (!obj)
                return -1;

            GameObject find = obj as GameObject;
            if (!find)
            {
                find = (obj as Component)?.gameObject;
            }
            if (!find)
                return -1;

            for (int i = 0; i < Count; i++)
            {
                if (this[i] == find)
                    return i;
            }
            return -1;
        }

        public void DestroyAllHiddenChild()
        {
            for (int i = ListObj.Count - 1; i >= 0; i--)
            {
                var uiit = ListObj[i];
                if (uiit.activeSelf == false)
                {
                    ListObj.Remove(uiit);

                    if (Application.isPlaying)
                        GameObject.Destroy(uiit);
                    else
                        GameObject.DestroyImmediate(uiit);
                }
            }

            m_ActiveCount = ListObj.Count;
        }

        void resetTailNodePos()
        {
            if (TailObject)
            {
                TailObject.transform.SetSiblingIndex(TailObject.transform.parent.childCount - 1);
            }
        }
        

        public void OnHierarchyBtnClick()
        {
#if UNITY_EDITOR
            UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
            for (int i = 1; i <= 5; i++)
            {
                int nOrder = i; //delegate不能直接用i，需要隔离
                menu.AddItem(new GUIContent("复制为:" + nOrder + " 个项目"), false, () =>
                {
                    Duplicate(nOrder-1);
                    DestroyAllHiddenChild();
                });
            }

            menu.ShowAsContext();
            GUI.FocusControl(null);
#endif
        }
    }
}