using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace NOAH.UI
{
    [InitializeOnLoad]
    public class UIMenu
    {
        static T AddUIComponent<T>(MenuCommand menuCommand) where T : Graphic
        {
            GameObject parent = menuCommand.context as GameObject ?? Selection.activeGameObject;
            if (parent != null)
            {
                var canvas = parent.GetComponentsInParent<Canvas>();
                if (canvas.Length == 0)
                    parent = null;
            }
            if (parent == null)
            {
                Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject obj = new GameObject("Canvas");
                    Undo.RegisterCreatedObjectUndo(obj, "Create Canvas");
                    canvas = obj.AddComponent<Canvas>();
                    canvas.gameObject.layer = LayerMask.NameToLayer("UI");
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                }
                parent = canvas.gameObject;
            }

            GameObject go = new GameObject(typeof(T).Name);
            GameObjectUtility.SetParentAndAlign(go, parent);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeGameObject = go;

            return go.AddComponent<T>();
        }

        [MenuItem("GameObject/⛵NOAH/UI/Text Mesh Pro #&t", false, 2001)]
        public static void AddTextPro(MenuCommand menuCommand)
        {
            TextMeshProUGUI tmpGUI = AddUIComponent<TextMeshProUGUI>(menuCommand) as TextMeshProUGUI;
            tmpGUI.lineSpacing = 0;
            tmpGUI.alignment = TextAlignmentOptions.Center;

        }
        
        static Dictionary<string, string> m_mapToTsR = new Dictionary<string,string> {
            {"Btn","UnityEngine.UI.Button"},
            {"Img","UnityEngine.UI.Image"},
            {"RImg","UnityEngine.UI.RawImage"},
            {"Input","TMPro.TMP_InputField"},
            {"Text","TMPro.TextMeshProUGUI"},
            {"ScrollView","UnityEngine.UI.ScrollRect"},
            {"Slider","UnityEngine.UI.Slider"},
            {"Progress","UnityEngine.UI.Slider"}
        };

        // [MenuItem("GameObject/To Ts R Simple", false, 22)]
        // public static void ToTsRSimple()
        // {
        //     var template = "{0}: {1},";
        //     var defaul =  "null";
        //     ToTsRImpl(template, defaul);
        // }
        // [MenuItem("GameObject/To Ts R", false, 21)]
        // public static void ToTsRFull()
        // {
        //     var template = "{0}:   this.Q(\"{1}\", {2}),";
        //     var defaul = "UnityEngine.RectTransform";
        //     ToTsRImpl(template, defaul);
        // }
        
        // public static void ToTsRImpl(string template,string defaul)
        // {
        //     var goes = Selection.gameObjects;
        //     var mapCmd = new Dictionary<string, string>();
        //     var hashSetRepeated = new HashSet<string>();
        //     //var listTrans = new List<Transform>();
        //     var paramCount = template.Contains("{2}") ? 3 : 2;
        //
        //     
        //
        //     string GetCmd(GameObject go)
        //     {
        //         if (go.name[0] >= 'A' && go.name[0] <= 'Z' && !go.name.StartsWith("TMP")) //大写的才需要
        //         {
        //             if (hashSetRepeated.Contains(go.name)) return "";
        //             if (mapCmd.TryGetValue(go.name, out var cmd))
        //             {
        //                 UnityEngine.Debug.LogWarning($"相同名字的节点： {go.name} ,它将不会添加到生成代码中");
        //                 hashSetRepeated.Add(go.name);
        //                 mapCmd.Remove(go.name);
        //                 return "";
        //             }
        //
        //             // var findType = false;
        //             var typ = defaul;
        //             foreach (var pair in m_mapToTsR)
        //             {
        //                 if (go.name.StartsWith(pair.Key))
        //                 {
        //                     typ = pair.Value;
        //                     break;
        //                 }
        //             }
        //
        //             if (paramCount == 3)
        //             {
        //                 cmd = string.Format(template, go.name, go.name, typ);
        //             }
        //             else
        //             {
        //                 cmd = string.Format(template, go.name, typ);
        //             }
        //
        //             return cmd;
        //         }
        //
        //         return "";
        //     }
        //     
        //     void GetChildCmd(GameObject rootGo)
        //     {
        //         if (rootGo.transform.childCount == 0) return;
        //         foreach (Transform t in rootGo.transform)
        //         {
        //             // if(t.TryGetComponent<JsInjector>(out var _)) continue;
        //             var cmd = GetCmd(t.gameObject);
        //             if(cmd != "") mapCmd.Add(t.name,cmd);
        //             GetChildCmd(t.gameObject);
        //         }
        //     }
        //     
        //     foreach (var rootGo in goes)
        //     {
        //         var cmd = GetCmd(rootGo);
        //         if(cmd != "") mapCmd.Add(rootGo.name,cmd);
        //         GetChildCmd(rootGo);
        //     }
        //
        //     var content =  String.Join("\n", mapCmd.Values.ToArray());
        //     
        //     var te = new UnityEngine.TextEditor();
        //     te.text = content;
        //     te.SelectAll();
        //     te.Copy();
        //     // System.Windows.Forms.Clipboard.SetText(content);
        // }
        
        // [MenuItem("GameObject/⛵NOAH/UI/Text #&t", false, 2001)]
        // public static void AddText(MenuCommand menuCommand)
        // {
        //     AddUIComponent<Text>(menuCommand);
        // }

        [MenuItem("GameObject/⛵NOAH/UI/Image #&e", false, 2002)]
        public static void AddImage(MenuCommand menuCommand)
        {
            AddUIComponent<Image>(menuCommand);
        }

        [MenuItem("GameObject/⛵NOAH/UI/RawImage #&w", false, 2003)]
        public static void AddRawImage(MenuCommand menuCommand)
        {
            AddUIComponent<RawImage>(menuCommand);
        }

        [MenuItem("GameObject/⛵NOAH/UI/InfinityScrollView", false, 10)]
        static void CreateInfinityScrollView()
        {
            CreateWrapContent.CreateInfinityScrollView();
        }

        [MenuItem("GameObject/⛵NOAH/UI/DynamicScrollView", false, 10)]
        static void CreateDynamicScrollView()
        {
            CreateWrapContent.CreateDynamicScrollView();
        }

        [MenuItem("GameObject/⛵NOAH/UI/InfinityGridScrollView", false, 10)]
        static void CreateGridScrollView()
        {
            CreateWrapContent.CreateGridScrollView();
        }

        [MenuItem("GameObject/⛵NOAH/UI/UIPageView", false, 10)]
        static void CreatePageView()
        {
            CreateWrapContent.CreatePageView();
        }

        [MenuItem("GameObject/⛵NOAH/UI/Add PolygonCollider2D")]
        static void AddPolygonCollider2D()
        {
            if (Selection.activeObject != null)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    RectTransform rectTransform = go.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        PolygonCollider2D polygonCollider = go.GetComponent<PolygonCollider2D>();
                        if (polygonCollider == null)
                        {
                            polygonCollider = go.AddComponent<PolygonCollider2D>();
                        }

                        var rect = rectTransform.rect;

                        Vector2[] vectors = new Vector2[4];
                        vectors[0] = new Vector2(0, 0);
                        vectors[1] = new Vector2(rect.width, 0);
                        vectors[2] = new Vector2(rect.width, rect.height);
                        vectors[3] = new Vector2(0, rect.height);

                        for (int i = 0; i < vectors.Length; ++i)
                        {
                            vectors[i] -= new Vector2(rect.width * rectTransform.pivot.x, rect.height * rectTransform.pivot.y);
                        }

                        polygonCollider.points = vectors;
                    }
                }
            }
        }

        [MenuItem(("GameObject/⛵NOAH/UI/ScrollViewEx"))]
        public static void AddScrollViewEx()
        {
            GameObject ob = new GameObject();
            ob.transform.parent = Selection.activeTransform;
            ob.name = "ScrollViewEx";
            ScrollRectEx sc = ob.AddComponent<ScrollRectEx>();
            sc.vertical = false;
            sc.horizontal = true;
            ScrollRectSnap scrollRectSnap = ob.AddComponent<ScrollRectSnap>();
            scrollRectSnap.autoSnap = false;
            RectTransform parentR = ob.GetComponent<RectTransform>();
            if (parentR == null)
            {
                ob.AddComponent<RectTransform>();
            }
            ob.transform.localScale = Vector3.one;
            ob.transform.localPosition = Vector3.zero;
            ob.transform.localRotation = Quaternion.identity;
            parentR.sizeDelta = new Vector2(600, 300);

            GameObject viewport = new GameObject();
            viewport.transform.parent = ob.transform;
            viewport.name = "Viewport";
            viewport.AddComponent<RectMask2D>();
            CanvasRenderer crd = viewport.AddComponent<CanvasRenderer>();
            crd.cullTransparentMesh = true;
            sc.viewport = viewport.GetComponent<RectTransform>();
            Image img = viewport.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.0f);
            viewport.AddComponent<EventHandler>();
            RectTransform r = viewport.GetComponent<RectTransform>();
            if (r == null)
            {
                viewport.AddComponent<RectTransform>();
            }
            r.localPosition = Vector3.zero;
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
            r.anchorMax = new Vector2(1.0f, 1.0f);
            r.anchorMin = new Vector2(0.0f, 0.0f);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentR.rect.width);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentR.rect.height);
            GameObject content = new GameObject("content");
            RectTransform contentRT = content.AddComponent<RectTransform>();
            content.transform.SetParent(viewport.transform, false);
            contentRT.anchorMin = Vector2.up;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            contentRT.pivot = Vector2.up;
            sc.content = contentRT;
        }

        [MenuItem("GameObject/⛵NOAH/UI/Button To ButtonWithImagesChildren", false, 2004)]
        static void UILabelToUIHtmlLabel()
        {
            if (Selection.activeObject != null)
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Button[] buttons = go.GetComponentsInChildren<Button>();
                    foreach(var button in buttons)
                    {
                        if (button != null)
                        {
                            if (button.GetType() == typeof(Button))
                            {
                                GameObject tempGO = new GameObject();
                                var tempButton = tempGO.AddComponent<Button>();
                                EditorUtility.CopySerialized(button, tempButton);
                                var gameObject = button.gameObject;
                                GameObject.DestroyImmediate(button);

                                var newButton = gameObject.AddComponent<ButtonWithImagesChildren>();
                                newButton.interactable = tempButton.interactable;
                                newButton.transition = tempButton.transition;
                                newButton.targetGraphic = tempButton.targetGraphic;
                                newButton.colors = tempButton.colors;

                                GameObject.DestroyImmediate(tempGO);
                            }
                        }
                    }
                }
            }
        }
    }
}
