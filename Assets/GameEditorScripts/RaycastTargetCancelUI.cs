// using UnityEngine;
// using UnityEditor;
// using UnityEngine.UI;
// public class RaycastTargetCancelUI {
//     [MenuItem("GameObject/UI/Image")]
//     static void CreatImage()
//     {
//         if (Selection.activeTransform)
//         {
//             if (Selection.activeTransform.GetComponentInParent<Canvas>())
//             {
//                 GameObject go = new GameObject("Image", typeof(Image));
//                 go.GetComponent<Image>().raycastTarget = false;
//                 go.transform.SetParent(Selection.activeTransform);
//             }
//         }
//     }
//     [MenuItem("GameObject/UI/Text")]
//     static void CreatText()
//     {
//         if (Selection.activeTransform)
//         {
//             if (Selection.activeTransform.GetComponentInParent<Canvas>())
//             {
//                 GameObject go = new GameObject("Text", typeof(Text));  
//                 go.GetComponent<Text>().raycastTarget = false;  
//                 go.transform.SetParent(Selection.activeTransform);
//             }
//         }
//     }
//     [MenuItem("GameObject/UI/Raw Image")]
//     static void CreatRawImage()
//     {
//         if (Selection.activeTransform)
//         {
//             if (Selection.activeTransform.GetComponentInParent<Canvas>())
//             { 
//                 GameObject go = new GameObject("RawImage", typeof(RawImage)); 
//                 go.GetComponent<RawImage>().raycastTarget = false;
//                 go.transform.SetParent(Selection.activeTransform);
//             }
//         }
//     }
// }