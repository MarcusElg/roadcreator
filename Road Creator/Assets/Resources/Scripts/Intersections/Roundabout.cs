using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roundabout : MonoBehaviour
{

    public float diameter = 5;
    public float width = 2;
    public float heightOffset = 0.02f;
    public Vector3[] points;
    public Material centerMaterial;

    public List<bool> connectionOpen = new List<bool>();
    public List<int> connectionVertexIndex = new List<int>();
    public List<float> connectionWidth = new List<float>();
    public List<Material> connectionMaterial = new List<Material>();

    public GlobalSettings globalSettings;

    public void GenerateMeshes()
    {
        if (centerMaterial == null)
        {
            centerMaterial = Resources.Load("Materials/Intersections/Roundabouts/2L Roundabout") as Material;
        }

        for (int i = 0; i < connectionMaterial.Count; i++)
        {
            if (connectionMaterial[i] == null)
            {
                connectionMaterial[i] = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
            }
        }

        GenerateCircle();

        for (int i = 0; i < connectionVertexIndex.Count; i++)
        {
            GenerateConnection(i, connectionVertexIndex[i]);
        }
    }

    private void GenerateCircle()
    {
        points = new Vector3[Mathf.RoundToInt(globalSettings.resolution * (diameter + width)) * 8];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Misc.FindPointInCircle(diameter / 2, i, 360f / (points.Length - 1));
        }

        Vector3[] vertices = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numTriangles = 2 * (points.Length - 1);
        int[] triangles = new int[numTriangles * 3];
        int verticeIndex = 0;
        int triangleIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            vertices[verticeIndex] = Misc.FindPointInCircle(diameter / 2 + width, i, 360f / (points.Length - 1)) + new Vector3(0, heightOffset, 0);
            vertices[verticeIndex + 1] = Misc.FindPointInCircle(diameter / 2 - width, i, 360f / (points.Length - 1)) + new Vector3(0, heightOffset, 0);

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[verticeIndex] = new Vector2(0, v);
            uvs[verticeIndex + 1] = new Vector2(1, v);

            if (i < points.Length - 1)
            {
                triangles[triangleIndex] = verticeIndex;
                triangles[triangleIndex + 1] = (verticeIndex + 2) % vertices.Length;
                triangles[triangleIndex + 2] = verticeIndex + 1;

                triangles[triangleIndex + 3] = verticeIndex + 1;
                triangles[triangleIndex + 4] = (verticeIndex + 2) % vertices.Length;
                triangles[triangleIndex + 5] = (verticeIndex + 3) % vertices.Length;
            }

            verticeIndex += 2;
            triangleIndex += 6;
        }

        Mesh generatedMesh = new Mesh();
        generatedMesh.vertices = vertices;
        generatedMesh.triangles = triangles;
        generatedMesh.uv = uvs;

        transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = generatedMesh;
        transform.GetChild(1).GetComponent<MeshCollider>().sharedMesh = generatedMesh;
        transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = centerMaterial;

        StartCoroutine(FixTextureStretch(points.Length - (4 * width)));
    }

    private void GenerateConnection(int objectIndex, int i)
    {
        transform.GetChild(0).GetChild(objectIndex).transform.localPosition = Misc.FindPointInCircle(diameter / 2, i, 360f / (points.Length - 1));
        Vector3 rotation = new Vector3(0, (360f / (points.Length - 1)) * i + 90, 0);
        transform.GetChild(0).GetChild(objectIndex).transform.localRotation = Quaternion.Euler(rotation);
        transform.GetChild(0).GetChild(objectIndex).GetChild(1).localPosition = new Vector3(0, 0, width);

        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = Vector3.left * connectionWidth[objectIndex] + new Vector3(0, heightOffset, 0);
        vertices[1] = -Vector3.left * connectionWidth[objectIndex] + new Vector3(0, heightOffset, 0);
        vertices[2] = Vector3.left * connectionWidth[objectIndex] + Vector3.forward * (width) + new Vector3(0, heightOffset, 0);
        vertices[3] = -Vector3.left * connectionWidth[objectIndex] + Vector3.forward * (width) + new Vector3(0, heightOffset, 0);

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };
        mesh.uv = uvs;

        transform.GetChild(0).GetChild(objectIndex).GetChild(0).GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.GetChild(0).GetChild(objectIndex).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = connectionMaterial[i];
        transform.GetChild(0).GetChild(objectIndex).GetChild(0).GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    IEnumerator FixTextureStretch(float length)
    {
        yield return new WaitForSeconds(0.01f);

        float textureRepeat = length * globalSettings.resolution;

        Material material = new Material(transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial);
        material.mainTextureScale = new Vector2(1, textureRepeat);
        transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = material;
    }
}
