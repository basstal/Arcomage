using System.Collections.Generic;
using UnityEngine;
using Whiterice;

namespace GameScripts
{
    /// <summary>
    /// 缓存特效实例
    /// </summary>
    public class EffectCache : MonoBehaviour
    {
        /// <summary>
        /// 特效名称对应到缓存特效的根节点
        /// </summary>
        private Dictionary<string, Transform> m_cachedEffects;

        public Dictionary<string, Transform> cachedEffects => m_cachedEffects ??= new Dictionary<string, Transform>();


        public EffectInstance CreateEffect(string inName)
        {
            if (cachedEffects.TryGetValue(inName, out Transform effectRoot) && effectRoot.childCount > 0)
            {
                for (int i = 0; i < effectRoot.childCount; ++i)
                {
                    Transform child = effectRoot.GetChild(i);
                    var effectInstance = child.GetComponent<EffectInstance>();
                    if (!effectInstance.gameObject.activeSelf)
                    {
                        effectInstance.gameObject.SetActive(true);
                        return effectInstance;
                    }
                }
            }

            if (effectRoot == null)
            {
                effectRoot = GetOrCreateEffectRoot(inName);
            }


            GameObject targetEffectObject = AssetManager.Instance.InstantiatePrefab(inName, effectRoot, parent: effectRoot);
            var result = targetEffectObject.GetComponent<EffectInstance>();
            result.name = inName;
            result.cacheInstance = this;
            return result;
        }

        public Transform GetOrCreateEffectRoot(string inName)
        {
            if (!cachedEffects.TryGetValue(inName, out Transform effectRoot))
            {
                var effectRootObject = new GameObject(inName);
                effectRoot = effectRootObject.transform;
                effectRoot.SetParent(transform);
                cachedEffects.Add(inName, effectRoot);
            }

            return effectRoot;
        }

        public void Recycle(EffectInstance instance)
        {
            var effectRoot = GetOrCreateEffectRoot(instance.name);
            var instanceTransform = instance.transform;
            instanceTransform.SetParent(effectRoot, false);
            instanceTransform.gameObject.SetActive(false);
        }
    }
}