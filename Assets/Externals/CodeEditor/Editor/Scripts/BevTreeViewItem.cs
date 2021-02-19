using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using CodeEditor;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeEditor
{
    public class BevTreeViewItem : CodeEditorTreeViewItem
    {
        public GUIStyle GUIStyle = new GUIStyle()
        {
            wordWrap = true,
            richText = true,
        };
        private static int m_itemId = 1;

        private int m_debugId;
        public override int moveId
        {
            get => m_debugId;
            set
            {
                m_debugId = value;
                DisplayNameRefresh();
            }
        }
        public CodeNode data { get; set; }

        public string nameWithDebugId { get; set; }
        public string nameNoId { get; set; }
        public BevTreeViewItem(CodeNode data, int depth)
        {
            this.id = m_itemId++;
            this.data = data;
            this.depth = depth;

            DisplayNameRefresh();
            switch (data?.NodeType)
            {
                case BevNodeType.Action:
                    icon = BevTreeWindow.instance.actionTex;
                    break;
                case BevNodeType.Condition:
                    icon = BevTreeWindow.instance.conditionTex;
                    break;
            }
        }

        void DisplayNameRefresh()
        {
            if (data == null) return;
            StringBuilder displayName = new StringBuilder();
            switch (BevTreeWindow.DISPLAY_TYPE)
            {
                case BevTreeDisplayType.EXPLANATION:
                case BevTreeDisplayType.FIELD_NAME_AND_EXPLANATION:
                    if (BevTreeWindow.instance.explanations.TryGetValue(data.Type, out var explanation))
                    {
                        displayName.Append(explanation);
                    }
                    else
                    {
                        displayName.Append(data.Type);
                    }
                    break;
                default:
                    displayName.Append(data.Type);
                    break;

            }
            displayName.Append(string.IsNullOrEmpty(data.Name) ? "" : $"({data.Name})");
            Composite composite = null;
            // switch(data.Type)
            // {
            //     case "Selector":
            //         composite = data.Selector;
            //         break;
            //     case "Sequence":
            //         composite = data.Sequence;
            //         break;
            // }
            if (composite != null && composite.AbortMode != BevAbortMode.None)
            {
                displayName.Append($"[{composite.AbortMode}]");
            }

            this.nameNoId = $"{displayName}";
            this.nameWithDebugId = $"{displayName} : {this.moveId}";
        }

        public override CodeEditorTreeViewItem Clone()
        {
            return Build(data.Clone());
        }

        public override CodeEditorTreeViewItem Build<T>(T data, int depth = 0)
        {
            if (data == null)
            {
                Debug.LogError("Build data is null??");
                return null;
            }

            if (data is CodeNode codeNode)
            {

                var item = new BevTreeViewItem(codeNode, depth);
                // ** todo 这里有时候会报空异常
                item.displayName = BevTreeWindow.instance.isBevTreeDisplayDebugId ? item.nameWithDebugId : item.nameNoId;

                if (codeNode.Children?.Count > 0)
                {
                    for (var i = 0; i < codeNode.Children.Count; ++i)
                    {
                        var childItem = Build(codeNode.Children[i], depth + 1);
                        item.AddChild(childItem);
                    }
                }
                return item;
            }
            throw new InvalidCastException($"BevTreeViewItem cannot build data type {typeof(T).Name}");
        }
        public override void Draw()
        {

            EditorTools.DrawIMessage(data);
        }
    }
}
