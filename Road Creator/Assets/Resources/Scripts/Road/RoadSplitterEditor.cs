using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSplitter))]
public class RoadSplitterEditor : Editor {

    private RoadSplitter roadSplitter;
    private Tool lastTool;

    private void OnEnable()
    {
        roadSplitter = (RoadSplitter)target;

        if (roadSplitter.globalSettings == null)
        {
            roadSplitter.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (roadSplitter.transform.childCount == 0)
        {
            GameObject connections = new GameObject("Connections");
            connections.transform.SetParent(roadSplitter.transform);
            connections.transform.localPosition = Vector3.zero;
            connections.transform.localRotation = Quaternion.Euler(Vector3.zero);
            connections.hideFlags = HideFlags.NotEditable;

            AddConnection("Left");
            AddConnection("Upper Right");
            AddConnection("Lower Right");

            GameObject mesh = new GameObject("Mesh");
            mesh.transform.SetParent(roadSplitter.transform);
            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localRotation = Quaternion.Euler(Vector3.zero);
            mesh.hideFlags = HideFlags.NotEditable;
            mesh.AddComponent<MeshFilter>();
            mesh.AddComponent<MeshRenderer>();
            mesh.AddComponent<MeshCollider>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        roadSplitter.GenerateMesh();
    }

    private void AddConnection(string name)
    {
        GameObject connectionPoint = new GameObject(name + " Connection Point");
        connectionPoint.AddComponent<BoxCollider>();
        connectionPoint.GetComponent<BoxCollider>().size = new Vector3(roadSplitter.globalSettings.pointSize, roadSplitter.globalSettings.pointSize, roadSplitter.globalSettings.pointSize);
        connectionPoint.transform.SetParent(roadSplitter.transform.GetChild(0));
        connectionPoint.transform.localPosition = Vector3.zero;
        connectionPoint.layer = roadSplitter.globalSettings.intersectionPointsLayer;
        connectionPoint.hideFlags = HideFlags.NotEditable;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        roadSplitter.material = (Material)EditorGUILayout.ObjectField("Material", roadSplitter.material, typeof(Material), false);
        roadSplitter.leftWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Width", roadSplitter.leftWidth));
        roadSplitter.rightWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Width", roadSplitter.rightWidth));
        roadSplitter.height = Mathf.Max(0.1f, EditorGUILayout.FloatField("Height", roadSplitter.height));
        roadSplitter.heightOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", roadSplitter.heightOffset));

        roadSplitter.rightXOffset = EditorGUILayout.FloatField("Right X Offset", roadSplitter.rightXOffset);
        roadSplitter.lowerRightXOffset = EditorGUILayout.FloatField("Lower Right X Offset", roadSplitter.lowerRightXOffset);
        roadSplitter.upperRightXOffset = EditorGUILayout.FloatField("Upper Right X Offset", roadSplitter.upperRightXOffset);

        if (EditorGUI.EndChangeCheck() == true || roadSplitter.transform.hasChanged == true)
        {
            Misc.UpdateAllIntersectionConnections();
            roadSplitter.GenerateMesh();
            roadSplitter.transform.hasChanged = false;
        }

        if (GUILayout.Button("Generate Mesh"))
        {
            roadSplitter.GenerateMesh();
        }

        if (GUILayout.Button("Convert To Meshes"))
        {
            MeshFilter[] meshFilters = roadSplitter.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length > 0 && meshFilters[0].sharedMesh != null)
            {
                GameObject roadSplitterMesh = new GameObject("Road Splitter Mesh");
                Undo.RegisterCreatedObjectUndo(roadSplitterMesh, "Created Road Splitter Mesh");
                roadSplitterMesh.transform.position = roadSplitter.transform.position;

                for (int i = 0; i < meshFilters.Length; i++)
                {
                    if (meshFilters[i].sharedMesh != null)
                    {
                        Undo.SetTransformParent(meshFilters[i].transform, roadSplitterMesh.transform, "Created Road Splitter Mesh");
                        meshFilters[i].name = "Mesh";
                        meshFilters[i].transform.localPosition = Vector3.zero;
                        meshFilters[i].gameObject.hideFlags = HideFlags.None;
                    }
                }

                Undo.DestroyObjectImmediate(roadSplitter.gameObject);
                Selection.activeObject = roadSplitterMesh;
            }
        }
    }

    private void OnSceneGUI()
    {
        // Draw
        if (roadSplitter.material != null)
        {
            for (int i = 0; i < roadSplitter.transform.GetChild(0).childCount; i++)
            {
                Handles.color = Color.green;
                Handles.CylinderHandleCap(0, roadSplitter.transform.GetChild(0).GetChild(i).position, Quaternion.Euler(90, 0, 0), roadSplitter.globalSettings.pointSize, EventType.Repaint);
            }
        }

        GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
    }

}
