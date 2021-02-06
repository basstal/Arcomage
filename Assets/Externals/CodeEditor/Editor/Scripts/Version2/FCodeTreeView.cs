using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using static UnityEditor.GenericMenu;
using System.Reflection.Emit;
using UnityEditorInternal;
using CodeEditor;

namespace CodeEditor2
{
    public class FCodeTreeView : CodeEditorTreeView<FCodeLogic>
    {
        public GUIStyle GUIStyle = new GUIStyle()
        {
            wordWrap = true,
            richText = true,
        };
        private FCodeLogic m_fcodeLogic;
        private FCodeTreeViewItem m_buildRoot;
        public ReorderableList instancesList;
        public FCodeLogic fcodeLogic => m_fcodeLogic;

        public override CodeEditorTreeViewItem buildRoot
        {
            get => m_buildRoot;
        }
        public FCodeTreeView(TreeViewState state, string name, FCodeLogic logic) : base(state, name)
        {
            m_fcodeLogic = logic;
            this.name = name;
            if (FCodeWindow.instance.savedCopyItems != null)
            {
                copyItems = FCodeWindow.instance.savedCopyItems;
            }
            InstanceListReload();
        }

        public FCodeTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            if (m_buildRoot == null)
            {
                var root = new FCodeTreeViewItem(-1);
                root.id = -1;
                m_buildRoot = (FCodeTreeViewItem)root.Build(m_fcodeLogic);
                root.AddChild(m_buildRoot);
            }
            ReorderMoveId();
            
            return m_buildRoot.parent;
        }

        public override object ToData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("local LogicList");
            sb.AppendLine("LogicList = {");
            if (buildRoot != null && buildRoot.children != null)
            {
                ValidateInstance();
                foreach (FCodeTreeViewItem item in buildRoot.children)
                {
                    if (item.fcodeNodeType == FCodeNodeType.Timing)
                    {
                        var singleLogic = (FCodeLogic.SingleLogic)item.data;
                        sb.AppendLine("\t{");
                        sb.AppendLine($"\t\t-- {FCodeWindow.instance.fcodeSignature["SingleLogicStart"]}");
                        // ** todo 这里根据项目不同有不用的枚举获取方式 先暂时这么写
                        sb.AppendLine($"\t\ttiming = PBEnum.FCodeTiming.{singleLogic.timing},");
                        WriteFunction("condition", item, sb);
                        WriteFunction("command", item, sb);
                        sb.AppendLine($"\t\t-- {FCodeWindow.instance.fcodeSignature["SingleLogicEnd"]}");
                        sb.AppendLine("\t},");
                    }
                }

                sb.AppendLine("\tvars = ");
                sb.AppendLine($"\t-- {FCodeWindow.instance.fcodeSignature["VarsStart"]}");
                sb.AppendLine("\t{");

                HashSet<string> checkRepeated = new HashSet<string>();
                foreach (FCodeTreeViewItem item in buildRoot.children)
                {
                    if (item.fcodeNodeType == FCodeNodeType.Vars)
                    {
                        var luaFunction = (FCodeLuaFunction)item.data;
                        var isRepeatedName = checkRepeated.Contains(luaFunction.name);
                        var firstIsLetter = Char.IsLetter(luaFunction.name[0]);
                        if (!isRepeatedName && firstIsLetter)
                        {
                            WriteFunction(luaFunction.name, item, sb);
                            checkRepeated.Add(luaFunction.name);
                        }
                        else if (isRepeatedName)
                        {
                            Debug.LogWarning($"vars 输出错误：\n有重复的vars变量名 : {luaFunction.name}，仅输出第一个！");
                        }
                        else
                        {
                            Debug.LogWarning($"vars 输出错误：\n变量名不合法，第一个字符必须为字母 [a-z] or [A-Z] ");
                        }
                    }
                }
                sb.AppendLine("\t},");
                sb.AppendLine($"\t-- {FCodeWindow.instance.fcodeSignature["VarsEnd"]}");

                sb.AppendLine("\tinstances = ");
                sb.AppendLine($"\t-- {FCodeWindow.instance.fcodeSignature["InstancesStart"]}");
                sb.AppendLine("\t{");

                // ** 先输出默认的instance 再输出其他instance
                var templateIndex = m_fcodeLogic.instances.FindIndex(v => v.instanceName.Equals(FCodeLogic.templateInstance));
                if (templateIndex > 0)
                {
                    var template = m_fcodeLogic.instances[templateIndex];
                    m_fcodeLogic.instances.RemoveAt(templateIndex);
                    m_fcodeLogic.instances.Insert(0, template);
                }
                foreach (var instanceConfig in m_fcodeLogic.instances)
                {
                    WriteInstance(instanceConfig, sb);
                }
                sb.AppendLine("\t},");
                sb.AppendLine($"\t-- {FCodeWindow.instance.fcodeSignature["InstancesEnd"]}");
            }
            sb.AppendLine("}");
            sb.AppendLine();
            sb.Append("return LogicList");


            return sb.ToString();
        }

        // ** 检查合法的变量名，修正不合法的输出
        void ValidateInstance()
        {
            // ** id 为已占用命名 不能取名为id
            HashSet<string> checkRepeated = new HashSet<string>(){"id"};

            var templateInstanceConfig = m_fcodeLogic.instances.Find(v => v.instanceName.Equals(FCodeLogic.templateInstance));
            for (int i = 0; i < templateInstanceConfig.values.Count; ++i )
            {
                var configVal = templateInstanceConfig.values[i];
                var checkName = configVal.key;
                if (string.IsNullOrEmpty(checkName) || checkRepeated.Contains(checkName))
                {
                    var newName = GenerateConfigValKey(ref templateInstanceConfig);
                    checkRepeated.Add(newName);
                    RefreshConfigValKey(ref configVal, newName);
                    EditorUtility.DisplayDialog("命名修正", $"{checkName} 命名将被自动修正为合法命名: {newName}", "ok");
                }
                else if (!Char.IsLetter(checkName[0]))
                {
                    var match = Regex.Match(checkName, @"[a-zA-Z_][a-zA-Z0-9_]*");
                    string rename = match.Value;
                    if (string.IsNullOrEmpty(rename) || checkRepeated.Contains(rename))
                    {
                        rename = GenerateConfigValKey(ref templateInstanceConfig);
                    }
                    checkRepeated.Add(rename);
                    RefreshConfigValKey(ref configVal, rename);
                    EditorUtility.DisplayDialog("命名修正", $"{checkName} 命名将被自动修正为合法命名: {rename}", "ok");
                }
            }
        }
        void WriteInstance(FCodeLogic.InstanceConfig instanceConfig, StringBuilder sb)
        {
            // ** 这是为了兼容用数字作为key的改动
            sb.AppendLine($"\t\t[\"{instanceConfig.instanceName}\"] = ");
            sb.AppendLine("\t\t{");
            foreach(var configVal in instanceConfig.values)
            {
                string output = "nil";
                switch(configVal.type)
                {
                    case FCodeLogic.InstanceConfigType.String:
                        output = $"\"{configVal.value}\"";
                        break;
                    case FCodeLogic.InstanceConfigType.Number:
                        if (configVal.value == null || !Double.TryParse((string)configVal.value, out var dVal))
                        {
                            output = "0";
                        }
                        else
                        {
                            output = dVal.ToString();
                        }
                        configVal.value = output;
                        break;
                }
                sb.AppendLine($"\t\t\t{configVal.key} = {output},");
            }
            sb.AppendLine("\t\t},");
        }
        void WriteFunction(string word, FCodeTreeViewItem item, StringBuilder sb)
        {
            FCodeTreeViewItem logicItem = item.displayName == word ? item : null;
            var c1 = item.children?.Find(o =>
            {
                var c = (FCodeTreeViewItem) o;
                return (c.fcodeNodeType == FCodeNodeType.Command || c.fcodeNodeType == FCodeNodeType.Condition || c.fcodeNodeType == FCodeNodeType.Vars) && c.displayName == word;
            });
            logicItem = logicItem ?? (c1 != null ? (FCodeTreeViewItem)c1 : null);
            if (logicItem != null && logicItem.children?.Count > 0)
            {
                sb.AppendLine($"\t\t{word} = function()");
                if (logicItem.fcodeNodeType == FCodeNodeType.Vars)
                {
                    sb.AppendLine($"\t\t\tlocal {word} = nil");
                }
                Stack<int> s = new Stack<int>();
                logicItem.Traverse(child =>
                {
                    if (child != logicItem)
                    {
                        var c = (FCodeTreeViewItem) child;
                        var diff = c.depth - logicItem.depth;
                        var t = new string('\t', diff);
                        if (c.fcodeNodeType == FCodeNodeType.If || c.fcodeNodeType == FCodeNodeType.Foreach)
                        {
                            if (s.Count == 0 || s.Peek() < diff)
                            {
                                s.Push(diff);
                            }
                            else 
                            {
                                while (s.Count > 0 && s.Peek() >= diff)
                                {
                                    var nt = s.Pop();
                                    sb.AppendLine($"\t\t{new string('\t', nt)}end");
                                }
                                s.Push(diff);
                            }
                        }
                        else
                        {
                            while (s.Count > 0 && s.Peek() >= diff)
                            {
                                var nt = s.Pop();
                                sb.AppendLine($"\t\t{new string('\t', nt)}end");
                            }
                        }
                        sb.AppendLine($"\t\t{t}{child.displayName}");
                    }
                });
                while (s.Count > 0)
                {
                    var nt = s.Pop();
                    sb.AppendLine($"\t\t{new string('\t', nt)}end");
                }

                if (logicItem.fcodeNodeType == FCodeNodeType.Vars)
                {
                    sb.AppendLine($"\t\t\treturn {word}");
                }
                sb.AppendLine("\t\tend,");
            }
        }

        protected override bool ValidInsert(CodeEditorTreeViewItem t, CodeEditorTreeViewItem v)
        {
            var target = (FCodeTreeViewItem) t;
            var validate = (FCodeTreeViewItem) v;
            var result = true;
            // ** todo 这里也许可以继续简化
            switch (target.fcodeNodeType)
            {
                case FCodeNodeType.None:
                    result = validate.fcodeNodeType == FCodeNodeType.Timing ||
                             validate.fcodeNodeType == FCodeNodeType.Vars;
                    if (!result)
                    {
                        EditorUtility.DisplayDialog("非法的粘贴行为！", "根节点不允许粘贴除 timing or vars 以外的节点", "确认");
                    }
                    break;
                case FCodeNodeType.Command:
                case FCodeNodeType.Condition:
                case FCodeNodeType.Vars:
                case FCodeNodeType.If:
                case FCodeNodeType.Foreach:
                    result = validate.fcodeNodeType != FCodeNodeType.Timing &&
                             validate.fcodeNodeType != FCodeNodeType.Vars;
                    if (!result)
                    {
                        EditorUtility.DisplayDialog("非法的粘贴行为！", "timing or vars 只允许粘贴在根节点下", "确认");
                    }
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        protected override bool ValidCopy(CodeEditorTreeViewItem item)
        {
            var nodeType = ((FCodeTreeViewItem) item).fcodeNodeType;
            return nodeType != FCodeNodeType.None && nodeType != FCodeNodeType.Command &&
                   nodeType != FCodeNodeType.Condition;
        }

        protected override void ContextClickedItem(int id)
        {
            var clicked = (FCodeTreeViewItem)FindItem(id, rootItem);
            itemClicked = clicked;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy"), false, Copy);
            if (copyItems?.Count > 0)
            {
                menu.AddItem(new GUIContent("Paste"), false, () => Paste());
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste"));
            }
            menu.AddSeparator("");

            if(clicked.fcodeNodeType == FCodeNodeType.None)
            {
                var values = Enum.GetValues(typeof(FCodeTiming));
                List<FCodeTiming> timings = new List<FCodeTiming>();
                foreach(FCodeTiming value in values)
                {
                    if(value != 0)
                    {
                        timings.Add(value);
                    }
                }
                timings.Sort((a, b) => String.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                foreach (var timing in timings)
                {
                    menu.AddItem(new GUIContent($"Insert/Timings/{timing}"), false, ()=>
                    {
                        var command = FCodeParse.GenFunction("command");
                        var condition = FCodeParse.GenFunction("condition");
                        var data = new FCodeLogic.SingleLogic(timing, command, condition);
                        Insert(data);
                    });
                }

                menu.AddItem(new GUIContent("Insert/Vars"), false, () =>
                {
                    // ** todo 先临时写到这里 可以挪到配置中去
                    string defaultName = "default";
                    // ** 找到重复的命名 则在命名后 + 1
                    while (m_buildRoot.children.FindIndex(item =>
                    {
                        var fItem = (FCodeTreeViewItem) item;
                        return fItem.fcodeNodeType == FCodeNodeType.Vars && ((FCodeLuaFunction)fItem.data).name == defaultName;
                    }) != -1)
                    {
                        defaultName += "1";
                    }
                    var data = new FCodeLuaFunction(0) { name = defaultName, shortName = defaultName };
                    Insert(data);
                });
            }
            else if (clicked.fcodeNodeType == FCodeNodeType.Vars || clicked.fcodeNodeType == FCodeNodeType.Condition || clicked.fcodeNodeType == FCodeNodeType.Command || clicked.fcodeNodeType == FCodeNodeType.If || clicked.fcodeNodeType == FCodeNodeType.Foreach)
            {
                var dict = FCodeWindow.instance.fcodeLib.fcodeFunctions.OrderBy(o => o.Key);
                Dictionary<GUIContent, string> others = new Dictionary<GUIContent, string>();

                foreach (var entry in dict)
                {
                    var name = entry.Value.shortName;
                    var prefix = (string)FCodeWindow.instance.fcodeProperties["FCodeFunctionPrefix"];
                    var m = Regex.Match(name, $"{prefix}(set|get|condition|change)");
                    if (m.Groups.Count > 1)
                    {
                        menu.AddItem(new GUIContent($"Insert/Function/{m.Groups[1]}/{name}"), false, () =>
                        {
                            var data = new List<Token>() { new NameToken(name) };
                            Insert(data);
                        });
                    }
                    else
                    {
                        var interval = name.StartsWith(prefix) ? "Function" : "Expression_Math";
                        others.Add(new GUIContent($"Insert/{interval}/{name}"), name);
                    }
                }
                foreach(var entry in others)
                {
                    menu.AddItem(entry.Key, false, () => {
                        var data = new List<Token>() { new NameToken(entry.Value) };
                        Insert(data);
                    });
                }
                menu.AddItem(new GUIContent($"Insert/Expression"), false, () => { Insert(new List<Token>() { new LiteralToken('?') }); });

                menu.AddItem(new GUIContent($"Insert/If"), false, () => {
                    Insert(new List<Token>() { new TypedToken(TK.IF), new LiteralToken('?'), new TypedToken(TK.THEN) });
                });

                menu.AddItem(new GUIContent($"Insert/Foreach"), false, () => { 
                    Insert(new List<Token>() { new TypedToken(TK.FOR), new LiteralToken('?'), new LiteralToken(','), new LiteralToken('?'), new TypedToken(TK.IN) , new NameToken("pairs") , new LiteralToken('('), new LiteralToken('?'),new LiteralToken(')'), new TypedToken(TK.DO) });
                });
            }
            
            menu.AddItem(new GUIContent("Delete"), false, Delete);

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Move ↑"), false, () =>
            {
                Move(MoveOption.Up);
            });
            menu.AddItem(new GUIContent("Move ↓"), false, () =>
            {
                Move(MoveOption.Down);
            });

            menu.AddSeparator("");

            menu.ShowAsContext();
        }

        public override void Copy()
        {
            base.Copy();
            FCodeWindow.instance.savedCopyItems = copyItems;
        }
        void RefreshConfigValKey(ref FCodeLogic.InstanceConfig.ConfigVal configVal, string editKey)
        {
            var sameConfigKey = configVal.key;
            // ** 更新模板命名
            foreach(var instanceConfig in m_fcodeLogic.instances)
            {
                // ** 剔除模板
                if (instanceConfig.instanceName != FCodeLogic.templateInstance)
                {
                    var sameConfig = instanceConfig.values.Find(v => v.key.Equals(sameConfigKey));
                    if (sameConfig != null)
                    {
                        sameConfig.key = editKey;
                    }
                }
            }
            // ** 更新逻辑中的config命名
            m_buildRoot.ConfigNameRefresh(configVal.key, editKey);
            configVal.key = editKey;
        }
        string GenerateConfigValKey(ref FCodeLogic.InstanceConfig instanceConfig)
        {
            string newValName = "newVal";
            bool hasThisNewVal = true;
            int count = 0;
            do
            {
                if(instanceConfig.values.Find(v => v.key.Equals($"{newValName}{count}")) != null)
                {
                    count++;
                }
                else
                {
                    hasThisNewVal = false;
                }
            } while (hasThisNewVal);
            return $"{newValName}{count}";
        }
        public bool DrawInstanceConfig(bool isTemplate, FCodeLogic.InstanceConfig.ConfigVal configVal, FCodeLogic.InstanceConfig templateInstanceConfig)
        {
            if (isTemplate)
            {
                var editKey = GUILayout.TextField(configVal.key);
                if (!editKey.Equals(configVal.key))
                {
                    var result = templateInstanceConfig.values.FindAll(v => v.key.Equals(editKey)).ToList();
                    if (result.Count > 0)
                    {
                        EditorUtility.DisplayDialog("重复的参数名", $"{editKey} 已存在重复的名称！", "ok");
                        return false;
                    }
                    RefreshConfigValKey(ref configVal, editKey);
                    changed = true;
                }
                GUILayout.BeginHorizontal();
                {
                    var editType = (FCodeLogic.InstanceConfigType)EditorGUILayout.EnumPopup(configVal.type);
                    if (editType != configVal.type)
                    {
                        configVal.type = editType;
                        m_fcodeLogic.SyncConfigValChange(configVal);
                    }
                    if(GUILayout.Button("-", GUILayout.MaxWidth(50f)))
                    {
                        templateInstanceConfig.values.Remove(configVal);
                        m_fcodeLogic.SyncConfigValRemove(configVal);
                        m_buildRoot.ConfigNameRefresh(configVal.key, "", true);
                        return true;
                    };
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                var template = templateInstanceConfig.values.Find(v => v.key.Equals(configVal.key));
                if (template == null)
                {
                    Debug.LogWarning($"当前实例的参数列表中变量 {configVal.key} 在Template中未定义");
                    return false;
                }
                GUILayout.Label(configVal.key);
                var editVal = GUILayout.TextField(configVal.value != null ? configVal.value.ToString() : "");
                switch(template.type)
                {
                    case FCodeLogic.InstanceConfigType.Number:
                        configVal.value = editVal;
                        break;
                    case FCodeLogic.InstanceConfigType.String:
                        if (!string.IsNullOrEmpty(editVal))
                        {
                            configVal.value = editVal;
                        }
                        else
                        {
                            configVal.value = "";
                        }
                        break;
                    default:
                        configVal.value = null;
                        break;
                }
            }
            GUILayout.Space(5);
            return false;
        }
        public void DrawInstances()
        {
            GUI.changed = false;
            var currentSelection = (string)instancesList.list[instancesList.index];
            var instanceConfig = m_fcodeLogic.instances.Find(v => v.instanceName.Equals(currentSelection));
            var templateInstanceConfig = m_fcodeLogic.instances.Find(v => v.instanceName.Equals(FCodeLogic.templateInstance));
            GUILayout.Label("参数列表");
            EditorGUILayout.Separator();
            if (instanceConfig == null)
            {
                Debug.LogWarning("参数列表为null？？");
                return;
            }
            bool isTemplate = templateInstanceConfig == instanceConfig;
            if (isTemplate)
            {
                GUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button("+"))
                    {
                        var newConfigVal = new FCodeLogic.InstanceConfig.ConfigVal()
                        {
                            key = GenerateConfigValKey(ref instanceConfig),
                        };
                        instanceConfig.values.Add(newConfigVal);
                        m_fcodeLogic.SyncConfigValAdd(newConfigVal);
                    };
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Separator();
            }
            foreach(var config in instanceConfig.values)
            {
                var valuesChanged = DrawInstanceConfig(isTemplate, config, templateInstanceConfig);
                if(valuesChanged)
                {
                    break;
                };
            }
            if (GUI.changed)
            {
                changed = true;
            }
        }
        public void InstanceListReload(string selectedInstance = FCodeLogic.templateInstance)
        {
            if (m_fcodeLogic == null) return;
            selectedInstance = selectedInstance ?? FCodeLogic.templateInstance;
            var iList = m_fcodeLogic.instances.Select(v => v.instanceName).ToList();
            if (instancesList == null)
            {
                instancesList = new ReorderableList(iList, typeof(string), false, false, true, true);
                instancesList.drawElementCallback += (rect, index, isActive, isFocused) => {
                    var name = (string)instancesList.list[index];
                    if (name == FCodeLogic.templateInstance)
                    {
                        EditorGUI.LabelField(rect, $"<color=#FFFF00>{name}</color>", GUIStyle);
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, name);
                    }
                };
                instancesList.onAddCallback += list => {
                    var newFile = EditorWindow.GetWindow<NewFileWindow>("新建FCode脚本instance");
                    newFile.Open(NewFileOption.FCodeInstance, FCodeWindow.instance);
                };
                instancesList.onSelectCallback += list => {
                    var focusInstance = (string)list.list[list.index];
                    list.index = list.list.IndexOf(focusInstance);
                };
                instancesList.onRemoveCallback += list =>
                {
                    var thisList = list.list;
                    var deleteInstanceName = (string)thisList[list.index];
                    if (deleteInstanceName.Equals(FCodeLogic.templateInstance))
                    {
                        EditorUtility.DisplayDialog("删除文件", "无法删除默认的instance", "ok");
                        return;
                    }
                    else if (EditorUtility.DisplayDialog("删除文件", $"确认删除instance [{deleteInstanceName}]?", "Yes", "No"))
                    {
                        var delete = m_fcodeLogic.instances.Find(v => v.instanceName.Equals(deleteInstanceName));
                        m_fcodeLogic.instances.Remove(delete);
                        // ** 删除前列表中有多于1个文件，按下标选中没有被删且【下标 -1】的文件，如果是下标为0的文件被删，则特殊选中下标为1的文件
                        if (thisList.Count > 1)
                        {
                            InstanceListReload((string)thisList[list.index > 0 ? list.index - 1 : 1]);
                        }
                        else
                        {
                            // ** 删除前列表中只有1个文件，则整个列表将会为空
                            InstanceListReload();
                        }
                    }
                };
                instancesList.index = iList.IndexOf(selectedInstance);
            }
            else
            {
                instancesList.list = iList;
                instancesList.index = iList.IndexOf(selectedInstance);
            }
            if (instancesList.index != -1)
            {
                // ** 按照找到的下标位置来选中对应的行为树
                instancesList.onSelectCallback(instancesList);
            }
            else
            {
                Debug.LogWarning($"没有默认的instance？？将自动添加一个默认的instance");
                m_fcodeLogic.NewInstance("default");
                InstanceListReload();
            }
        }
    }
}