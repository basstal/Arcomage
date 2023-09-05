#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using NOAH.UI;

namespace NOAH.EditorExtends
{
    [CustomEditor(typeof(PolygonRawImage))]
    public class PolygonRawImageInspector : GraphicEditor
    {
        public bool m_InEditMode;
        public PolygonRawImage polygonRawImage;
        SerializedProperty m_Texture;
        SerializedProperty m_vertices;
        SerializedProperty m_realWidth;
        SerializedProperty m_realHeight;
        SerializedProperty m_keepNativeSize;

        Vector2 m_centerPos = Vector2.zero;

        #region square
        float m_width = 100f;
        float m_height = 100f;
        #endregion

        #region polygon
        int m_edgeCount = 3;
        int m_startAngle = 0;
        float m_radius = 100f;
        float m_fillAmount = 1f;
        bool m_hollow = false;
        float m_innerRadius = 50f;
        #endregion

        class HandleInfo
        {
            public Tool defaultTool;
            public int currentPoint = -1;
            public bool isPointSelected = false;
            public int currentLine = -1;
            public bool isLineSelected = false;
            public bool isOnWholeHandle = false;
            public bool isWholeHandleSelected = false;
            public bool needRepaint = false;
            public List<Vector3> editPoints = new List<Vector3>();
            public int pointRadius = 5;
            public Vector2 moveWholeHandle;
        }

        HandleInfo handleInfo;

        Vector3 m_lastPos;

        string[] m_assistTypes = { "Manually", "Square", "Polygon" };
        int m_assistType = 0;

        string m_tips = @"
        Base            : mouse left button
        Move            : select item and drag
        Move all        : select rightest red dot and drag                
        Add a point     : click an empty place
        Insert a point  : click + 'Shift' when select a line
        Delete a point  : click + 'Ctrl' when select a point        
        ";


        override protected void OnEnable()
        {
            base.OnEnable();
            UpdateSerializedObject();
            m_Texture = serializedObject.FindProperty("m_Texture");
            m_vertices = serializedObject.FindProperty("m_vertices");
            m_realWidth = serializedObject.FindProperty("m_realWidth");
            m_realHeight = serializedObject.FindProperty("m_realHeight");
            m_keepNativeSize = serializedObject.FindProperty("m_keepNativeSize");

            handleInfo = new HandleInfo();
            UnityEditor.Tools.hidden = false;
            m_assistType = 0;
        }

        override protected void OnDisable()
        {
            if (m_InEditMode)
            {
                polygonRawImage.LeaveEditMode();
                UnityEditor.Tools.hidden = false;
            }

        }

        public override void OnInspectorGUI()
        {
            UpdateSerializedObject();
            EditorGUILayout.PropertyField(m_Texture);
            AppearanceControlsGUI();
            RaycastControlsGUI();
            if (polygonRawImage != null)
            {
                EditorGUILayout.BeginHorizontal();
                m_keepNativeSize.boolValue = EditorGUILayout.Toggle("Native Size", m_keepNativeSize.boolValue);
                string s = string.Format("[ {0} , {1} ]", m_realWidth.intValue, m_realHeight.intValue);
                EditorGUILayout.LabelField(s);
                EditorGUILayout.EndHorizontal();
                if (m_keepNativeSize.boolValue)
                {
                    polygonRawImage.SetRealSize();
                }
                var oriColor = GUI.backgroundColor;
                if (m_InEditMode)
                    GUI.backgroundColor = Color.green;
                if (GUILayout.Button(new GUIContent("Edit Mode", m_tips)))
                {
                    m_InEditMode = !m_InEditMode;
                    if (m_InEditMode)
                    {
                        UnityEditor.Tools.hidden = true;
                        polygonRawImage.EnterEditMode();
                        ReplaceEditorPointList();
                    }
                    else
                    {
                        polygonRawImage.LeaveEditMode();
                        UnityEditor.Tools.hidden = false;
                    }
                }

                GUI.backgroundColor = oriColor;
                if (m_InEditMode)
                {
                    m_assistType = GUILayout.SelectionGrid(m_assistType, m_assistTypes, 3);
                    if (m_assistType != 0)
                    {
                        if (m_assistType == 1)
                        {
                            GUISquare();
                        }
                        else if (m_assistType == 2)
                        {
                            GUIPolygon();
                        }
                    }
                    EditorGUILayout.PropertyField(m_vertices, new GUIContent("list"), true);

                    serializedObject.ApplyModifiedProperties();
                    polygonRawImage.UpdateShape();
                    if (!HasSelectedTarget())
                        ReplaceEditorPointList();
                }
            }
            serializedObject.ApplyModifiedProperties();
            polygonRawImage.UpdateShape();
        }

        private static void OriginalOnInspectorGUI() { }

        void GUISquare()
        {
            m_width = EditorGUILayout.FloatField("Width", m_width);
            m_height = EditorGUILayout.FloatField("Height", m_height);
            m_centerPos = EditorGUILayout.Vector2Field("Center Pos", m_centerPos);

            int count = m_vertices.arraySize;
            for (int i = count - 1; i >= 0; i--)
            {
                m_vertices.DeleteArrayElementAtIndex(i);
            }
            m_vertices.arraySize = 4;
            var halfW = 0.5f * m_width;
            var halfH = 0.5f * m_height;
            var x = m_centerPos.x;
            var y = m_centerPos.y;
            Vector2 v = new Vector2(x - halfW, y - halfH);
            m_vertices.GetArrayElementAtIndex(0).vector2Value = v;
            v = new Vector2(x - halfW, y + halfH);
            m_vertices.GetArrayElementAtIndex(1).vector2Value = v;
            v = new Vector2(x + halfW, y + halfH);
            m_vertices.GetArrayElementAtIndex(2).vector2Value = v;
            v = new Vector2(x + halfW, y - halfH);
            m_vertices.GetArrayElementAtIndex(3).vector2Value = v;
            // serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Done"))
            {
                m_assistType = 0;
            }
        }

        void GUIPolygon()
        {
            m_centerPos = EditorGUILayout.Vector2Field("Center Pos", m_centerPos);
            m_edgeCount = EditorGUILayout.IntField("Edge Count", m_edgeCount);
            m_radius = EditorGUILayout.FloatField("Radius", m_radius);
            m_startAngle = EditorGUILayout.IntField("Start Angle", m_startAngle);
            m_fillAmount = EditorGUILayout.Slider("Fill Amount", m_fillAmount, 0, 1);

            m_innerRadius = EditorGUILayout.FloatField("Inner radius", m_innerRadius);
            m_hollow = EditorGUILayout.Toggle("Is hollow", m_hollow);


            int count = m_vertices.arraySize;
            for (int i = count - 1; i >= 0; i--)
            {
                m_vertices.DeleteArrayElementAtIndex(i);
            }

            float degree = 360f / m_edgeCount; //Step Angle
            int sliceCount = m_fillAmount == 1 ? m_edgeCount : Mathf.FloorToInt(m_edgeCount * m_fillAmount);
            int pointCount;
            
            if(m_hollow)
            {
                pointCount = sliceCount * 2 + 2;
            }
            else
            {
                pointCount = m_fillAmount == 1 ? sliceCount : sliceCount + 2;
            }

            count = 0;
            m_vertices.arraySize = pointCount;
            if(m_hollow)
            {
                int outBoundPointCount = sliceCount + 1;
                for (int i = 0; i < outBoundPointCount; i++)
                {
                    float startAngle = m_startAngle + degree * i;
                    float startRad = Mathf.Deg2Rad * startAngle;
                    float deltaX = Mathf.Cos(startRad);
                    float deltaY = Mathf.Sin(startRad);

                    var posOut = m_centerPos + new Vector2(deltaX * m_radius, deltaY * m_radius);
                    var posIn = m_centerPos + new Vector2(deltaX * m_innerRadius, deltaY * m_innerRadius);

                    m_vertices.GetArrayElementAtIndex(2 * outBoundPointCount - i - 1).vector2Value = posOut;
                    m_vertices.GetArrayElementAtIndex(i).vector2Value = posIn;
                }
            }
            else
            {
                
                if (m_fillAmount != 1)
                {
                    count++;
                    m_vertices.GetArrayElementAtIndex(count - 1).vector2Value = m_centerPos;
                }

                int outBoundPointCount = m_fillAmount == 1 ? m_edgeCount : sliceCount + 1;

                for (int i = 0; i < outBoundPointCount; i++)
                {
                    float startAngle = m_startAngle + degree * i;
                    float startRad = Mathf.Deg2Rad * startAngle;
                    float deltaX = Mathf.Cos(startRad);
                    float deltaY = Mathf.Sin(startRad);
                    var pos = m_centerPos + new Vector2(deltaX * m_radius, deltaY * m_radius);
                    m_vertices.GetArrayElementAtIndex(count).vector2Value = pos;
                    count++;
                }
            }

            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Done"))
            {
                m_assistType = 0;
            }
        }

        void OnSceneGUI()
        {
            if (!m_InEditMode) return;

            Event guiEvent = Event.current;
            if (guiEvent.type == EventType.Repaint)
                Draw();
            else if (guiEvent.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                HandleInput(guiEvent);
                if (handleInfo.needRepaint)
                {
                    HandleUtility.Repaint();
                }
            }
        }

        void HandleInput(Event guiEvent)
        {
            if (UnityEditor.Tools.current != UnityEditor.Tool.Move)
                return;

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            float drawPlaneHeight = 0;
            float dstToPlane = (drawPlaneHeight - mouseRay.origin.z) / mouseRay.direction.z;
            Vector3 mousePos = mouseRay.GetPoint(dstToPlane);

            if (!HasSelectedTarget())
            {
                UpdateMouseOver(mousePos);
            }
            if (guiEvent.button == 0)
            {
                UpdateSerializedObject();
                if (guiEvent.modifiers == EventModifiers.None)
                {

                    if (guiEvent.type == EventType.MouseDown)
                        HandleLeftMouseDown(mousePos);
                    if (guiEvent.type == EventType.MouseUp)
                        HandleLeftMouseUp(mousePos);
                    if (guiEvent.type == EventType.MouseDrag)
                        HandleLeftMouseDrag(mousePos);

                }
                else if (guiEvent.modifiers == EventModifiers.Control)
                {
                    if (guiEvent.type == EventType.MouseUp)
                    {
                        HandleLeftMouseUpWithCtrl(mousePos);
                    }
                }
                else if (guiEvent.modifiers == EventModifiers.Shift)
                {
                    if (guiEvent.type == EventType.MouseDown)
                    {
                        HandleLeftMouseDownWithShift(mousePos);
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        #region Event of mouse left button pure
        void HandleLeftMouseDown(Vector3 pos)
        {
            bool needRepaint = true;
            if (handleInfo.currentLine != -1)
            {
                handleInfo.isLineSelected = true;
                m_lastPos = pos;
            }
            else if (handleInfo.currentPoint != -1)
            {
                handleInfo.isPointSelected = true;
            }
            else if (handleInfo.isOnWholeHandle)
            {
                handleInfo.isWholeHandleSelected = true;
                m_lastPos = pos;
            }
            else if (handleInfo.currentPoint == -1 && m_assistType == 0) // Add new point
            {
                handleInfo.isPointSelected = true;
                handleInfo.editPoints.Add(pos);
                m_vertices.arraySize++;
                UpdateTargetPoint(m_vertices.arraySize - 1, pos);
                handleInfo.currentPoint = handleInfo.editPoints.Count - 1;
            }
            else
                needRepaint = false;
            handleInfo.needRepaint = handleInfo.needRepaint || needRepaint;
        }

        void HandleLeftMouseDrag(Vector3 pos)
        {
            bool needRepaint = true;
            if (handleInfo.isPointSelected)
            {
                handleInfo.editPoints[handleInfo.currentPoint] = pos;
            }
            else if (handleInfo.isLineSelected)
            {
                var span = pos - m_lastPos;
                m_lastPos = pos;
                handleInfo.editPoints[handleInfo.currentLine] += span;
                int nextIndex = (handleInfo.currentLine + 1) % handleInfo.editPoints.Count;
                handleInfo.editPoints[nextIndex] += span;
            }
            else if (handleInfo.isWholeHandleSelected)
            {
                var span = pos - m_lastPos;
                m_lastPos = pos;
                for (int i = 0; i < handleInfo.editPoints.Count; i++)
                {
                    handleInfo.editPoints[i] += span;
                }
            }
            else
            {
                needRepaint = false;
            }
            handleInfo.needRepaint = handleInfo.needRepaint || needRepaint;
        }

        void HandleLeftMouseUp(Vector3 pos)
        {
            bool needRepaint = true;
            if (handleInfo.isPointSelected)
            {
                handleInfo.editPoints[handleInfo.currentPoint] = pos;
                UpdateTargetPoint(handleInfo.currentPoint, pos);
                handleInfo.currentPoint = -1;
                handleInfo.isPointSelected = false;
            }
            else if (handleInfo.isLineSelected)
            {
                var span = pos - m_lastPos;
                m_lastPos = pos;
                handleInfo.editPoints[handleInfo.currentLine] += span;
                UpdateTargetPoint(handleInfo.currentLine);

                int nextIndex = (handleInfo.currentLine + 1) % handleInfo.editPoints.Count;
                handleInfo.editPoints[nextIndex] += span;
                UpdateTargetPoint(nextIndex);

                handleInfo.currentLine = -1;
                handleInfo.isLineSelected = false;
            }
            else if (handleInfo.isWholeHandleSelected)
            {
                var span = pos - m_lastPos;
                m_lastPos = pos;
                if(m_assistType != 0 && handleInfo.editPoints.Count > 0)
                {
                    var latestPos = handleInfo.editPoints[0] + span;
                    Vector2 recordPos = polygonRawImage.transform.InverseTransformPoint(latestPos);
                    var localSpan = recordPos - m_vertices.GetArrayElementAtIndex(0).vector2Value;
                    m_centerPos += localSpan;
                }

                for (int i = 0; i < handleInfo.editPoints.Count; i++)
                {
                    handleInfo.editPoints[i] += span;
                    UpdateTargetPoint(i);
                }

                handleInfo.isOnWholeHandle = false;
                handleInfo.isWholeHandleSelected = false;
            }
            else
            {
                needRepaint = false;
            }
            handleInfo.needRepaint = handleInfo.needRepaint || needRepaint;
        }
        #endregion

        void HandleLeftMouseUpWithCtrl(Vector3 pos)
        {
            if (m_assistType != 0) return;
            if (handleInfo.currentPoint != -1)
            {
                handleInfo.editPoints.RemoveAt(handleInfo.currentPoint);
                m_vertices.DeleteArrayElementAtIndex(handleInfo.currentPoint);
                handleInfo.currentPoint = -1;
                handleInfo.needRepaint = true;
            }
        }

        void HandleLeftMouseDownWithShift(Vector3 pos)
        {
            if (handleInfo.currentLine != -1) // 插入一个点
            {
                handleInfo.editPoints.Insert(handleInfo.currentLine + 1, pos);

                m_vertices.InsertArrayElementAtIndex(handleInfo.currentLine + 1);
                UpdateTargetPoint(handleInfo.currentLine + 1, pos);
                handleInfo.currentPoint = handleInfo.currentLine + 1;
                handleInfo.currentLine = -1;
            }
            if (handleInfo.currentPoint != -1)
                handleInfo.isPointSelected = true;

            handleInfo.needRepaint = true;
        }

        void UpdateMouseOver(Vector3 pos)
        {
            int oldPoint = handleInfo.currentPoint;
            int oldLine = handleInfo.currentLine;
            bool oldWholeHandleState = handleInfo.isOnWholeHandle;

            handleInfo.currentLine = -1;
            handleInfo.isWholeHandleSelected = false;
            handleInfo.currentPoint = -1;
            handleInfo.isPointSelected = false;
            handleInfo.isOnWholeHandle = false;
            handleInfo.isWholeHandleSelected = false;

            if (m_assistType == 0)
            {
                bool isOnPoint = CoverAPoint(pos);
                if (isOnPoint)
                {
                    if (oldPoint != handleInfo.currentPoint)
                        handleInfo.needRepaint = true;
                    return;
                }
                bool isOnLine = CoverALine(pos);
                if (isOnLine)
                {
                    if (oldLine != handleInfo.currentLine)
                        handleInfo.needRepaint = true;
                    return;
                }
            }
            handleInfo.isOnWholeHandle = Vector3.Distance(handleInfo.moveWholeHandle, pos) < handleInfo.pointRadius;
            if (oldWholeHandleState != handleInfo.isOnWholeHandle)
                handleInfo.needRepaint = true;
        }

        bool CoverAPoint(Vector3 pos)
        {
            int index = -1;
            int count = handleInfo.editPoints.Count;
            float span = handleInfo.pointRadius;
            for (int i = 0; i < count; i++)
            {
                Vector3 p = handleInfo.editPoints[i];
                if (Vector3.Distance(p, pos) < span)
                {
                    index = i;
                    break;
                }
            }
            if (handleInfo.currentPoint != index)
            {
                handleInfo.currentPoint = index;
                handleInfo.needRepaint = true;
            }
            return handleInfo.currentPoint != -1;
        }

        bool CoverALine(Vector3 pos)
        {
            int index = -1;
            int count = handleInfo.editPoints.Count;
            float minDistance = handleInfo.pointRadius;
            for (int i = 0; i < count; i++)
            {
                Vector3 curP = handleInfo.editPoints[i];
                Vector3 nextP = handleInfo.editPoints[(i + 1) % count];
                var dstPointToLine = HandleUtility.DistancePointToLineSegment(pos, curP, nextP);
                if (dstPointToLine <= minDistance)
                {
                    minDistance = dstPointToLine;
                    index = i;
                }
            }
            if (handleInfo.currentLine != index)
            {
                handleInfo.currentLine = index;
                handleInfo.needRepaint = true;
            }
            return handleInfo.currentLine != -1;
        }

        void UpdateSerializedObject()
        {
            serializedObject.Update();
            polygonRawImage = target as PolygonRawImage;
        }

        void UpdateTargetPoint(int index, Vector3 pos)
        {
            Vector3 recordPos = polygonRawImage.transform.InverseTransformPoint(pos);
            m_vertices.GetArrayElementAtIndex(index).vector2Value = recordPos;
        }

        void UpdateTargetPoint(int index)
        {
            var oriPos = handleInfo.editPoints[index];
            Vector3 recordPos = polygonRawImage.transform.InverseTransformPoint(oriPos);
            m_vertices.GetArrayElementAtIndex(index).vector2Value = recordPos;
        }

        void ReplaceEditorPointList()
        {
            UpdateSerializedObject();
            handleInfo.editPoints.Clear();
            int count = m_vertices.arraySize;
            var parentTransform = polygonRawImage.transform;
            for (int i = 0; i < count; i++)
            {
                var recordPos = m_vertices.GetArrayElementAtIndex(i).vector2Value;
                var pos = parentTransform.TransformPoint(recordPos);
                handleInfo.editPoints.Add(pos);
            }
            handleInfo.needRepaint = true;
        }

        bool IsManuallyEditing()
        {
            return handleInfo.isPointSelected || handleInfo.isLineSelected || (handleInfo.isWholeHandleSelected && m_assistType == 0);
        }

        bool HasSelectedTarget()
        {
            return handleInfo.isPointSelected || handleInfo.isLineSelected || handleInfo.isWholeHandleSelected;
        }

        void Draw()
        {
            // Label
            var count = handleInfo.editPoints.Count;
            Vector2 rightestPoint = Vector2.one * float.MinValue;
            Color color;
            for (int i = 0; i < count; i++)
            {
                // point
                Vector3 curP = handleInfo.editPoints[i];
                bool isHover = (handleInfo.isWholeHandleSelected || (i == handleInfo.currentPoint));
                color = isHover ? Color.yellow : Color.green;
                color.a = 0.2f;
                Handles.color = color;
                Handles.DrawSolidDisc(curP, Vector3.forward, handleInfo.pointRadius);

                // line
                Vector3 nextP = handleInfo.editPoints[(i + 1) % count];
                isHover = (handleInfo.isWholeHandleSelected || (i == handleInfo.currentLine));
                Handles.color = isHover ? Color.yellow : Color.green;
                if (isHover)
                {
                    Handles.DrawLine(curP, nextP);
                }
                else
                {
                    Handles.DrawDottedLine(curP, nextP, 5);
                }
                if (curP.x > rightestPoint.x)
                    rightestPoint = curP;
            }

            handleInfo.moveWholeHandle = rightestPoint + new Vector2(20, 0);
            color = handleInfo.isOnWholeHandle ? Color.yellow : Color.red;
            color.a = 0.2f;
            Handles.color = color;
            Handles.DrawSolidDisc(handleInfo.moveWholeHandle, Vector3.forward, handleInfo.pointRadius);

            handleInfo.needRepaint = false;
            // serializedObject.ApplyModifiedProperties();
            polygonRawImage.UpdateShape();
        }
    }
}
#endif