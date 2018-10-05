using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment : MonoBehaviour
{

    public Material roadMaterial;
    public PhysicMaterial roadPhysicsMaterial;
    public float startRoadWidth = 2;
    public float endRoadWidth = 2;
    public bool flipped = false;
    public float textureTilingY = 1;
    public bool curved = true;

    public enum TerrainOption { adapt, deform, ignore };
    public TerrainOption terrainOption;

    public List<bool> extraMeshOpen = new List<bool>();
    public List<bool> extraMeshLeft = new List<bool>();
    public List<Material> extraMeshMaterial = new List<Material>();
    public List<PhysicMaterial> extraMeshPhysicMaterial = new List<PhysicMaterial>();
    public List<float> extraMeshXOffset = new List<float>();
    public List<float> extraMeshWidth = new List<float>();
    public List<float> extraMeshYOffset = new List<float>();

    public Vector3[] startGuidelinePoints;
    public Vector3[] centerGuidelinePoints;
    public Vector3[] endGuidelinePoints;

    public void CreateRoadMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, float heightOffset, Transform segment, int smoothnessAmount, RoadCreator roadCreator)
    {
        if (roadMaterial == null)
        {
            roadMaterial = Resources.Load("Materials/Roads/2 Lane Roads/2L Road") as Material;
        }

        for (int i = 0; i < extraMeshOpen.Count; i++)
        {
            if (extraMeshMaterial[i] == null)
            {
                extraMeshMaterial[i] = Resources.Load("Materials/Asphalt") as Material;
            }
        }

        if (segment.GetSiblingIndex() == 0)
        {
            SetGuidelines(points, nextSegmentPoints, true);
        }
        else
        {
            SetGuidelines(points, nextSegmentPoints, false);
        }

        GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(0), "Road", roadMaterial, smoothnessAmount, roadCreator, roadPhysicsMaterial);

        for (int i = 0; i < extraMeshOpen.Count; i++)
        {
            GenerateMesh(points, nextSegmentPoints, previousPoint, heightOffset, segment, transform.GetChild(1).GetChild(i + 1), "Extra Mesh", extraMeshMaterial[i], smoothnessAmount, roadCreator, extraMeshPhysicMaterial[i], extraMeshXOffset[i], extraMeshWidth[i], extraMeshYOffset[i], extraMeshLeft[i]);
        }
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
            left = Misc.CalculateLeft(transform.GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(2).position);

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
            }
            else
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
        else
        {
            startGuidelinePoints = null;
            centerGuidelinePoints = null;
            endGuidelinePoints = null;
        }
    }

    private void GenerateMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, float heightOffset, Transform segment, Transform mesh, string name, Material material, int smoothnessAmount, RoadCreator roadCreator, PhysicMaterial physicMaterial, float xOffset = 0, float width = 0, float yOffset = 0, bool extraMeshLeft = true)
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
            /*GameObject g = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            g.transform.position = points[i];
            g.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            g.transform.GetComponent<Collider>().enabled = false;
            g.name = i + "";*/
            Vector3 left = Misc.CalculateLeft(points, nextSegmentPoints, previousPoint, i);
            float correctedHeightOffset = heightOffset;

            if (i > 0)
            {
                currentDistance += Vector3.Distance(points[i - 1], points[i]);
            }

            float roadWidth = Mathf.Lerp(startRoadWidth, endRoadWidth, currentDistance / totalDistance);

            if (i == 0 && previousPoint != Misc.MaxVector3)
            {
                correctedHeightOffset = (previousPoint.y + heightOffset);
            }
            else if (i == points.Length - 1 && nextSegmentPoints != null && nextSegmentPoints.Length == 1)
            {
                correctedHeightOffset = (nextSegmentPoints[0].y + heightOffset);
            }

            if (name == "Road")
            {
                vertices[verticeIndex] = (points[i] + left * roadWidth + new Vector3(0, correctedHeightOffset, 0)) - segment.position;
                vertices[verticeIndex + 1] = (points[i] - left * roadWidth + new Vector3(0, correctedHeightOffset, 0)) - segment.position;
            }
            else
            {
                if (extraMeshLeft == true)
                {
                    vertices[verticeIndex] = (points[i] + left * -xOffset + new Vector3(0, correctedHeightOffset, 0)) - segment.position;
                    vertices[verticeIndex + 1] = (points[i] + left * (-xOffset - width) + new Vector3(0, correctedHeightOffset + yOffset, 0)) - segment.position;
                }
                else
                {
                    vertices[verticeIndex] = (points[i] + left * (xOffset + width) + new Vector3(0, correctedHeightOffset + yOffset, 0)) - segment.position;
                    vertices[verticeIndex + 1] = (points[i] + left * xOffset + new Vector3(0, correctedHeightOffset, 0)) - segment.position;
                }
            }

            if (terrainOption == TerrainOption.deform)
            {
                DeformTerrain(vertices[verticeIndex] + segment.transform.position, roadCreator.globalSettings.roadLayer, roadCreator);
                DeformTerrain(vertices[verticeIndex + 1] + segment.transform.position, roadCreator.globalSettings.roadLayer, roadCreator);
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

        // Last
        if (nextSegmentPoints != null && nextSegmentPoints.Length == 1)
        {
            // Intersection
            vertices = fixVertices(0, vertices, (nextSegmentPoints[0] - points[points.Length - 1]).normalized, segment, roadCreator, 2);
            vertices = fixVertices(-1, vertices, (nextSegmentPoints[0] - points[points.Length - 1]).normalized, segment, roadCreator, 2);
        }

        if (nextSegmentPoints != null && smoothnessAmount > 0)
        {
            // Smoothness at end of segment
            //vertices = fixVertices(vertices.Length - 1 - (2 * smoothnessAmount), vertices, (points[points.Length - 1 - smoothnessAmount] - points[points.Length - 2 - smoothnessAmount]).normalized, segment, roadCreator, 2);
            vertices = fixVertices(vertices.Length - (2 * smoothnessAmount), vertices, (points[points.Length - 1 - smoothnessAmount] - points[points.Length - 2 - smoothnessAmount]).normalized, segment, roadCreator, 2);
        }

        // First
        /*if (previousPoint != Misc.MaxVector3)
        {
            // Intersection
            vertices = fixVertices(0, vertices, (points[0] - previousPoint).normalized, segment, roadCreator, -2);
            vertices = fixVertices(1, vertices, (points[0] - previousPoint).normalized, segment, roadCreator, -2);
        }*/

        Mesh generatedMesh = new Mesh();
        generatedMesh.vertices = vertices;
        generatedMesh.triangles = triangles;
        generatedMesh.uv = uvs;
        generatedMesh = GenerateUvs(generatedMesh);

        mesh.GetComponent<MeshFilter>().sharedMesh = generatedMesh;
        mesh.GetComponent<MeshCollider>().sharedMesh = generatedMesh;
        mesh.GetComponent<MeshCollider>().sharedMaterial = physicMaterial;
        mesh.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private Vector3[] fixVertices(int position, Vector3[] vertices, Vector3 forward, Transform segment, RoadCreator roadCreator, int change)
    {
        int startVertex = 0;
        for (int i = position; startVertex == 0; i -= change)
        {
            if (i < 1)
            {
                return vertices;
            }

            float direction = Vector3.Dot(forward.normalized, (vertices[position] - vertices[i]).normalized);
            if (direction > 0.0f)
            {
                startVertex = i += change;
            }

        }
        int amount = Mathf.Abs(startVertex - position);
        float part = 1f / amount;
        float index = 0;

        for (int i = startVertex; i < amount * 2; i += change)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.position = vertices[startVertex] + segment.position;
            g.name = "Start Vertex";
            g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.position = vertices[position] + segment.position;
            g.name = "Start Position";

            vertices[i] = Vector3.Lerp(vertices[startVertex], vertices[position], part * index);
            index += 1;
        }

        return vertices;
    }

    private void DeformTerrain(Vector3 position, int roadLayer, RoadCreator roadCreator)
    {
        // Change y position
        RaycastHit raycastHit;
        if (Physics.Raycast(position + new Vector3(0, 100, 0), Vector3.down, out raycastHit, Mathf.Infinity, ~(1 << roadLayer | 1 << roadCreator.globalSettings.ignoreMouseRayLayer)))
        {
            Terrain terrain = raycastHit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                Vector3 localTerrainPoint = raycastHit.point - terrain.transform.position;

                int terrainPointX = (int)((localTerrainPoint.x / terrainData.size.x) * terrainData.heightmapWidth);
                float terrainPointY = position.y / terrainData.size.y;
                int terrainPointZ = (int)((localTerrainPoint.z / terrainData.size.z) * terrainData.heightmapHeight);
                float[,] modifiedHeights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
                modifiedHeights[terrainPointZ, terrainPointX] = Mathf.Clamp01(terrainPointY);

                terrainData.SetHeights(0, 0, modifiedHeights);
            }
        }
    }

    private Mesh GenerateUvs(Mesh mesh)
    {
        Vector2[] uvs = mesh.uv;
        Vector2[] widths = new Vector2[uvs.Length];
        Vector3[] vertices = mesh.vertices;

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

        // Left
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

        // Right
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

        float totalWidth = endRoadWidth * 2;
        if (startRoadWidth > endRoadWidth)
        {
            totalWidth = startRoadWidth * 2;
        }

        for (int i = 0; i < widths.Length; i += 2)
        {
            float currentRoadWidth = Vector3.Distance(vertices[i], vertices[i + 1]);
            float currentLocalDistance = currentRoadWidth / totalWidth;
            uvs[i].x *= currentLocalDistance;
            uvs[i + 1].x *= currentLocalDistance;
            widths[i].x = currentLocalDistance;
            widths[i + 1].x = currentLocalDistance;
        }

        mesh.uv = uvs;
        mesh.uv2 = widths;
        return mesh;
    }
}
