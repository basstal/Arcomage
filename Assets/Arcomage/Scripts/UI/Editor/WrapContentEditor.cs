using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;

namespace NOAH.UI
{
    public class CreateWrapContent
    {
        // Start is called before the first frame update

        [MenuItem("⛵NOAH/🔧Tools/UI/WrapLayoutValidate")]
        public static void ResetWrapLayoutScale()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets/AssetBundle/UI/Prefab/Window" });
            List<string> paths = new List<string>();
            guids.ToList().ForEach(m => paths.Add(AssetDatabase.GUIDToAssetPath(m)));
            int count = 0;
            foreach (string path in paths)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Validating", path, ((float)count++ / paths.Count)))
                {
                    break;
                }
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UIEditorUtil.UpdateUIPrefab(prefab, (instance) =>
                {
                    bool modifed = false;
                    UIWrapLayoutBase[] wraplayouts = instance.GetComponentsInChildren<UIWrapLayoutBase>(true);
                    foreach (var wrap in wraplayouts)
                    {
                        if(wrap.GetComponent<RectTransform>().localScale == Vector3.zero)
                        {
                            wrap.GetComponent<RectTransform>().localScale = Vector3.one;
                            modifed = true;
                        }
                    }
                    return modifed;
                },false);
            }
            EditorUtility.ClearProgressBar();
        }
        
        static void CreateHWrapContent(GameObject parent, int type)
        {
            GameObject ob = new GameObject();
            ob.transform.parent = parent.transform;
            UIWrapLayout l = null;
            if (type == 0)
            {
                ob.name = "WrapContent";
                l = ob.AddComponent<UIWrapLayout>();
            }
            else if (type == 1)
            {
                ob.name = "PageView";
                l = ob.AddComponent<UIPageView>() as UIWrapLayout;
            }

            l.Axis = UIWrapLayout.Direction.HORIZONTAL;
            RectTransform r = ob.GetComponent<RectTransform>();
            if (r == null)
            {
                ob.AddComponent<RectTransform>();
            }
            r.localScale = Vector3.one;
            r.localPosition = Vector3.zero;
            r.localRotation = Quaternion.identity;

            ScrollRect s = ob.GetComponentInParent<ScrollRect>();
            if (s)
            {
                s.content = ob.GetComponent<RectTransform>();
            }

            GameObject temCell = new GameObject();
            temCell.transform.parent = ob.transform;
            temCell.name = "Cell";
            r = temCell.GetComponent<RectTransform>();
            if (r == null)
            {
                temCell.AddComponent<RectTransform>();
            }
            UIWrapCell wrapCell = temCell.AddComponent<UIWrapCell>();
            temCell.transform.localScale = Vector3.one;
            temCell.transform.localPosition = Vector3.zero;
            temCell.transform.localRotation = Quaternion.identity;

            l.ResetEditorScrollView();
        }

        static void CreateDynamicWrapContent(GameObject parent)
        {
            GameObject ob = new GameObject();
            ob.transform.parent = parent.transform;
            UIWrapDynamicLayout l = null;

            ob.name = "DynamicWrapContent";
            l = ob.AddComponent<UIWrapDynamicLayout>();

            l.Axis = UIWrapLayout.Direction.VERTICAL;

            RectTransform r = ob.GetComponent<RectTransform>();
            if (r == null)
            {
                ob.AddComponent<RectTransform>();
            }
            r.localScale = Vector3.one;
            r.localPosition = Vector3.zero;
            r.localRotation = Quaternion.identity;

            ScrollRect s = ob.GetComponentInParent<ScrollRect>();
            if (s)
            {
                s.content = ob.GetComponent<RectTransform>();
            }

            GameObject temCell = new GameObject();
            temCell.transform.parent = ob.transform;
            temCell.name = "Cell";
            r = temCell.GetComponent<RectTransform>();
            if (r == null)
            {
                temCell.AddComponent<RectTransform>();
            }
            UIWrapCell wrapCell = temCell.AddComponent<UIWrapCell>();
            temCell.transform.localScale = Vector3.one;
            temCell.transform.localPosition = Vector3.zero;
            temCell.transform.localRotation = Quaternion.identity;
            
            l.ResetEditorScrollView();
        }

        static void CreateGridWrapContent(GameObject parent)
        {
            GameObject ob = new GameObject();
            ob.transform.parent = parent.transform;
            ob.name = "WrapGridContent";
            UIWrapGridLayout l = ob.AddComponent<UIWrapGridLayout>();
            l.Axis = UIWrapLayout.Direction.VERTICAL;
            RectTransform r = ob.GetComponent<RectTransform>();
            if (r == null)
            {
                ob.AddComponent<RectTransform>();
            }
            r.localScale = Vector3.one;
            r.localPosition = Vector3.zero;
            r.localRotation = Quaternion.identity;

            ScrollRect s = ob.GetComponentInParent<ScrollRect>();
            if (s)
            {
                s.content = ob.GetComponent<RectTransform>();
            }

            GameObject GridCell = new GameObject();
            GridCell.transform.parent = ob.transform;
            GridCell.name = "GridCell";
            r = GridCell.GetComponent<RectTransform>();
            if (r == null)
            {
                GridCell.AddComponent<RectTransform>();
            }
            GridCell.AddComponent<UIWrapGridCell>();

            GameObject temCell = new GameObject();
            temCell.transform.parent = GridCell.transform;
            temCell.name = "Cell";
            r = temCell.GetComponent<RectTransform>();
            if (r == null)
            {
                r = temCell.AddComponent<RectTransform>();
            }
            temCell.AddComponent<UIWrapCell>();
            r.anchorMax = new Vector2(0, 1.0f);
            r.anchorMin = new Vector2(0, 1.0f);
            temCell.transform.localScale = Vector3.one;
            temCell.transform.localPosition = Vector3.zero;
            temCell.transform.localRotation = Quaternion.identity;
            l.ResetEditorScrollView();

        }


        public static void CreateInfinityScrollView(string name = null, int type = 0)
        {
            if (name == null)
            {
                name = "InfinityScrollView";
            }
            GameObject ob = new GameObject();
            ob.transform.parent = Selection.activeTransform;
            ob.name = name;
            ScrollRect sc = ob.AddComponent<ScrollRect>();
            sc.vertical = false;
            sc.horizontal = true;
            RectTransform parentR = ob.GetComponent<RectTransform>();
            if (parentR == null)
            {
                ob.AddComponent<RectTransform>();
            }
            ob.transform.localScale = Vector3.one;
            ob.transform.localPosition = Vector3.zero;
            ob.transform.localRotation = Quaternion.identity;
            parentR.sizeDelta = new Vector2(600, 300);

            GameObject viewReport = new GameObject();
            viewReport.transform.parent = ob.transform;
            viewReport.name = "Viewport";
            viewReport.AddComponent<RectMask2D>();
            CanvasRenderer crd = viewReport.AddComponent<CanvasRenderer>();
            crd.cullTransparentMesh = true;
            sc.viewport = viewReport.GetComponent<RectTransform>();
            Image img = viewReport.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.0f);
            viewReport.AddComponent<EventHandler>();
            // Sprite sp = Resources.Load<Sprite>("unity_builtin_extra/UIMask");
            // img.sprite = sp;
            RectTransform r = viewReport.GetComponent<RectTransform>();
            if (r == null)
            {
                viewReport.AddComponent<RectTransform>();
            }
            r.localPosition = Vector3.zero;
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
            r.anchorMax = new Vector2(1.0f, 1.0f);
            r.anchorMin = new Vector2(0.0f, 0.0f);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentR.rect.width);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentR.rect.height);
            CreateHWrapContent(viewReport, type);

        }

        public static void CreateDynamicScrollView(string name = null, int type = 0)
        {
            if (name == null)
            {
                name = "DynamicScrollView";
            }
            GameObject ob = new GameObject();
            ob.transform.parent = Selection.activeTransform;
            ob.name = name;
            ScrollRect sc = ob.AddComponent<ScrollRect>();
            sc.vertical = false;
            sc.horizontal = true;
            RectTransform parentR = ob.GetComponent<RectTransform>();
            if (parentR == null)
            {
                ob.AddComponent<RectTransform>();
            }
            ob.transform.localScale = Vector3.one;
            ob.transform.localPosition = Vector3.zero;
            ob.transform.localRotation = Quaternion.identity;
            parentR.sizeDelta = new Vector2(600, 300);

            GameObject viewReport = new GameObject();
            viewReport.transform.parent = ob.transform;
            viewReport.name = "Viewport";
            viewReport.AddComponent<RectMask2D>();
            CanvasRenderer crd = viewReport.AddComponent<CanvasRenderer>();
            crd.cullTransparentMesh = true;
            sc.viewport = viewReport.GetComponent<RectTransform>();
            Image img = viewReport.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.0f);
            viewReport.AddComponent<EventHandler>();
            // Sprite sp = Resources.Load<Sprite>("unity_builtin_extra/UIMask");
            // img.sprite = sp;
            RectTransform r = viewReport.GetComponent<RectTransform>();
            if (r == null)
            {
                viewReport.AddComponent<RectTransform>();
            }
            r.localPosition = Vector3.zero;
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
            r.anchorMax = new Vector2(1.0f, 1.0f);
            r.anchorMin = new Vector2(0.0f, 0.0f);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentR.rect.width);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentR.rect.height);
            CreateDynamicWrapContent(viewReport);

        }

        public static void CreatePageView()
        {
            CreateInfinityScrollView("PageScrollView", 1);
        }

        public static void CreateGridScrollView()
        {
            GameObject ob = new GameObject();
            ob.transform.parent = Selection.activeTransform;
            ob.name = "InfinityGridScrollView";
            ScrollRect sc = ob.AddComponent<ScrollRect>();
            sc.vertical = false;
            sc.horizontal = true;
            RectTransform parentR = ob.GetComponent<RectTransform>();
            if (parentR == null)
            {
                ob.AddComponent<RectTransform>();
            }
            ob.transform.localScale = Vector3.one;
            ob.transform.localPosition = Vector3.zero;
            ob.transform.localRotation = Quaternion.identity;
            parentR.sizeDelta = new Vector2(600, 300);

            GameObject viewReport = new GameObject();
            viewReport.transform.parent = ob.transform;
            viewReport.name = "ViewReport";
            viewReport.AddComponent<RectMask2D>();
            CanvasRenderer crd = viewReport.AddComponent<CanvasRenderer>();
            crd.cullTransparentMesh = true;
            Image img = viewReport.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.0f);
            viewReport.AddComponent<EventHandler>();
            RectTransform r = viewReport.GetComponent<RectTransform>();
            if (r == null)
            {
                viewReport.AddComponent<RectTransform>();
            }
            r.localPosition = Vector3.zero;
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
            r.anchorMax = new Vector2(1.0f, 1.0f);
            r.anchorMin = new Vector2(0.0f, 0.0f);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentR.rect.width);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentR.rect.height);
            CreateGridWrapContent(viewReport);

        }
    }
}