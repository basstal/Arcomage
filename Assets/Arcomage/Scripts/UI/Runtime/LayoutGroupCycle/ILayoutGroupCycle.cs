using UnityEngine;

namespace NOAH.UI
{
    public delegate void OnPopulateChild(GameObject gameObject, int index);

    public interface ILayoutGroupCycle
    {
        void SetSize(uint value);

        void Populate();

        Rect? GetCellRect(uint index);
    }
}
