using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SquareIntersection))]
public class SquareIntersectionEditor : Editor
{

    private SquareIntersection intersection;
    private Tool lastTool;

    private void OnEnable()
    {
        intersection = (SquareIntersection)target;

        if (intersection.globalSettings == null)
        {
            intersection.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (intersection.transform.childCount == 0)
        {
            GameObject sides = new GameObject("Sides");
            sides.transform.SetParent(intersection.transform);
            sides.transform.localPosition = Vector3.zero;

            AddSide("Up");
            AddSide("Down").transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            AddSide("Left").transform.localRotation = Quaternion.Euler(new Vector3(0, 270, 0));
            AddSide("Right").transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));

            GameObject mainMesh = new GameObject("Main Mesh");
            mainMesh.transform.SetParent(intersection.transform);
            mainMesh.transform.localPosition = Vector3.zero;
            mainMesh.AddComponent<MeshFilter>();
            mainMesh.AddComponent<MeshRenderer>();
            mainMesh.AddComponent<MeshCollider>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        intersection.GenerateMeshes();
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
        GUILayout.Label("Up Connection", guiStyle);
        intersection.upConnection = EditorGUILayout.Toggle("Up Connection", intersection.upConnection);
        if (intersection.upConnection == true)
        {
            intersection.upConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Up Connection Height", intersection.upConnectionHeight));
            intersection.upConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Up Connection Width", intersection.upConnectionWidth));
        }

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

            intersection.GenerateMeshes();
        }

        if (GUILayout.Button("Generate Intersection"))
        {
            intersection.GenerateMeshes();
        }
    }

    private void OnSceneGUI()
    {
        // Draw
        if (intersection.centerMaterial != null && intersection.connectionMaterial != null)
        {
            for (int i = 0; i < intersection.transform.GetChild(0).childCount; i++)
            {
                Handles.color = Color.green;
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(i).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }
        }
    }

}
