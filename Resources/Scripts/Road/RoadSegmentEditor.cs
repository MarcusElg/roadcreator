﻿using System;
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
        serializedObject.FindProperty("roadPhysicsMaterial").objectReferenceValue = (PhysicMaterial)EditorGUILayout.ObjectField("Road Physic Material", serializedObject.FindProperty("roadPhysicsMaterial").objectReferenceValue, typeof(PhysicMaterial), false);

        if (EditorGUI.EndChangeCheck() == true)
        {
            Change();
        }

        EditorGUI.BeginChangeCheck();
        serializedObject.FindProperty("startRoadWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Start Road Width", serializedObject.FindProperty("startRoadWidth").floatValue));
        serializedObject.FindProperty("endRoadWidth").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("End Road Width", serializedObject.FindProperty("endRoadWidth").floatValue));
        if (EditorGUI.EndChangeCheck() == true)
        {
            Change();
            for (int i = 0; i < targets.Length; i++)
            {
                RoadCreator roadCreator = ((RoadSegment)targets[i]).transform.parent.parent.GetComponent<RoadCreator>();
                RoadSegment roadSegment = (RoadSegment)targets[i];

                if (roadSegment.transform.GetSiblingIndex() == roadSegment.transform.parent.childCount - 1)
                {
                    Debug.Log("3");
                    if (roadCreator.endIntersectionConnection != null)
                    {
                        Debug.Log("T");
                        roadCreator.CreateMesh();
                        roadCreator.endIntersectionConnection.leftPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2] + roadSegment.transform.position);
                        roadCreator.endIntersectionConnection.rightPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 1] + roadSegment.transform.position);
                        roadCreator.endIntersection.GenerateMesh();
                    }
                }
                else if (roadSegment.transform.GetSiblingIndex() == 0)
                {
                    if (roadCreator.startIntersectionConnection != null)
                    {
                        roadCreator.CreateMesh();
                        roadCreator.startIntersectionConnection.leftPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[1] + roadSegment.transform.position);
                        roadCreator.startIntersectionConnection.rightPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[0] + roadSegment.transform.position);
                        roadCreator.startIntersection.GenerateMesh();
                    }
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        serializedObject.FindProperty("flipped").boolValue = EditorGUILayout.Toggle("Road Flipped", serializedObject.FindProperty("flipped").boolValue);
        serializedObject.FindProperty("textureTilingY").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Texture Tiling Y Multiplier", serializedObject.FindProperty("textureTilingY").floatValue), 0, 10);
        serializedObject.FindProperty("terrainOption").enumValueIndex = (int)(RoadSegment.TerrainOption)EditorGUILayout.EnumPopup("Terrain Option", (RoadSegment.TerrainOption)Enum.GetValues(typeof(RoadSegment.TerrainOption)).GetValue(serializedObject.FindProperty("terrainOption").enumValueIndex));

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        if (targets.Length == 1)
        {
            GUILayout.Label("");
            GUILayout.Label("Extra Meshes", guiStyle);
            for (int i = 0; i < serializedObject.FindProperty("extraMeshOpen").arraySize; i++)
            {
                serializedObject.FindProperty("extraMeshOpen").GetArrayElementAtIndex(i).boolValue = EditorGUILayout.Foldout(serializedObject.FindProperty("extraMeshOpen").GetArrayElementAtIndex(i).boolValue, "Extra Mesh " + i);
                if (serializedObject.FindProperty("extraMeshOpen").GetArrayElementAtIndex(i).boolValue == true)
                {
                    serializedObject.FindProperty("extraMeshLeft").GetArrayElementAtIndex(i).boolValue = EditorGUILayout.Toggle("Left", serializedObject.FindProperty("extraMeshLeft").GetArrayElementAtIndex(i).boolValue);
                    serializedObject.FindProperty("extraMeshMaterial").GetArrayElementAtIndex(i).objectReferenceValue = (Material)EditorGUILayout.ObjectField("Material", serializedObject.FindProperty("extraMeshMaterial").GetArrayElementAtIndex(i).objectReferenceValue, typeof(Material), false);
                    serializedObject.FindProperty("extraMeshPhysicMaterial").GetArrayElementAtIndex(i).objectReferenceValue = (PhysicMaterial)EditorGUILayout.ObjectField("Physic Material", serializedObject.FindProperty("extraMeshPhysicMaterial").GetArrayElementAtIndex(i).objectReferenceValue, typeof(PhysicMaterial), false);
                    serializedObject.FindProperty("extraMeshWidth").GetArrayElementAtIndex(i).floatValue = Mathf.Max(EditorGUILayout.FloatField("Width", serializedObject.FindProperty("extraMeshWidth").GetArrayElementAtIndex(i).floatValue), 0);
                    serializedObject.FindProperty("extraMeshYOffset").GetArrayElementAtIndex(i).floatValue = EditorGUILayout.FloatField("Y Offset", serializedObject.FindProperty("extraMeshYOffset").GetArrayElementAtIndex(i).floatValue);

                    if (GUILayout.Button("Remove Extra Mesh") == true && ((RoadSegment)target).transform.GetChild(1).childCount > 1)
                    {
                        serializedObject.FindProperty("extraMeshOpen").DeleteArrayElementAtIndex(i);
                        serializedObject.FindProperty("extraMeshLeft").DeleteArrayElementAtIndex(i);
                        serializedObject.FindProperty("extraMeshMaterial").DeleteArrayElementAtIndex(i);
                        serializedObject.FindProperty("extraMeshPhysicMaterial").DeleteArrayElementAtIndex(i);
                        serializedObject.FindProperty("extraMeshWidth").DeleteArrayElementAtIndex(i);
                        serializedObject.FindProperty("extraMeshYOffset").DeleteArrayElementAtIndex(i);

                        for (int j = 0; j < targets.Length; j++)
                        {
                            DestroyImmediate(((RoadSegment)targets[j]).transform.GetChild(1).GetChild(i + 1).gameObject);
                        }
                    }
                }
            }

            if (GUILayout.Button("Add Extra Mesh"))
            {
                serializedObject.FindProperty("extraMeshOpen").InsertArrayElementAtIndex(serializedObject.FindProperty("extraMeshOpen").arraySize);
                serializedObject.FindProperty("extraMeshOpen").GetArrayElementAtIndex(serializedObject.FindProperty("extraMeshOpen").arraySize - 1).boolValue = true;
                serializedObject.FindProperty("extraMeshLeft").InsertArrayElementAtIndex(serializedObject.FindProperty("extraMeshLeft").arraySize);
                serializedObject.FindProperty("extraMeshLeft").GetArrayElementAtIndex(serializedObject.FindProperty("extraMeshLeft").arraySize - 1).boolValue = true;
                serializedObject.FindProperty("extraMeshMaterial").InsertArrayElementAtIndex(serializedObject.FindProperty("extraMeshMaterial").arraySize);
                serializedObject.FindProperty("extraMeshMaterial").GetArrayElementAtIndex(serializedObject.FindProperty("extraMeshMaterial").arraySize - 1).objectReferenceValue = Resources.Load("Materials/Low Poly/Asphalt") as Material;
                serializedObject.FindProperty("extraMeshPhysicMaterial").InsertArrayElementAtIndex(serializedObject.FindProperty("extraMeshPhysicMaterial").arraySize);
                serializedObject.FindProperty("extraMeshWidth").InsertArrayElementAtIndex(serializedObject.FindProperty("extraMeshWidth").arraySize);
                serializedObject.FindProperty("extraMeshWidth").GetArrayElementAtIndex(serializedObject.FindProperty("extraMeshWidth").arraySize - 1).floatValue = 1;
                serializedObject.FindProperty("extraMeshYOffset").InsertArrayElementAtIndex(serializedObject.FindProperty("extraMeshYOffset").arraySize);
                serializedObject.FindProperty("extraMeshYOffset").GetArrayElementAtIndex(serializedObject.FindProperty("extraMeshYOffset").arraySize - 1).floatValue = 0;

                GameObject extraMesh = new GameObject("Extra Mesh");
                extraMesh.AddComponent<MeshFilter>();
                extraMesh.AddComponent<MeshRenderer>();
                extraMesh.AddComponent<MeshCollider>();
                extraMesh.transform.SetParent(((RoadSegment)target).transform.GetChild(1));
                extraMesh.transform.localPosition = Vector3.zero;
                extraMesh.layer = ((RoadSegment)target).transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadLayer;
                extraMesh.hideFlags = HideFlags.NotEditable;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            Change();
        }
        GUILayout.Label("");

        if (GUILayout.Button("Straighten"))
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Transform points = ((RoadSegment)targets[i]).transform.GetChild(0);
                if (points.childCount == 3)
                {
                    points.parent.GetComponent<RoadSegment>().curved = false;
                    points.GetChild(1).position = Misc.GetCenter(points.GetChild(0).position, points.GetChild(2).position);
                }

                points.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
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

    private void Change()
    {
        serializedObject.ApplyModifiedProperties();

        for (int i = 0; i < targets.Length; i++)
        {
            ((RoadSegment)targets[i]).transform.parent.parent.GetComponent<RoadCreator>().CreateMesh();
        }
    }

}
