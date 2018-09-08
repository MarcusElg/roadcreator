using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TriangleIntersection))]
public class TriangleIntersectionEditor : Editor
{

    private TriangleIntersection intersection;
    private Tool lastTool;

    private void OnEnable()
    {
        intersection = (TriangleIntersection)target;

        if (intersection.globalSettings == null)
        {
            intersection.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (intersection.transform.childCount == 0)
        {
            GameObject sides = new GameObject("Sides");
            sides.transform.SetParent(intersection.transform);
            sides.transform.localPosition = Vector3.zero;
            sides.transform.localRotation = Quaternion.Euler(Vector3.zero);

            AddSide("Down").transform.localRotation = Quaternion.Euler(0, 180, 0);
            AddSide("Left");
            AddSide("Right");

            GameObject mainMesh = new GameObject("Main Mesh");
            mainMesh.transform.SetParent(intersection.transform);
            mainMesh.transform.localPosition = Vector3.zero;
            mainMesh.transform.localRotation = Quaternion.Euler(Vector3.zero);
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
            intersection.downConnectionMaterial = (Material)EditorGUILayout.ObjectField("Down Connection Material", intersection.downConnectionMaterial, typeof(Material), false);
            intersection.downConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Down Connection Height", intersection.downConnectionHeight));
            intersection.downConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Down Connection Width", intersection.downConnectionWidth));
            intersection.downConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Down Connection Resolution", intersection.downConnectionResolution), 2, 15);
        }

        GUILayout.Label("");
        GUILayout.Label("Left Connection", guiStyle);
        intersection.leftConnection = EditorGUILayout.Toggle("Left Connection", intersection.leftConnection);
        if (intersection.leftConnection == true)
        {
            intersection.leftConnectionMaterial = (Material)EditorGUILayout.ObjectField("Left Connection Material", intersection.leftConnectionMaterial, typeof(Material), false);
            intersection.leftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Connection Height", intersection.leftConnectionHeight));
            intersection.leftConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Connection Width", intersection.leftConnectionWidth));
            intersection.leftConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Left Connection Resolution", intersection.leftConnectionResolution), 2, 15);
        }

        GUILayout.Label("");
        GUILayout.Label("Right Connection", guiStyle);
        intersection.rightConnection = EditorGUILayout.Toggle("Right Connection", intersection.rightConnection);
        if (intersection.rightConnection == true)
        {
            intersection.rightConnectionMaterial = (Material)EditorGUILayout.ObjectField("Right Connection Material", intersection.rightConnectionMaterial, typeof(Material), false);
            intersection.rightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Connection Height", intersection.rightConnectionHeight));
            intersection.rightConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Connection Width", intersection.rightConnectionWidth));
            intersection.rightConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Right Connection Resolution", intersection.rightConnectionResolution), 2, 15);
        }

        if (EditorGUI.EndChangeCheck() == true || intersection.transform.hasChanged == true)
        {
            Misc.UpdateAllIntersectionConnections();
            intersection.GenerateMeshes();
            intersection.transform.hasChanged = false;
        }

        if (GUILayout.Button("Generate Intersection"))
        {
            intersection.GenerateMeshes();
        }

        if (GUILayout.Button("Convert To Meshes"))
        {
            Misc.ConvertIntersectionToMesh(intersection.gameObject, "Triangle Intersection Mesh");
        }
    }

    private void OnSceneGUI()
    {
        // Draw
        if (intersection.centerMaterial != null)
        {
            Handles.color = Color.green;

            if (intersection.downConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(0).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }

            if (intersection.leftConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(1).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }

            if (intersection.rightConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(2).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }
        }

        GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
    }

}
