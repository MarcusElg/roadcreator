using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSegment))]
public class RoadSegmentEditor : Editor {

    public RoadSegment segment;

    private void OnEnable()
    {
        segment = (RoadSegment)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        segment.roadMaterial = (Material)EditorGUILayout.ObjectField("Road Material", segment.roadMaterial, typeof(Material), false);
        segment.roadWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Road Width", segment.roadWidth));
        segment.flipped = EditorGUILayout.Toggle("Road Flipped", segment.flipped);

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("");
        GUILayout.Label("Left Shoulder", guiStyle);
        segment.leftShoulder = EditorGUILayout.Toggle("Left Shoulder", segment.leftShoulder);
        if (segment.leftShoulder == true)
        {
            segment.leftShoulderMaterial = (Material)EditorGUILayout.ObjectField("Left Shoulder Material", segment.leftShoulderMaterial, typeof(Material), false);
            segment.leftShoulderWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Shoulder Width", segment.leftShoulderWidth));
            segment.leftShoulderHeightOffset = EditorGUILayout.FloatField("Left Shoulder Y Offset", segment.leftShoulderHeightOffset);
        }

        GUILayout.Label("");
        GUILayout.Label("Right Shoulder", guiStyle);
        segment.rightShoulder = EditorGUILayout.Toggle("Right Shoulder", segment.rightShoulder);
        if (segment.rightShoulder == true)
        {
            segment.rightShoulderMaterial = (Material)EditorGUILayout.ObjectField("Right Shoulder Material", segment.rightShoulderMaterial, typeof(Material), false);
            segment.rightShoulderWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Shoulder Width", segment.rightShoulderWidth));
            segment.rightShoulderHeightOffset = EditorGUILayout.FloatField("Right Shoulder Y Offset", segment.rightShoulderHeightOffset);
        }

        if (EditorGUI.EndChangeCheck())
        {
            segment.transform.parent.parent.GetComponent<RoadCreator>().roadEditor.UpdateMesh();
        }
    }

}
