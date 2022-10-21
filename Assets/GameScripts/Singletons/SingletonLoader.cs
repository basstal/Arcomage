// using System.Linq;
// using System.Reflection;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.SceneManagement;
//
// public class SingletonLoader : MonoBehaviour
// {
//     public SingletonBase[] singletons;
//     public AssetReference layout;
//     private void Awake()
//     {
//         InitSingletons();
//         LoadGameMain();
//     }
//     private async void LoadGameMain()
//     {
//         var asset = await layout.LoadAssetAsync<GameObject>().Task;
//         Instantiate(asset, new Vector3(0, 0, 0), Quaternion.identity);
//     }
//
//     private async void InitSingletons()
//     {
//         foreach (var singleton in singletons.Where(v => v != null))
//         {
//             await singleton.Init();
//         }
//     }
//     private async void UninitSingletons()
//     {
//         foreach (var singleton in singletons.Where(v => v != null))
//         {
//             await singleton.Uninit();
//         }
//     }
//
//     private void OnDestroy()
//     {
//         UninitSingletons();
//     }
// }