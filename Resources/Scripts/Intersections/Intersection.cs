using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{

    public Material material;
    public PhysicMaterial physicMaterial;
    public List<IntersectionConnection> connections = new List<IntersectionConnection>();
    public float yOffset;

    public void GenerateMesh()
    {
        if (connections.Count < 2)
        {
            GameObject.DestroyImmediate(gameObject);
        }

        if (material == null)
        {
            material = Resources.Load("Materials/asphalt") as Material;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int vertexIndex = 0;

        for (int i = 0; i < connections.Count; i++)
        {
            Vector3 firstPoint = connections[i].leftPoint.ToNormalVector3();
            Vector3 firstCenterPoint = connections[i].lastPoint.ToNormalVector3();
            Vector3 nextPoint;
            Vector3 nextCenterPoint;
            float totalLength = connections[i].length;

            if (i == connections.Count - 1)
            {
                nextPoint = connections[0].rightPoint.ToNormalVector3();
                nextCenterPoint = connections[0].lastPoint.ToNormalVector3();
                totalLength += connections[0].length;
            }
            else
            {
                nextPoint = connections[i + 1].rightPoint.ToNormalVector3();
                nextCenterPoint = connections[i + 1].lastPoint.ToNormalVector3();
                totalLength += connections[i + 1].length;
            }

            float segments = totalLength * GameObject.FindObjectOfType<GlobalSettings>().resolution;
            float distancePerSegment = 1f / segments;

            for (float t = 0; t <= 1 + distancePerSegment; t += distancePerSegment)
            {
                vertices.Add(Vector3.Lerp(firstPoint, nextPoint, t) + new Vector3(0, yOffset, 0) - transform.position);
                uvs.Add(new Vector2(0, t));

                if (t < 0.5f)
                {
                    vertices.Add(Vector3.Lerp(firstCenterPoint, transform.position, t * 2) + new Vector3(0, yOffset, 0) - transform.position);
                }
                else
                {
                    vertices.Add(Vector3.Lerp(transform.position, nextCenterPoint, 2 * (t - 0.5f)) + new Vector3(0, yOffset, 0) - transform.position);
                }
                uvs.Add(new Vector2(1, t));

                if (t < 1)
                {
                    triangles = AddTriangles(triangles, vertexIndex);
                } 

                vertexIndex += 2;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sharedMaterial = material;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMaterial = physicMaterial;
        }
    }

    private List<int> AddTriangles (List<int> triangles, int vertexIndex)
    {
        triangles.Add(vertexIndex);
        triangles.Add((vertexIndex + 2));
        triangles.Add(vertexIndex + 1);

        triangles.Add(vertexIndex + 1);
        triangles.Add((vertexIndex + 2));
        triangles.Add((vertexIndex + 3));

        return triangles;
    }
}
