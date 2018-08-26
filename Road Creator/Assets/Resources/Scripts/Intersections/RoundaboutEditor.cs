using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Roundabout))]
public class RoundaboutEditor : Editor
{

    private Roundabout roundabout;
    private Tool lastTool;

    private void OnEnable()
    {
        roundabout = (Roundabout)target;

        if (roundabout.globalSettings == null)
        {
            roundabout.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (roundabout.transform.childCount == 0)
        {
            GameObject connections = new GameObject("Connections");
            connections.transform.SetParent(roundabout.transform);
            connections.transform.localPosition = Vector3.zero;
            connections.transform.localRotation = Quaternion.Euler(Vector3.zero);

            GameObject mainMesh = new GameObject("Main Mesh");
            mainMesh.transform.SetParent(roundabout.transform);
            mainMesh.transform.localPosition = new Vector3(0, 0.001f, 0);
            mainMesh.transform.localRotation = Quaternion.Euler(Vector3.zero);
            mainMesh.AddComponent<MeshFilter>();
            mainMesh.AddComponent<MeshRenderer>();
            mainMesh.AddComponent<MeshCollider>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        roundabout.GenerateMeshes();
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        roundabout.centerMaterial = (Material)EditorGUILayout.ObjectField("Center Material", roundabout.centerMaterial, typeof(Material), false);
        roundabout.diameter = Mathf.Max(1.2f, EditorGUILayout.FloatField("Diameter", roundabout.diameter));
        roundabout.width = Mathf.Max(0.1f, EditorGUILayout.FloatField("Width", roundabout.width));
        roundabout.heightOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", roundabout.heightOffset));

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        for (int i = 0; i < roundabout.connectionVertexIndex.Count; i++)
        {
            roundabout.connectionOpen[i] = EditorGUILayout.Foldout(roundabout.connectionOpen[i], "Connection #" + i);

            if (roundabout.connectionOpen[i] == true)
            {
                roundabout.connectionVertexIndex[i] = Mathf.Clamp(EditorGUILayout.IntField("Connection Vertex Index", roundabout.connectionVertexIndex[i]), 0, roundabout.points.Length);
                roundabout.connectionWidth[i] = Mathf.Max(0.1f, EditorGUILayout.FloatField("Connection Width", roundabout.connectionWidth[i]));
                roundabout.connectionMaterial[i] = (Material)EditorGUILayout.ObjectField("Connection Material", roundabout.connectionMaterial[i], typeof(Material), false);

                if (GUILayout.Button("Remove Connection") == true)
                {
                    roundabout.connectionOpen.RemoveAt(i);
                    roundabout.connectionVertexIndex.RemoveAt(i);
                    roundabout.connectionWidth.RemoveAt(i);
                    roundabout.connectionMaterial.RemoveAt(i);
                    DestroyImmediate(roundabout.transform.GetChild(0).GetChild(i).gameObject);
                }
            }
        }

        GUILayout.Label("");

        if (GUILayout.Button("Add Connection"))
        {
            roundabout.connectionOpen.Add(true);
            roundabout.connectionVertexIndex.Add(0);
            roundabout.connectionWidth.Add(2);
            roundabout.connectionMaterial.Add(Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material);

            GameObject connection = new GameObject("Connection " + roundabout.transform.GetChild(0).childCount);
            connection.transform.SetParent(roundabout.transform.GetChild(0));
            connection.transform.localPosition = Vector3.zero;

            GameObject connectionMesh = new GameObject("Mesh");
            connectionMesh.AddComponent<MeshFilter>();
            connectionMesh.AddComponent<MeshRenderer>();
            connectionMesh.AddComponent<MeshCollider>();
            connectionMesh.transform.SetParent(connection.transform);
            connectionMesh.transform.localPosition = Vector3.zero;

            GameObject connectionPoint = new GameObject("Connection Point");
            connectionPoint.AddComponent<BoxCollider>();
            connectionPoint.GetComponent<BoxCollider>().size = new Vector3(roundabout.globalSettings.pointSize, roundabout.globalSettings.pointSize, roundabout.globalSettings.pointSize);
            connectionPoint.transform.SetParent(connection.transform);
            connectionPoint.transform.localPosition = Vector3.zero;
            connectionPoint.layer = roundabout.globalSettings.intersectionPointsLayer;
        }

        if (EditorGUI.EndChangeCheck() == true || roundabout.transform.hasChanged == true)
        {
            Misc.UpdateAllIntersectionConnections();
            roundabout.GenerateMeshes();
            roundabout.transform.hasChanged = false;
        }

        if (GUILayout.Button("Generate Roundabout"))
        {
            roundabout.GenerateMeshes();
        }
    }

    private void OnSceneGUI()
    {
        // Draw
        if (roundabout.centerMaterial != null && roundabout.connectionMaterial != null)
        {
            for (int i = 0; i < roundabout.transform.GetChild(0).childCount; i++)
            {
                if (roundabout.connectionMaterial[i] != null)
                {
                    Handles.color = Color.green;
                    Handles.CylinderHandleCap(0, roundabout.transform.GetChild(0).GetChild(i).GetChild(1).position, Quaternion.Euler(90, 0, 0), roundabout.globalSettings.pointSize, EventType.Repaint);
                }
            }
        }

        GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
    }

}
