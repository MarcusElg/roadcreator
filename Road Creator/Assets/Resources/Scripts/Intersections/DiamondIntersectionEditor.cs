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
            sides.transform.localRotation = Quaternion.Euler(Vector3.zero);

            AddSide("Upper Left");
            AddSide("Upper Right");
            AddSide("Lower Left");
            AddSide("Lower Right");

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
        GUILayout.Label("Upper Left Connection", guiStyle);
        intersection.upperLeftConnection = EditorGUILayout.Toggle("Upper Left Connection", intersection.upperLeftConnection);
        if (intersection.upperLeftConnection == true)
        {
            intersection.upperLeftConnectionMaterial = (Material)EditorGUILayout.ObjectField("Upper Left Connection Material", intersection.upperLeftConnectionMaterial, typeof(Material), false);
            intersection.upperLeftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Left Connection Height", intersection.upperLeftConnectionHeight));
            intersection.upperLeftConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Left Connection Width", intersection.upperLeftConnectionWidth));
            intersection.upperLeftConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Upper Left Connection Resolution", intersection.upperLeftConnectionResolution), 2, 15);
        }

        GUILayout.Label("");
        GUILayout.Label("Upper Right Connection", guiStyle);
        intersection.upperRightConnection = EditorGUILayout.Toggle("Upper Right Connection", intersection.upperRightConnection);
        if (intersection.upperRightConnection == true)
        {
            intersection.upperRightConnectionMaterial = (Material)EditorGUILayout.ObjectField("Upper Right Connection Material", intersection.upperRightConnectionMaterial, typeof(Material), false);
            intersection.upperRightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Right Connection Height", intersection.upperRightConnectionHeight));
            intersection.upperRightConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Upper Right Connection Width", intersection.upperRightConnectionWidth));
            intersection.upperRightConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Upper Right Connection Resolution", intersection.upperRightConnectionResolution), 2, 15);
        }

        GUILayout.Label("");
        GUILayout.Label("Lower Left Connection", guiStyle);
        intersection.lowerLeftConnection = EditorGUILayout.Toggle("Lower Left Connection", intersection.lowerLeftConnection);
        if (intersection.lowerLeftConnection == true)
        {
            intersection.lowerLeftConnectionMaterial = (Material)EditorGUILayout.ObjectField("Lower Left Connection Material", intersection.lowerLeftConnectionMaterial, typeof(Material), false);
            intersection.lowerLeftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Left Connection Height", intersection.lowerLeftConnectionHeight));
            intersection.lowerLeftConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Left Connection Width", intersection.lowerLeftConnectionWidth));
            intersection.lowerLeftConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Lower Left Connection Resolution", intersection.lowerLeftConnectionResolution), 2, 15);
        }

        GUILayout.Label("");
        GUILayout.Label("Lower Right Connection", guiStyle);
        intersection.lowerRightConnection = EditorGUILayout.Toggle("Lower Right Connection", intersection.lowerRightConnection);
        if (intersection.lowerRightConnection == true)
        {
            intersection.lowerRightConnectionMaterial = (Material)EditorGUILayout.ObjectField("Lower Right Connection Material", intersection.lowerRightConnectionMaterial, typeof(Material), false);
            intersection.lowerRightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Right Connection Height", intersection.lowerRightConnectionHeight));
            intersection.lowerRightConnectionWidth = Mathf.Max(0.1f, EditorGUILayout.FloatField("Lower Right Connection Width", intersection.lowerRightConnectionWidth));
            intersection.lowerRightConnectionResolution = Mathf.Clamp(EditorGUILayout.IntField("Lower Right Connection Resolution", intersection.lowerRightConnectionResolution), 2, 15);
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
            Misc.ConvertIntersectionToMesh(intersection.gameObject, "Diamond Intersection Mesh");
        }
    }

    private void OnSceneGUI()
    {
        // Draw
        if (intersection.centerMaterial != null)
        {
            Handles.color = Color.green;

            if (intersection.upperLeftConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(0).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }

            if (intersection.upperRightConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(1).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }

            if (intersection.lowerLeftConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(2).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }

            if (intersection.lowerRightConnection == true)
            {
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(0).GetChild(3).GetChild(1).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }
        }

        GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
    }

}
