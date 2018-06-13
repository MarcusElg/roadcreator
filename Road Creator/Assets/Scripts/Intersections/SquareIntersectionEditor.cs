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

        GenerateMeshes();
    }

    private GameObject AddSide(string name)
    {
        GameObject side = new GameObject(name + " Side");
        side.transform.SetParent(intersection.transform.GetChild(0));

        GameObject mesh = new GameObject(name + " Mesh");
        mesh.AddComponent<MeshFilter>();
        mesh.AddComponent<MeshRenderer>();
        mesh.AddComponent<MeshCollider>();
        mesh.transform.SetParent(side.transform);

        GameObject connectionPoint = new GameObject(name + " Connection Point");
        connectionPoint.AddComponent<BoxCollider>();
        connectionPoint.GetComponent<BoxCollider>().size = new Vector3(intersection.globalSettings.pointSize, intersection.globalSettings.pointSize, intersection.globalSettings.pointSize);
        connectionPoint.transform.SetParent(side.transform);

        return side;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
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
        }

        GUILayout.Label("");
        GUILayout.Label("Down Connection", guiStyle);
        intersection.downConnection = EditorGUILayout.Toggle("Dpwn Connection", intersection.downConnection);
        if (intersection.downConnection == true)
        {
            intersection.downConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Down Connection Height", intersection.downConnectionHeight));
        }

        GUILayout.Label("");
        GUILayout.Label("Left Connection", guiStyle);
        intersection.leftConnection = EditorGUILayout.Toggle("Left Connection", intersection.leftConnection);
        if (intersection.leftConnection == true)
        {
            intersection.leftConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Left Connection Height", intersection.leftConnectionHeight));
        }

        GUILayout.Label("");
        GUILayout.Label("Right Connection", guiStyle);
        intersection.rightConnection = EditorGUILayout.Toggle("Right Connection", intersection.rightConnection);
        if (intersection.rightConnection == true)
        {
            intersection.rightConnectionHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Right Connection Height", intersection.rightConnectionHeight));
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            GenerateMeshes();
        }

        if (GUILayout.Button("Generate intersection"))
        {
            GenerateMeshes();
        }
    }

    private void OnSceneGUI()
    {
    }

    private void GenerateMeshes()
    {
        GenerateMesh(intersection.transform.GetChild(1), new Vector3(-intersection.width / 2, intersection.heightOffset, -intersection.height / 2), new Vector3(intersection.width / 2, intersection.heightOffset, -intersection.height / 2), new Vector3(-intersection.width / 2, intersection.heightOffset, intersection.height / 2), new Vector3(intersection.width / 2, intersection.heightOffset, intersection.height / 2));

        if (intersection.upConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, intersection.height / 2);
            intersection.transform.GetChild(0).GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, intersection.upConnectionHeight);
            GenerateMesh(intersection.transform.GetChild(0).GetChild(0).GetChild(0), new Vector3(-intersection.width / 2, intersection.heightOffset, 0), new Vector3(intersection.width / 2, intersection.heightOffset, 0), new Vector3(-intersection.upConnectionWidth, intersection.heightOffset, intersection.upConnectionHeight), new Vector3(intersection.upConnectionWidth, intersection.heightOffset, intersection.upConnectionHeight));
        }

        if (intersection.downConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, -intersection.height / 2);
            intersection.transform.GetChild(0).GetChild(1).GetChild(1).localPosition = new Vector3(0, 0, intersection.downConnectionHeight);
            GenerateMesh(intersection.transform.GetChild(0).GetChild(1).GetChild(0), new Vector3(-intersection.width / 2, intersection.heightOffset, 0), new Vector3(intersection.width / 2, intersection.heightOffset, 0), new Vector3(-intersection.downConnectionWidth, intersection.heightOffset, intersection.downConnectionHeight), new Vector3(intersection.downConnectionWidth, intersection.heightOffset, intersection.downConnectionHeight));
        }

        if (intersection.leftConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(2).localPosition = new Vector3(-intersection.width / 2, 0, 0);
            intersection.transform.GetChild(0).GetChild(2).GetChild(1).localPosition = new Vector3(0, 0, intersection.leftConnectionHeight);
            GenerateMesh(intersection.transform.GetChild(0).GetChild(2).GetChild(0), new Vector3(-intersection.width / 2, intersection.heightOffset, 0), new Vector3(intersection.width / 2, intersection.heightOffset, 0), new Vector3(-intersection.leftConnectionWidth, intersection.heightOffset, intersection.leftConnectionHeight), new Vector3(intersection.leftConnectionWidth, intersection.heightOffset, intersection.leftConnectionHeight));
        }

        if (intersection.rightConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(3).localPosition = new Vector3(intersection.width / 2, 0, 0);
            intersection.transform.GetChild(0).GetChild(3).GetChild(1).localPosition = new Vector3(0, 0, intersection.rightConnectionHeight);
            GenerateMesh(intersection.transform.GetChild(0).GetChild(3).GetChild(0), new Vector3(-intersection.width / 2, intersection.heightOffset, 0), new Vector3(intersection.width / 2, intersection.heightOffset, 0), new Vector3(-intersection.rightConnectionWidth, intersection.heightOffset, intersection.rightConnectionHeight), new Vector3(intersection.rightConnectionWidth, intersection.heightOffset, intersection.rightConnectionHeight));
        }
    }

    private void GenerateMesh (Transform meshOwner, Vector3 pointOne, Vector3 pointTwo, Vector3 pointThree, Vector3 pointFour)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = pointOne;
        vertices[1] = pointTwo;
        vertices[2] = pointThree;
        vertices[3] = pointFour;

        for (int i = 0; i < 4; i++)
        {
            uvs[i] = vertices[i];
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };
        mesh.uv = uvs;

        meshOwner.GetComponent<MeshFilter>().sharedMesh = mesh;
        meshOwner.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
