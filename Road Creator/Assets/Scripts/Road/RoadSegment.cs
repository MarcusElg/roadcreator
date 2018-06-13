using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment : MonoBehaviour
{

    public Material roadMaterial;
    public float roadWidth = 3;
    public bool flipped = false;

    public bool leftShoulder = false;
    public float leftShoulderWidth = 1;
    public float leftShoulderHeightOffset = 0;
    public Material leftShoulderMaterial;

    public bool rightShoulder = false;
    public float rightShoulderWidth = 1;
    public float rightShoulderHeightOffset = 0;
    public Material rightShoulderMaterial;

    public void CreateRoadMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, float heightOffset, Transform segment, int smoothnessAmount)
    {
        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(0), "Road", roadMaterial, smoothnessAmount);
        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(1), "Left Shoulder", leftShoulderMaterial, smoothnessAmount, leftShoulder);
        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(2), "Right Shoulder", rightShoulderMaterial, smoothnessAmount, rightShoulder);
    }
    
    private void GenerateMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, float heightOffset, Transform segment, Transform mesh, string name, Material material, int smoothnessAmount, bool generate = true)
    {
        if (generate == true)
        {
            Vector3[] vertices = new Vector3[points.Length * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int numTriangles = 2 * (points.Length - 1);
            int[] triangles = new int[numTriangles * 3];
            int verticeIndex = 0;
            int triangleIndex = 0;

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 left = Misc.CalculateLeft(points, nextSegmentPoints, previousPoint, i);

                if (name == "Road")
                {
                    vertices[verticeIndex] = (points[i] + left * roadWidth + new Vector3(0, heightOffset, 0)) - segment.position;
                    vertices[verticeIndex + 1] = (points[i] - left * roadWidth + new Vector3(0, heightOffset, 0)) - segment.position;
                }
                else if (name == "Left Shoulder")
                {
                    vertices[verticeIndex] = (points[i] + left * roadWidth + left * leftShoulderWidth + new Vector3(0, heightOffset + leftShoulderHeightOffset, 0)) - segment.position;
                    vertices[verticeIndex + 1] = (points[i] + left * roadWidth + new Vector3(0, heightOffset, 0)) - segment.position;
                }
                else if (name == "Right Shoulder")
                {
                    vertices[verticeIndex] = (points[i] - left * roadWidth + new Vector3(0, heightOffset, 0)) - segment.position;
                    vertices[verticeIndex + 1] = (points[i] - left * roadWidth - left * rightShoulderWidth + new Vector3(0, heightOffset + rightShoulderHeightOffset, 0)) - segment.position;
                }

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

            // Fix indent
            if (nextSegmentPoints != null)
            {
                // NOT WORKING :C
                /*Vector3 startPosition = vertices[vertices.Length - 2 * ( smoothnessAmount + 1) - 1];
                Vector3 endPosition = vertices[vertices.Length - 2 * (smoothnessAmount + 1) + 3];
                Vector3 centerPosition = Misc.GetCenter(startPosition, endPosition);
                vertices[(vertices.Length - (2 * (points.Length - 1 - smoothnessAmount))) - 1] = centerPosition;
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                g.transform.position = vertices[(vertices.Length - 1 - (2 * smoothnessAmount))] + segment.position;
                g.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                startPosition = vertices[(vertices.Length - 2 * (points.Length - 1 - smoothnessAmount) - 1) + 1];
                endPosition = vertices[(vertices.Length - 2 * (points.Length - 1 - smoothnessAmount) - 1) - 3];
                centerPosition = Misc.GetCenter(startPosition, endPosition);
                vertices[(vertices.Length - (2 * (points.Length - 1 - smoothnessAmount))) - 2] = centerPosition;*/
            }

            Mesh generatedMesh = new Mesh();
            generatedMesh.vertices = vertices;
            generatedMesh.triangles = triangles;
            generatedMesh.uv = GenerateUvs(uvs, vertices);

            mesh.GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            mesh.GetComponent<MeshCollider>().sharedMesh = generatedMesh;
        } else
        {
            mesh.GetComponent<MeshFilter>().sharedMesh = null;
            mesh.GetComponent<MeshCollider>().sharedMesh = null;
        }

        mesh.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private Vector2[] GenerateUvs (Vector2[] uvs, Vector3[] vertices)
    {
        // Calculate total distance
        float totalDistanceLeft = 0;
        float totalDistanceRight = 0;
        float currentDistance = 0;

        // Left
        for (int i = 2; i < vertices.Length; i += 2)
        {
            totalDistanceLeft += Vector3.Distance(vertices[i - 2], vertices[i]);
        }

        for (int i = 3; i < vertices.Length; i += 2)
        {
            totalDistanceRight += Vector3.Distance(vertices[i - 2], vertices[i]);
        }

        for (int i = 0; i < uvs.Length; i += 2)
        {
            if (i > 0)
            {
                currentDistance += Vector3.Distance(vertices[i - 2], vertices[i]);
            }

            if (flipped == false)
            {
                uvs[i] = new Vector2(0, currentDistance / totalDistanceLeft);
            }
            else
            {
                uvs[i] = new Vector2(1, currentDistance / totalDistanceLeft);
            }
        }

        for (int i = 1; i < uvs.Length; i += 2)
        {
            if (i > 1)
            {
                currentDistance += Vector3.Distance(vertices[i - 2], vertices[i]);
            } else
            {
                currentDistance = 0;
            }

            if (flipped == false)
            {
                uvs[i] = new Vector2(1, currentDistance / totalDistanceRight);
            }
            else
            {
                uvs[i] = new Vector2(0, currentDistance / totalDistanceRight);
            }
        }

        return uvs;
    }
}
