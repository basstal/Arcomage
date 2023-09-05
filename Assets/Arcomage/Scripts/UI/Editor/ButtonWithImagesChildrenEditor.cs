using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ButtonWithImagesChildren),true)]
[CanEditMultipleObjects]
public class ButtonWithImagesChildrenEditor : ButtonEditor
{
    SerializedProperty m_OnLongPressProperty;
    private SerializedProperty m_longPressTimeThreshold;
    private SerializedProperty m_playDefaultUiClickSound;
 
    protected override void OnEnable()
    {
        base.OnEnable();

        m_OnLongPressProperty = serializedObject.FindProperty("m_onLongPress");
        m_longPressTimeThreshold = serializedObject.FindProperty("m_longPressTimeThreshold");
        m_playDefaultUiClickSound = serializedObject.FindProperty("m_playDefaultUiClickSound");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_OnLongPressProperty);
        EditorGUILayout.PropertyField(m_longPressTimeThreshold);
        EditorGUILayout.PropertyField(m_playDefaultUiClickSound);

        serializedObject.ApplyModifiedProperties();
    }
}