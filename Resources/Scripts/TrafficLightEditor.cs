#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrafficLight))]
[CanEditMultipleObjects]
public class TrafficLightEditor : Editor
{

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("Materials", guiStyle);
        serializedObject.FindProperty("greenActive").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Green Active Material", serializedObject.FindProperty("greenActive").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("greenNonActive").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Green Non-active Material", serializedObject.FindProperty("greenNonActive").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("yellowActive").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Yellow Active Material", serializedObject.FindProperty("yellowActive").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("yellowNonActive").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Yellow Non-active Material", serializedObject.FindProperty("yellowNonActive").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("redActive").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Red Active Material", serializedObject.FindProperty("redActive").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("redNonActive").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Red Non-active Material", serializedObject.FindProperty("redNonActive").objectReferenceValue, typeof(Material), false);

        GUILayout.Space(20);
        GUILayout.Label("Timing", guiStyle);

        serializedObject.FindProperty("greenTime").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Green Time", serializedObject.FindProperty("greenTime").floatValue));
        serializedObject.FindProperty("yellowBeforeRedTime").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Yellow Before Red Time", serializedObject.FindProperty("yellowBeforeRedTime").floatValue));
        serializedObject.FindProperty("redTime").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Red Time", serializedObject.FindProperty("redTime").floatValue));
        serializedObject.FindProperty("yellowBeforeGreenTime").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Yellow Before Green Time", serializedObject.FindProperty("yellowBeforeGreenTime").floatValue));
        serializedObject.FindProperty("startColour").enumValueIndex = (int)(TrafficLight.TrafficColour)EditorGUILayout.EnumPopup("Start Colour", (TrafficLight.TrafficColour)System.Enum.GetValues(typeof(TrafficLight.TrafficColour)).GetValue(serializedObject.FindProperty("startColour").enumValueIndex));

        if (EditorGUI.EndChangeCheck() == true)
        {
            serializedObject.FindProperty("currentColour").enumValueIndex = serializedObject.FindProperty("startColour").enumValueIndex;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            for (int i = 0; i < targets.Length; i++)
            {
                ((TrafficLight)targets[i]).UpdateMaterials();
            }
        }
    }

    public void OnSceneGUI()
    {
        ((TrafficLight)target).transform.parent.GetComponent<RoadSystem>().ShowCreationButtons(false);
    }
}
#endif
