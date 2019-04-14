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
    public float yOffset;
    public GlobalSettings globalSettings;
    public GameObject objectToMove;
    public bool stretchTexture = false;

    public bool generateBridge = true;
    public BridgeSettings bridgeSettings;

    public bool placePillars = true;
    public GameObject pillarPrefab;
    public float extraPillarHeight = 0.2f;
    public float xzPillarScale = 3;

    public List<ExtraMesh> extraMeshes = new List<ExtraMesh>();

    public void MovePoints(RaycastHit raycastHit, Vector3 position, Event currentEvent)
    {
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            if (objectToMove == null)
            {
                if (raycastHit.transform.name == "Connection Point" && raycastHit.transform.parent.gameObject == Selection.activeGameObject)
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
            objectToMove.transform.position = position;
        }
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && objectToMove != null)
        {
            objectToMove.GetComponent<BoxCollider>().enabled = true;

            int nextIndex = objectToMove.transform.GetSiblingIndex();
            if (nextIndex >= connections.Count)
            {
                nextIndex = 0;
            }

            Vector3 center = Misc.GetCenter(connections[objectToMove.transform.GetSiblingIndex() - 1].leftPoint.ToNormalVector3(), connections[nextIndex].rightPoint.ToNormalVector3());
            center.y -= yOffset;
            connections[objectToMove.transform.GetSiblingIndex() - 1].curvePoint = new SerializedVector3(new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z));
            objectToMove = null;
            CreateMesh(false);

            for (int i = 0; i < connections.Count; i++)
            {
                nextIndex = i + 2;
                if (nextIndex >= connections.Count)
                {
                    nextIndex = 0;
                }

                transform.GetChild(i + 1).transform.position = new Vector3(connections[i].curvePoint.ToNormalVector3().x, transform.position.y + yOffset, connections[i].curvePoint.ToNormalVector3().z);
            }
        }
    }

    public void CreateMesh(bool fromRoad = false)
    {
        if (globalSettings == null)
        {
            globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].road == null)
            {
                connections.RemoveAt(i);
            }
        }

        if (connections.Count < 2)
        {
            RoadCreator[] roads = GameObject.FindObjectsOfType<RoadCreator>();

            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i].startIntersection == this)
                {
                    Undo.RecordObject(roads[i], "Remove Intersection");
                    roads[i].startIntersection = null;
                    roads[i].startIntersectionConnectionIndex = -1; 
                }
                else if (roads[i].endIntersection == this)
                {
                    Undo.RecordObject(roads[i], "Remove Intersection");
                    roads[i].endIntersection = null;
                    roads[i].endIntersectionConnectionIndex = -1;
                }
            }

            Undo.DestroyObjectImmediate(gameObject);
        }
        else
        {

            if (baseMaterial == null)
            {
                baseMaterial = Resources.Load("Materials/Low Poly/Intersections/Asphalt Intersection") as Material;
            }

            if (bridgeSettings.bridgeMaterials == null || bridgeSettings.bridgeMaterials.Length == 0 || bridgeSettings.bridgeMaterials[0] == null)
            {
                bridgeSettings.bridgeMaterials = new Material[] { Resources.Load("Materials/Low Poly/Concrete") as Material };
            }

            for (int i = 0; i < extraMeshes.Count; i++)
            {
                if (extraMeshes[i].material == null)
                {
                    extraMeshes[i].material = Resources.Load("Materials/Low Poly/Asphalt") as Material;
                }
            }

            if (pillarPrefab == null || pillarPrefab.GetComponent<MeshFilter>() == null)
            {
                pillarPrefab = Resources.Load("Prefabs/Low Poly/Bridges/Pillars/Cylinder Bridge Pillar") as GameObject;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> firstVertexIndexes = new List<int>();
            float[] totalLength = new float[connections.Count];
            float[] exactLengths = new float[connections.Count];
            int vertexIndex = 0;
            Vector3 lastVertexPosition = Misc.MaxVector3;

            for (int i = 0; i < connections.Count; i++)
            {
                Vector3 firstPoint = connections[i].leftPoint.ToNormalVector3();
                Vector3 firstCenterPoint = connections[i].lastPoint.ToNormalVector3();
                Vector3 nextPoint;
                Vector3 nextCenterPoint;
                totalLength[i] = connections[i].length;
                firstVertexIndexes.Add(vertexIndex);

                if (i == connections.Count - 1)
                {
                    nextPoint = connections[0].rightPoint.ToNormalVector3();
                    nextCenterPoint = connections[0].lastPoint.ToNormalVector3();
                    totalLength[i] += connections[0].length;
                }
                else
                {
                    nextPoint = connections[i + 1].rightPoint.ToNormalVector3();
                    nextCenterPoint = connections[i + 1].lastPoint.ToNormalVector3();
                    totalLength[i] += connections[i + 1].length;
                }

                if (connections[i].curvePoint == null)
                {
                    return;
                }

                float segments = totalLength[i] * globalSettings.resolution * 5;
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

                    vertices.Add(Misc.Lerp3CenterHeight(firstPoint, connections[i].curvePoint.ToNormalVector3(), nextPoint, modifiedT) + new Vector3(0, yOffset, 0) - transform.position);

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

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name == "Bridge")
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                    break;
                }
            }

            // Extra meshes
            float[] startWidths = new float[firstVertexIndexes.Count];
            float[] endWidths = new float[firstVertexIndexes.Count];
            float[] heights = new float[firstVertexIndexes.Count];

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
                triangles.Clear();
                uvs.Clear();
                vertexIndex = 0;

                for (int j = firstVertexIndexes[extraMeshes[i].index]; j < endVertexIndex; j += 2)
                {
                    currentLength += Vector3.Distance(lastPosition, vertices[j + 1]);

                    Vector3 forward = (vertices[j + 1] - vertices[j]).normalized;
                    extraMeshVertices.Add(vertices[j] - forward * (Mathf.Lerp(extraMeshes[i].startWidth, extraMeshes[i].endWidth, currentLength / totalLength[extraMeshes[i].index]) + Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLength[extraMeshes[i].index])));
                    extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, extraMeshes[i].yOffset + heights[extraMeshes[i].index], 0);
                    extraMeshVertices.Add(vertices[j] - forward * Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLength[extraMeshes[i].index]));
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
                vertexIndex = 0;

                transform.GetChild(0).GetChild(i).GetComponent<MeshFilter>().sharedMesh = mesh;
                transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMesh = mesh;
                transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMaterial = extraMeshes[i].physicMaterial;
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].material };

                startWidths[extraMeshes[i].index] += extraMeshes[i].startWidth;
                endWidths[extraMeshes[i].index] += extraMeshes[i].endWidth;
                heights[extraMeshes[i].index] += extraMeshes[i].yOffset;
            }

            if (generateBridge == true)
            {
                BridgeGeneration.GenerateSimpleBridgeIntersection(GetComponent<MeshFilter>().sharedMesh.vertices, this, bridgeSettings.bridgeMaterials, startWidths, endWidths, firstVertexIndexes.ToArray());
            }

            CreateCurvePoints();
        }

        if (fromRoad == false)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh(true);
            }
        }
    }

    public void CreateCurvePoints()
    {
        RemoveCurvePoints();

        for (int i = 0; i < connections.Count; i++)
        {
            GameObject curvePoint = null;
            curvePoint = new GameObject("Connection Point");
            curvePoint.transform.SetParent(transform);
            curvePoint.layer = globalSettings.ignoreMouseRayLayer;
            curvePoint.AddComponent<BoxCollider>();
            curvePoint.GetComponent<BoxCollider>().size = new Vector3(globalSettings.pointSize, globalSettings.pointSize, globalSettings.pointSize);
            curvePoint.transform.position = connections[i].curvePoint.ToNormalVector3();
            curvePoint.transform.position = new Vector3(curvePoint.transform.position.x, yOffset + transform.position.y, curvePoint.transform.position.z);
        }

        if (transform.Find("Bridge") != null)
        {
            transform.Find("Bridge").SetAsLastSibling();
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

            connections[i].curvePoint = new SerializedVector3(Misc.GetCenter(connections[i].leftPoint.ToNormalVector3(), connections[nextIndex].rightPoint.ToNormalVector3()));
        }
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
}
