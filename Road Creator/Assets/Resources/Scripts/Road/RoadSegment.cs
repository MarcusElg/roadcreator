using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment : MonoBehaviour
{

    public Material roadMaterial;
    public float startRoadWidth = 2;
    public float endRoadWidth = 2;
    public bool flipped = false;

    public enum TerrainOption { adapt, deform, ignore };
    public TerrainOption terrainOption;

    public bool leftShoulder = false;
    public float leftShoulderWidth = 1;
    public float leftShoulderHeightOffset = 0;
    public Material leftShoulderMaterial;

    public bool rightShoulder = false;
    public float rightShoulderWidth = 1;
    public float rightShoulderHeightOffset = 0;
    public Material rightShoulderMaterial;

    public Vector3[] startGuidelinePoints;
    public Vector3[] centerGuidelinePoints;
    public Vector3[] endGuidelinePoints;

    public void CreateRoadMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, float heightOffset, Transform segment, int smoothnessAmount, RoadCreator roadCreator)
    {
        if (roadMaterial == null)
        {
            roadMaterial = Resources.Load("Materials/Roads/2 Lane Roads/2L Road") as Material;
        }

        if (leftShoulderMaterial == null)
        {
            leftShoulderMaterial = Resources.Load("Materials/Asphalt") as Material;
        }

        if (rightShoulderMaterial == null)
        {
            rightShoulderMaterial = Resources.Load("Materials/Asphalt") as Material;
        }
        if (segment.GetSiblingIndex() == 0)
        {
            SetGuidelines(points, nextSegmentPoints, true);
        } else
        {
            SetGuidelines(points, nextSegmentPoints, false);
        }

        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(0), "Road", roadMaterial, smoothnessAmount, roadCreator);
        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(1), "Left Shoulder", leftShoulderMaterial, smoothnessAmount, roadCreator, leftShoulder);
        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(2), "Right Shoulder", rightShoulderMaterial, smoothnessAmount, roadCreator, rightShoulder);
    }

    private void SetGuidelines(Vector3[] currentPoints, Vector3[] nextPoints, bool first)
    {
        // Start Guidelines
        Vector3 forward;
        Vector3 left;
        int guidelineAmount = transform.parent.parent.GetComponent<RoadCreator>().globalSettings.amountRoadGuidelines;

        if (guidelineAmount > 0)
        {
            if (first == true)
            {
                forward = currentPoints[1] - currentPoints[0];
                left = new Vector3(-forward.z, 0, forward.x).normalized;

                startGuidelinePoints = new Vector3[guidelineAmount * 2];
                for (int i = 0; i < (guidelineAmount * 2) - 1; i += 2)
                {
                    startGuidelinePoints[i] = transform.GetChild(0).GetChild(0).position + left * (i + 1);
                    startGuidelinePoints[i + 1] = transform.GetChild(0).GetChild(0).position - left * (i + 1);
                }
            }

            // Center Guidelines
            forward = transform.GetChild(0).GetChild(2).position - transform.GetChild(0).GetChild(0).position;
            left = new Vector3(-forward.z, 0, forward.x).normalized;

            centerGuidelinePoints = new Vector3[guidelineAmount * 2];
            for (int i = 0; i < (guidelineAmount * 2) - 1; i += 2)
            {
                centerGuidelinePoints[i] = transform.GetChild(0).GetChild(1).position + left * (i + 1);
                centerGuidelinePoints[i + 1] = transform.GetChild(0).GetChild(1).position - left * (i + 1);
            }

            // End guidelines
            endGuidelinePoints = new Vector3[guidelineAmount * 2];

            if (nextPoints == null)
            {
                forward = currentPoints[currentPoints.Length - 1] - currentPoints[currentPoints.Length - 2];
                left = new Vector3(-forward.z, 0, forward.x).normalized;
            }
            else if (nextPoints.Length > 1)
            {
                forward = nextPoints[1] - currentPoints[currentPoints.Length - 1];
                left = new Vector3(-forward.z, 0, forward.x).normalized;
            } else
            {
                endGuidelinePoints = new Vector3[0];
                return;
            }

            for (int i = 0; i < (guidelineAmount * 2) - 1; i += 2)
            {
                endGuidelinePoints[i] = transform.GetChild(0).GetChild(2).position + left * (i + 1);
                endGuidelinePoints[i + 1] = transform.GetChild(0).GetChild(2).position - left * (i + 1);
            }
        }
    }

    private void GenerateMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, float heightOffset, Transform segment, Transform mesh, string name, Material material, int smoothnessAmount, RoadCreator roadCreator, bool generate = true)
    {
        if (generate == true)
        {
            Vector3[] vertices = new Vector3[points.Length * 2];
            Vector2[] uvs = new Vector2[vertices.Length];
            int numTriangles = 2 * (points.Length - 1);
            int[] triangles = new int[numTriangles * 3];
            int verticeIndex = 0;
            int triangleIndex = 0;
            float totalDistance = 0;
            float currentDistance = 0;

            for (int i = 1; i < points.Length; i++)
            {
                totalDistance += Vector3.Distance(points[i - 1], points[i]);
            }

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 left = Misc.CalculateLeft(points, nextSegmentPoints, previousPoint, i);

                if (i > 0)
                {
                    currentDistance += Vector3.Distance(points[i - 1], points[i]);
                }

                float roadWidth = Mathf.Lerp(startRoadWidth, endRoadWidth, currentDistance / totalDistance);

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

                if (i > 1)
                {
                    // Third point and forward
                    // Left side
                    float distanceFirstSecond = Vector3.Distance(vertices[verticeIndex - 4], vertices[verticeIndex - 2]);
                    float distanceFirstThird = Vector3.Distance(vertices[verticeIndex - 4], vertices[verticeIndex]);

                    if (distanceFirstThird < distanceFirstSecond)
                    {
                        vertices[verticeIndex - 2] = Misc.GetCenter(vertices[verticeIndex - 4], vertices[verticeIndex]);
                    }

                    // Right side
                    distanceFirstSecond = Vector3.Distance(vertices[verticeIndex - 3], vertices[verticeIndex - 1]);
                    distanceFirstThird = Vector3.Distance(vertices[verticeIndex - 3], vertices[verticeIndex + 1]);

                    if (distanceFirstThird < distanceFirstSecond)
                    {
                        vertices[verticeIndex - 1] = Misc.GetCenter(vertices[verticeIndex - 3], vertices[verticeIndex + 1]);
                    }
                }

                /*if (terrainOption == TerrainOption.deform)
                {
                    // Change y position
                    RaycastHit raycastHit;
                    if (Physics.Raycast(vertices[verticeIndex] + segment.position, Vector3.down, out raycastHit, 100f, ~(1 << roadCreator.globalSettings.roadLayer)))
                    {
                        Terrain terrain = raycastHit.collider.GetComponent<Terrain>();
                        if (terrain != null)
                        {
                            TerrainData terrainData = terrain.terrainData;

                            int terrainPointX = (int)((raycastHit.point.x / terrainData.size.x) * terrainData.heightmapWidth);
                            int terrainPointY = (int)((vertices[verticeIndex] + segment.position).y * terrainData.size.y);
                            int terrainPointZ = (int)((raycastHit.point.z / terrainData.size.z) * terrainData.heightmapWidth);
                            Debug.Log(terrainPointX + ", " + terrainPointZ);
                            float[,] modifiedHeights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

                            modifiedHeights[terrainPointX - (int)terrain.transform.position.x, terrainPointZ - (int)terrain.transform.position.z] = terrainPointY;
                            terrainData.SetHeights(0, 0, modifiedHeights);
                        }
                    }
                }*/

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

            if (nextSegmentPoints != null && nextSegmentPoints.Length == 1)
            {
                // Intersection connection
                vertices[vertices.Length - 3] = Misc.GetCenter(vertices[vertices.Length - 1], vertices[vertices.Length - 5]);
                vertices[vertices.Length - 4] = Misc.GetCenter(vertices[vertices.Length - 2], vertices[vertices.Length - 6]);
            }

            Mesh generatedMesh = new Mesh();
            generatedMesh.vertices = vertices;
            generatedMesh.triangles = triangles;
            generatedMesh.uv = GenerateUvs(uvs, vertices);

            mesh.GetComponent<MeshFilter>().sharedMesh = generatedMesh;
            mesh.GetComponent<MeshCollider>().sharedMesh = generatedMesh;
        }
        else
        {
            mesh.GetComponent<MeshFilter>().sharedMesh = null;
            mesh.GetComponent<MeshCollider>().sharedMesh = null;
        }

        mesh.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private Vector2[] GenerateUvs(Vector2[] uvs, Vector3[] vertices)
    {
        // Calculate total distance
        float totalDistanceLeft = 0;
        float totalDistanceRight = 0;
        float currentDistance = 0;

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
            }
            else
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
