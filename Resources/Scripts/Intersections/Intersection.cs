using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Intersection : MonoBehaviour
{

    public Material baseMaterial;
    public Material overlayMaterial;
    public PhysicMaterial physicMaterial;
    public List<IntersectionConnection> connections = new List<IntersectionConnection>();
    public float yOffset;
    public GlobalSettings globalSettings;
    public GameObject objectToMove;

    public RoadSegment.BridgeGenerator bridgeGenerator;
    public Material[] bridgeMaterials;

    public float yOffsetFirstStep = 0.25f;
    public float yOffsetSecondStep = 0.5f;
    public float widthPercentageFirstStep = 0.6f;
    public float widthPercentageSecondStep = 0.6f;

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

            int nextIndex = objectToMove.transform.GetSiblingIndex() + 1;
            if (nextIndex >= connections.Count)
            {
                nextIndex = 0;
            }

            connections[objectToMove.transform.GetSiblingIndex()].curviness = Vector3.Distance(Misc.GetCenter(connections[objectToMove.transform.GetSiblingIndex()].leftPoint.ToNormalVector3(), connections[nextIndex].rightPoint.ToNormalVector3()), objectToMove.transform.position);
            objectToMove = null;
            GenerateMesh(false);

            for (int i = 0; i < connections.Count; i++)
            {
                nextIndex = i + 1;
                if (nextIndex >= connections.Count)
                {
                    nextIndex = 0;
                }

                transform.GetChild(i).transform.position = Misc.GetCenter(connections[i].leftPoint.ToNormalVector3(), connections[nextIndex].rightPoint.ToNormalVector3()) - Misc.CalculateLeft(connections[i].leftPoint.ToNormalVector3(), connections[nextIndex].rightPoint.ToNormalVector3()) * connections[i].curviness;
            }
        }
    }

    public void GenerateMesh(bool fromRoad = false)
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
                    roads[i].startIntersection = null;
                    break;
                }
                else if (roads[i].endIntersection == this)
                {
                    roads[i].endIntersection = this;
                    break;
                }
            }

            GameObject.DestroyImmediate(gameObject);
        }
        else
        {

            if (baseMaterial == null)
            {
                baseMaterial = Resources.Load("Materials/Low Poly/Intersections/Intersection Connections/2L Connection") as Material;
            }

            if (bridgeMaterials == null || bridgeMaterials.Length == 0 || bridgeMaterials[0] == null)
            {
                bridgeMaterials = new Material[] { Resources.Load("Materials/Low Poly/Concrete") as Material };
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

                float segments = totalLength * globalSettings.resolution * 5;
                segments = Mathf.Max(3, segments);
                float distancePerSegment = 1f / segments;

                for (float t = 0; t <= 1 + distancePerSegment; t += distancePerSegment)
                {
                    float modifiedT = t;
                    if (Mathf.Abs(0.5f - t) < distancePerSegment)
                    {
                        modifiedT = 0.5f;
                    }

                    if (modifiedT > 1)
                    {
                        modifiedT = 1;
                    }

                    vertices.Add(Misc.Lerp3(firstPoint, Misc.GetCenter(firstPoint, nextPoint) - Misc.CalculateLeft(firstPoint, nextPoint) * connections[i].curviness, nextPoint, modifiedT) + new Vector3(0, yOffset, 0) - transform.position);
                    uvs.Add(new Vector2(0, modifiedT));
                    uvs.Add(new Vector2(1, modifiedT));

                    if (modifiedT < 0.5f)
                    {
                        Vector3 point = Vector3.Lerp(firstCenterPoint, transform.position, modifiedT * 2) + new Vector3(0, yOffset, 0) - transform.position;
                        point.y = Mathf.Lerp(firstPoint.y, nextPoint.y, modifiedT) - transform.position.y + yOffset;
                        vertices.Add(point);
                    }
                    else
                    {
                        Vector3 point = Vector3.Lerp(transform.position, nextCenterPoint, 2 * (modifiedT - 0.5f)) + new Vector3(0, yOffset, 0) - transform.position;
                        point.y = Mathf.Lerp(firstPoint.y, nextPoint.y, modifiedT) - transform.position.y + yOffset;
                        vertices.Add(point);
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
                GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, overlayMaterial };
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name == "Bridge")
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                    break;
                }
            }

            if (bridgeGenerator == RoadSegment.BridgeGenerator.simple)
            {
                BridgeGeneration.GenerateSimpleBridgeIntersection(GetComponent<MeshFilter>().sharedMesh.vertices, transform, yOffset, yOffsetFirstStep, yOffsetSecondStep, widthPercentageFirstStep, widthPercentageSecondStep, bridgeMaterials[0]);
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
