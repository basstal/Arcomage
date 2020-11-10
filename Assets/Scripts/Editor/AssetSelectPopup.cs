// using System;
// using UnityEditor;
// using UnityEditor.IMGUI.Controls;
// using UnityEngine;
// using System.IO;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.ResourceManagement.ResourceLocations;
//
// public class AssetSelectTreeViewItem : TreeViewItem
// {
//     public string path;
//     public AssetSelectTreeViewItem(string _path, int depth, string displayName)
//         : base(_path.GetHashCode(), depth, displayName)
//     {
//         path = _path;
//         icon = EditorGUIUtility.ObjectContent(null, typeof(UnityEngine.TextAsset)).image as Texture2D;
//     }
// }
//
// public class AssetSelectTreeView : TreeView
// {
//     public string searchText;
//
//     private Action<string> OnAssetSelected;
//     private Func<AsyncOperationHandle<IList<IResourceLocation>>> GetAssetCandidates;
//
//     public AssetSelectTreeView(TreeViewState state, Func<AsyncOperationHandle<IList<IResourceLocation>>> _GetAssetCandidates, Action<string> _OnAssetSelected)
//         : base(state)
//     {
//         GetAssetCandidates = _GetAssetCandidates;
//         OnAssetSelected = _OnAssetSelected;
//         Reload();
//     }
//
//     protected override TreeViewItem BuildRoot()
//     {
//         var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
//         BuildList(root);
//         return root;
//     }
//
//     protected override bool CanMultiSelect(TreeViewItem item)
//     {
//         return false;
//     }
//
//     public void Select(int id)
//     {
//         var item = FindItem(id, rootItem) as AssetSelectTreeViewItem;
//         OnAssetSelected?.Invoke(item != null ? item.path : null);
//     }
//
//     protected override void DoubleClickedItem(int id)
//     {
//         base.DoubleClickedItem(id);
//         Select(id);
//     }
//
//     private async void BuildList(TreeViewItem root)
//     {
//         var segs = searchText != null ? searchText.Split(' ').Select(s => s.Trim()).Where(s => s.Length > 0) : null;
//
//         
//         var result = await GetAssetCandidates().Task;
//         foreach (var location in result)
//         {
//             Debug.Log($"location : {location} , primaryKey : {location.PrimaryKey}");
//             // var name = Path.GetFileNameWithoutExtension(path);
//             // bool matches = segs == null || !segs.Any(s => name.IndexOf(s, StringComparison.OrdinalIgnoreCase) < 0);
//             // if (matches)
//             // {
//             //     root.AddChild(new AssetSelectTreeViewItem(path, root.depth + 1, name));
//             // }
//         }
//
//         if (!root.hasChildren)
//         {
//             root.AddChild(new TreeViewItem(0, root.depth + 1, "none"));
//         }
//     }
// }
// public class AssetSelectPopup : PopupWindowContent
// {
//     private SearchField searchField;
//
//     [SerializeField]
//     private AssetSelectTreeView treeView;
//
//     private Func<AsyncOperationHandle<IList<IResourceLocation>>> GetAssetCandidates;
//     private Action<string> OnSelect;
//     private Rect rect;
//
//     public static void Open(Rect rect, Func<AsyncOperationHandle<IList<IResourceLocation>>> GetAssetCandidates, Action<string> OnSelect)
//     {
//         var popup = new AssetSelectPopup();
//         popup.GetAssetCandidates = GetAssetCandidates;
//         popup.OnSelect = OnSelect;
//         popup.rect = rect;
//         popup.Focus();
//         PopupWindow.Show(rect, popup);
//     }
//
//     public override Vector2 GetWindowSize()
//     {
//         return new Vector2(rect.width, 300);
//     }
//
//     public void Focus()
//     {
//         if (searchField != null)
//         {
//             searchField.SetFocus();
//         }
//     }
//
//     public override void OnGUI(Rect rect)
//     {
//         if (treeView == null)
//         {
//             treeView = new AssetSelectTreeView(new TreeViewState(), GetAssetCandidates, (path) =>
//             {
//                 OnSelect?.Invoke(path);
//                 editorWindow.Close();
//             });
//         }
//         if (searchField == null)
//         {
//             searchField = new SearchField();
//             searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
//             searchField.SetFocus();
//         }
//
//         EditorGUI.BeginChangeCheck();
//         treeView.searchText = searchField.OnToolbarGUI(treeView.searchText);
//         if (EditorGUI.EndChangeCheck())
//         {
//             treeView.Reload();
//         }
//         rect.y += EditorGUIUtility.singleLineHeight;
//         rect.height -= EditorGUIUtility.singleLineHeight;
//         treeView.OnGUI(rect);
//
//         if (Event.current.keyCode == KeyCode.Return)
//         {
//             var selection = treeView.GetSelection();
//             if (selection.Count > 0)
//             {
//                 treeView.Select(selection[0]);
//             }
//         }
//     }
// }