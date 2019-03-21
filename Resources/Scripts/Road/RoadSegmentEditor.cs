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
        serializedObject.FindProperty("baseRoadMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Base Road Material", serializedObject.FindProperty("baseRoadMaterial").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("overlayRoadMaterial").objectReferenceValue = (Material)EditorGUILayout.ObjectField("Overlay Road Material", serializedObject.FindProperty("overlayRoadMaterial").objectReferenceValue, typeof(Material), false);
        serializedObject.FindProperty("roadPhysicsMaterial").objectReferenceValue = (PhysicMaterial)EditorGUILayout.ObjectField("Road Physic Material", serializedObject.FindProperty("roadPhysicsMaterial").objectReferenceValue, typeof(PhysicMaterial), false);

        if (EditorGUI.EndChangeCheck() == true)
        {
            Change();
        }

        EditorGUI.BeginChangeCheck();
        serializedObject.FindProperty("startRoadWidth").floatValue = Mathf.Max(0.01f, EditorGUILayout.FloatField("Start Road Width", serializedObject.FindProperty("startRoadWidth").floatValue));
        serializedObject.FindProperty("endRoadWidth").floatValue = Mathf.Max(0.01f, EditorGUILayout.FloatField("End Road Width", serializedObject.FindProperty("endRoadWidth").floatValue));
        if (EditorGUI.EndChangeCheck() == true)
        {
            Change();
            for (int i = 0; i < targets.Length; i++)
            {
                RoadCreator roadCreator = ((RoadSegment)targets[i]).transform.parent.parent.GetComponent<RoadCreator>();
                RoadSegment roadSegment = (RoadSegment)targets[i];

                if (roadSegment.transform.GetSiblingIndex() == roadSegment.transform.parent.childCount - 1)
                {
                    if (roadCreator.endIntersection != null)
                    {
                        roadCreator.CreateMesh();
                        roadCreator.endIntersection.connections[roadCreator.endIntersectionConnectionIndex].leftPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2] + roadSegment.transform.position);
                        roadCreator.endIntersection.connections[roadCreator.endIntersectionConnectionIndex].rightPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 1] + roadSegment.transform.position);
                        roadCreator.endIntersection.CreateMesh();
                    }
                }
                else if (roadSegment.transform.GetSiblingIndex() == 0)
                {
                    if (roadCreator.startIntersection != null)
                    {
                        roadCreator.CreateMesh();
                        roadCreator.startIntersection.connections[roadCreator.startIntersectionConnectionIndex].leftPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[1] + roadSegment.transform.position);
                        roadCreator.startIntersection.connections[roadCreator.startIntersectionConnectionIndex].rightPoint = new SerializedVector3(roadSegment.transform.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[0] + roadSegment.transform.position);
                        roadCreator.startIntersection.CreateMesh();
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

        GUILayout.Space(20);
        GUILayout.Label("Bridge", guiStyle);
        serializedObject.FindProperty("bridgeGenerator").enumValueIndex = (int)(RoadSegment.BridgeGenerator)EditorGUILayout.EnumPopup("Generator", (RoadSegment.BridgeGenerator)Enum.GetValues(typeof(RoadSegment.BridgeGenerator)).GetValue(serializedObject.FindProperty("bridgeGenerator").enumValueIndex));

        if (serializedObject.FindProperty("bridgeGenerator").enumValueIndex > 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bridgeMaterials"), true);
            serializedObject.FindProperty("yOffsetFirstStep").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Y Offset First Step", serializedObject.FindProperty("yOffsetFirstStep").floatValue), 0, 2);
            serializedObject.FindProperty("yOffsetSecondStep").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Y Offset Second Step", serializedObject.FindProperty("yOffsetSecondStep").floatValue), 0, 2);
            serializedObject.FindProperty("widthPercentageFirstStep").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Width Percentage First Step", serializedObject.FindProperty("widthPercentageFirstStep").floatValue), 0, 1);
            serializedObject.FindProperty("widthPercentageSecondStep").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Width Percentage Second Step", serializedObject.FindProperty("widthPercentageSecondStep").floatValue), 0, 1);
            serializedObject.FindProperty("extraWidth").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Extra Width", serializedObject.FindProperty("extraWidth").floatValue), 0, 1);

            GUILayout.Space(20);
            GUILayout.Label("Pillar Placement", guiStyle);
            serializedObject.FindProperty("placePillars").boolValue = EditorGUILayout.Toggle("Place Pillars", serializedObject.FindProperty("placePillars").boolValue);
            if (serializedObject.FindProperty("placePillars").boolValue == true)
            {
                serializedObject.FindProperty("pillarPrefab").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("Prefab", serializedObject.FindProperty("pillarPrefab").objectReferenceValue, typeof(GameObject), false);
                serializedObject.FindProperty("pillarGap").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Gap", serializedObject.FindProperty("pillarGap").floatValue));
                serializedObject.FindProperty("pillarPlacementOffset").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Placement Offset", serializedObject.FindProperty("pillarPlacementOffset").floatValue));
                serializedObject.FindProperty("extraPillarHeight").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("Extra Height", serializedObject.FindProperty("extraPillarHeight").floatValue));
                serializedObject.FindProperty("xzPillarScale").floatValue = Mathf.Max(0, EditorGUILayout.FloatField("XZ Pillar Scale", serializedObject.FindProperty("xzPillarScale").floatValue));
            }
        }

        if (targets.Length == 1)
        {
            GUILayout.Space(20);
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
        GUILayout.Space(20);

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
            GUILayout.Space(20);
            GUILayout.Label("Debug", guiStyle);
            GUILayout.Label(serializedObject.FindProperty("curved").boolValue.ToString());
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

    private void OnSceneGUI()
    {
        GameObject.FindObjectOfType<RoadSystem>().ShowCreationButtons();
    }

}
