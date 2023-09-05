// using NOAH.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NOAH.UI
{
    public class UIWrapMagnetic : UIWrapLayoutBase
    {
        // Start is called before the first frame update
        [ReadOnly]
        public Direction _axis = Direction.HORIZONTAL;
    

        bool m_bDragging = false;
   
   

        public float m_BounceSpeed = 500.0f;
        Vector2 m_boundsOffset = Vector2.zero;

        // [SerializeField,SetProperty("MagneticCenter")]
        bool m_MagneticCenter = false;
        bool m_bInitMagnetic = false;
        public bool MagneticCenter{
            get{
                return m_MagneticCenter;
            }
            set{
                m_MagneticCenter = value;
                m_MagneticLeft   = false;
                UpdateBoundsOffset();
            }
        }
        // [SerializeField,SetProperty("ChildLen")]
        protected int _childLen = 0;
        override public int ChildLen
        {
            get{return base.ChildLen;}
            set{
                SetChildLen(value,true);
            }
        }
        override public void SetChildLen(int len,bool bForceUpdate = false)
        {
            _childLen = len;
            base.SetChildLen(len,bForceUpdate);
        }

        // [SerializeField,SetProperty("MagneticLeft")]
        bool m_MagneticLeft = false;
        public bool MagneticLeft{
            get{
                return m_MagneticLeft;
            }
            set{
                m_MagneticLeft = value;
                m_MagneticCenter = false;
                UpdateBoundsOffset();
            }
        }
        void UpdateBoundsOffset()
        {
            if(m_MagneticCenter)
            {
                if(Axis == Direction.HORIZONTAL)
                {
                    m_boundsOffset.x = ScrollSize.x / 2;
                    m_boundsOffset.y = 0;
                }
                else
                {
                    m_boundsOffset.x = 0;
                    m_boundsOffset.y = ScrollSize.y / 2;
                }
            }
            else if(m_MagneticLeft)
            {
                m_boundsOffset.x = 0;
                m_boundsOffset.y = 0;
            }
        }
  


        override protected void InitEvent()
        {
            base.InitEvent();
            scrollCtrl.OnDraging.AddListener(OnDrag);
        }


        private Vector2 CalculateOffsetByCellIndex(int index,bool isLeftBounds)
        {
            if (index >= m_cellPosition.Count)
            {
                return Vector2.zero;
            }
            float offset = m_cellPosition[index];
            if (Axis == Direction.HORIZONTAL)
            {
                Vector2 result;
                if (isLeftBounds)
                {
                    result = new Vector2(m_boundsOffset.x - (offset + rectTransform.anchoredPosition.x)  ,0);
                }
                else
                {
                    result = new Vector2(m_boundsOffset.x - (offset + m_cellSize[index].x + rectTransform.anchoredPosition.x),0);
                }
                return result;
            }
            else
            {
                return Vector2.zero;
            }
        }

//        public override void SetLayoutVertical()
//        {
//            if (m_axis == Direction.VERTICAL)
//            {
//                base.SetLayoutVertical();
//            }
//        }
//   
//        public override void SetLayoutHorizontal()
//        {
//            if (m_axis == Direction.HORIZONTAL)
//            {
//                base.SetLayoutHorizontal();
//            }
//        }

        int GetCurrClosetIndex()
        {
            RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            Vector2 target = rectTransform.anchoredPosition;
            // if (m_axis == Direction.HORIZONTAL)
            // {
            //     target.x -= m_boundsOffset.x;
            // }
            int targetIndex = GetClosestIndex(target);
            return targetIndex;
        }

        protected override void UpdateChildLayout()
        {
            base.UpdateChildLayout();
            if (!Application.IsPlaying(this))
            {
                InitMagneticCenterScroll();
            }
            else
            {
                if (!m_bInitMagnetic)
                {
                    m_bInitMagnetic = true;
                    InitMagneticCenterScroll();
                }
            }
        }
        void UpdateMagneticCenterScroll()
        {
            if (m_cellPosition.Count > 0)
            {
                int targetIndex = GetCurrClosetIndex();
                float deltaTime = Time.unscaledDeltaTime;
                float currVec = 0.0f;
                if (Axis == Direction.HORIZONTAL)
                {
                    currVec = scrollRect.velocity.x;
                }
                else
                {
                    currVec = scrollRect.velocity.y;
                }
                if (targetIndex <= 0 || targetIndex == GetCellPositionCount() || Mathf.Abs(currVec) <= 0.1f)
                {
                    Vector2 offset = CalculateOffsetByCellIndex(targetIndex,true);
                    if (offset != Vector2.zero)
                    {
                        Vector2 position = rectTransform.anchoredPosition;
                        for (int axis = 0; axis < 2; axis++)
                        {
                            if (offset[axis] != 0)
                            {
                                float speed = scrollRect.velocity[axis];
                                Vector2 vec = scrollRect.velocity;
                                position[axis] = Mathf.SmoothDamp(position[axis], position[axis] + offset[axis], ref speed, 0.2f);
                                if (Mathf.Abs(speed) < 1)
                                    speed = 0;
                                vec[axis] = speed;
                                scrollRect.velocity = vec;
                            }
                            else
                            {
                                Vector2 tempVec = new Vector2(scrollRect.velocity.x,scrollRect.velocity.y);
                                tempVec[axis] = 0;
                                scrollRect.velocity = tempVec;
                            }
                        }
                        SetContentAnchoredPosition(position);
                    }
                }
            }
        }

        void InitMagneticCenterScroll()
        {
            if (m_cellPosition.Count > 0)
            {
                int targetIndex = 0;
                Vector2 offset = CalculateOffsetByCellIndex(targetIndex,true);
                if (offset != Vector2.zero)
                {
                    Vector2 position = rectTransform.anchoredPosition;
                    position += offset;
                    SetContentAnchoredPosition(position);
                }
            }
        }

        float CalculateCellCenterPosition(int index)
        {
            Vector2 cellSize;
            float result = 0.0f;
            if (m_cellSize.TryGetValue(index,out cellSize) && index < m_cellPosition.Count)
            {
                result = Mathf.Abs(m_cellPosition[index]) ;
            }
            return result;
        }

        int FindClosestIndex(int left,int right,Vector2 target)
        {
            if (left >= right)
            {
                return left;
            }
            if (left >= m_cellPosition.Count)
            {
                return -1;
            }
            if (right >= m_cellPosition.Count)
            {
                return -1;
            }
            float centerPosition = m_axis == Direction.HORIZONTAL?target.x:target.y;

            int middle     = (left + right) / 2;
            float leftPt   = CalculateCellCenterPosition(left) + target.x;
            float rightPt  = CalculateCellCenterPosition(right) + target.x;
            float middlePt = CalculateCellCenterPosition(middle) + target.x;

            float leftDistance   = Mathf.Abs(m_boundsOffset.x - leftPt);
            float rightDistance  = Mathf.Abs(m_boundsOffset.x - rightPt);
            float middleDistance = Mathf.Abs(middlePt - m_boundsOffset.x);
            if (right - left == 1)
            {
                return leftDistance < rightDistance?left:right;
            }
            if (leftDistance <= rightDistance)
            {
                return FindClosestIndex(left,middle,target);
            }
            else
            {
                return FindClosestIndex(middle,right,target);
            }
        }

        int GetClosestIndex(Vector2 target)
        {
            int left  = 0;
            int right = GetCellPositionCount();
            if (left >= right)
            {
                return left;
            }
            return FindClosestIndex(left,right,target);
        }

        protected void LateUpdate() {
        
            if (!gameObject.activeSelf || m_bDragging || scrollRect.velocity == Vector2.zero)
            {
                return;
            }
            int targetIndex = GetCurrClosetIndex();
            if (Mathf.Abs(scrollRect.velocity.x) > m_BounceSpeed && targetIndex != 0 && targetIndex != GetCellPositionCount())
            {
                return;
            }
            UnityEngine.Debug.Log(string.Format("targetIndex:{0}",targetIndex));
            bool bLeftBounds = true;
            Vector2 offset = CalculateOffsetByCellIndex(targetIndex,bLeftBounds);
            float deltaTime = Time.unscaledDeltaTime;
        
            Vector2 position = rectTransform.anchoredPosition;
            if(  offset != Vector2.zero)
            {
                for (int axis = 0; axis < 2; axis++)
                {
                    if (offset[axis] != 0)
                    {
                        float speed = scrollRect.velocity[axis];
                        Vector2 vec = scrollRect.velocity;
                        position[axis] = Mathf.SmoothDamp(position[axis], position[axis] + offset[axis], ref speed, 0.2f);
                        if (Mathf.Abs(speed) < 1)
                            speed = 0;
                        vec[axis] = speed;
                        scrollRect.velocity = vec;
                    }
                }
            }
            SetContentAnchoredPosition(position);
        }
        override protected void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            m_bDragging = false;
        }
        protected override void Start() {
            m_childLen = _childLen;
            base.Start();
            UpdateBoundsOffset();
        }


        [ContextMenu("Reset ScrollView")]
        override public void ResetEditorScrollView()
        {
            base.ResetEditorScrollView();
        }
        override protected void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;
            m_bDragging = true;

        }
        public void OnDrag(PointerEventData eventData){
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!gameObject.activeSelf)
                return;

            Vector2 position = rectTransform.anchoredPosition; 
            int targetIndex = 0;
            bool needFix = false;
            Vector2 offset = CalculateOffsetByCellIndex(targetIndex,true);
            if (offset.x < 0)
            {
                position += offset;
                needFix = true;
            }
            else
            {
                targetIndex = GetCellPositionCount();
                if (targetIndex > 0)
                {
                    offset = CalculateOffsetByCellIndex(targetIndex,false);
                }
                if (offset.x > 0)
                {
                    position += offset;
                    needFix = true;
                }
            }
            // Debug.Log(string.Format("offset:{0} targetIndex:{1} positon:{2}",offset.x,targetIndex,position.x));
            if (needFix)
                position.x = position.x - RubberDelta(offset.x, ScrollSize.x);
            //Debug.Log(string.Format("target position:{0} ",position.x));
            SetContentAnchoredPosition(position);

        }

    

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!scrollRect.horizontal)
                position.x = rectTransform.anchoredPosition.x;
            if (!scrollRect.vertical)
                position.y = rectTransform.anchoredPosition.y;

            if (position != rectTransform.anchoredPosition)
            {
                rectTransform.anchoredPosition = position;
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }
   
    

    }
}
