using System;
using System.Collections;
using System.Collections.Generic;
// using NOAH.Debug;
using UnityEngine;

namespace NOAH.UI
{
    public class ScrollTriggerWatcher : MonoBehaviour
    {
        // public global::GamePlay.AxisAction viewMove;
        private bool dirty = false;
        private bool deleteDirty = false;
        private List<ScrollTrigger> triggers = new List<ScrollTrigger>();

        //外部强制控制，作为可选项，默认自动控制是否开启自动滑动
        public bool UpdateForce = false;


        public void OnDisable()
        {
            dirty = false;
            deleteDirty = false;
        }

        public void AddTrigger(ScrollTrigger tri)
        {
            triggers.Add(tri);
        }

        public void SetDirtyForce()
        {
            RefreshInputVisible();
        }

        public bool CheckCanScroll()
        {
            var canScroll = false;
            if (enabled && gameObject.activeInHierarchy)
            {
                canScroll = triggers.Find(t => t.CheckCanScroll(true)) != null;
            }

            return canScroll;
        }

        public void SetDirty()
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                if (!dirty && !UpdateForce)
                {
                    dirty = true;
                    StartCoroutine(DelayedRefreshInputVisible());
                }
            }
        }

        private IEnumerator DelayedRefreshInputVisible()
        {
            yield return null;
            RefreshInputVisible();
            dirty = false;
        }

        private void RefreshInputVisible()
        {
            var visible = false;
            if (enabled && gameObject.activeInHierarchy)
            {
                visible = triggers.Find(t => t.CheckCanScroll(true)) != null;
            }

            // LogTool.LogInfo("ScrollTrigger", "visible " + visible);
            // viewMove.Visualize = visible;
        }

        public void DeleteTrigger()
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                if (!deleteDirty)
                {
                    deleteDirty = true;
                    StartCoroutine(DelayedDeleteTrigger());
                }
            }
        }

        private IEnumerator DelayedDeleteTrigger()
        {
            yield return null;
            triggers.RemoveAll(t => !t.gameObject.activeInHierarchy);
            deleteDirty = false;
        }
    }
}
