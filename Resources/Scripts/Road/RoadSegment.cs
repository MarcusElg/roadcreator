#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Segments")]
public class RoadSegment : MonoBehaviour
{

    public Material baseRoadMaterial;
    public Material overlayRoadMaterial;
    public PhysicMaterial roadPhysicsMaterial;
    public float startRoadWidth = 2;
    public float endRoadWidth = 2;
    public bool flipped = false;
    public float textureTilingY = 1;
    public bool curved = true;

    public enum TerrainOption { adapt, deform, ignore };
    public TerrainOption terrainOption;

    public bool generateSimpleBridge = false;
    public bool generateCustomBridge = false;
    public BridgeSettings bridgeSettings = new BridgeSettings();

    public bool placePillars = true;
    public GameObject pillarPrefab;
    public bool adaptGapToCustomBridge = false;

    // Spaced
    public float pillarGap = 5;
    public float pillarPlacementOffset = 5;
    public float extraPillarHeight = 0.2f;
    public float xPillarScale = 1;
    public float xPillarScaleMultiplier = 1;
    public float zPillarScale = 1;
    public PrefabLineCreator.RotationDirection pillarRotationDirection;

    public List<ExtraMesh> extraMeshes = new List<ExtraMesh>();

    public RoadGuideline startGuidelinePoints;
    public RoadGuideline centerGuidelinePoints;
    public RoadGuideline endGuidelinePoints;

#if UNITY_EDITOR
    public SerializedObject settings;
#endif

    public void CreateRoadMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, Vector3[] previousVertices, float heightOffset, Transform segment, Transform previousSegment, RoadCreator roadCreator)
    {
        CheckMaterialsAndPrefabs();

        if (transform.Find("Bridge") != null)
        {
            DestroyImmediate(transform.Find("Bridge").gameObject);
        }

        if (transform.Find("Custom Bridge") != null)
        {
            DestroyImmediate(transform.Find("Custom Bridge").gameObject);
        }

        if (segment.GetSiblingIndex() == 0)
        {
            SetGuidelines(points, nextSegmentPoints, true);
        }
        else
        {
            SetGuidelines(points, nextSegmentPoints, false);
        }

        GenerateMesh(points, nextSegmentPoints, previousPoint, previousVertices, heightOffset, segment, previousSegment, transform.GetChild(1).GetChild(0), "Road", baseRoadMaterial, overlayRoadMaterial, roadPhysicsMaterial);
        GenerateExtraMeshes(points, nextSegmentPoints, previousPoint, previousVertices, heightOffset, segment, previousSegment);
        GenerateBridges(points, nextSegmentPoints, previousPoint, previousSegment);
    }

    private void CheckMaterialsAndPrefabs()
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        if (baseRoadMaterial == null)
        {
            baseRoadMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
        }

        if (bridgeSettings.bridgeMaterials == null || bridgeSettings.bridgeMaterials.Length == 0 || bridgeSettings.bridgeMaterials[0] == null)
        {
            Material[] materials = new Material[settings.FindProperty("defaultSimpleBridgeMaterials").arraySize];
            for (int i = 0; i < settings.FindProperty("defaultSimpleBridgeMaterials").arraySize; i++)
            {
                materials[i] = (Material)settings.FindProperty("defaultSimpleBridgeMaterials").GetArrayElementAtIndex(i).objectReferenceValue;
            }

            bridgeSettings.bridgeMaterials = materials;
        }

        if (pillarPrefab == null || pillarPrefab.GetComponent<MeshFilter>() == null)
        {
            pillarPrefab = (GameObject)settings.FindProperty("defaultPillarPrefab").objectReferenceValue;
        }

        if (bridgeSettings.bridgeMesh == null || bridgeSettings.bridgeMesh.GetComponent<MeshFilter>() == null)
        {
            bridgeSettings.bridgeMesh = (GameObject)settings.FindProperty("defaultCustomBridgePrefab").objectReferenceValue;
        }

        for (int i = 0; i < extraMeshes.Count; i++)
        {
            if (extraMeshes[i].baseMaterial == null)
            {
                extraMeshes[i].baseMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
            }
        }

        for (int i = 0; i < extraMeshes.Count; i++)
        {
            if (extraMeshes[i].overlayMaterial == null)
            {
                extraMeshes[i].overlayMaterial = (Material)settings.FindProperty("defaultExtraMeshOverlayMaterial").objectReferenceValue;
            }
        }
    }

    private void GenerateExtraMeshes(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, Vector3[] previousVertices, float heightOffset, Transform segment, Transform previousSegment)
    {
        for (int i = 0; i < extraMeshes.Count; i++)
        {
            float leftYOffset = extraMeshes[i].yOffset;
            float startXOffset = 0;
            float endXOffset = 0;
            float yOffset = heightOffset;

            if (i > 0)
            {
                bool foundLast = false;
                for (int j = i - 1; j > -1; j -= 1)
                {
                    if (extraMeshes[j].left == extraMeshes[i].left && j != i)
                    {
                        if (foundLast == false)
                        {
                            leftYOffset = extraMeshes[j].yOffset;
                            foundLast = true;
                        }

                        startXOffset += extraMeshes[j].startWidth;
                        endXOffset += extraMeshes[j].endWidth;
                        yOffset += extraMeshes[j].yOffset;
                    }
                }
            }

            float currentHeight = heightOffset;
            for (int j = i - 1; j > -1; j -= 1)
            {
                if (extraMeshes[j].left == extraMeshes[i].left && j != i)
                {
                    currentHeight += extraMeshes[j].yOffset;
                }
            }

            GenerateMesh(points, nextSegmentPoints, previousPoint, previousVertices, heightOffset, segment, previousSegment, transform.GetChild(1).GetChild(i + 1), "Extra Mesh", extraMeshes[i].baseMaterial, extraMeshes[i].overlayMaterial, extraMeshes[i].physicMaterial, startXOffset, endXOffset, extraMeshes[i].startWidth, extraMeshes[i].endWidth, currentHeight + extraMeshes[i].yOffset, currentHeight, extraMeshes[i].left);
        }
    }

    private void GenerateBridges(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, Transform previousSegment)
    {
        if (generateSimpleBridge == true || generateCustomBridge == true)
        {
            float startExtraWidthLeft = bridgeSettings.extraWidth;
            float endExtraWidthLeft = bridgeSettings.extraWidth;
            float startExtraWidthRight = bridgeSettings.extraWidth;
            float endExtraWidthRight = bridgeSettings.extraWidth;

            for (int i = 0; i < extraMeshes.Count; i++)
            {
                if (extraMeshes[i].left == true)
                {
                    startExtraWidthLeft += extraMeshes[i].startWidth;
                    endExtraWidthLeft += extraMeshes[i].endWidth;
                }
                else
                {
                    startExtraWidthRight += extraMeshes[i].startWidth;
                    endExtraWidthRight += extraMeshes[i].endWidth;
                }
            }

            PrefabLineCreator customBridge = null;

            if (generateSimpleBridge == true)
            {
                BridgeGeneration.GenerateSimpleBridge(points, nextSegmentPoints, previousPoint, this, previousSegment, startExtraWidthLeft, endExtraWidthLeft, startExtraWidthRight, endExtraWidthRight, bridgeSettings.bridgeMaterials, transform.GetChild(0).GetChild(0).transform.position, transform.GetChild(0).GetChild(1).transform.position, transform.GetChild(0).GetChild(2).transform.position);
            }

            if (generateCustomBridge == true)
            {
                customBridge = BridgeGeneration.GenerateCustomBridge(this, startExtraWidthLeft + startRoadWidth, startExtraWidthRight + startRoadWidth, endExtraWidthLeft + endRoadWidth, endExtraWidthRight + endRoadWidth);
            }

            if (placePillars == true)
            {
                if (generateSimpleBridge == true)
                {
                    BridgeGeneration.GeneratePillars(points, transform.GetChild(0).GetChild(0).transform.position, transform.GetChild(0).GetChild(1).transform.position, transform.GetChild(0).GetChild(2).transform.position, this, transform.Find("Bridge").gameObject, true, customBridge, startExtraWidthLeft, startExtraWidthRight, endExtraWidthLeft, endExtraWidthRight);
                }
                else if (generateCustomBridge == true)
                {
                    BridgeGeneration.GeneratePillars(points, transform.GetChild(0).GetChild(0).transform.position, transform.GetChild(0).GetChild(1).transform.position, transform.GetChild(0).GetChild(2).transform.position, this, transform.Find("Custom Bridge").gameObject, false, customBridge, startExtraWidthLeft, startExtraWidthRight, endExtraWidthLeft, endExtraWidthRight);
                }
            }
        }
    }

    private void SetGuidelines(Vector3[] currentPoints, Vector3[] nextPoints, bool first)
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        // Start Guidelines
        Vector3 left;
        float roadGuidelinesLength = settings.FindProperty("roadGuidelinesLength").floatValue;

        if (roadGuidelinesLength > 0)
        {
            if (first == true)
            {
                left = Misc.CalculateLeft(currentPoints[0], currentPoints[1]);
                startGuidelinePoints = new RoadGuideline(transform.GetChild(0).GetChild(0).position + left * roadGuidelinesLength, transform.GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(0).position - left * roadGuidelinesLength);
            }

            // Center Guidelines
            if (currentPoints.Length > 3)
            {
                left = Misc.CalculateLeft(currentPoints[(currentPoints.Length + 1) / 2], currentPoints[(currentPoints.Length + 1) / 2 + 1]);
                centerGuidelinePoints = new RoadGuideline(transform.GetChild(0).GetChild(1).position + left * roadGuidelinesLength, transform.GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(1).position - left * roadGuidelinesLength);
            }

            // End guidelines
            if (nextPoints == null)
            {
                left = Misc.CalculateLeft(currentPoints[currentPoints.Length - 2], currentPoints[currentPoints.Length - 1]);
            }
            else if (nextPoints.Length > 1)
            {
                left = Misc.CalculateLeft(currentPoints[currentPoints.Length - 1], nextPoints[1]);
            }
            else
            {
                endGuidelinePoints = null;
                return;
            }

            endGuidelinePoints = new RoadGuideline(transform.GetChild(0).GetChild(2).position + left * roadGuidelinesLength, transform.GetChild(0).GetChild(2).position, transform.GetChild(0).GetChild(2).position - left * roadGuidelinesLength);
        }
        else
        {
            startGuidelinePoints = null;
            centerGuidelinePoints = null;
            endGuidelinePoints = null;
        }
    }

    private void GenerateMesh(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 previousPoint, Vector3[] previousVertices, float heightOffset, Transform segment, Transform previousSegment, Transform mesh, string name, Material baseMaterial, Material overlayMaterial, PhysicMaterial physicMaterial, float startXOffset = 0, float endXOffset = 0, float startWidth = 0, float endWidth = 0, float yOffset = 0, float leftYOffset = 0, bool extraMeshLeft = true)
    {
        if (name != "Road" && startWidth == 0 && endWidth == 0)
        {
            mesh.GetComponent<MeshFilter>().sharedMesh = null;
            mesh.GetComponent<MeshCollider>().sharedMesh = null;
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int verticeIndex = 0;
        float totalDistance = 0;
        float currentDistance = 0;

        for (int i = 1; i < points.Length; i++)
        {
            totalDistance += Vector3.Distance(points[i - 1], points[i]);
        }

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 left = Misc.CalculateLeft(points, nextSegmentPoints, previousPoint, i);
            float correctedHeightOffset = heightOffset;

            if (i > 0)
            {
                currentDistance += Vector3.Distance(points[i - 1], points[i]);
            }

            float roadWidth = Mathf.Lerp(startRoadWidth, endRoadWidth, currentDistance / totalDistance);
            if (i == 0 && previousSegment != null)
            {
                roadWidth = previousSegment.GetComponent<RoadSegment>().endRoadWidth;
            }

            if (i == 0 && previousPoint != Misc.MaxVector3)
            {
                correctedHeightOffset = previousPoint.y - points[i].y;
            }
            else if (i == points.Length - 1 && nextSegmentPoints != null && nextSegmentPoints.Length == 1)
            {
                correctedHeightOffset = nextSegmentPoints[0].y - points[i].y;
            }

            if (name == "Road")
            {
                vertices.Add((points[i] + left * roadWidth) - segment.position);
                vertices[verticeIndex] = new Vector3(vertices[verticeIndex].x, correctedHeightOffset + points[i].y - segment.position.y, vertices[verticeIndex].z);
                vertices.Add((points[i] - left * roadWidth) - segment.position);
                vertices[verticeIndex + 1] = new Vector3(vertices[verticeIndex + 1].x, correctedHeightOffset + points[i].y - segment.position.y, vertices[verticeIndex + 1].z);
            }
            else
            {
                float modifiedXOffset = Mathf.Lerp(startXOffset, endXOffset, currentDistance / totalDistance) + roadWidth;
                float width = Mathf.Lerp(startWidth, endWidth, currentDistance / totalDistance);

                if (extraMeshLeft == false)
                {
                    vertices.Add((points[i] + left * -modifiedXOffset) - segment.position);
                    vertices[verticeIndex] = new Vector3(vertices[verticeIndex].x, correctedHeightOffset + leftYOffset + points[i].y - segment.position.y - heightOffset, vertices[verticeIndex].z);
                    vertices.Add((points[i] + left * (-modifiedXOffset - width)) - segment.position);
                    vertices[verticeIndex + 1] = new Vector3(vertices[verticeIndex + 1].x, correctedHeightOffset + yOffset + points[i].y - segment.position.y - heightOffset, vertices[verticeIndex + 1].z);
                }
                else
                {
                    vertices.Add((points[i] + left * (modifiedXOffset + width)) - segment.position);
                    vertices[verticeIndex] = new Vector3(vertices[verticeIndex].x, correctedHeightOffset + yOffset + points[i].y - segment.position.y - heightOffset, vertices[verticeIndex].z);
                    vertices.Add((points[i] + left * modifiedXOffset) - segment.position);
                    vertices[verticeIndex + 1] = new Vector3(vertices[verticeIndex + 1].x, correctedHeightOffset + leftYOffset + points[i].y - segment.position.y - heightOffset, vertices[verticeIndex + 1].z);
                }
            }

            if (i < points.Length - 1)
            {
                triangles.Add(verticeIndex);
                triangles.Add(verticeIndex + 2);
                triangles.Add(verticeIndex + 1);

                triangles.Add(verticeIndex + 1);
                triangles.Add(verticeIndex + 2);
                triangles.Add(verticeIndex + 3);
            }

            verticeIndex += 2;
        }

        TerrainDeformation(vertices, points, heightOffset, segment);

        // First
        if (previousVertices != null)
        {
            if (vertices.Count > 4 && previousVertices.Length > 3 && name == "Road")
            {
                vertices = fixVertices(0, vertices, (vertices[2] - vertices[4]).normalized);
                vertices = fixVertices(1, vertices, (vertices[3] - vertices[5]).normalized);
            }
        }

        Mesh generatedMesh = new Mesh();
        generatedMesh.vertices = vertices.ToArray();
        generatedMesh.triangles = triangles.ToArray();

        if (name == "Road")
        {
            generatedMesh = GenerateUvs(generatedMesh, flipped);
        }
        else
        {
            generatedMesh = GenerateUvs(generatedMesh, extraMeshLeft);
        }

        generatedMesh.RecalculateNormals();
        mesh.GetComponent<MeshFilter>().sharedMesh = generatedMesh;
        mesh.GetComponent<MeshCollider>().sharedMaterial = physicMaterial;

        if (segment.parent.parent.GetComponent<RoadCreator>().generateCollider == true)
        {
            mesh.GetComponent<MeshCollider>().sharedMesh = generatedMesh;
        }
        else
        {
            mesh.GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (overlayMaterial == null)
        {
            mesh.GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial };
        }
        else
        {
            mesh.GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, overlayMaterial };
        }
    }

    private void TerrainDeformation(List<Vector3> vertices, Vector3[] points, float heightOffset, Transform segment)
    {
        // Terrain deformation
        if (terrainOption == TerrainOption.deform)
        {
            float[,] modifiedHeights;
            RaycastHit raycastHit;
            if (Physics.Raycast(points[0] + new Vector3(0, 100, 0), Vector3.down, out raycastHit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Road") | 1 << LayerMask.NameToLayer("Ignore Mouse Ray"))))
            {
                Terrain terrain = raycastHit.collider.GetComponent<Terrain>();
                if (terrain != null)
                {
                    TerrainData terrainData = terrain.terrainData;
                    modifiedHeights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
                    float zDivisions = Vector3.Distance(points[0], points[1]);

                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector3 forward = (vertices[i * 2 + 1] - vertices[i * 2]).normalized;

                        for (float offset = 0; offset < 1; offset += 1 / zDivisions)
                        {
                            Vector3 leftVertex = vertices[i * 2];
                            Vector3 rightVertex = vertices[i * 2 + 1];
                            if (i > 0)
                            {
                                leftVertex = Vector3.Lerp(vertices[(i - 1) * 2], vertices[i * 2], offset);
                                rightVertex = Vector3.Lerp(vertices[(i - 1) * 2 + 1], vertices[i * 2 + 1], offset);
                            }

                            float roadWidth = Mathf.Lerp(startRoadWidth, endRoadWidth, (i / points.Length));
                            float divisions = Mathf.Max(2f, roadWidth * 10);

                            for (float t = 0; t <= 1; t += 1f / divisions)
                            {
                                Vector3 position = Vector3.Lerp(rightVertex + forward * 2f - new Vector3(0, heightOffset, 0) + segment.transform.position, leftVertex - forward * 2f - new Vector3(0, heightOffset, 0) + segment.transform.position, t);
                                Vector3 localTerrainPoint = position - terrain.transform.position;

                                int terrainPointX = (int)((localTerrainPoint.x / terrainData.size.x) * terrainData.heightmapWidth);
                                float terrainPointY = position.y / terrainData.size.y;
                                int terrainPointZ = (int)((localTerrainPoint.z / terrainData.size.z) * terrainData.heightmapHeight);

                                if (terrainPointX > terrainData.heightmapWidth || terrainPointZ > terrainData.heightmapHeight)
                                {
                                    continue;
                                }

                                modifiedHeights[terrainPointZ, terrainPointX] = Mathf.Clamp01(terrainPointY);
                            }
                        }
                    }

                    terrainData.SetHeights(0, 0, modifiedHeights);
                }
            }
        }
    }

    private List<Vector3> fixVertices(int position, List<Vector3> vertices, Vector3 forward)
    {
        int startVertex = 0;
        for (int i = position; startVertex == 0; i += 2)
        {
            if (i > vertices.Count - 1)
            {
                return vertices;
            }

            float direction = Vector3.Dot(forward.normalized, (vertices[position] - vertices[i]).normalized);
            if (direction >= 0)
            {
                startVertex = i;
            }
        }

        int amount = Mathf.Abs(startVertex - position) / 2;
        float part = 1f / amount;
        float index = 0;

        for (int i = startVertex; index < amount * 2; i -= 2)
        {
            vertices[i] = Vector3.Lerp(vertices[startVertex], vertices[position], part * index);
            index += 2;
        }

        return vertices;
    }

    private Mesh GenerateUvs(Mesh mesh, bool left)
    {
        Vector2[] uvs = new Vector2[mesh.vertices.Length];
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

            if (left == false)
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

            if (left == false)
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
#endif