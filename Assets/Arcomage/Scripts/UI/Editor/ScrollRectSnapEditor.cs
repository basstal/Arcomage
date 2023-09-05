using UnityEditor;
using NOAH.UI;

[CanEditMultipleObjects]
[CustomEditor(typeof(ScrollRectSnap), true)]
public class ScrollRectSnapEditor : Editor
{
    SerializedProperty m_TargetParent;
    SerializedProperty m_ViewSnapPivot;
    SerializedProperty m_ViewSnapOffset;
    SerializedProperty m_TargetSnapPivot;
    SerializedProperty m_TargetSnapOffset;
    SerializedProperty m_SmoothTime;
    SerializedProperty m_EaseType;
    SerializedProperty m_ClampWithinContent;
    SerializedProperty m_AutoSnap;
    SerializedProperty m_InertialPredictTime;
    SerializedProperty m_OnEndSnap;
    SerializedProperty m_easeCureve;
    SerializedProperty m_distanceLimitForElasticMovement;

    protected virtual void OnEnable()
    {
        m_TargetParent = serializedObject.FindProperty("m_TargetParent");
        m_ViewSnapPivot = serializedObject.FindProperty("m_ViewSnapPivot");
        m_ViewSnapOffset = serializedObject.FindProperty("m_ViewSnapOffset");
        m_TargetSnapPivot = serializedObject.FindProperty("m_TargetSnapPivot");
        m_TargetSnapOffset = serializedObject.FindProperty("m_TargetSnapOffset");
        m_SmoothTime = serializedObject.FindProperty("m_SmoothTime");
        m_EaseType = serializedObject.FindProperty("m_EaseType");
        m_ClampWithinContent = serializedObject.FindProperty("m_ClampWithinContent");
        m_AutoSnap = serializedObject.FindProperty("m_AutoSnap");
        m_InertialPredictTime = serializedObject.FindProperty("m_InertialPredictTime");
        m_OnEndSnap = serializedObject.FindProperty("m_OnEndSnap");
        m_easeCureve = serializedObject.FindProperty("easeCurve");
        m_distanceLimitForElasticMovement = serializedObject.FindProperty("m_distanceLimitForElasticMovement");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_TargetParent);
        EditorGUILayout.PropertyField(m_ViewSnapPivot);
        EditorGUILayout.PropertyField(m_ViewSnapOffset);
        EditorGUILayout.PropertyField(m_TargetSnapPivot);
        EditorGUILayout.PropertyField(m_TargetSnapOffset);
        EditorGUILayout.PropertyField(m_SmoothTime);
        EditorGUILayout.PropertyField(m_EaseType);
        EditorGUILayout.PropertyField(m_easeCureve);

        EditorGUILayout.PropertyField(m_ClampWithinContent);
        EditorGUILayout.PropertyField(m_AutoSnap);
        if (m_AutoSnap.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_InertialPredictTime);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(m_OnEndSnap);
        EditorGUILayout.PropertyField(m_distanceLimitForElasticMovement);

        serializedObject.ApplyModifiedProperties();
    }
}