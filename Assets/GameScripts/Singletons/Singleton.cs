// using System.Threading.Tasks;
// using UnityEngine;
//
// public class SingletonBase : MonoBehaviour
// {
// #pragma warning disable 1998
//     public virtual async Task Init() { }
//     public virtual async Task Uninit() { }
// #pragma warning restore 1998
// }
//
// public abstract class Singleton<T> : SingletonBase where T : MonoBehaviour
// {
//     public static T Instance;
// #pragma warning disable 1998
//     public override async Task Init()
//     {
//         Instance = this as T;
//     }
//     public override async Task Uninit()
//     {
//         Instance = null;
//     }
// #pragma warning restore 1998
// }