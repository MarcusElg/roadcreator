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
            GenerateMeshes();

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
            GenerateMeshes();
        }
    }

    private void GenerateMeshes()
    {
        if (intersection.centerMaterial == null)
        {
            Debug.Log("You have to select a center material before generating the intersection");
            return;
        }

        if (intersection.connectionMaterial == null)
        {
            Debug.Log("You have to select a connection material before generating the intersection");
            return;
        }

        GenerateCenterMesh();

        if (intersection.downConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, -intersection.height);
            intersection.transform.GetChild(0).GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, intersection.downConnectionHeight);
            GenerateMesh(intersection.transform.GetChild(0).GetChild(0).GetChild(0), new Vector3(-intersection.width, intersection.heightOffset, 0), new Vector3(intersection.width, intersection.heightOffset, 0), new Vector3(-intersection.downConnectionWidth, intersection.heightOffset, intersection.downConnectionHeight), new Vector3(intersection.downConnectionWidth, intersection.heightOffset, intersection.downConnectionHeight), intersection.connectionMaterial);
        }
        else
        {
            intersection.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            intersection.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (intersection.leftConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(1).localPosition = Misc.GetCenter(new Vector3(-intersection.width, intersection.heightOffset, -intersection.height), new Vector3(0, intersection.heightOffset, intersection.height));
            intersection.transform.GetChild(0).GetChild(1).rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(0, intersection.heightOffset, intersection.height) - new Vector3(-intersection.width, intersection.heightOffset, -intersection.height));
            intersection.transform.GetChild(0).GetChild(1).GetChild(1).localPosition = new Vector3(0, 0, intersection.leftConnectionHeight);
            float connectionHeight = Vector3.Distance(new Vector3(-intersection.width, intersection.heightOffset, -intersection.height), new Vector3(0, intersection.heightOffset, intersection.height)) / 2;
            GenerateMesh(intersection.transform.GetChild(0).GetChild(1).GetChild(0), new Vector3(-connectionHeight, intersection.heightOffset, 0), new Vector3(connectionHeight, intersection.heightOffset, 0), new Vector3(-intersection.leftConnectionWidth, intersection.heightOffset, intersection.leftConnectionHeight), new Vector3(intersection.leftConnectionWidth, intersection.heightOffset, intersection.leftConnectionHeight), intersection.connectionMaterial);
        }
        else
        {
            intersection.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            intersection.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (intersection.rightConnection == true)
        {
            intersection.transform.GetChild(0).GetChild(2).localPosition = Misc.GetCenter(new Vector3(intersection.width, intersection.heightOffset, -intersection.height), new Vector3(0, intersection.heightOffset, intersection.height));
            intersection.transform.GetChild(0).GetChild(2).rotation = Quaternion.FromToRotation(Vector3.left, new Vector3(0, intersection.heightOffset, intersection.height) - new Vector3(intersection.width, intersection.heightOffset, -intersection.height));
            intersection.transform.GetChild(0).GetChild(2).GetChild(1).localPosition = new Vector3(0, 0, intersection.rightConnectionHeight);
            float connectionHeight = Vector3.Distance(new Vector3(-intersection.width, intersection.heightOffset, -intersection.height), new Vector3(0, intersection.heightOffset, intersection.height)) / 2;
            GenerateMesh(intersection.transform.GetChild(0).GetChild(2).GetChild(0), new Vector3(-connectionHeight, intersection.heightOffset, 0), new Vector3(connectionHeight, intersection.heightOffset, 0), new Vector3(-intersection.rightConnectionWidth, intersection.heightOffset, intersection.rightConnectionHeight), new Vector3(intersection.rightConnectionWidth, intersection.heightOffset, intersection.rightConnectionHeight), intersection.connectionMaterial);
        }
        else
        {
            intersection.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            intersection.transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }
    }

    private void GenerateCenterMesh ()
    {
        //GenerateMesh(new Vector3(-intersection.width, intersection.heightOffset, -intersection.height), new Vector3(intersection.width, intersection.heightOffset, -intersection.height), new Vector3(-intersection.width, intersection.heightOffset, intersection.height), new Vector3(intersection.width, intersection.heightOffset, intersection.height));
        Vector3[] vertices = new Vector3[3];
        Vector2[] uvs = new Vector2[3];

        vertices[0] = new Vector3(-intersection.width, intersection.heightOffset, -intersection.height);
        vertices[1] = new Vector3(intersection.width, intersection.heightOffset, -intersection.height);
        vertices[2] = new Vector3(0, intersection.heightOffset, intersection.height);

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0.5f, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0 };
        mesh.uv = uvs;

        intersection.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = mesh;
        Material newMaterial = intersection.centerMaterial;
        Texture texture = newMaterial.mainTexture;
        texture.wrapMode = TextureWrapMode.Clamp;
        newMaterial.mainTexture = texture;
        intersection.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = intersection.centerMaterial;
        intersection.transform.GetChild(1).GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void GenerateMesh(Transform meshOwner, Vector3 pointOne, Vector3 pointTwo, Vector3 pointThree, Vector3 pointFour, Material material)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = pointOne;
        vertices[1] = pointTwo;
        vertices[2] = pointThree;
        vertices[3] = pointFour;

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };
        mesh.uv = uvs;

        meshOwner.GetComponent<MeshFilter>().sharedMesh = mesh;
        Material newMaterial = material;
        Texture texture = newMaterial.mainTexture;
        texture.wrapMode = TextureWrapMode.Clamp;
        newMaterial.mainTexture = texture;
        meshOwner.GetComponent<MeshRenderer>().sharedMaterial = material;
        meshOwner.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
