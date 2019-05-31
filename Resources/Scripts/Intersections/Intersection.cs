using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    public bool generateBridge = true;
    public BridgeSettings bridgeSettings = new BridgeSettings();

    public bool placePillars = true;
    public GameObject pillarPrefab;
    public float extraPillarHeight = 0.2f;
    public float xzPillarScale = 3;

    public List<ExtraMesh> extraMeshes = new List<ExtraMesh>();

    public bool roundaboutMode = false;
    public float roundaboutRadius = 5f;
    public float roundaboutWidth = 2f;
    public float maxRoundaboutRadius = 100;
    public Material connectionBaseMaterial;
    public Material connectionOverlayMaterial;
    public float textureTilingY = 1;

    public void Setup()
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();
        gameObject.GetComponent<Transform>().hideFlags = HideFlags.NotEditable;
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

    public void MovePoints(RaycastHit raycastHit, Vector3 position, Event currentEvent)
    {
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            if (objectToMove == null)
            {
                bool isConnectedPoint = false;
                if ((raycastHit.transform.name == "Start Point" || raycastHit.transform.name == "End Point") && raycastHit.transform.GetComponent<Point>().roadPoint == true)
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

                if ((raycastHit.transform.name == "Connection Point" && raycastHit.transform.parent.gameObject == Selection.activeGameObject) || isConnectedPoint == true)
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
        else if (currentEvent.type == EventType.MouseDrag && objectToMove != null)
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

                if (generateBridge == true)
                {
                    index -= 1;
                }

                if (index % 3 == 0)
                {
                    connections[index / 3].curvePoint = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
                }
                else if (index % 3 == 1)
                {
                    connections[(index - 1) / 3].curvePoint2 = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
                }
                else if (index % 3 == 2)
                {
                    connections[(index - 2) / 3].curvePoint3 = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
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

            if (roundaboutMode == true)
            {
                GenerateRoundabout();
            }
            else
            {
                GenerateNormalMesh();
            }
        }

        if (fromRoad == false)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh(true);
            }
        }
    }

    private void GenerateNormalMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> firstVertexIndexes = new List<int>();
        float[] totalLengths = new float[connections.Count];
        float[] exactLengths = new float[connections.Count];
        int vertexIndex = 0;
        Vector3 lastVertexPosition = Misc.MaxVector3;

        for (int i = 0; i < connections.Count; i++)
        {
            Vector3 firstPoint = connections[i].leftPoint;
            Vector3 firstCenterPoint = connections[i].lastPoint;
            Vector3 nextPoint;
            Vector3 nextCenterPoint;
            totalLengths[i] = connections[i].length;
            firstVertexIndexes.Add(vertexIndex);

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

            float segments = totalLengths[i] * settings.FindProperty("resolution").floatValue * 5;
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
                    uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 1], vertices[vertices.Count - 2]), modifiedT));
                }

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
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMaterial = physicMaterial;

            if (overlayMaterial == null)
            {
                GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial };
            }
            else
            {
                GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, overlayMaterial };
            }
        }

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

    private void GenerateRoundabout()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int> connectionTriangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uvs2 = new List<Vector2>();

        int segments = (int)Mathf.Max(6, settings.FindProperty("resolution").floatValue * roundaboutRadius * 30f);
        float degreesPerSegment = 1f / segments;
        float textureRepeations = Mathf.PI * roundaboutRadius * textureTilingY * 0.5f;

        // Create roundabout vertices
        for (float f = 0; f < 1 + degreesPerSegment; f += degreesPerSegment)
        {
            float modifiedF = f;
            if (f > 1)
            {
                modifiedF = 1;
            }

            vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (roundaboutRadius + roundaboutWidth));
            vertices[vertices.Count - 1] += new Vector3(0, yOffset, 0);
            vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (roundaboutRadius - roundaboutWidth));
            vertices[vertices.Count - 1] += new Vector3(0, yOffset, 0);

            uvs.Add(new Vector2(0, modifiedF * textureRepeations));
            uvs.Add(new Vector2(1, modifiedF * textureRepeations));
            uvs2.Add(Vector2.one);
            uvs2.Add(Vector2.one);
        }

        List<int> nearestLeftPoints = new List<int>();
        List<int> nearestRightPoints = new List<int>();
        int addedVertices = 0;

        // Create road connections
        for (int i = 0; i < connections.Count; i++)
        {
            float nearestLeftDistance = float.MaxValue;
            float nearestRightDistance = float.MaxValue;

            Vector3 forward = Misc.CalculateLeft(connections[i].rightPoint - connections[i].leftPoint);
            Vector3 leftPoint = CalculateNearestIntersectionPoint(connections[i].leftPoint, forward);
            Vector3 rightPoint = CalculateNearestIntersectionPoint(connections[i].rightPoint, forward);

            nearestLeftPoints.Add(0);
            nearestRightPoints.Add(0);

            for (int j = 0; j < vertices.Count; j += 2)
            {
                float currentDistance = Vector3.Distance(leftPoint, vertices[j] + transform.position);

                if (currentDistance < nearestLeftDistance)
                {
                    nearestLeftDistance = currentDistance;
                    nearestLeftPoints[i] = j;
                }

                currentDistance = Vector3.Distance(rightPoint, vertices[j] + transform.position);

                if (currentDistance < nearestRightDistance)
                {
                    nearestRightDistance = currentDistance;
                    nearestRightPoints[i] = j;
                }
            }

            nearestLeftPoints[i] += 2;
            nearestRightPoints[i] -= 2;

            if (nearestRightPoints[i] < 0)
            {
                nearestRightPoints[i] += vertices.Count;
            }

            if (nearestRightPoints[i] > vertices.Count - 2)
            {
                nearestRightPoints[i] -= vertices.Count - 3;
            }

            Vector3 centerPoint = (vertices[nearestLeftPoints[i]] + vertices[nearestLeftPoints[i] + 1] + vertices[nearestRightPoints[i]] + vertices[nearestRightPoints[i] + 1] + connections[i].lastPoint - transform.position) / 5;
            centerPoint.y = yOffset;

            // Set curve points
            connections[i].defaultCurvePoint = Misc.GetCenter(connections[i].leftPoint, vertices[nearestLeftPoints[i]] + transform.position);
            connections[i].defaultCurvePoint2 = Misc.GetCenter(connections[i].rightPoint, vertices[nearestRightPoints[i]] + transform.position);
            connections[i].defaultCurvePoint3 = Misc.GetCenter(vertices[nearestLeftPoints[i] + 1] + transform.position, vertices[nearestRightPoints[i] + 1] + transform.position);

            if (connections[i].curvePoint == null)
            {
                connections[i].curvePoint = connections[i].defaultCurvePoint;
            }

            if (connections[i].curvePoint2 == null)
            {
                connections[i].curvePoint2 = connections[i].defaultCurvePoint2;
            }

            if (connections[i].curvePoint3 == null)
            {
                connections[i].curvePoint3 = connections[i].defaultCurvePoint3;
            }

            // Add new vertices
            segments = (int)Mathf.Max(3, settings.FindProperty("resolution").floatValue * 20);

            float distancePerSegment = 1f / segments;
            int actualSegments = 0;

            for (float f = 0; f <= 1 + distancePerSegment; f += distancePerSegment)
            {
                float modifiedF = f;
                actualSegments += 1;

                if (modifiedF > 0.5f && modifiedF < 0.5f + distancePerSegment)
                {
                    modifiedF = 0.5f;
                }
                else if (modifiedF > 1)
                {
                    modifiedF = 1f;
                }

                // Left, right and inner
                vertices.Add(Misc.Lerp3CenterHeight(connections[i].leftPoint - transform.position + new Vector3(0, yOffset, 0), connections[i].curvePoint - transform.position, vertices[nearestLeftPoints[i]], modifiedF));
                vertices.Add(Misc.Lerp3CenterHeight(connections[i].rightPoint - transform.position + new Vector3(0, yOffset, 0), connections[i].curvePoint2 - transform.position, vertices[nearestRightPoints[i]], modifiedF));
                vertices.Add(Misc.Lerp3CenterHeight(vertices[nearestLeftPoints[i] + 1], connections[i].curvePoint3 - transform.position, vertices[nearestRightPoints[i] + 1], modifiedF));

                if (modifiedF < 0.5f)
                {
                    vertices.Add(Vector3.Lerp(connections[i].lastPoint + new Vector3(0, yOffset, 0) - transform.position, centerPoint, modifiedF * 2));
                    vertices.Add(Vector3.Lerp(connections[i].lastPoint + new Vector3(0, yOffset, 0) - transform.position, centerPoint, modifiedF * 2));
                    vertices.Add(Vector3.Lerp(Misc.GetCenter(vertices[nearestLeftPoints[i]], vertices[nearestLeftPoints[i] + 1]), centerPoint, modifiedF * 2));
                }
                else
                {
                    vertices.Add(Vector3.Lerp(centerPoint, Misc.GetCenter(vertices[nearestLeftPoints[i]], vertices[nearestLeftPoints[i] + 1]), (modifiedF - 0.5f) * 2));
                    vertices.Add(Vector3.Lerp(centerPoint, Misc.GetCenter(vertices[nearestRightPoints[i]], vertices[nearestRightPoints[i] + 1]), (modifiedF - 0.5f) * 2));
                    vertices.Add(Vector3.Lerp(centerPoint, Misc.GetCenter(vertices[nearestRightPoints[i]], vertices[nearestRightPoints[i] + 1]), (modifiedF - 0.5f) * 2));
                }

                addedVertices += 6;

                if (stretchTexture == true)
                {
                    uvs.Add(new Vector2(0, modifiedF));
                    uvs.Add(new Vector2(0, modifiedF));
                    uvs.Add(new Vector2(0, modifiedF));
                    uvs.Add(new Vector2(1, modifiedF));
                    uvs.Add(new Vector2(1, modifiedF));
                    uvs.Add(new Vector2(1, modifiedF));
                }
                else
                {
                    uvs.Add(new Vector2(0, modifiedF));
                    uvs.Add(new Vector2(0, modifiedF));
                    uvs.Add(new Vector2(0, modifiedF));
                    uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 3], vertices[vertices.Count - 6]), modifiedF));
                    uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 2], vertices[vertices.Count - 5]), modifiedF));
                    uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 1], vertices[vertices.Count - 4]), modifiedF));
                }

                uvs2.Add(Vector2.one);
                uvs2.Add(Vector2.one);
                uvs2.Add(Vector2.one);
                uvs2.Add(Vector2.one);
                uvs2.Add(Vector2.one);
                uvs2.Add(Vector2.one);
            }

            // Add new triangles
            connectionTriangles.AddRange(AddConnectionTriangles(vertices, triangles, actualSegments));
        }

        // Create road sections
        for (int i = 0; i < connections.Count + 1; i++)
        {
            if (i < connections.Count || connections.Count == 0)
            {
                int startIndex;
                int endIndex;

                if (connections.Count == 0)
                {
                    startIndex = 0;
                    endIndex = vertices.Count - 2 - addedVertices;
                }
                else
                {
                    startIndex = nearestLeftPoints[i];

                    if (i == connections.Count - 1)
                    {
                        endIndex = nearestRightPoints[0];
                    }
                    else
                    {
                        endIndex = nearestRightPoints[i + 1];
                    }
                }

                List<RoundaboutExtraMesh> leftExtraMeshes = new List<RoundaboutExtraMesh>();
                float startOffsetLeft = 0;
                float endOffsetLeft = 0;
                float yOffsetLeft = 0;

                for (int j = 0; j < extraMeshes.Count; j++)
                {
                    if (extraMeshes[j].index > 0 && extraMeshes[j].index - 1 == i)
                    {
                        if (leftExtraMeshes.Count > 0)
                        {
                            startOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.startWidth;
                            endOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.endWidth;
                            yOffsetLeft += leftExtraMeshes[leftExtraMeshes.Count - 1].extraMesh.yOffset;
                        }

                        leftExtraMeshes.Add(new RoundaboutExtraMesh(extraMeshes[j], j, startOffsetLeft, endOffsetLeft, yOffsetLeft));
                    }
                }

                if (startIndex % 2f == 0 && endIndex % 2f == 0)
                {
                    int vertexIndex = startIndex;
                    int verticesLooped = 0;
                    int verticesToLoop = 0;

                    if (endIndex > startIndex)
                    {
                        verticesToLoop = endIndex - startIndex;
                    }
                    else
                    {
                        verticesToLoop = vertices.Count - 2 - startIndex - addedVertices;
                        verticesToLoop += endIndex;
                    }

                    while (vertexIndex != endIndex && triangles.Count < 10000)
                    {
                        if (vertexIndex > vertices.Count - 4 - addedVertices)
                        {
                            vertexIndex = 0;
                        }

                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);

                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 3);
                        triangles.Add(vertexIndex + 1);

                        // Extra meshes
                        float progress = (float)verticesLooped / (float)verticesToLoop;
                        leftExtraMeshes = AddTrianglesToExtraMeshes(leftExtraMeshes, vertices, vertexIndex, progress, verticesLooped);

                        verticesLooped += 2;
                        vertexIndex += 2;
                    }

                    leftExtraMeshes = AddTrianglesToExtraMeshes(leftExtraMeshes, vertices, vertexIndex, 1, verticesLooped);
                    AssignExtraMeshes(leftExtraMeshes);
                }
                else
                {
                    Debug.LogError("For some reason a roundabout connection's start/end index is uneven");
                }
            }
        }

        CreateCenterExtraMeshes();
        SetupRoundaboutMesh(vertices, triangles, connectionTriangles, uvs, uvs2);
    }

    private void CreateCenterExtraMeshes()
    {
        List<ExtraMesh> centerExtraMeshes = new List<ExtraMesh>();
        List<int> indexes = new List<int>();
        int segments = (int)Mathf.Max(6, settings.FindProperty("resolution").floatValue * roundaboutRadius * 30f);
        float degreesPerSegment = 1f / segments;
        float textureRepeations = Mathf.PI * roundaboutRadius * textureTilingY * 0.5f;

        for (int i = 0; i < extraMeshes.Count; i++)
        {
            if (extraMeshes[i].index == 0)
            {
                centerExtraMeshes.Add(extraMeshes[i]);
                indexes.Add(i);
            }
        }

        float currentStartOffset = 0;
        float currentEndOffset = 0;
        float currentYOffset = yOffset;

        for (int i = 0; i < centerExtraMeshes.Count; i++)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector2> uvs2 = new List<Vector2>();

            int vertexIndex = 0;

            for (float f = 0; f < 1 + degreesPerSegment; f += degreesPerSegment)
            {
                float modifiedF = f;

                if (f > 1)
                {
                    modifiedF = 1;
                }

                vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (roundaboutRadius - roundaboutWidth - Mathf.Lerp(currentStartOffset, currentEndOffset, modifiedF)));
                vertices[vertices.Count - 1] += new Vector3(0, currentYOffset, 0);
                vertices.Add(Quaternion.Euler(0, modifiedF * 360, 0) * Vector3.forward * (roundaboutRadius - roundaboutWidth - Mathf.Lerp(currentStartOffset + centerExtraMeshes[i].startWidth, currentEndOffset + centerExtraMeshes[i].endWidth, modifiedF)));
                vertices[vertices.Count - 1] += new Vector3(0, currentYOffset + centerExtraMeshes[i].yOffset, 0);

                float localWidth = Mathf.Lerp(centerExtraMeshes[i].startWidth, centerExtraMeshes[i].endWidth, modifiedF) / centerExtraMeshes[i].endWidth;

                uvs.Add(new Vector2(0, modifiedF * textureRepeations));
                uvs.Add(new Vector2(localWidth, modifiedF * textureRepeations));
                uvs2.Add(new Vector3(localWidth, 1));
                uvs2.Add(new Vector3(localWidth, 1));

                if (vertexIndex > 0)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex - 1);
                    triangles.Add(vertexIndex - 2);

                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex - 1);
                }

                vertexIndex += 2;
            }

            // Update offsets
            currentStartOffset += centerExtraMeshes[i].startWidth;
            currentEndOffset += centerExtraMeshes[i].endWidth;
            currentYOffset += centerExtraMeshes[i].yOffset;

            // Assign mesh
            AssignCenterExtraMesh(vertices, triangles, uvs, uvs2, centerExtraMeshes, i, indexes);
        }
    }

    private void AssignCenterExtraMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector2> uvs2, List<ExtraMesh> centerExtraMeshes, int i, List<int> indexes)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uvs2.ToArray();
        mesh.RecalculateNormals();

        transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshCollider>().sharedMesh = mesh;
        transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshCollider>().sharedMaterial = centerExtraMeshes[i].physicMaterial;

        if (centerExtraMeshes[i].overlayMaterial == null)
        {
            transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { centerExtraMeshes[i].baseMaterial };
        }
        else
        {
            transform.GetChild(0).GetChild(indexes[i]).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { centerExtraMeshes[i].baseMaterial, centerExtraMeshes[i].overlayMaterial };
        }
    }

    private List<RoundaboutExtraMesh> AddTrianglesToExtraMeshes(List<RoundaboutExtraMesh> extraMeshes, List<Vector3> vertices, int vertexIndex, float progress, int verticesLooped)
    {
        for (int j = 0; j < extraMeshes.Count; j++)
        {
            Vector3 left = (vertices[vertexIndex] - vertices[vertexIndex + 1]).normalized;

            extraMeshes[j].vertices.Add(vertices[vertexIndex] + left * Mathf.Lerp(extraMeshes[j].startOffset, extraMeshes[j].endOffset, progress));
            extraMeshes[j].vertices.Add(vertices[vertexIndex] + left * Mathf.Lerp(extraMeshes[j].extraMesh.startWidth + extraMeshes[j].startOffset, extraMeshes[j].extraMesh.endWidth + extraMeshes[j].endOffset, progress));
            extraMeshes[j].vertices[extraMeshes[j].vertices.Count - 2] += new Vector3(0, extraMeshes[j].yOffset, 0);
            extraMeshes[j].vertices[extraMeshes[j].vertices.Count - 1] += new Vector3(0, extraMeshes[j].extraMesh.yOffset + extraMeshes[j].yOffset, 0);

            extraMeshes[j].uvs.Add(new Vector3(0, progress));
            extraMeshes[j].uvs.Add(new Vector3(1, progress));

            if (verticesLooped > 0)
            {
                extraMeshes[j].triangles.Add(verticesLooped);
                extraMeshes[j].triangles.Add(verticesLooped - 2);
                extraMeshes[j].triangles.Add(verticesLooped - 1);

                extraMeshes[j].triangles.Add(verticesLooped);
                extraMeshes[j].triangles.Add(verticesLooped - 1);
                extraMeshes[j].triangles.Add(verticesLooped + 1);
            }
        }

        return extraMeshes;
    }

    private void AssignExtraMeshes(List<RoundaboutExtraMesh> extraMeshes)
    {
        for (int i = 0; i < extraMeshes.Count; i++)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = extraMeshes[i].vertices.ToArray();
            mesh.triangles = extraMeshes[i].triangles.ToArray();
            mesh.uv = extraMeshes[i].uvs.ToArray();
            mesh.RecalculateNormals();

            transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshFilter>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshCollider>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshCollider>().sharedMaterial = extraMeshes[i].extraMesh.physicMaterial;

            if (extraMeshes[i].extraMesh.overlayMaterial == null)
            {
                transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].extraMesh.baseMaterial };
            }
            else
            {
                transform.GetChild(0).GetChild(extraMeshes[i].listIndex).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].extraMesh.baseMaterial, extraMeshes[i].extraMesh.overlayMaterial };
            }
        }
    }

    private void SetupRoundaboutMesh(List<Vector3> vertices, List<int> triangles, List<int> connectionTriangles, List<Vector2> uvs, List<Vector2> uvs2)
    {
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 4;
        mesh.vertices = vertices.ToArray();

        int materialIndex = 0;

        List<Material> materials = new List<Material>();
        materials.Add(baseMaterial);
        mesh.SetTriangles(triangles, materialIndex);
        materialIndex += 1;

        if (overlayMaterial != null)
        {
            materials.Add(overlayMaterial);
            mesh.SetTriangles(triangles, materialIndex);
            materialIndex += 1;
        }

        materials.Add(connectionBaseMaterial);
        mesh.SetTriangles(connectionTriangles, materialIndex);
        materialIndex += 1;

        if (connectionOverlayMaterial != null)
        {
            materials.Add(connectionOverlayMaterial);
            mesh.SetTriangles(connectionTriangles, materialIndex);
            materialIndex += 1;
        }

        mesh.subMeshCount = materialIndex;
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uvs2.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMaterial = physicMaterial;
    }

    private List<int> AddConnectionTriangles(List<Vector3> vertices, List<int> inputTriangles, int segments)
    {
        List<int> triangles = new List<int>(inputTriangles);

        for (int j = vertices.Count - segments * 6; j < vertices.Count - 7; j += 6)
        {
            // Left
            triangles.Add(j);
            triangles.Add(j + 6);
            triangles.Add(j + 3);

            triangles.Add(j + 3);
            triangles.Add(j + 6);
            triangles.Add(j + 9);

            // Right
            triangles.Add(j + 1);
            triangles.Add(j + 4);
            triangles.Add(j + 7);

            triangles.Add(j + 10);
            triangles.Add(j + 7);
            triangles.Add(j + 4);

            // Inner
            triangles.Add(j + 2);
            triangles.Add(j + 11);
            triangles.Add(j + 5);

            triangles.Add(j + 2);
            triangles.Add(j + 8);
            triangles.Add(j + 11);
        }

        return triangles;
    }

    private Vector3 CalculateNearestIntersectionPoint(Vector3 originalPoint, Vector3 forward)
    {
        Vector3 point = originalPoint;

        for (float d = 0; d < 10; d += 0.1f)
        {
            point += forward * d;

            if (Vector3.Distance(point, transform.position) < roundaboutRadius + roundaboutWidth / 2)
            {
                return point;
            }
        }

        return originalPoint + forward * 2;
    }

    private void GenerateExtraMeshes(List<int> firstVertexIndexes, List<Vector3> vertices, float[] exactLengths, float[] totalLengths, ref float[] startWidths, ref float[] endWidths, ref float[] heights)
    {
        for (int i = 0; i < extraMeshes.Count; i++)
        {
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

            List<Vector3> extraMeshVertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            int vertexIndex = 0;

            for (int j = firstVertexIndexes[extraMeshes[i].index]; j < endVertexIndex; j += 2)
            {
                currentLength += Vector3.Distance(lastPosition, vertices[j + 1]);

                Vector3 forward = (vertices[j + 1] - vertices[j]).normalized;
                extraMeshVertices.Add(vertices[j] - forward * (Mathf.Lerp(extraMeshes[i].startWidth, extraMeshes[i].endWidth, currentLength / totalLengths[extraMeshes[i].index]) + Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLengths[extraMeshes[i].index])));
                extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, extraMeshes[i].yOffset + heights[extraMeshes[i].index], 0);
                extraMeshVertices.Add(vertices[j] - forward * Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLengths[extraMeshes[i].index]));
                extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, heights[extraMeshes[i].index], 0);

                uvs.Add(new Vector2(0, (currentLength / exactLengths[extraMeshes[i].index])));
                uvs.Add(new Vector2(1, (currentLength / exactLengths[extraMeshes[i].index])));

                if (j < endVertexIndex - 2)
                {
                    triangles = AddTriangles(triangles, vertexIndex);
                    vertexIndex += 2;
                }

                lastPosition = vertices[j + 1];
            }

            Mesh mesh = new Mesh();
            mesh.vertices = extraMeshVertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            vertexIndex = 0;

            transform.GetChild(0).GetChild(i).GetComponent<MeshFilter>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMaterial = extraMeshes[i].physicMaterial;

            if (overlayMaterial == null)
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial };
            }
            else
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, extraMeshes[i].overlayMaterial };
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
                CreateCurvePoint(new Vector3(connections[i].curvePoint3.x, yOffset + transform.position.y, connections[i].curvePoint3.z));
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
        curvePoint.layer = settings.FindProperty("ignoreMouseRayLayer").intValue;
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
                connections[i].curvePoint = Misc.GetCenter(connections[i].leftPoint, connections[nextIndex].rightPoint);
            }
        }
    }

    public void ResetConnectionCurvePoints(IntersectionConnection intersectionConnection)
    {
        intersectionConnection.curvePoint = intersectionConnection.defaultCurvePoint;
        intersectionConnection.curvePoint2 = intersectionConnection.defaultCurvePoint2;
        intersectionConnection.curvePoint3 = intersectionConnection.defaultCurvePoint3;
    }

    public void ResetExtraMeshes()
    {
        for (int i = transform.GetChild(0).childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(0).GetChild(i).gameObject);
            extraMeshes.Clear();
        }
    }

    public void RemoveCurvePoints()
    {
        if (gameObject != null)
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
                if (connections[i].road.Equals(connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection.road))
                {
                    connections[i] = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection;
                }
                else
                {
                    connections[i] = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnection;
                }
            }
        }
    }

}
