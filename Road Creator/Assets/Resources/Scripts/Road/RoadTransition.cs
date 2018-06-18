using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTransition : MonoBehaviour {

    public float leftWidth = 2;
    public float rightWidth = 3;
    public float height = 3;
    public float heightOffset = 0.02f;

    public Material material;

    public GlobalSettings globalSettings;

    public void GenerateMesh()
    {
        transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, heightOffset, 0);
        transform.GetChild(0).GetChild(1).transform.localPosition = new Vector3(0, heightOffset, height);

        if (material == null)
        {
            material = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = new Vector3(-leftWidth, heightOffset, 0);
        vertices[1] = new Vector3(leftWidth, heightOffset, 0);
        vertices[2] = new Vector3(-rightWidth, heightOffset, height);
        vertices[3] = new Vector3(rightWidth, heightOffset, height);

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };
        mesh.uv = uvs;

        transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = material;
        transform.GetChild(1).GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
