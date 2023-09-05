using System;
using System.Collections;
using System.Collections.Generic;
// using NOAH.Debug;
using UnityEngine;

namespace NOAH.UI
{
    //用于TS那边调用
    public class ScrollWatchHandler : MonoBehaviour
    {
        private ScrollTriggerWatcher watcher;

        private void Awake()
        {
            watcher = GetComponent<ScrollTriggerWatcher>();
        }

        public void SetDirty()
        {
            watcher.SetDirtyForce();
        }

        public bool CheckCanScroll()
        {
            return watcher.CheckCanScroll();
        }
    }
}
