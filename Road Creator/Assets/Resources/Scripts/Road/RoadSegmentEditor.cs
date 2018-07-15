using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSegment))]
[CanEditMultipleObjects]
public class RoadSegmentEditor : Editor
{

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        serializedObject.FindProperty("roadMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Road Material", serializedObject.FindProperty("roadMaterial").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("startRoadWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Start Road Width", serializedObject.FindProperty("startRoadWidth").floatValue));
        serializedObject.FindProperty("endRoadWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("End Road Width", serializedObject.FindProperty("endRoadWidth").floatValue));
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
        if (serializedObject.FindProperty("rightShoulder").boolValue == true)
        {
            serializedObject.FindProperty("rightShoulderMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Right Shoulder Material", serializedObject.FindProperty("rightShoulderMaterial").objectReferenceValue, typeof(Material), false);
            serializedObject.FindProperty("rightShoulderWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Shoulder Width", serializedObject.FindProperty("rightShoulderWidth").floatValue));
            serializedObject.FindProperty("rightShoulderHeightOffset").floatValue = EditorGUILayout.FloatField("Right Shoulder Y Offset", serializedObject.FindProperty("rightShoulderHeightOffset").floatValue);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();

            for (int i = 0; i < targets.Length; i++)
            {
                ((RoadSegment)targets[i]).transform.parent.parent.GetComponent<RoadCreator>().CreateMesh();
            }
        }

        GUILayout.Label("");
        if (GUILayout.Button("Detach Connections"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                ((RoadSegment)targets[i]).transform.GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection = null;

                if (((RoadSegment)targets[i]).transform.GetChild(0).childCount == 3)
                {
                    ((RoadSegment)targets[i]).transform.GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection = null;
                }
            }
        }

        if (GUILayout.Button("Straighten"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Transform points = ((RoadSegment)targets[i]).transform.GetChild(0);
                if (points.childCount == 3)
                {
                    points.GetChild(1).position = Misc.GetCenter(points.GetChild(0).position, points.GetChild(2).position);
                }

                points.parent.GetComponent<RoadCreator>().CreateMesh();
            }
        }

        if (GameObject.FindObjectOfType<GlobalSettings>().debug == true)
        {
            GUILayout.Label("");
            GUILayout.Label("Debug", guiStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startGuidelinePoints"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("centerGuidelinePoints"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("endGuidelinePoints"), true);
        }
    }

}
