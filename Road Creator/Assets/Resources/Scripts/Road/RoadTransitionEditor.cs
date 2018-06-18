using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadTransition))]
public class RoadTransitionEditor : Editor {

    private RoadTransition roadTransition;
    private Tool lastTool;

    private void OnEnable()
    {
        roadTransition = (RoadTransition)target;

        if (roadTransition.globalSettings == null)
        {
            roadTransition.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (roadTransition.transform.childCount == 0)
        {
            GameObject connections = new GameObject("Connections");
            connections.transform.SetParent(roadTransition.transform);
            connections.transform.localPosition = Vector3.zero;

            AddConnection("Left");
            AddConnection("Right");

            GameObject mesh = new GameObject("Mesh");
            mesh.transform.SetParent(roadTransition.transform);
            mesh.transform.localPosition = Vector3.zero;
            mesh.AddComponent<MeshFilter>();
            mesh.AddComponent<MeshRenderer>();
            mesh.AddComponent<MeshCollider>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        roadTransition.GenerateMesh();
    }

    private void AddConnection(string name)
    {
        GameObject connectionPoint = new GameObject(name + " Connection Point");
        connectionPoint.AddComponent<BoxCollider>();
        connectionPoint.GetComponent<BoxCollider>().size = new Vector3(roadTransition.globalSettings.pointSize, roadTransition.globalSettings.pointSize, roadTransition.globalSettings.pointSize);
        connectionPoint.transform.SetParent(roadTransition.transform.GetChild(0));
        connectionPoint.transform.localPosition = Vector3.zero;
        connectionPoint.layer = roadTransition.globalSettings.intersectionPointsLayer;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        roadTransition.material = (Material)EditorGUILayout.ObjectField("Material", roadTransition.material, typeof(Material), false);
        roadTransition.leftWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Width", roadTransition.leftWidth));
        roadTransition.rightWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Width", roadTransition.rightWidth));
        roadTransition.height = Mathf.Max(0.1f, EditorGUILayout.FloatField("Height", roadTransition.height));
        roadTransition.heightOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", roadTransition.heightOffset));
        roadTransition.flipped = GUILayout.Toggle(roadTransition.flipped, "Flipped");

        if (EditorGUI.EndChangeCheck() == true)
        {
            roadTransition.GenerateMesh();

            // Update connections
            Point[] gameObjects = GameObject.FindObjectsOfType<Point>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i].intersectionConnection != null)
                {
                    gameObjects[i].transform.position = gameObjects[i].intersectionConnection.transform.position;
                    gameObjects[i].transform.parent.parent.parent.parent.GetComponent<RoadCreator>().UpdateMesh();
                }
            }
        }

        if (GUILayout.Button("Generate Mesh"))
        {
            roadTransition.GenerateMesh();
        }
    }

    private void OnSceneGUI()
    {
        // Draw
        if (roadTransition.material != null)
        {
            for (int i = 0; i < roadTransition.transform.GetChild(0).childCount; i++)
            {
                Handles.color = Color.green;
                Handles.CylinderHandleCap(0, roadTransition.transform.GetChild(0).GetChild(i).position, Quaternion.Euler(90, 0, 0), roadTransition.globalSettings.pointSize, EventType.Repaint);
            }
        }
    }

}
