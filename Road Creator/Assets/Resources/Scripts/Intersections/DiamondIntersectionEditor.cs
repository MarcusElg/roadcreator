using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DiamondIntersection))]
public class DiamondIntersectionEditor : Editor
{

    private DiamondIntersection intersection;
    private Tool lastTool;

    private void OnEnable()
    {
        intersection = (DiamondIntersection)target;

        if (intersection.globalSettings == null)
        {
            intersection.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (intersection.transform.childCount == 0)
        {
            GameObject sides = new GameObject("Sides");
            sides.transform.SetParent(intersection.transform);
            sides.transform.localPosition = Vector3.zero;

            AddSide("Upper Left");
            AddSide("Upper Right");
            AddSide("Lower Left");
            AddSide("Lower Right");

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
        GUILayout.Label("Upper Left Connection", guiStyle);
        intersection.upperLeftConnection = EditorGUILayout.Toggle("Upper Left Connection", intersection.upperLeftConnection);
        if (intersection.upperLeftConnection == true)
        {
            intersection.upperLeftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Left Connection Height", intersection.upperLeftConnectionHeight));
            intersection.upperLeftConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Left Connection Width", intersection.upperLeftConnectionWidth));
        }

        GUILayout.Label("");
        GUILayout.Label("Upper Right Connection", guiStyle);
        intersection.upperRightConnection = EditorGUILayout.Toggle("Upper Right Connection", intersection.upperRightConnection);
        if (intersection.upperRightConnection == true)
        {
            intersection.upperRightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Right Connection Height", intersection.upperRightConnectionHeight));
            intersection.upperRightConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Right Connection Width", intersection.upperRightConnectionWidth));
        }

        GUILayout.Label("");
        GUILayout.Label("Lower Left Connection", guiStyle);
        intersection.lowerLeftConnection = EditorGUILayout.Toggle("Lower Left Connection", intersection.lowerLeftConnection);
        if (intersection.lowerLeftConnection == true)
        {
            intersection.lowerLeftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Left Connection Height", intersection.lowerLeftConnectionHeight));
            intersection.lowerLeftConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Left Connection Width", intersection.lowerLeftConnectionWidth));
        }

        GUILayout.Label("");
        GUILayout.Label("Lower Right Connection", guiStyle);
        intersection.lowerRightConnection = EditorGUILayout.Toggle("Lower Right Connection", intersection.lowerRightConnection);
        if (intersection.lowerRightConnection == true)
        {
            intersection.lowerRightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Right Connection Height", intersection.lowerRightConnectionHeight));
            intersection.lowerRightConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Right Connection Width", intersection.lowerRightConnectionWidth));
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
