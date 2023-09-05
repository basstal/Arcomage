using System;
using UnityEngine;
// using NOAH.Utility;
using Sirenix.OdinInspector;

namespace NOAH.UI
{
    public enum AlignPosition
    {
        /*[EnumLabel("对齐上边，重合左边")]*/ AlignTopStickLeft = 1,
        /*[EnumLabel("对齐上边，重合右边")]*/ AlignTopStickRight = 2,
        /*[EnumLabel("对齐下边，重合左边")]*/ AlignBottomStickLeft = 3,
        /*[EnumLabel("对齐下边，重合右边")]*/ AlignBottomStickRight = 4,
        /*[EnumLabel("对齐左边，重合上边")]*/ AlignLeftStickTop = 5,
        /*[EnumLabel("对齐左边，重合下边")]*/ AlignLeftStickBottom = 6,
        /*[EnumLabel("对齐右边，重合上边")]*/ AlignRightStickTop = 7,
        /*[EnumLabel("对齐右边，重合下边")]*/ AlignRightStickBottom = 8,
        /*[EnumLabel("中心对齐")]*/ AlignCenter = 9,
    }

    [Serializable]
    public struct PositionSetting
    {
        [LabelText("类型")] public AlignPosition Type;
        public float AlignOffset;
        public float StickOffset;
    }

    public class AlignTargetInBox : MonoBehaviour
    {
        public PositionSetting[] Positions;
        public RectTransform Box;
#if UNITY_EDITOR
        public RectTransform Target;

        [Button]
        void Refresh()
        {
            SetTarget(Target);
        }
#endif

        Vector4 GetBorder(RectTransform rt)
        {
            var scale = rt.lossyScale;
            var w = rt.rect.size.x * scale.x;
            var h = rt.rect.size.y * scale.y;
            var pos = rt.position;
            var offsetX = w * (rt.pivot.x - 0.5f);
            var offsetY = h * (rt.pivot.y - 0.5f);
            var data = Vector4.zero;
            data.x = pos.x - w / 2 - offsetX; //左
            data.y = pos.x + w / 2 - offsetX; //右
            data.z = pos.y + h / 2 - offsetY; //上
            data.w = pos.y - h / 2 - offsetY; //下
            return data;
        }

        public void SetTarget(RectTransform target)
        {
            var boxScale = Box.lossyScale;
            var boxBorder = GetBorder(Box);

            var targetScale = target.lossyScale;
            var scale = Vector2.one;
            scale.x = targetScale.x / boxScale.x;
            scale.y = targetScale.y / boxScale.y;
            var targetBorder = GetBorder(target);

            var selfRt = transform as RectTransform;
            var selfScale = selfRt.lossyScale;
            scale = Vector2.one;
            scale.x = selfScale.x / boxScale.x;
            scale.y = selfScale.y / boxScale.y;
            var selfBorder = GetBorder(selfRt);
            var selfW = selfBorder.y - selfBorder.x;
            var selfH = selfBorder.z - selfBorder.w;
            foreach (var data in Positions)
            {
                var alignCenter = data.Type == AlignPosition.AlignCenter;

                var alignTop = data.Type == AlignPosition.AlignTopStickLeft ||
                               data.Type == AlignPosition.AlignTopStickRight;
                if (alignTop) //对齐上边缘   : 上下边缘都不能超出
                {
                    if (targetBorder.z + data.AlignOffset > boxBorder.z) continue;
                    if (targetBorder.z + data.AlignOffset - selfH < boxBorder.w) continue;
                }

                var alignBottom = data.Type == AlignPosition.AlignBottomStickLeft ||
                                  data.Type == AlignPosition.AlignBottomStickRight;
                if (alignBottom) //对齐下边缘
                {
                    if (targetBorder.w + data.AlignOffset < boxBorder.w) continue;
                    if (targetBorder.w + data.AlignOffset + selfH > boxBorder.z) continue;
                }

                var alignLeft = data.Type == AlignPosition.AlignLeftStickTop ||
                                data.Type == AlignPosition.AlignLeftStickBottom;
                if (alignLeft) //对齐左边缘
                {
                    if (targetBorder.x + data.AlignOffset < boxBorder.x) continue;
                    if (targetBorder.x + data.AlignOffset + selfW > boxBorder.y) continue;
                }

                var alignRight = data.Type == AlignPosition.AlignRightStickTop ||
                                 data.Type == AlignPosition.AlignRightStickBottom;
                if (alignRight) //对齐右边缘
                {
                    if (targetBorder.y + data.AlignOffset > boxBorder.y) continue;
                    if (targetBorder.y + data.AlignOffset - selfW < boxBorder.x) continue;
                }

                var stickTop = data.Type == AlignPosition.AlignLeftStickTop ||
                               data.Type == AlignPosition.AlignRightStickTop;
                if (stickTop) //贴紧上边缘
                {
                    if (targetBorder.z + data.StickOffset + selfH > boxBorder.z) continue;
                    if (targetBorder.z + data.StickOffset < boxBorder.w) continue;
                }

                var stickBottom = data.Type == AlignPosition.AlignLeftStickBottom ||
                                  data.Type == AlignPosition.AlignRightStickBottom;
                if (stickBottom) //贴紧下边缘
                {
                    if (targetBorder.w + data.StickOffset - selfH < boxBorder.w) continue;
                    if (targetBorder.w + data.StickOffset > boxBorder.z) continue;
                }

                var stickLeft = data.Type == AlignPosition.AlignTopStickLeft ||
                                data.Type == AlignPosition.AlignBottomStickLeft;
                if (stickLeft) //贴紧左边缘
                {
                    if (targetBorder.x + data.StickOffset - selfW < boxBorder.x) continue;
                    if (targetBorder.x + data.StickOffset > boxBorder.y) continue;
                }

                var stickRight = data.Type == AlignPosition.AlignTopStickRight ||
                                 data.Type == AlignPosition.AlignBottomStickRight;
                if (stickRight) //贴紧右边缘
                {
                    if (targetBorder.y + data.StickOffset + selfW > boxBorder.y) continue;
                    if (targetBorder.y + data.StickOffset < boxBorder.x) continue;
                }

                var pos = Vector2.zero;
                if (alignTop)
                {
                    pos.y = targetBorder.z - selfH / 2 + data.AlignOffset;
                }

                if (alignBottom)
                {
                    pos.y = targetBorder.w + selfH / 2 + data.AlignOffset;
                }

                if (alignLeft)
                {
                    pos.x = targetBorder.x + selfW / 2 + data.AlignOffset;
                }

                if (alignRight)
                {
                    pos.x = targetBorder.y - selfW / 2 + data.AlignOffset;
                }

                if (stickTop)
                {
                    pos.y = targetBorder.z + selfH / 2 + data.StickOffset;
                }

                if (stickBottom)
                {
                    pos.y = targetBorder.w - selfH / 2 + data.StickOffset;
                }

                if (stickLeft)
                {
                    pos.x = targetBorder.x - selfW / 2 + data.StickOffset;
                }

                if (stickRight)
                {
                    pos.x = targetBorder.y + selfW / 2 + data.StickOffset;
                }

                if (alignCenter)
                {
                    pos.x = targetBorder.x + (targetBorder.y - targetBorder.x) / 2 + data.StickOffset;
                    pos.y = targetBorder.w + (targetBorder.z - targetBorder.w) / 2 + data.StickOffset;
                }

                var realPos = GetRealPos(pos, selfBorder);
                transform.position = new Vector3(realPos.x, realPos.y, transform.position.z);
                return;
            }

            // NOAH.Debug.LogTool.LogWarning("UI", "No Suitable Position");
        }

        Vector2 GetRealPos(Vector2 centerPos, Vector4 selfBorder)
        {
            var pos = centerPos;
            var rt = transform as RectTransform;
            var w = selfBorder.y - selfBorder.x;
            var h = selfBorder.z - selfBorder.w;
            var offsetX = w * (rt.pivot.x - 0.5f);
            var offsetY = h * (rt.pivot.y - 0.5f);

            pos.x += offsetX;
            pos.y += offsetY;
            return pos;
        }
    }
}
