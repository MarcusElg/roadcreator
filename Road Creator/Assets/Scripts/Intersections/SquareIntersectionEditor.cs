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
        if (intersection.sideVariables == null)
        {
            intersection.sideVariables = new List<Key>();
        }

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
            AddSide("Down");
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

    private void AddSide(string name)
    {
        GameObject side = new GameObject(name + " Side");
        side.transform.SetParent(intersection.transform.GetChild(0));

        GameObject mesh = new GameObject(name + " Mesh");
        mesh.AddComponent<MeshFilter>();
        mesh.AddComponent<MeshRenderer>();
        mesh.AddComponent<MeshCollider>();
        mesh.transform.SetParent(side.transform);

        GameObject connectionPoint = new GameObject(name + " Connnection Point");
        connectionPoint.AddComponent<BoxCollider>();
        connectionPoint.GetComponent<BoxCollider>().size = new Vector3(intersection.globalSettings.pointSize, intersection.globalSettings.pointSize, intersection.globalSettings.pointSize);
        connectionPoint.transform.SetParent(side.transform);

        intersection.sideVariables.Add(new Key(name, true));
        intersection.sideVariables.Add(new Key(name + " Height", 1));
        intersection.sideVariables.Add(new Key(name + " Width", 1));
        intersection.sideVariables.Add(new Key(name + " Lower Width", 1));
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate intersection"))
        {
            GenerateMeshes();
        }

        for (int i = 0; i < intersection.sideVariables.Count; i++)
        {
            float outFloat;
            bool outBool;

            if (float.TryParse(intersection.sideVariables[i].value.ToString(), out outFloat) != false)
            {
                intersection.sideVariables[i].value = EditorGUILayout.FloatField(intersection.sideVariables[i].name, outFloat);
            } else if (bool.TryParse(intersection.sideVariables[i].value.ToString(), out outBool) != false)
            {
                intersection.sideVariables[i].value = GUILayout.Toggle(outBool, intersection.sideVariables[i].name);
            }
        }
    }

    private void OnSceneGUI()
    {
    }

    private void GenerateMeshes()
    {
        GenerateMesh(intersection.transform.GetChild(1), Vector3.zero, intersection.height, intersection.width, intersection.width);

        if (bool.Parse(intersection.sideVariables[0].value.ToString()) == true)
        {
            intersection.transform.GetChild(0).GetChild(0).position = new Vector3(0, 0, intersection.height);
            GenerateMesh(intersection.transform.GetChild(0).GetChild(0).GetChild(0), new Vector3(0, 0, 0.2f), float.Parse(intersection.sideVariables[1].value.ToString()), float.Parse(intersection.sideVariables[2].value.ToString()), float.Parse(intersection.sideVariables[3].value.ToString()));
        }
    }

    private void GenerateMesh (Transform meshOwner, Vector3 offset, float height, float width, float secondWidth)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = new Vector3(-height, 0, -width);
        vertices[1] = new Vector3(-height, 0, width);
        vertices[2] = new Vector3(height, 0, -secondWidth) + offset;
        vertices[3] = new Vector3(height, 0, secondWidth) + offset;

        for (int i = 0; i < 4; i++)
        {
            uvs[i] = vertices[i];
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 3, 2, 1 };
        mesh.uv = uvs;

        meshOwner.GetComponent<MeshFilter>().sharedMesh = mesh;
        meshOwner.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
