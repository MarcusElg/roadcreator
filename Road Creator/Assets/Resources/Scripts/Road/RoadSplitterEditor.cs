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

            AddConnection("Left");
            AddConnection("Upper Right");
            AddConnection("Lower Right");

            GameObject mesh = new GameObject("Mesh");
            mesh.transform.SetParent(roadSplitter.transform);
            mesh.transform.localPosition = Vector3.zero;
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

        if (EditorGUI.EndChangeCheck() == true)
        {
            // Update connections
            Point[] gameObjects = GameObject.FindObjectsOfType<Point>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i].intersectionConnection != null)
                {
                    gameObjects[i].transform.position = gameObjects[i].intersectionConnection.transform.position;
                    gameObjects[i].transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                }
            }

            roadSplitter.GenerateMesh();
        }

        if (GUILayout.Button("Generate Mesh"))
        {
            roadSplitter.GenerateMesh();
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
