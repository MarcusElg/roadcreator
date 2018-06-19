using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSegment))]
[CanEditMultipleObjects]
public class RoadSegmentEditor : Editor
{

    public RoadSegment segment;

    private void OnEnable()
    {
        segment = (RoadSegment)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.FindProperty("roadMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Road Material", serializedObject.FindProperty("roadMaterial").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("roadWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Road Width", serializedObject.FindProperty("roadWidth").floatValue));
        serializedObject.FindProperty("flipped").boolValue = EditorGUILayout.Toggle("Road Flipped", serializedObject.FindProperty("flipped").boolValue);
        serializedObject.FindProperty("terrainOption").enumValueIndex = (int)(RoadSegment.TerrainOption)EditorGUILayout.EnumPopup("Terrain Option", (RoadSegment.TerrainOption)Enum.GetValues(typeof(RoadSegment.TerrainOption)).GetValue(serializedObject.FindProperty("terrainOption").enumValueIndex));

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("");
        GUILayout.Label("Left Shoulder", guiStyle);
        serializedObject.FindProperty("leftShoulder").boolValue = EditorGUILayout.Toggle("Left Shoulder", serializedObject.FindProperty("leftShoulder").boolValue);
        if (serializedObject.FindProperty("leftShoulder").boolValue == true)
        {
            serializedObject.FindProperty("leftShoulderMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Left Shoulder Material", serializedObject.FindProperty("leftShoulderMaterial").objectReferenceValue, typeof(Material), false);
            serializedObject.FindProperty("leftShoulderWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Shoulder Width", serializedObject.FindProperty("leftShoulderWidth").floatValue));
            serializedObject.FindProperty("leftShoulderHeightOffset").floatValue = EditorGUILayout.FloatField("Left Shoulder Y Offset", serializedObject.FindProperty("leftShoulderHeightOffset").floatValue);
        }

        GUILayout.Label("");
        GUILayout.Label("Right Shoulder", guiStyle);
        serializedObject.FindProperty("rightShoulder").boolValue = EditorGUILayout.Toggle("Right Shoulder", serializedObject.FindProperty("rightShoulder").boolValue);
        if (segment.rightShoulder == true)
        {
            serializedObject.FindProperty("rightShoulderMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Right Shoulder Material", serializedObject.FindProperty("rightShoulderMaterial").objectReferenceValue, typeof(Material), false);
            serializedObject.FindProperty("rightShoulderWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Shoulder Width", serializedObject.FindProperty("rightShoulderWidth").floatValue));
            serializedObject.FindProperty("rightShoulderHeightOffset").floatValue = EditorGUILayout.FloatField("Right Shoulder Y Offset", serializedObject.FindProperty("rightShoulderHeightOffset").floatValue);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            segment.transform.parent.parent.GetComponent<RoadCreator>().CreateMesh();
        }
    }

}
