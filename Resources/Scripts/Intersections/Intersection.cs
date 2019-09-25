#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Intersections")]
public class Intersection : MonoBehaviour
{

    public Material baseMaterial;
    public Material overlayMaterial;
    public PhysicMaterial physicMaterial;
    public List<IntersectionConnection> connections = new List<IntersectionConnection>();
    public float yOffset = 0.02f;

    public SerializedObject settings;

    public GameObject objectToMove;
    public bool stretchTexture = false;
    public float textureScale = 1;
    public float resolutionMultiplier = 1;
    public bool resetCurvePointsOnUpdate = true;

    public List<MainRoad> mainRoads = new List<MainRoad>();

    public bool generateBridge = true;
    public BridgeSettings bridgeSettings = new BridgeSettings();

    public bool placePillars = true;
    public GameObject pillarPrefab;
    public float extraPillarHeight = 0.2f;
    public float xzPillarScale = 3;

    public bool outerExtraMeshesAsRoads = false;
    public List<ExtraMesh> extraMeshes = new List<ExtraMesh>();

    // Roundabout
    public bool roundaboutMode = false;
    public float roundaboutRadius = 5f;
    public float roundaboutWidth = 2f;
    public float maxRoundaboutRadius = 100;
    public Material connectionBaseMaterial;
    public Material connectionOverlayMaterial;
    public Material connectionSectionMaterial;
    public float textureTilingY = 1;

    public float pillarGap = 5;
    public float pillarPlacementOffset = 5;
    public float xPillarScale = 1;
    public float zPillarScale = 1;
    public PrefabLineCreator.RotationDirection pillarRotationDirection;

    public void Setup()
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();
        gameObject.GetComponent<MeshFilter>().hideFlags = HideFlags.NotEditable;
        gameObject.GetComponent<MeshCollider>().hideFlags = HideFlags.NotEditable;
        gameObject.GetComponent<MeshRenderer>().hideFlags = HideFlags.NotEditable;

        GameObject extraMeshes = new GameObject("Extra Meshes");
        extraMeshes.transform.SetParent(transform, false);

        if (settings.FindProperty("hideNonEditableChildren").boolValue == true)
        {
            extraMeshes.hideFlags = HideFlags.HideInHierarchy;
        }
        else
        {
            extraMeshes.hideFlags = HideFlags.NotEditable;
        }

        if (roundaboutMode == true)
        {
            CreateMesh();
        }
    }

    public void MovePoints(RaycastHit raycastHit, Vector3 position, Event currentEvent, bool curvePoint)
    {
        if (currentEvent.type == EventType.MouseDown)
        {
            if (currentEvent.button == 0)
            {
                if (objectToMove == null)
                {
                    bool isConnectedPoint = false;
                    if (curvePoint == false && (raycastHit.transform.name == "Start Point" || raycastHit.transform.name == "End Point") && raycastHit.transform.GetComponent<Point>().roadPoint == true)
                    {
                        for (int i = 0; i < connections.Count; i++)
                        {
                            if (connections[i].road == raycastHit.transform.GetComponent<Point>())
                            {
                                isConnectedPoint = true;
                                break;
                            }
                        }
                    }

                    if (curvePoint == true && (raycastHit.transform.name == "Connection Point" && raycastHit.transform.parent.gameObject == Selection.activeGameObject && resetCurvePointsOnUpdate == false) || isConnectedPoint == true)
                    {
                        if (raycastHit.transform.GetComponent<BoxCollider>().enabled == false)
                        {
                            return;
                        }

                        objectToMove = raycastHit.transform.gameObject;
                        objectToMove.GetComponent<BoxCollider>().enabled = false;
                    }
                }
            }
            else if (currentEvent.button == 1)
            {
                // Reset Point
                if (raycastHit.transform.name == "Connection Point" && curvePoint == true)
                {
                    int currentIndex = raycastHit.transform.GetSiblingIndex() - 1;
                    int nextIndex = raycastHit.transform.GetSiblingIndex();

                    if (nextIndex >= connections.Count)
                    {
                        nextIndex = 0;
                    }

                    if (roundaboutMode == true)
                    {
                        if (currentIndex % 2 == 0)
                        {
                            connections[currentIndex / 2].curvePoint = connections[currentIndex / 2].defaultCurvePoint;
                        }
                        else
                        {
                            connections[(currentIndex - 1) / 2].curvePoint2 = connections[(currentIndex - 1) / 2].defaultCurvePoint2;
                        }
                    }
                    else
                    {
                        ResetCurvePointPosition(currentIndex);
                    }

                    CreateCurvePoints();
                    CreateMesh();
                }
            }
        }
        else if (currentEvent.type == EventType.MouseDrag && objectToMove != null && ((curvePoint == true && objectToMove.name == "Connection Point") || curvePoint == false))
        {
            Undo.RecordObject(objectToMove.transform, "Moved Point");
            objectToMove.transform.position = position;
        }
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && objectToMove != null)
        {
            DropMovingPoint();
        }
    }

    private void DropMovingPoint()
    {
        objectToMove.GetComponent<BoxCollider>().enabled = true;

        if (objectToMove.transform.name == "Connection Point")
        {
            int nextIndex = objectToMove.transform.GetSiblingIndex();
            if (nextIndex >= connections.Count)
            {
                nextIndex = 0;
            }

            if (roundaboutMode == true)
            {
                int index = objectToMove.transform.GetSiblingIndex() - 1;

                if (index % 2 == 0)
                {
                    connections[index / 2].curvePoint = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
                }
                else
                {
                    connections[(index - 1) / 2].curvePoint2 = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
                }
            }
            else
            {
                connections[objectToMove.transform.GetSiblingIndex() - 1].curvePoint = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
            }

            objectToMove = null;
            CreateMesh();
            CreateCurvePoints();
        }
        else
        {
            if (objectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
            {
                objectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(objectToMove.transform.parent.GetChild(0).position, objectToMove.transform.parent.GetChild(2).position);
            }

            CreateMesh();
            objectToMove.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();

            if (objectToMove.transform.name == "Start Point")
            {
                objectToMove.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().UpdateStartConnectionData();
            }
            else
            {
                objectToMove.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().UpdateEndConnectionData();
            }

            objectToMove = null;
            Roundabout.UpdateMaxRadius(this);
        }
    }

    public void CreateMesh(bool fromRoad = false)
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].road == null)
            {
                connections.RemoveAt(i);
            }
        }

        if (connections.Count < 2 && roundaboutMode == false)
        {
            RemoveIntersection();
        }
        else
        {
            CheckMaterialsAndPrefabs();

            if (resetCurvePointsOnUpdate == true)
            {
                ResetCurvePointPositions();
            }

            if (roundaboutMode == true)
            {
                Roundabout.GenerateRoundabout(this);
            }
            else
            {
                if (outerExtraMeshesAsRoads == true)
                {
                    ResetExtraMeshes();
                    CreateExtraMeshesFromRoads();
                }

                GenerateMesh();
            }
        }

        if (fromRoad == false)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                RoadCreator roadCreator = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>();
                roadCreator.CreateMesh(true);
            }
        }
    }

    private void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> firstVertexIndexes = new List<int>();
        float[] totalLengths = new float[connections.Count];
        float[] exactLengths = new float[connections.Count];
        int vertexIndex = 0;
        Vector3 lastVertexPosition = Misc.MaxVector3;

        List<Vector3> mainRoadsVertices = new List<Vector3>();
        List<List<int>> mainRoadsTriangles = new List<List<int>>();
        List<Vector2> mainRoadsUvs = new List<Vector2>();
        List<float> lengths = new List<float>();

        for (int i = 0; i < connections.Count; i++)
        {
            Vector3 firstPoint = connections[i].leftPoint;
            Vector3 firstCenterPoint = connections[i].lastPoint;
            Vector3 nextPoint;
            Vector3 nextCenterPoint;
            totalLengths[i] = connections[i].length;
            firstVertexIndexes.Add(vertexIndex);
            mainRoadsTriangles.Add(new List<int>());

            if (i == connections.Count - 1)
            {
                nextPoint = connections[0].rightPoint;
                nextCenterPoint = connections[0].lastPoint;
                totalLengths[i] += connections[0].length;
            }
            else
            {
                nextPoint = connections[i + 1].rightPoint;
                nextCenterPoint = connections[i + 1].lastPoint;
                totalLengths[i] += connections[i + 1].length;
            }

            if (connections[i].curvePoint == null)
            {
                return;
            }

            float segments = totalLengths[i] * settings.FindProperty("resolution").floatValue * resolutionMultiplier * 5;
            segments = Mathf.Max(3, segments);
            float distancePerSegment = 1f / segments;

            for (float t = 0; t <= 1; t += 0.1f)
            {
                Vector3 pos = Misc.Lerp3(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f), Vector3.one, t);
            }

            for (float t = 0; t <= 1 + distancePerSegment; t += distancePerSegment)
            {
                float modifiedT = t;
                if (Mathf.Abs(0.5f - t) < distancePerSegment && t > 0.5f)
                {
                    modifiedT = 0.5f;
                }

                if (t > 1)
                {
                    modifiedT = 1;
                }

                vertices.Add(Misc.Lerp3CenterHeight(firstPoint, connections[i].curvePoint, nextPoint, modifiedT) + new Vector3(0, yOffset, 0) - transform.position);

                if (t > 0)
                {
                    exactLengths[i] += Vector3.Distance(lastVertexPosition, vertices[vertices.Count - 1]);
                    lastVertexPosition = vertices[vertices.Count - 1];
                }
                else
                {
                    lastVertexPosition = vertices[vertices.Count - 1];
                }

                if (modifiedT < 0.5f)
                {
                    Vector3 point = Vector3.Lerp(firstCenterPoint, transform.position, modifiedT * 2) - transform.position;
                    point.y = Mathf.Lerp(firstPoint.y, nextPoint.y, modifiedT) - transform.position.y + yOffset;
                    vertices.Add(point);
                }
                else
                {
                    Vector3 point = Vector3.Lerp(transform.position, nextCenterPoint, 2 * (modifiedT - 0.5f)) - transform.position;
                    point.y = Mathf.Lerp(firstPoint.y, nextPoint.y, modifiedT) - transform.position.y + yOffset;
                    vertices.Add(point);
                }

                uvs.Add(new Vector2(0, modifiedT));

                if (stretchTexture == true)
                {
                    uvs.Add(new Vector2(1, modifiedT));
                }
                else
                {
                    uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 1], vertices[vertices.Count - 2]) / textureScale, modifiedT));
                }

                if (t < 1)
                {
                    triangles = AddTriangles(triangles, vertexIndex);
                }

                vertexIndex += 2;
            }
        }

        if (connections.Count > 2)
        {
            for (int i = 0; i < mainRoads.Count; i++)
            {
                Vector3 startForward = Misc.CalculateLeft(connections[mainRoads[i].startIndex].rightPoint - connections[mainRoads[i].startIndex].leftPoint);
                Vector3 endForward = Misc.CalculateLeft(connections[mainRoads[i].endIndex].rightPoint - connections[mainRoads[i].endIndex].leftPoint);
                Vector3 leftControlPoint = Misc.GetLineIntersection(connections[mainRoads[i].startIndex].leftPoint, startForward, connections[mainRoads[i].endIndex].rightPoint, endForward);
                Vector3 rightControlPoint = Misc.GetLineIntersection(connections[mainRoads[i].startIndex].rightPoint, startForward, connections[mainRoads[i].endIndex].leftPoint, endForward);

                if (leftControlPoint == Misc.MaxVector3)
                {
                    leftControlPoint = Misc.GetCenter(connections[mainRoads[i].startIndex].leftPoint, connections[mainRoads[i].endIndex].rightPoint);
                }

                if (rightControlPoint == Misc.MaxVector3)
                {
                    rightControlPoint = Misc.GetCenter(connections[mainRoads[i].startIndex].rightPoint, connections[mainRoads[i].endIndex].leftPoint);
                }

                float leftSegments = Misc.CalculateDistance(connections[mainRoads[i].startIndex].leftPoint, leftControlPoint, connections[mainRoads[i].endIndex].rightPoint) * settings.FindProperty("resolution").floatValue * resolutionMultiplier * 5;
                float rightSegments = Misc.CalculateDistance(connections[mainRoads[i].startIndex].rightPoint, rightControlPoint, connections[mainRoads[i].endIndex].leftPoint) * settings.FindProperty("resolution").floatValue * resolutionMultiplier * 5;
                float segments = Mathf.Max(leftSegments, rightSegments);
                lengths.Add(Misc.CalculateDistance(connections[mainRoads[i].startIndex].lastPoint, leftControlPoint, connections[mainRoads[i].endIndex].lastPoint));
                segments = Mathf.Max(3, segments);
                float distancePerSegment = 1f / segments;
                vertexIndex = vertices.Count + mainRoadsVertices.Count;

                for (float t = 0; t <= 1 + distancePerSegment; t += distancePerSegment)
                {
                    float modifiedT = t;

                    if (t > 1)
                    {
                        modifiedT = 1;
                    }

                    mainRoadsVertices.Add(Misc.Lerp3(connections[mainRoads[i].startIndex].leftPoint, leftControlPoint, connections[mainRoads[i].endIndex].rightPoint, modifiedT) - transform.position + new Vector3(0, 0.03f, 0));
                    mainRoadsVertices.Add(Misc.Lerp3(connections[mainRoads[i].startIndex].rightPoint, rightControlPoint, connections[mainRoads[i].endIndex].leftPoint, modifiedT) - transform.position + new Vector3(0, 0.03f, 0));

                    if (mainRoads[i].flipTexture == true)
                    {
                        mainRoadsUvs.Add(new Vector2(1, modifiedT));
                        mainRoadsUvs.Add(new Vector2(0, modifiedT));
                    }
                    else
                    {
                        mainRoadsUvs.Add(new Vector2(0, modifiedT));
                        mainRoadsUvs.Add(new Vector2(1, modifiedT));
                    }

                    if (t < 1)
                    {
                        mainRoadsTriangles[i].Add(vertexIndex);
                        mainRoadsTriangles[i].Add(vertexIndex + 2);
                        mainRoadsTriangles[i].Add(vertexIndex + 1);

                        mainRoadsTriangles[i].Add(vertexIndex + 1);
                        mainRoadsTriangles[i].Add(vertexIndex + 2);
                        mainRoadsTriangles[i].Add(vertexIndex + 3);
                    }

                    vertexIndex += 2;
                }
            }

            vertices.AddRange(mainRoadsVertices.ToArray());
            uvs.AddRange(mainRoadsUvs.ToArray());
        }

        SetupMesh(vertices, triangles, uvs, mainRoadsTriangles, lengths);

        float[] startWidths = new float[firstVertexIndexes.Count];
        float[] endWidths = new float[firstVertexIndexes.Count];
        float[] heights = new float[firstVertexIndexes.Count];

        GenerateExtraMeshes(firstVertexIndexes, vertices, exactLengths, totalLengths, ref startWidths, ref endWidths, ref heights);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name == "Bridge")
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
                break;
            }
        }

        if (generateBridge == true)
        {
            BridgeGeneration.GenerateSimpleBridgeIntersection(GetComponent<MeshFilter>().sharedMesh.vertices, this, bridgeSettings.bridgeMaterials, startWidths, endWidths, firstVertexIndexes.ToArray());
        }

        CreateCurvePoints();
    }

    private void SetupMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<List<int>> mainRoadsTriangles, List<float> lengths)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.subMeshCount = 4;

        int nextIndex = 1;
        if (overlayMaterial != null)
        {
            mesh.SetTriangles(triangles.ToArray(), 1);
            nextIndex = 2;
        }

        for (int i = 0; i < mainRoads.Count; i++)
        {
            mesh.SetTriangles(mainRoadsTriangles[i].ToArray(), nextIndex);
            nextIndex += 1;
        }

        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.subMeshCount = nextIndex + 1;
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMaterial = physicMaterial;

        List<Material> materials = new List<Material>();
        materials.Add(baseMaterial);

        if (overlayMaterial != null)
        {
            materials.Add(overlayMaterial);
        }

        for (int i = 0; i < mainRoads.Count; i++)
        {
            materials.Add(mainRoads[i].material);

            // texture offset
            Material lastMaterial = null;
            float textureRepeat = 1;

            if (connections[mainRoads[i].startIndex].road.name == "End Point")
            {
                lastMaterial = connections[mainRoads[i].startIndex].road.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().sharedMaterials[1];
                textureRepeat = lengths[i] / 4 * connections[mainRoads[i].startIndex].road.transform.parent.parent.GetComponent<RoadSegment>().textureTilingY;
            }
            else if (connections[mainRoads[i].endIndex].road.name == "End Point")
            {
                lastMaterial = connections[mainRoads[i].endIndex].road.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().sharedMaterials[1];
                textureRepeat = lengths[i] / 4 * connections[mainRoads[i].endIndex].road.transform.parent.parent.GetComponent<RoadSegment>().textureTilingY;
            }

            if (lastMaterial != null)
            {
                float lastTextureRepeat = lastMaterial.GetVector("_Tiling").y;
                float lastTextureOffset = lastMaterial.GetVector("_Offset").y;

                Material material = Instantiate(mainRoads[i].material);
                material.SetTextureScale("_BaseMap", new Vector2(1, textureRepeat));
                material.SetTextureOffset("_BaseMap", new Vector2(0, (lastTextureRepeat % 1.0f) + lastTextureOffset));
                material.mainTextureScale = new Vector2(1, textureRepeat);
                material.mainTextureOffset = new Vector2(0, (lastTextureRepeat % 1.0f) + lastTextureOffset);
                mainRoads[i].material = material;
            }
        }

        GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
    }

    private void GenerateExtraMeshes(List<int> firstVertexIndexes, List<Vector3> vertices, float[] exactLengths, float[] totalLengths, ref float[] startWidths, ref float[] endWidths, ref float[] heights)
    {
        for (int i = 0; i < extraMeshes.Count; i++)
        {
            if (extraMeshes[i].startWidth == 0 && extraMeshes[i].endWidth == 0 && extraMeshes[i].yOffset == 0)
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshFilter>().sharedMesh = null;
                transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMesh = null;
                continue;
            }

            int endVertexIndex;
            float currentLength = 0f;
            Vector3 lastPosition = vertices[firstVertexIndexes[extraMeshes[i].index] + 1];

            if (extraMeshes[i].index < firstVertexIndexes.Count - 1)
            {
                endVertexIndex = firstVertexIndexes[extraMeshes[i].index + 1];
            }
            else
            {
                endVertexIndex = vertices.Count;
            }

            int startIndex = firstVertexIndexes[extraMeshes[i].index];
            Vector3 startLeft = (vertices[firstVertexIndexes[extraMeshes[i].index]] - vertices[firstVertexIndexes[extraMeshes[i].index] + 1]).normalized;
            Vector3 endLeft = (vertices[endVertexIndex - 2] - vertices[endVertexIndex - 1]).normalized;
            Vector3 overlapPosition = Misc.GetLineIntersection(vertices[startIndex] + transform.position, startLeft, vertices[endVertexIndex - 2] + transform.position, endLeft, extraMeshes[i].startWidth, extraMeshes[i].endWidth);

            List<Vector3> extraMeshVertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector2> uvs2 = new List<Vector2>();
            int vertexIndex = 0;

            for (int j = startIndex; j < endVertexIndex; j += 2)
            {
                currentLength += Vector3.Distance(lastPosition, vertices[j + 1]);

                Vector3 forward = (vertices[j + 1] - vertices[j]).normalized;
                Vector3 vertex = vertices[j] - forward * (Mathf.Lerp(extraMeshes[i].startWidth, extraMeshes[i].endWidth, currentLength / totalLengths[extraMeshes[i].index]) + Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLengths[extraMeshes[i].index]));
                if (overlapPosition != Misc.MaxVector3)
                {
                    extraMeshVertices.Add(overlapPosition - transform.position);
                }
                else if (j > startIndex && j < endVertexIndex - 1)
                {
                    float currentWidth = Mathf.Lerp(extraMeshes[i].startWidth, extraMeshes[i].endWidth, currentLength / totalLengths[extraMeshes[i].index]);
                    Vector3 localOverlapPosition = Misc.GetLineIntersection(vertex + transform.position, forward, vertices[startIndex] + transform.position, startLeft, currentWidth, extraMeshes[i].startWidth);
                    if (localOverlapPosition != Misc.MaxVector3)
                    {
                        extraMeshVertices.Add(localOverlapPosition - transform.position);
                    }
                    else
                    {
                        localOverlapPosition = Misc.GetLineIntersection(vertex + transform.position, forward, vertices[endVertexIndex - 2] + transform.position, endLeft, currentWidth, extraMeshes[i].endWidth);
                        if (localOverlapPosition != Misc.MaxVector3)
                        {
                            extraMeshVertices.Add(localOverlapPosition - transform.position);
                        }
                        else
                        {
                            extraMeshVertices.Add(vertex);
                        }
                    }
                } else
                {
                    extraMeshVertices.Add(vertex);
                }

                extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, extraMeshes[i].yOffset + heights[extraMeshes[i].index], 0);
                extraMeshVertices.Add(vertices[j] - forward * Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLengths[extraMeshes[i].index]));
                extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, heights[extraMeshes[i].index], 0);

                if (extraMeshes[i].flipped == false)
                {
                    uvs.Add(new Vector2(0, (currentLength / exactLengths[extraMeshes[i].index])));
                    uvs.Add(new Vector2(1, (currentLength / exactLengths[extraMeshes[i].index])));
                }
                else
                {
                    uvs.Add(new Vector2(1, (currentLength / exactLengths[extraMeshes[i].index])));
                    uvs.Add(new Vector2(0, (currentLength / exactLengths[extraMeshes[i].index])));
                }

                uvs2.Add(Vector2.one);
                uvs2.Add(Vector2.one);

                if (j < endVertexIndex - 2)
                {
                    triangles = AddTriangles(triangles, vertexIndex);
                    vertexIndex += 2;
                }

                lastPosition = vertices[j + 1];
            }

            Material material = Instantiate(extraMeshes[i].baseMaterial);
            material.SetVector("_Tiling", new Vector2(1, exactLengths[extraMeshes[i].index] / 2));
            material.mainTextureScale = new Vector2(1, exactLengths[extraMeshes[i].index] / 2);
            extraMeshes[i].baseMaterial = material;

            if (extraMeshes[i].overlayMaterial != null)
            {
                material = Instantiate(extraMeshes[i].overlayMaterial);
                material.SetVector("_Tiling", new Vector2(1, exactLengths[extraMeshes[i].index] / 2));
                material.mainTextureScale = new Vector2(1, exactLengths[extraMeshes[i].index] / 2);
                extraMeshes[i].baseMaterial = overlayMaterial;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = extraMeshVertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.uv2 = uvs2.ToArray();
            mesh.RecalculateNormals();
            vertexIndex = 0;

            transform.GetChild(0).GetChild(i).GetComponent<MeshFilter>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMaterial = extraMeshes[i].physicMaterial;

            if (extraMeshes[i].overlayMaterial == null)
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].baseMaterial };
            }
            else
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].baseMaterial, extraMeshes[i].overlayMaterial };
            }

            startWidths[extraMeshes[i].index] += extraMeshes[i].startWidth;
            endWidths[extraMeshes[i].index] += extraMeshes[i].endWidth;
            heights[extraMeshes[i].index] += extraMeshes[i].yOffset;
        }
    }

    private void RemoveIntersection()
    {
        RoadCreator[] roads = GameObject.FindObjectsOfType<RoadCreator>();

        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i].startIntersection == this)
            {
                Undo.RecordObject(roads[i], "Remove Intersection");
                roads[i].startIntersection = null;
                roads[i].startIntersectionConnection = null;
            }
            else if (roads[i].endIntersection == this)
            {
                Undo.RecordObject(roads[i], "Remove Intersection");
                roads[i].endIntersection = null;
                roads[i].endIntersectionConnection = null;
            }
        }

        Undo.DestroyObjectImmediate(gameObject);
    }

    private void CheckMaterialsAndPrefabs()
    {
        if (baseMaterial == null)
        {
            baseMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
        }

        if (connectionBaseMaterial == null)
        {
            connectionBaseMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
        }

        if (connectionSectionMaterial == null)
        {
            connectionSectionMaterial = (Material)settings.FindProperty("defaultRoundaboutConnectionSectionsMaterial").objectReferenceValue;
        }

        for (int i = 0; i < mainRoads.Count; i++)
        {
            if (mainRoads[i].material == null)
            {
                mainRoads[i].material = (Material)settings.FindProperty("defaultIntersectionMainRoadMaterial").objectReferenceValue;
            }
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

        if (pillarPrefab == null || pillarPrefab.GetComponent<MeshFilter>() == null)
        {
            pillarPrefab = (GameObject)settings.FindProperty("defaultBridgePillarPrefab").objectReferenceValue;
        }
    }

    public void CreateCurvePoints()
    {
        RemoveCurvePoints();

        for (int i = 0; i < connections.Count; i++)
        {
            CreateCurvePoint(new Vector3(connections[i].curvePoint.x, yOffset + transform.position.y, connections[i].curvePoint.z));

            if (roundaboutMode == true)
            {
                CreateCurvePoint(new Vector3(connections[i].curvePoint2.x, yOffset + transform.position.y, connections[i].curvePoint2.z));
            }
        }

        if (transform.Find("Bridge") != null)
        {
            transform.Find("Bridge").SetAsLastSibling();
        }
    }

    private void CreateCurvePoint(Vector3 position)
    {
        GameObject curvePoint = null;
        curvePoint = new GameObject("Connection Point");
        curvePoint.transform.SetParent(transform);
        curvePoint.layer = LayerMask.NameToLayer("Intersection");
        curvePoint.AddComponent<BoxCollider>();
        curvePoint.GetComponent<BoxCollider>().size = new Vector3(settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue);
        curvePoint.transform.position = position;

        if (settings.FindProperty("hideNonEditableChildren").boolValue == true)
        {
            curvePoint.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    public void ResetCurvePointPositions()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            ResetCurvePointPosition(i);
        }
    }

    private void ResetCurvePointPosition(int i)
    {
        int nextIndex = i + 1;
        if (nextIndex >= connections.Count)
        {
            nextIndex = 0;
        }

        if (roundaboutMode == true)
        {
            ResetConnectionCurvePoints(connections[i]);
        }
        else
        {
            Vector3 forwardCurrentConnection = Misc.CalculateLeft(connections[i].rightPoint - connections[i].leftPoint);
            Vector3 forwardNextConnection = Misc.CalculateLeft(connections[nextIndex].rightPoint - connections[nextIndex].leftPoint);
            Vector3 lineIntersection = Misc.GetLineIntersection(connections[i].leftPoint, forwardCurrentConnection, connections[nextIndex].rightPoint, forwardNextConnection);

            if (lineIntersection != Misc.MaxVector3)
            {
                connections[i].curvePoint = lineIntersection;
            }
            else
            {
                float angleDifference = Vector3.Angle(forwardCurrentConnection, forwardNextConnection);

                if (angleDifference < 90f)
                {
                    // When the connections are nearly pararell
                    connections[i].curvePoint = Misc.GetCenter(connections[i].leftPoint, connections[nextIndex].rightPoint) + forwardCurrentConnection * Vector3.Distance(connections[i].leftPoint, connections[nextIndex].rightPoint);
                }
                else
                {
                    connections[i].curvePoint = Misc.GetCenter(connections[i].leftPoint, connections[nextIndex].rightPoint);
                }
            }
        }
    }

    public Vector3 RecalculateDefaultRoundaboutCurvePoints(Vector3 point1, Vector3 direction1, Vector3 point2, Vector3 direction2)
    {
        Vector3 lineIntersection = Misc.GetLineIntersection(point1, direction1, point2, direction2);

        if (lineIntersection != Misc.MaxVector3)
        {
            return lineIntersection;
        }
        else
        {
            return Misc.GetCenter(point1, point2);
        }
    }

    public void ResetConnectionCurvePoints(IntersectionConnection intersectionConnection)
    {
        intersectionConnection.curvePoint = intersectionConnection.defaultCurvePoint;
        intersectionConnection.curvePoint2 = intersectionConnection.defaultCurvePoint2;
    }

    public void RemoveCurvePoints()
    {
        if (gameObject != null)
        {
            if (transform.childCount > 0)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    if (transform.GetChild(i).name == "Connection Point")
                    {
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    }
                }
            }
        }
    }

    private List<int> AddTriangles(List<int> triangles, int vertexIndex)
    {
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);

        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);

        return triangles;
    }

    public void FixConnectionReferences()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].road != null)
            {
                if (connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection != null && connections[i].road.Equals(connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection.road))
                {
                    connections[i] = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection;
                }
                else if (connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnection != null)
                {
                    connections[i] = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnection;
                }
            }
        }
    }

    public void CreateExtraMesh()
    {
        GameObject extraMesh = new GameObject("Extra Mesh");
        extraMesh.AddComponent<MeshFilter>();
        extraMesh.AddComponent<MeshRenderer>();
        extraMesh.AddComponent<MeshCollider>();
        extraMesh.transform.SetParent(transform.GetChild(0));
        extraMesh.transform.localPosition = Vector3.zero;
        extraMesh.layer = LayerMask.NameToLayer("Road");
        extraMesh.hideFlags = HideFlags.NotEditable;
    }

    private void ResetExtraMeshes()
    {
        for (int i = extraMeshes.Count - 1; i >= 0; i--)
        {
            extraMeshes.RemoveAt(i);
            GameObject.DestroyImmediate(transform.GetChild(0).GetChild(i).gameObject);
        }
    }

    private void CreateExtraMeshesFromRoads()
    {
        List<float> currentWidths = new List<float>();
        List<float> currentYOffsets = new List<float>();
        List<float> lastWidths = new List<float>();
        List<float> lastYOffsets = new List<float>();

        // Generate first widths
        RoadSegment lastRoadSegment = connections[connections.Count - 2].road.transform.parent.parent.GetComponent<RoadSegment>();
        RoadSegment currentRoadSegment = connections[0].road.transform.parent.parent.GetComponent<RoadSegment>();

        for (int j = 0; j < currentRoadSegment.extraMeshes.Count; j++)
        {
            if (currentRoadSegment.extraMeshes[j].left == true && connections[0].road.name == "Start Point")
            {
                lastYOffsets.Add(currentRoadSegment.extraMeshes[j].yOffset);
                lastWidths.Add(currentRoadSegment.extraMeshes[j].startWidth);
            }
            else if (currentRoadSegment.extraMeshes[j].left == false && connections[0].road.name == "End Point")
            {
                lastYOffsets.Add(currentRoadSegment.extraMeshes[j].yOffset);
                lastWidths.Add(currentRoadSegment.extraMeshes[j].endWidth);
            }
        }

        for (int i = connections.Count - 1; i >= 0; i--)
        {
            currentRoadSegment = connections[i].road.transform.parent.parent.GetComponent<RoadSegment>();
            currentWidths.Clear();
            currentYOffsets.Clear();
            int addedExtraMeshes = 0;

            for (int j = 0; j < currentRoadSegment.extraMeshes.Count; j++)
            {
                if (currentRoadSegment.extraMeshes[j].left == true && connections[i].road.name == "End Point")
                {
                    if (addedExtraMeshes < lastWidths.Count)
                    {
                        if (currentRoadSegment.extraMeshes[j].endWidth != 0 || currentRoadSegment.extraMeshes[j].yOffset != 0 || lastWidths[addedExtraMeshes] != 0 || lastYOffsets[addedExtraMeshes] != 0)
                        {
                            extraMeshes.Add(new ExtraMesh(true, i, currentRoadSegment.extraMeshes[j].baseMaterial, currentRoadSegment.extraMeshes[j].overlayMaterial, currentRoadSegment.extraMeshes[j].physicMaterial, currentRoadSegment.extraMeshes[j].endWidth, lastWidths[addedExtraMeshes], currentRoadSegment.extraMeshes[j].flipped, currentRoadSegment.extraMeshes[j].yOffset));
                        }
                    }
                    else if (currentRoadSegment.extraMeshes[j].endWidth != 0 || currentRoadSegment.extraMeshes[j].yOffset != 0)
                    {
                        extraMeshes.Add(new ExtraMesh(true, i, currentRoadSegment.extraMeshes[j].baseMaterial, currentRoadSegment.extraMeshes[j].overlayMaterial, currentRoadSegment.extraMeshes[j].physicMaterial, currentRoadSegment.extraMeshes[j].endWidth, 0, currentRoadSegment.extraMeshes[j].flipped, currentRoadSegment.extraMeshes[j].yOffset));
                    }

                    addedExtraMeshes += 1;
                    CreateExtraMesh();
                }
                else if (currentRoadSegment.extraMeshes[j].left == false && connections[i].road.name == "Start Point")
                {
                    if (addedExtraMeshes < lastWidths.Count)
                    {
                        if (currentRoadSegment.extraMeshes[j].startWidth != 0 || currentRoadSegment.extraMeshes[j].yOffset != 0 || lastWidths[addedExtraMeshes] != 0 || lastYOffsets[addedExtraMeshes] != 0)
                        {
                            extraMeshes.Add(new ExtraMesh(true, i, currentRoadSegment.extraMeshes[j].baseMaterial, currentRoadSegment.extraMeshes[j].overlayMaterial, currentRoadSegment.extraMeshes[j].physicMaterial, currentRoadSegment.extraMeshes[j].startWidth, lastWidths[addedExtraMeshes], currentRoadSegment.extraMeshes[j].flipped, currentRoadSegment.extraMeshes[j].yOffset));
                        }
                    }
                    else if (currentRoadSegment.extraMeshes[j].startWidth != 0 || currentRoadSegment.extraMeshes[j].yOffset != 0)
                    {
                        extraMeshes.Add(new ExtraMesh(true, i, currentRoadSegment.extraMeshes[j].baseMaterial, currentRoadSegment.extraMeshes[j].overlayMaterial, currentRoadSegment.extraMeshes[j].physicMaterial, currentRoadSegment.extraMeshes[j].startWidth, 0, currentRoadSegment.extraMeshes[j].flipped, currentRoadSegment.extraMeshes[j].yOffset));
                    }

                    addedExtraMeshes += 1;
                    CreateExtraMesh();
                }
                else
                {
                    if (connections[i].road.name == "Start Point")
                    {
                        currentWidths.Add(currentRoadSegment.extraMeshes[j].startWidth);
                    }
                    else
                    {
                        currentWidths.Add(currentRoadSegment.extraMeshes[j].endWidth);
                    }

                    currentYOffsets.Add(currentRoadSegment.extraMeshes[j].yOffset);
                }
            }

            for (int j = addedExtraMeshes; j < lastWidths.Count; j++)
            {
                if (lastWidths[j] != 0 || lastYOffsets[j] != 0)
                {
                    extraMeshes.Add(new ExtraMesh(true, i, lastRoadSegment.extraMeshes[j].baseMaterial, lastRoadSegment.extraMeshes[j].overlayMaterial, lastRoadSegment.extraMeshes[j].physicMaterial, 0, lastWidths[j], lastRoadSegment.extraMeshes[j].flipped, lastYOffsets[j]));
                    CreateExtraMesh();
                }
            }

            lastRoadSegment = currentRoadSegment;
            lastWidths = new List<float>(currentWidths);
            lastYOffsets = new List<float>(currentYOffsets);
        }
    }
}
#endif
