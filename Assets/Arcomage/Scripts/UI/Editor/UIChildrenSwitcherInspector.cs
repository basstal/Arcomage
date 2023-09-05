
using UnityEngine;
using UnityEditor;

namespace NOAH.UI
{
    [CustomEditor(typeof(UIChildrenSwitcher))]
    public class UIChildrenSwitcherInspector : UnityEditor.Editor
    {
        UIChildrenSwitcher m_Target;

        const float CHILD_W = 180;
        const float CELL_W = 40;
        
        void OnEnable()
        {
            m_Target = target as UIChildrenSwitcher;
        }

        void OnDisable()
        {
            m_Target = null;
        }


        public override void OnInspectorGUI()
        {
            if (m_Target == null)
                return;

            var sp = serializedObject.FindProperty("Desc");
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(sp);

                onGUIFirstLine();
                for (int i = 0; i < m_Target.StateList.Count; i++)
                {
                    onGUILine(m_Target.StateList[i]);
                }


                //新增的一行
                {
                    UIChildrenSwitcher.ChildState chst = new UIChildrenSwitcher.ChildState();
                    onGUILine(chst);

                    if (!string.IsNullOrEmpty(chst.Path))
                    {
                        m_Target.StateList.Add(chst);
                    }
                }
            
            }

            sp = serializedObject.FindProperty("AutoProcToggleEvent");
            EditorGUILayout.PropertyField(sp, new GUIContent("自动处理Toggle"));

            sp = serializedObject.FindProperty("SetStateOnlyDifferentWithLastState");
            EditorGUILayout.PropertyField(sp, new GUIContent("忽略重复设置state"));

            if (EditorGUI.EndChangeCheck())
            {
                var so_temp = new SerializedObject(m_Target);
                serializedObject.CopyFromSerializedPropertyIfDifferent(so_temp.FindProperty("StateCount"));
                serializedObject.CopyFromSerializedPropertyIfDifferent(so_temp.FindProperty("StateList"));
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_Target);
            }


        }

        void onGUIFirstLine()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField("", GUILayout.Width(CHILD_W + 40));

            for (int i = 0; i < m_Target.StateCount; i++)
            {
                if (GUILayout.Button(i.ToString(), GUILayout.Width(CELL_W)))
                {
                    m_Target.SetState(i);
                }
            }

            if (m_Target.StateCount >= 1)
            {
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    m_Target.StateCount--;
                }
            }

            if (m_Target.StateCount <= UIChildrenSwitcher.MaxStateNum - 1)
            {
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    m_Target.StateCount++;
                }
            }


            EditorGUILayout.EndHorizontal();
        }

        void onGUILine(UIChildrenSwitcher.ChildState chs)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("X",GUILayout.Width(20)))
            {
                m_Target.StateList.Remove(chs);
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            GUILayout.Space(20);

            //控件路径
            if (string.IsNullOrEmpty(chs.Path))
            {
                chs.ChildCached = null;
                Transform tr = EditorGUILayout.ObjectField(null, typeof(Transform), true, GUILayout.Width(CHILD_W)) as Transform;
                if (tr != null)
                {
                    chs.Path = getTransformRelativePath(m_Target.transform, tr);
                }
            }
            else
            {
                float nChildW = CHILD_W * 0.6f;
                chs.Path = EditorGUILayout.TextField(chs.Path, GUILayout.Width(nChildW));
                var type = (UIChildrenSwitcher.SwitchType)EditorGUILayout.EnumPopup(chs.SwitchType, GUILayout.Width(CHILD_W - nChildW));
                if (type != chs.SwitchType)
                {
                    chs.SwitchType = type;
                    m_Target.SetDefaultChildState(chs);
                }
            }//控件路径 END


            if (!string.IsNullOrEmpty(chs.Path))
            {
                for (int i = 0; i < m_Target.StateCount; i++)
                {
                    if (chs.SwitchType == UIChildrenSwitcher.SwitchType.active)
                    {
                        bool activeOld = chs.SData[i] != "0";
                        bool activeNew = EditorGUILayout.Toggle(activeOld, GUILayout.Width(CELL_W));
                        chs.SData[i] = activeNew ? "1" : "0";
                    }
                    else if (chs.SwitchType == UIChildrenSwitcher.SwitchType.color)
                    {
                        var cl = UIChildrenSwitcher.String2Color(chs.SData[i]);
                        var clNew = EditorGUILayout.ColorField(cl, GUILayout.Width(CELL_W));
                        if(clNew!=cl)
                            chs.SData[i] = UIChildrenSwitcher.Color2String(clNew);
                    }
                    else if (chs.SwitchType == UIChildrenSwitcher.SwitchType.scale)
                    {
                        Transform trTemp = null;
                        if (!string.IsNullOrEmpty(chs.Path))
                            trTemp = m_Target.transform.Find(chs.Path);
                        if (trTemp)
                        {
                            if (chs.SData[i] == "")
                                chs.SData[i] = "1";
                            chs.SData[i] = EditorGUILayout.TextField(chs.SData[i], GUILayout.Width(CELL_W));
                        }
                    }
                    else if (chs.SwitchType == UIChildrenSwitcher.SwitchType.pos)
                    {
                        Transform trTemp = null;
                        if (!string.IsNullOrEmpty(chs.Path))
                            trTemp = m_Target.transform.Find(chs.Path);
                        if (trTemp)
                        {
                            int curIndex = i;   //保存一下，免得for循环把i冲了
                            if(GUILayout.Button( new GUIContent("p", chs.SData[curIndex]), GUILayout.Width(CELL_W)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("移动到:" + chs.SData[curIndex]), false, () =>
                                {
                                    // trTemp.localPosition = global::Extension.String2Vector3(chs.SData[curIndex]);
                                });
                                menu.AddItem(new GUIContent("写入当前坐标:" + trTemp.localPosition.ToString() ), false, () =>
                                {
                                    var pos = trTemp.localPosition;
                                    chs.SData[curIndex] = $"{pos.x},{pos.y},{pos.z}";
                                });

                                menu.ShowAsContext();
                                GUI.FocusControl(null);


                            }
                            
                        }
                    }
                    else if (chs.SwitchType == UIChildrenSwitcher.SwitchType.rectSizeDelta)
                    {
                        Transform trTemp = null;
                        if (!string.IsNullOrEmpty(chs.Path))
                            trTemp = m_Target.transform.Find(chs.Path);
                        if (trTemp && trTemp is RectTransform)
                        {
                            int curIndex = i;   //保存一下，免得for循环把i冲了
                            if (GUILayout.Button(new GUIContent("s", chs.SData[curIndex]), GUILayout.Width(CELL_W)))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("缩放到:" + chs.SData[curIndex]), false, () =>
                                {
                                    // (trTemp as RectTransform).sizeDelta = global::Extension.String2Vector2(chs.SData[curIndex]);
                                });
                                menu.AddItem(new GUIContent("写入当前SizeDelta:" + (trTemp as RectTransform).sizeDelta.ToString()), false, () =>
                                {
                                    var sizeDelta = (trTemp as RectTransform).sizeDelta;
                                    chs.SData[curIndex] = $"{sizeDelta.x},{sizeDelta.y}";
                                });

                                menu.ShowAsContext();
                                GUI.FocusControl(null);
                            }

                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        //获取相对路径（不知道官方有没有实现）
        static string getTransformRelativePath(Transform parentTrans, Transform curTrans)
        {
            //从对象恢复path
            if (curTrans == parentTrans)
                return "";
            
            Transform tr = curTrans;
            string tempPath = tr.name;
            while (true)
            {
                tr = tr.parent;
                if (tr == null) //到顶了, 未找到，不是子节点
                    return "";

                if (tr == parentTrans)
                {
                    return tempPath;
                }
                tempPath = tr.name + "/" + tempPath;
            }
        }

    }

}
