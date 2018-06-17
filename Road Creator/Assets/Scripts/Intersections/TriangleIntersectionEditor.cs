using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriangleIntersection))]
public class TriangleIntersectionEditor : Editor {

    private TriangleIntersection intersection;
    private Tool lastTool;

    private void OnEnable()
    {
        intersection = (TriangleIntersection)target;

        if (intersection.globalSettings == null)
        {
            if (GameObject.FindObjectOfType<GlobalSettings>() == null)
            {
                intersection.globalSettings = new GameObject("Global Settings").AddComponent<GlobalSettings>();
            }
            else
            {
                intersection.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
            }
        }

        if (intersection.transform.childCount == 0)
        {
            GameObject sides = new GameObject("Sides");
            sides.transform.SetParent(intersection.transform);
            sides.transform.localPosition = Vector3.zero;

            AddSide("Down").transform.localRotation = Quaternion.Euler(0, 180, 0);
            AddSide("Left");
            AddSide("Right");

            GameObject mainMesh = new GameObject("Main Mesh");
            mainMesh.transform.SetParent(intersection.transform);
            mainMesh.transform.localPosition = Vector3.zero;
            mainMesh.AddComponent<MeshFilter>();
            mainMesh.AddComponent<MeshRenderer>();
            mainMesh.AddComponent<MeshCollider>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;
    }

    private GameObject AddSide(string name)
    {
        GameObject side = new GameObject(name + " Side");
        side.transform.SetParent(intersection.transform.GetChild(0));
        side.transform.localPosition = Vector3.zero;

        GameObject mesh = new GameObject(name + " Mesh");
        mesh.AddComponent<MeshFilter>();
        mesh.AddComponent<MeshRenderer>();
        mesh.AddComponent<MeshCollider>();
        mesh.transform.SetParent(side.transform);
        mesh.transform.localPosition = Vector3.zero;

        GameObject connectionPoint = new GameObject(name + " Connection Point");
        connectionPoint.AddComponent<BoxCollider>();
        connectionPoint.GetComponent<BoxCollider>().size = new Vector3(intersection.globalSettings.pointSize, intersection.globalSettings.pointSize, intersection.globalSettings.pointSize);
        connectionPoint.transform.SetParent(side.transform);
        connectionPoint.transform.localPosition = Vector3.zero;
        connectionPoint.layer = intersection.globalSettings.intersectionPointsLayer;

        return side;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        intersection.centerMaterial = (Material)EditorGUILayout.ObjectField("Center Material", intersection.centerMaterial, typeof(Material), false);
        intersection.connectionMaterial = (Material)EditorGUILayout.ObjectField("Connection Material", intersection.connectionMaterial, typeof(Material), false);
        intersection.width = Mathf.Max(0.1f, EditorGUILayout.FloatField("Width", intersection.width));
        intersection.height = Mathf.Max(0.1f, EditorGUILayout.FloatField("Height", intersection.height));
        intersection.heightOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", intersection.heightOffset));

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("");
        GUILayout.Label("Down Connection", guiStyle);
        intersection.downConnection = EditorGUILayout.Toggle("Down Connection", intersection.downConnection);
        if (intersection.downConnection == true)
        {
            intersection.downConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Down Connection Height", intersection.downConnectionHeight));
            intersection.downConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Down Connection Width", intersection.downConnectionWidth));
        }

        GUILayout.Label("");
        GUILayout.Label("Left Connection", guiStyle);
        intersection.leftConnection = EditorGUILayout.Toggle("Left Connection", intersection.leftConnection);
        if (intersection.leftConnection == true)
        {
            intersection.leftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Connection Height", intersection.leftConnectionHeight));
            intersection.leftConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Connection Width", intersection.leftConnectionWidth));
        }

        GUILayout.Label("");
        GUILayout.Label("Right Connection", guiStyle);
        intersection.rightConnection = EditorGUILayout.Toggle("Right Connection", intersection.rightConnection);
        if (intersection.rightConnection == true)
        {
            intersection.rightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Connection Height", intersection.rightConnectionHeight));
            intersection.rightConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Connection Width", intersection.rightConnectionWidth));
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            intersection.GenerateMeshes();

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

        if (GUILayout.Button("Generate Intersection"))
        {
            intersection.GenerateMeshes();
        }
    }

}
