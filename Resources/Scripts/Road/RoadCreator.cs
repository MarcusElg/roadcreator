using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Roads")]
public class RoadCreator : MonoBehaviour
{

    public float heightOffset = 0.02f;
    public bool createIntersections = true;

    public GlobalSettings globalSettings;
    public Preset segmentPreset;
    public GameObject objectToMove = null;
    public GameObject extraObjectToMove = null;
    private bool mouseDown;
    public bool sDown;

    public Intersection startIntersection;
    public Intersection endIntersection;
    public int startIntersectionConnectionIndex = -1;
    public int endIntersectionConnectionIndex = -1;

    public void CreateMesh(bool fromIntersection = false)
    {
        if (this != null)
        {
            Vector3[] currentPoints = null;

            for (int i = 0; i < transform.GetChild(0).childCount; i++)
            {
                if (transform.GetChild(0).GetChild(i).GetChild(0).childCount == 3)
                {
                    Vector3 previousPoint = Misc.MaxVector3;

                    if (i == 0)
                    {
                        currentPoints = CalculatePoints(transform.GetChild(0).GetChild(i));

                        if (transform.GetChild(0).GetChild(i).GetSiblingIndex() == 0 && startIntersection != null && startIntersectionConnectionIndex != -1 && startIntersectionConnectionIndex < startIntersection.connections.Count)
                        {
                            previousPoint = startIntersection.connections[startIntersectionConnectionIndex].lastPoint.ToNormalVector3() + (currentPoints[0] - currentPoints[1]).normalized;
                            previousPoint.y = startIntersection.yOffset + startIntersection.connections[startIntersectionConnectionIndex].lastPoint.y;
                        }
                    }

                    if (i < transform.GetChild(0).childCount - 1 && transform.GetChild(0).GetChild(i + 1).GetChild(0).childCount == 3)
                    {
                        Vector3[] nextPoints = CalculatePoints(transform.GetChild(0).GetChild(i + 1));
                        nextPoints[0] = currentPoints[currentPoints.Length - 1];

                        if (i == 0)
                        {
                            transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, null, heightOffset, transform.GetChild(0).GetChild(i), null, this);
                        }
                        else
                        {
                            transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, transform.GetChild(0).GetChild(i - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices, heightOffset, transform.GetChild(0).GetChild(i), transform.GetChild(0).GetChild(i - 1), this);
                        }

                        StartCoroutine(FixTextureStretch(Misc.CalculateDistance(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).position), i));
                        currentPoints = nextPoints;
                    }
                    else
                    {
                        Vector3[] nextPoints = null;

                        if (transform.GetChild(0).GetChild(i).GetSiblingIndex() == transform.GetChild(0).childCount - 1 && endIntersectionConnectionIndex != -1 && endIntersection != null && endIntersectionConnectionIndex < endIntersection.connections.Count)
                        {
                            nextPoints = new Vector3[1];
                            nextPoints[0] = endIntersection.connections[endIntersectionConnectionIndex].lastPoint.ToNormalVector3() + (currentPoints[currentPoints.Length - 1] - currentPoints[currentPoints.Length - 2]).normalized;
                            nextPoints[0].y = endIntersection.yOffset + endIntersection.connections[endIntersectionConnectionIndex].lastPoint.y;
                        }

                        if (i - 1 >= 0)
                        {
                            transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, transform.GetChild(0).GetChild(i - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices, heightOffset, transform.GetChild(0).GetChild(i), transform.GetChild(0).GetChild(i - 1), this);
                            StartCoroutine(FixTextureStretch(Misc.CalculateDistance(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).position), i));
                        }
                        else
                        {
                            transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, null, heightOffset, transform.GetChild(0).GetChild(i), null, this);
                            StartCoroutine(FixTextureStretch(Misc.CalculateDistance(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).position), i));
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < transform.GetChild(0).GetChild(i).childCount; j++)
                    {
                        transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshFilter>().sharedMesh = null;
                        transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshCollider>().sharedMesh = null;

                        if (transform.GetChild(0).GetChild(i).childCount == 3)
                        {
                            DestroyImmediate(transform.GetChild(0).GetChild(i).GetChild(2).gameObject);
                        }
                    }
                }
            }
        }

        if (fromIntersection == false)
        {
            if (startIntersectionConnectionIndex != -1 && startIntersection != null)
            {
                UpdateStartConnectionData(startIntersection);
                startIntersection.CreateMesh(true);
            }

            if (endIntersectionConnectionIndex != -1 && endIntersection != null)
            {
                UpdateEndConnectionData(endIntersection);
                endIntersection.CreateMesh(true);
            }
        }
    }

    IEnumerator FixTextureStretch(float length, int i)
    {
        yield return new WaitForSeconds(0.01f);

        if (transform.GetChild(0).childCount > i)
        {
            for (int j = 0; j < transform.GetChild(0).GetChild(i).GetChild(1).childCount; j++)
            {
                if (transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial != null)
                {
                    float textureRepeat = length / 4 * transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().textureTilingY;

                    Material material = new Material(transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial);
                    material.SetVector("_Tiling", new Vector2(1, textureRepeat));

                    float lastTextureRepeat = 0;
                    float lastTextureOffset = 0;

                    if (i > 0)
                    {
                        lastTextureRepeat = transform.GetChild(0).GetChild(i - 1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.GetVector("_Tiling").y;
                        lastTextureOffset = transform.GetChild(0).GetChild(i - 1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.GetVector("_Offset").y;
                        material.SetVector("_Offset", new Vector2(0, (lastTextureRepeat % 1.0f) + lastTextureOffset));
                    }

                    transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial = material;

                    if (transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterials.Length > 1)
                    {
                        material = new Material(transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterials[1]);
                        material.SetVector("_Tiling", new Vector2(1, textureRepeat));

                        if (i > 0)
                        {
                            material.SetVector("_Offset", new Vector2(0, (lastTextureRepeat % 1.0f) + lastTextureOffset));
                        }

                        transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterials = new Material[2] { transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterials[0], material };
                    }
                }
            }

            if (transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().bridgeGenerator != RoadSegment.BridgeGenerator.none)
            {
                if (transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().sharedMaterial != null)
                {
                    Material material = new Material(transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().sharedMaterial);
                    float textureRepeat = length / 4 * transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().textureTilingY;
                    material.SetVector("_Tiling", new Vector2(1, textureRepeat));
                    transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
        }
    }

    public void UndoUpdate()
    {
        CreateMesh();
    }

    public void CreatePoints(Vector3 hitPosition)
    {
        if (transform.GetChild(0).childCount > 0)
        {
            if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount == 1)
            {
                if (globalSettings.roadCurved == true)
                {
                    // Create control point
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created Point");
                }
                else
                {
                    // Create control and end points
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), Misc.GetCenter(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(0).position, hitPosition)), "Created Point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created Point");
                    CreateMesh();
                }
            }
            else if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount == 2)
            {
                // Create end point
                Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created Point");
                CreateMesh();
            }
            else
            {
                RoadSegment segment = CreateSegment(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).position);
                Undo.RegisterCreatedObjectUndo(segment.gameObject, "Create Point");
                Undo.RegisterCreatedObjectUndo(CreatePoint("Start Point", segment.transform.GetChild(0), segment.transform.position), "Created Point");

                if (globalSettings.roadCurved == true)
                {
                    segment.curved = true;
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", segment.transform.GetChild(0), hitPosition), "Created Point");
                }
                else
                {
                    segment.curved = false;
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), Misc.GetCenter(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(0).position, hitPosition)), "Created Point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created Point");
                    CreateMesh();
                }
            }
        }
        else
        {
            // Create first segment
            RoadSegment segment = CreateSegment(hitPosition);
            Undo.RegisterCreatedObjectUndo(segment.gameObject, "Create Point");
            Undo.RegisterCreatedObjectUndo(CreatePoint("Start Point", segment.transform.GetChild(0), hitPosition), "Created Point");

            if (globalSettings.roadCurved == false)
            {
                segment.curved = false;
            }
            else
            {
                segment.curved = true;
            }
        }
    }

    private GameObject CreatePoint(string name, Transform parent, Vector3 position)
    {
        GameObject point = new GameObject(name);
        point.gameObject.AddComponent<BoxCollider>();
        point.GetComponent<BoxCollider>().size = new Vector3(globalSettings.pointSize, globalSettings.pointSize, globalSettings.pointSize);
        point.transform.SetParent(parent);
        point.transform.position = position;
        point.GetComponent<BoxCollider>().hideFlags = HideFlags.NotEditable;
        point.layer = globalSettings.ignoreMouseRayLayer;
        point.AddComponent<Point>();
        point.GetComponent<Point>().hideFlags = HideFlags.NotEditable;

        return point;
    }

    private RoadSegment CreateSegment(Vector3 position)
    {
        RoadSegment segment = new GameObject("Segment").AddComponent<RoadSegment>();
        segment.transform.SetParent(transform.GetChild(0), false);
        segment.transform.position = position;
        segment.transform.hideFlags = HideFlags.NotEditable;

        GameObject points = new GameObject("Points");
        points.transform.SetParent(segment.transform);
        points.transform.localPosition = Vector3.zero;
        points.hideFlags = HideFlags.NotEditable;

        GameObject meshes = new GameObject("Meshes");
        meshes.transform.SetParent(segment.transform);
        meshes.transform.localPosition = Vector3.zero;
        meshes.hideFlags = HideFlags.NotEditable;

        GameObject mainMesh = new GameObject("Main Mesh");
        mainMesh.transform.SetParent(meshes.transform);
        mainMesh.transform.localPosition = Vector3.zero;
        mainMesh.hideFlags = HideFlags.NotEditable;
        mainMesh.AddComponent<MeshRenderer>();
        mainMesh.AddComponent<MeshFilter>();
        mainMesh.AddComponent<MeshCollider>();
        mainMesh.layer = globalSettings.roadLayer;

        if (segmentPreset == null)
        {
            if (transform.GetChild(0).childCount > 1)
            {
                RoadSegment oldLastSegment = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetComponent<RoadSegment>();
                segment.baseRoadMaterial = oldLastSegment.baseRoadMaterial;
                segment.overlayRoadMaterial = oldLastSegment.overlayRoadMaterial;
                segment.startRoadWidth = oldLastSegment.startRoadWidth;
                segment.endRoadWidth = oldLastSegment.endRoadWidth;
                segment.flipped = oldLastSegment.flipped;
                segment.terrainOption = oldLastSegment.terrainOption;

                segment.bridgeGenerator = oldLastSegment.bridgeGenerator;
                segment.bridgeSettings = oldLastSegment.bridgeSettings;

                segment.placePillars = oldLastSegment.placePillars;
                segment.pillarPrefab = oldLastSegment.pillarPrefab;
                segment.pillarGap = oldLastSegment.pillarGap;
                segment.pillarPlacementOffset = oldLastSegment.pillarPlacementOffset;
                segment.extraPillarHeight = oldLastSegment.extraPillarHeight;
                segment.xzPillarScale = oldLastSegment.xzPillarScale;

                for (int i = 0; i < oldLastSegment.extraMeshes.Count; i++)
                {
                    GameObject extraMesh = new GameObject("Extra Mesh");
                    extraMesh.AddComponent<MeshFilter>();
                    extraMesh.AddComponent<MeshRenderer>();
                    extraMesh.AddComponent<MeshCollider>();
                    extraMesh.transform.SetParent(segment.transform.GetChild(1));
                    extraMesh.transform.localPosition = Vector3.zero;
                    extraMesh.layer = globalSettings.roadLayer;
                    extraMesh.hideFlags = HideFlags.NotEditable;

                    segment.extraMeshes.Add(oldLastSegment.extraMeshes[i]);
                }
            }
        }
        else
        {
            segmentPreset.ApplyTo(segment);

            for (int i = 0; i < segment.extraMeshes.Count; i++)
            {
                GameObject extraMesh = new GameObject("Extra Mesh");
                extraMesh.AddComponent<MeshFilter>();
                extraMesh.AddComponent<MeshRenderer>();
                extraMesh.AddComponent<MeshCollider>();
                extraMesh.transform.SetParent(segment.transform.GetChild(1));
                extraMesh.transform.localPosition = Vector3.zero;
                extraMesh.layer = globalSettings.roadLayer;
                extraMesh.hideFlags = HideFlags.NotEditable;
            }
        }

        return segment;
    }

    public void UpdateStartConnectionData(Intersection startIntersection)
    {
        if (startIntersectionConnectionIndex != -1 && startIntersection != null)
        {
            UpdateStartConnectionVariables(startIntersection);

            // Update connection index
            RoadCreator[] roads = new RoadCreator[startIntersection.connections.Count];
            IntersectionConnection[] connections = new IntersectionConnection[startIntersection.connections.Count];
            bool[] end = new bool[startIntersection.connections.Count];

            for (int i = 0; i < startIntersection.connections.Count; i++)
            {
                roads[i] = startIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>();
                connections[i] = startIntersection.connections[i];

                if (startIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnectionIndex == -1 || startIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection == null || startIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection.connections[startIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnectionIndex] != connections[i])
                {
                    end[i] = true;
                }
                else
                {
                    end[i] = false;
                }
            }

            startIntersection.connections.Sort();

            for (int i = 0; i < roads.Length; i++)
            {
                if (end[i] == true)
                {
                    roads[i].endIntersectionConnectionIndex = System.Array.IndexOf(startIntersection.connections.ToArray(), connections[i]);
                }
                else
                {
                    roads[i].startIntersectionConnectionIndex = System.Array.IndexOf(startIntersection.connections.ToArray(), connections[i]);
                }
            }

            for (int i = 0; i < roads.Length; i++)
            {
                if (end[i] == true)
                {
                    roads[i].UpdateEndConnectionVariables(roads[i].endIntersection);
                }
                else
                {
                    roads[i].UpdateStartConnectionVariables(roads[i].startIntersection);
                }
            }

            Vector3 totalPosition = Vector3.zero;
            for (int i = 0; i < startIntersection.connections.Count; i++)
            {
                totalPosition += startIntersection.connections[i].lastPoint.ToNormalVector3();
            }

            Vector3 newPosition = totalPosition / startIntersection.connections.Count;
            newPosition.y = startIntersection.transform.position.y;
            startIntersection.transform.position = newPosition;
            startIntersection.CreateMesh();
        }
    }

    public void UpdateEndConnectionData(Intersection endIntersection)
    {
        if (endIntersectionConnectionIndex != -1 && endIntersection != null)
        {
            UpdateEndConnectionVariables(endIntersection);

            // Update connection index
            RoadCreator[] roads = new RoadCreator[endIntersection.connections.Count];
            IntersectionConnection[] connections = new IntersectionConnection[endIntersection.connections.Count];
            bool[] end = new bool[endIntersection.connections.Count];

            for (int i = 0; i < endIntersection.connections.Count; i++)
            {
                roads[i] = endIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>();
                connections[i] = endIntersection.connections[i];

                if (endIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnectionIndex == -1 || endIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection == null || endIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection.connections[endIntersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnectionIndex] != connections[i])
                {
                    end[i] = false;
                }
                else
                {
                    end[i] = true;
                }
            }

            endIntersection.connections.Sort();

            for (int i = 0; i < roads.Length; i++)
            {
                if (end[i] == true)
                {
                    roads[i].endIntersectionConnectionIndex = System.Array.IndexOf(endIntersection.connections.ToArray(), connections[i]);
                }
                else
                {
                    roads[i].startIntersectionConnectionIndex = System.Array.IndexOf(endIntersection.connections.ToArray(), connections[i]);
                }
            }

            for (int i = 0; i < roads.Length; i++)
            {
                if (end[i] == true)
                {
                    roads[i].UpdateEndConnectionVariables(roads[i].endIntersection);
                }
                else
                {
                    roads[i].UpdateStartConnectionVariables(roads[i].startIntersection);
                }
            }

            Vector3 totalPosition = Vector3.zero;
            for (int i = 0; i < endIntersection.connections.Count; i++)
            {
                totalPosition += endIntersection.connections[i].lastPoint.ToNormalVector3();
            }

            Vector3 newPosition = totalPosition / endIntersection.connections.Count;
            newPosition.y = endIntersection.transform.position.y;
            endIntersection.transform.position = newPosition;
            endIntersection.CreateMesh();
        }
    }

    public void UpdateStartConnectionVariables(Intersection startIntersection)
    {
        Vector3[] vertices = transform.GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
        startIntersection.connections[startIntersectionConnectionIndex].leftPoint = new SerializedVector3(vertices[1] + transform.GetChild(0).GetChild(0).transform.position);
        startIntersection.connections[startIntersectionConnectionIndex].leftPoint.y = startIntersection.transform.position.y;
        startIntersection.connections[startIntersectionConnectionIndex].rightPoint = new SerializedVector3(vertices[0] + transform.GetChild(0).GetChild(0).transform.position);
        startIntersection.connections[startIntersectionConnectionIndex].rightPoint.y = startIntersection.transform.position.y;
        startIntersection.connections[startIntersectionConnectionIndex].lastPoint = new SerializedVector3(Misc.GetCenter(vertices[0], vertices[1]) + transform.GetChild(0).GetChild(0).transform.position);
        startIntersection.connections[startIntersectionConnectionIndex].lastPoint.y = startIntersection.transform.position.y;
        startIntersection.connections[startIntersectionConnectionIndex].length = Vector3.Distance(startIntersection.transform.position, startIntersection.connections[startIntersectionConnectionIndex].lastPoint.ToNormalVector3());
        startIntersection.connections[startIntersectionConnectionIndex].YRotation = Quaternion.LookRotation((startIntersection.transform.position - startIntersection.connections[startIntersectionConnectionIndex].road.transform.position).normalized).eulerAngles.y;

    }

    public void UpdateEndConnectionVariables(Intersection endIntersection)
    {
        Vector3[] vertices = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
        endIntersection.connections[endIntersectionConnectionIndex].leftPoint = new SerializedVector3(vertices[vertices.Length - 2] + transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).position);
        endIntersection.connections[endIntersectionConnectionIndex].leftPoint.y = endIntersection.transform.position.y;
        endIntersection.connections[endIntersectionConnectionIndex].rightPoint = new SerializedVector3(vertices[vertices.Length - 1] + transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).position);
        endIntersection.connections[endIntersectionConnectionIndex].rightPoint.y = endIntersection.transform.position.y;
        endIntersection.connections[endIntersectionConnectionIndex].lastPoint = new SerializedVector3(Misc.GetCenter(vertices[vertices.Length - 1], vertices[vertices.Length - 2]) + transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).transform.position);
        endIntersection.connections[endIntersectionConnectionIndex].lastPoint.y = endIntersection.transform.position.y;
        endIntersection.connections[endIntersectionConnectionIndex].length = Vector3.Distance(endIntersection.transform.position, endIntersection.connections[endIntersectionConnectionIndex].lastPoint.ToNormalVector3());
        endIntersection.connections[endIntersectionConnectionIndex].YRotation = Quaternion.LookRotation((endIntersection.transform.position - endIntersection.connections[endIntersectionConnectionIndex].road.transform.position).normalized).eulerAngles.y;
    }

    public void CheckForIntersectionGeneration(GameObject point)
    {
        if (createIntersections == true)
        {
            RaycastHit raycastHitPoint;
            RaycastHit raycastHitRoad;

            if (Physics.Raycast(point.transform.position + new Vector3(0, 1, 0), Vector3.down, out raycastHitPoint, 100, 1 << globalSettings.ignoreMouseRayLayer) && raycastHitPoint.transform.GetComponent<Point>() != null && raycastHitPoint.transform.parent.parent.parent.parent.gameObject != point.transform.parent.parent.parent.parent.gameObject)
            {
                // Found Point
                if (point.transform.GetSiblingIndex() == 1 || raycastHitPoint.transform.GetSiblingIndex() == 1 || raycastHitPoint.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().createIntersections == false)
                {
                    return;
                }

                if ((point.transform.name == "Start Point" && point.transform.parent.parent.GetSiblingIndex() != 0) || (point.transform.name == "End Point" && point.transform.parent.parent.GetSiblingIndex() != point.transform.parent.parent.parent.childCount - 1))
                {
                    return;
                }

                if ((raycastHitPoint.transform.name == "Start Point" && raycastHitPoint.transform.parent.parent.GetSiblingIndex() != 0) || (raycastHitPoint.transform.name == "End Point" && raycastHitPoint.transform.parent.parent.GetSiblingIndex() != raycastHitPoint.transform.parent.parent.parent.childCount - 1))
                {
                    return;
                }

                Vector3 creationPosition = raycastHitPoint.point;
                creationPosition.y = raycastHitPoint.transform.position.y;
                GameObject intersection = CreateIntersection(creationPosition, point.transform.parent.parent.GetComponent<RoadSegment>());

                if (point.transform.GetSiblingIndex() == 0 && startIntersection == null)
                {
                    CreateIntersectionConnectionForNewIntersectionFirst(point, intersection.GetComponent<Intersection>());
                }
                else if (point.transform.GetSiblingIndex() == 2 && endIntersection == null)
                {
                    CreateIntersectionConnectionForNewIntersectionLast(point, intersection.GetComponent<Intersection>());
                }

                if (raycastHitPoint.transform.GetSiblingIndex() == 0 && raycastHitPoint.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection == null)
                {
                    CreateIntersectionConnectionForNewIntersectionFirst(raycastHitPoint.transform.gameObject, intersection.GetComponent<Intersection>());
                }
                else if (raycastHitPoint.transform.GetSiblingIndex() == 2 && raycastHitPoint.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection == null)
                {
                    CreateIntersectionConnectionForNewIntersectionLast(raycastHitPoint.transform.gameObject, intersection.GetComponent<Intersection>());
                }

                intersection.GetComponent<Intersection>().ResetCurvePointPositions();
                intersection.GetComponent<Intersection>().ResetExtraMeshes();
                intersection.GetComponent<Intersection>().CreateMesh();
            }
            else if (Physics.Raycast(point.transform.position + new Vector3(0, 1, 0), Vector3.down, out raycastHitRoad, 100, globalSettings.ignoreMouseRayLayer) && raycastHitRoad.transform.GetComponent<Intersection>() != null && sDown == false)
            {
                //Found road
                if (point.transform.GetSiblingIndex() == 0 && startIntersection == null)
                {
                    CreateMesh();
                    Vector3[] vertices = point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
                    Vector3 forward = Misc.GetCenter(vertices[0], vertices[1]) - Misc.GetCenter(vertices[2], vertices[3]);
                    point.transform.position += (-forward).normalized * 2;
                    CreateMesh();
                    CreateIntersectionConnectionFirst(raycastHitRoad.transform.GetComponent<Intersection>(), point);
                    startIntersection = raycastHitRoad.transform.GetComponent<Intersection>();
                    startIntersectionConnectionIndex = startIntersection.connections.Count - 1;

                    UpdateStartConnectionData(startIntersection);
                    startIntersection.GetComponent<Intersection>().ResetCurvePointPositions();
                    startIntersection.GetComponent<Intersection>().ResetExtraMeshes();
                }
                else if (point.transform.GetSiblingIndex() == 2 && point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection == null)
                {
                    CreateMesh();
                    Vector3[] vertices = point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
                    Vector3 forward = Misc.GetCenter(vertices[vertices.Length - 1], vertices[vertices.Length - 2]) - Misc.GetCenter(vertices[vertices.Length - 3], vertices[vertices.Length - 4]);
                    point.transform.position += (-forward).normalized * 2;
                    CreateMesh();
                    CreateIntersectionConnectionLast(raycastHitRoad.transform.GetComponent<Intersection>(), point);
                    endIntersection = raycastHitRoad.transform.GetComponent<Intersection>();
                    endIntersectionConnectionIndex = endIntersection.connections.Count - 1;

                    UpdateEndConnectionData(endIntersection);
                    endIntersection.GetComponent<Intersection>().ResetCurvePointPositions();
                    endIntersection.GetComponent<Intersection>().ResetExtraMeshes();
                }
            }
            else
            {
                //Found nothing
                if (sDown == true)
                {
                    CreateMesh();
                }
                else
                {
                    if (point.transform.GetSiblingIndex() == 0 && startIntersectionConnectionIndex != -1 && startIntersection != null)
                    {
                        Intersection intersection = startIntersection;
                        int index = startIntersectionConnectionIndex;
                        RemoveIntersectionConnection(startIntersection, startIntersectionConnectionIndex, true);

                        for (int i = index; i < intersection.connections.Count; i++)
                        {
                            if (i > index - 1)
                            {
                                if (intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnectionIndex == -1 || intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection == null || intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection != intersection)
                                {
                                    intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnectionIndex -= 1;
                                }
                                else
                                {
                                    intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnectionIndex -= 1;
                                }
                            }
                        }

                        intersection.ResetCurvePointPositions();
                        intersection.ResetExtraMeshes();
                        intersection.CreateMesh();
                    }
                    else if (point.transform.GetSiblingIndex() == 2 && endIntersectionConnectionIndex != -1 && endIntersection != null)
                    {
                        Intersection intersection = endIntersection;
                        int index = endIntersectionConnectionIndex;
                        RemoveIntersectionConnection(endIntersection, endIntersectionConnectionIndex, false);

                        for (int i = index; i < intersection.connections.Count; i++)
                        {
                            if (intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnectionIndex == -1 || intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection == null || intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection != intersection)
                            {
                                intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnectionIndex -= 1;
                            }
                            else
                            {
                                intersection.connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnectionIndex -= 1;
                            }
                        }

                        intersection.ResetCurvePointPositions();
                        intersection.ResetExtraMeshes();
                        intersection.CreateMesh();
                    }
                }
            }
        }
    }

    public void RemoveIntersectionConnection(Intersection intersection, int connectionIndex, bool start)
    {
        intersection.connections.RemoveAt(System.Array.IndexOf(intersection.connections.ToArray(), intersection.connections[connectionIndex]));

        if (start == true)
        {
            startIntersection = null;
            startIntersectionConnectionIndex = -1;
        }
        else
        {
            endIntersection = null;
            endIntersectionConnectionIndex = -1;
        }
    }

    public GameObject CreateIntersection(Vector3 position, RoadSegment segment)
    {
        GameObject intersection = new GameObject("Intersection");
        Undo.RegisterCreatedObjectUndo(intersection, "Create Intersection");
        intersection.transform.SetParent(transform.parent);
        intersection.transform.position = position;

        intersection.AddComponent<Intersection>();
        intersection.GetComponent<Intersection>().yOffset = heightOffset;

        if (segment.bridgeGenerator == RoadSegment.BridgeGenerator.simple)
        {
            intersection.GetComponent<Intersection>().bridgeGenerator = Intersection.BridgeGenerator.simple;
        }

        intersection.GetComponent<Intersection>().bridgeSettings = segment.bridgeSettings;
        intersection.GetComponent<Intersection>().placePillars = segment.placePillars;
        intersection.GetComponent<Intersection>().extraPillarHeight = segment.extraPillarHeight;
        intersection.GetComponent<Intersection>().xzPillarScale = segment.xzPillarScale;

        intersection.AddComponent<MeshFilter>();
        intersection.AddComponent<MeshRenderer>();
        intersection.AddComponent<MeshCollider>();
        intersection.GetComponent<Transform>().hideFlags = HideFlags.NotEditable;
        intersection.GetComponent<MeshFilter>().hideFlags = HideFlags.NotEditable;
        intersection.GetComponent<MeshCollider>().hideFlags = HideFlags.NotEditable;
        intersection.GetComponent<MeshRenderer>().hideFlags = HideFlags.NotEditable;

        GameObject gameObject = new GameObject("Extra Meshes");
        gameObject.transform.SetParent(intersection.transform, false);

        return intersection;
    }

    public void CreateIntersectionConnectionForNewIntersectionFirst(GameObject gameObject, Intersection intersection)
    {
        CreateIntersectionConnectionForNewIntersection(gameObject, intersection, intersection.transform.position - gameObject.transform.parent.GetChild(2).position, true);
    }

    public void CreateIntersectionConnectionForNewIntersectionLast(GameObject gameObject, Intersection intersection)
    {
        CreateIntersectionConnectionForNewIntersection(gameObject, intersection, intersection.transform.position - gameObject.transform.parent.GetChild(0).position, false);
    }

    public void CreateIntersectionConnectionForNewIntersection(GameObject gameObject, Intersection intersection, Vector3 forward, bool first)
    {
        gameObject.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
        Vector3[] vertices = gameObject.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
        gameObject.transform.position += (-forward).normalized * 2;
        gameObject.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();

        if (first == true)
        {
            CreateIntersectionConnectionFirst(intersection.GetComponent<Intersection>(), gameObject);
            gameObject.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection = intersection;
            gameObject.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnectionIndex = intersection.connections.Count - 1;
        }
        else
        {
            CreateIntersectionConnectionLast(intersection.GetComponent<Intersection>(), gameObject);
            gameObject.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection = intersection;
            gameObject.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnectionIndex = intersection.connections.Count - 1;
        }
    }


    public IntersectionConnection CreateIntersectionConnectionFirst(Intersection intersection, GameObject point)
    {
        return CreateIntersectionConnection(intersection, point, 1, 0);
    }

    public IntersectionConnection CreateIntersectionConnectionLast(Intersection intersection, GameObject point)
    {
        return CreateIntersectionConnection(intersection, point, point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2, point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 1);
    }

    public IntersectionConnection CreateIntersectionConnection(Intersection intersection, GameObject point, int firstVertex, int secondVertex)
    {
        IntersectionConnection intersectionConnection = new IntersectionConnection();
        intersection.connections.Add(intersectionConnection);

        intersectionConnection.leftPoint = new SerializedVector3(point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[firstVertex] + point.transform.parent.parent.position);
        intersectionConnection.leftPoint.y = intersection.transform.position.y;
        intersectionConnection.rightPoint = new SerializedVector3(point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[secondVertex] + point.transform.parent.parent.position);
        intersectionConnection.rightPoint.y = intersection.transform.position.y;
        intersectionConnection.lastPoint = new SerializedVector3(Misc.GetCenter(intersectionConnection.leftPoint.ToNormalVector3(), intersectionConnection.rightPoint.ToNormalVector3()));
        intersectionConnection.lastPoint.y = intersection.transform.position.y;
        intersectionConnection.YRotation = Quaternion.LookRotation((intersection.transform.position - point.transform.parent.GetChild(0).position).normalized).eulerAngles.y;
        intersectionConnection.length = Vector3.Distance(intersection.transform.position, point.transform.position);
        intersectionConnection.road = point.GetComponent<Point>();
        intersectionConnection.curvePoint = new SerializedVector3(intersection.transform.position);

        return intersectionConnection;
    }

    public bool IsLastSegmentCurved()
    {
        if (transform.GetChild(0).childCount > 0)
        {
            if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount == 1)
            {
                if (transform.GetChild(0).childCount > 1)
                {
                    return transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 2).GetComponent<RoadSegment>().curved;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved;
            }
        }
        else
        {
            return false;
        }
    }

    public void RemovePoints()
    {
        if (transform.GetChild(0).childCount > 0)
        {
            if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved == true)
            {
                if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount == 2)
                {
                    Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(1).gameObject);
                }
                else if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).childCount == 1)
                {
                    Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).gameObject);

                    if (transform.GetChild(0).childCount > 0)
                    {
                        if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved == false)
                        {
                            Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                            Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(1).gameObject);
                            transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved = true;
                            CreateMesh();
                        }
                        else
                        {
                            if (transform.GetChild(0).childCount > 0)
                            {
                                for (int i = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).childCount - 1; i >= 0; i -= 1)
                                {
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).GetComponent<MeshFilter>().sharedMesh = null;
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).GetComponent<MeshCollider>().sharedMesh = null;
                                }

                                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                            }
                        }
                    }
                }
                else
                {
                    Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);

                    for (int i = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).childCount - 1; i >= 0; i -= 1)
                    {
                        transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).GetComponent<MeshFilter>().sharedMesh = null;
                        transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(i).GetComponent<MeshCollider>().sharedMesh = null;
                    }

                    CreateMesh();
                }
            }
            else
            {
                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(1).gameObject);
                CreateMesh();
            }
        }
    }

    public void MovePoints(Vector3 hitPosition, Event guiEvent, RaycastHit raycastHit)
    {
        if (hitPosition == Misc.MaxVector3)
        {
            if (objectToMove != null)
            {
                dropMovingPoint();
            }
        }
        else
        {
            if (mouseDown == true && objectToMove != null)
            {
                if (guiEvent.keyCode == KeyCode.Plus || guiEvent.keyCode == KeyCode.KeypadPlus)
                {
                    Undo.RecordObject(objectToMove.transform, "Moved Point");
                    objectToMove.transform.position += new Vector3(0, 0.2f, 0);

                    if (guiEvent.control == true)
                    {
                        objectToMove.transform.position = new Vector3(objectToMove.transform.position.x, Mathf.Ceil(objectToMove.transform.position.y), objectToMove.transform.position.z);
                    }

                    if (extraObjectToMove != null)
                    {
                        Undo.RecordObject(extraObjectToMove.transform, "Moved Point");
                        extraObjectToMove.transform.position = objectToMove.transform.position;

                        if (guiEvent.control == true)
                        {
                            extraObjectToMove.transform.position = new Vector3(extraObjectToMove.transform.position.x, Mathf.Ceil(extraObjectToMove.transform.position.y), extraObjectToMove.transform.position.z);
                        }
                    }
                }
                else if (guiEvent.keyCode == KeyCode.Minus || guiEvent.keyCode == KeyCode.KeypadMinus)
                {
                    Vector3 position = objectToMove.transform.position - new Vector3(0, 0.2f, 0);

                    if (guiEvent.control == true)
                    {
                        position = new Vector3(position.x, Mathf.Floor(position.y), position.z);
                    }

                    if (position.y < raycastHit.point.y)
                    {
                        position.y = raycastHit.point.y;
                    }

                    Undo.RecordObject(objectToMove.transform, "Moved Point");
                    objectToMove.transform.position = position;

                    if (extraObjectToMove != null)
                    {
                        Undo.RecordObject(extraObjectToMove.transform, "Moved Point");
                        extraObjectToMove.transform.position = position;
                    }
                }
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                mouseDown = true;
                if (objectToMove == null)
                {
                    if (raycastHit.transform.name.Contains("Point") && raycastHit.transform.GetComponent<Point>() != null && raycastHit.transform.parent.parent.parent.parent.gameObject == Selection.activeGameObject)
                    {
                        if (raycastHit.transform.GetComponent<BoxCollider>().enabled == false)
                        {
                            return;
                        }

                        if (raycastHit.collider.gameObject.name == "Control Point")
                        {
                            objectToMove = raycastHit.collider.gameObject;
                            objectToMove.GetComponent<BoxCollider>().enabled = false;
                        }
                        else if (raycastHit.collider.gameObject.name == "Start Point")
                        {
                            objectToMove = raycastHit.collider.gameObject;
                            objectToMove.GetComponent<BoxCollider>().enabled = false;

                            if (objectToMove.transform.parent.parent.GetSiblingIndex() > 0)
                            {
                                extraObjectToMove = raycastHit.collider.gameObject.transform.parent.parent.parent.GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() - 1).GetChild(0).GetChild(2).gameObject;
                                extraObjectToMove.GetComponent<BoxCollider>().enabled = false;
                            }
                        }
                        else if (raycastHit.collider.gameObject.name == "End Point")
                        {
                            objectToMove = raycastHit.collider.gameObject;
                            objectToMove.GetComponent<BoxCollider>().enabled = false;

                            if (objectToMove.transform.parent.parent.GetSiblingIndex() < objectToMove.transform.parent.parent.parent.childCount - 1 && raycastHit.collider.gameObject.transform.parent.parent.parent.GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() + 1).GetChild(0).childCount == 3)
                            {
                                extraObjectToMove = raycastHit.collider.gameObject.transform.parent.parent.parent.GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() + 1).GetChild(0).GetChild(0).gameObject;
                                extraObjectToMove.GetComponent<BoxCollider>().enabled = false;
                            }
                        }

                    }
                }
            }
            else if (guiEvent.type == EventType.MouseDrag && objectToMove != null)
            {
                Undo.RecordObject(objectToMove.transform, "Moved Point");
                objectToMove.transform.position = hitPosition;

                if (extraObjectToMove != null)
                {
                    Undo.RecordObject(extraObjectToMove.transform, "Moved Point");
                    extraObjectToMove.transform.position = hitPosition;
                }
            }
            else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && objectToMove != null)
            {
                dropMovingPoint();
            }
        }
    }

    public void dropMovingPoint()
    {
        mouseDown = false;
        if (objectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
        {
            if (objectToMove.transform.GetSiblingIndex() == 1)
            {
                objectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved = true;
            }
            else
            {
                if (objectToMove.transform.parent.childCount == 3)
                {
                    objectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(objectToMove.transform.parent.GetChild(0).position, objectToMove.transform.parent.GetChild(2).position);
                }
            }
        }

        if (extraObjectToMove != null)
        {
            if (extraObjectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
            {
                if (extraObjectToMove.transform.parent.childCount == 3)
                {
                    extraObjectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(extraObjectToMove.transform.parent.GetChild(0).position, extraObjectToMove.transform.parent.GetChild(2).position);
                }
            }

            extraObjectToMove.GetComponent<BoxCollider>().enabled = true;
            extraObjectToMove = null;
        }
        else
        {
            if ((objectToMove.transform.GetSiblingIndex() == 2 && objectToMove.transform.parent.parent.GetSiblingIndex() == objectToMove.transform.parent.parent.parent.childCount - 1) || (objectToMove.transform.GetSiblingIndex() == 0 && objectToMove.transform.parent.parent.GetSiblingIndex() == 0))
            {
                CheckForIntersectionGeneration(objectToMove);
            }
        }

        if (startIntersectionConnectionIndex != -1)
        {
            startIntersection.CreateMesh();
        }

        if (endIntersectionConnectionIndex != -1)
        {
            endIntersection.CreateMesh();
        }

        objectToMove.GetComponent<BoxCollider>().enabled = true;
        objectToMove = null;

        CreateMesh();
        globalSettings.UpdateRoadGuidelines();
    }

    public Vector3[] CalculatePoints(Transform segment)
    {
        float distance = Misc.CalculateDistance(segment.GetChild(0).GetChild(0).position, segment.GetChild(0).GetChild(1).position, segment.GetChild(0).GetChild(2).position);
        float divisions = globalSettings.resolution * 4 * distance;
        divisions = Mathf.Max(2, divisions);
        List<Vector3> points = new List<Vector3>();
        float distancePerDivision = 1 / divisions;
        float globalDistancePerDivision = distancePerDivision * distance;
        Vector3 lastPosition = segment.transform.GetChild(0).GetChild(0).position;
        points.Add(RaycastedPosition(lastPosition, segment.GetComponent<RoadSegment>()));

        for (float t = 0; t < 1; t += distancePerDivision / 10)
        {
            Vector3 position = Misc.Lerp3CenterHeight(segment.GetChild(0).GetChild(0).position, segment.GetChild(0).GetChild(1).position, segment.GetChild(0).GetChild(2).position, t);

            float calculatedDistance = Vector3.Distance(position, lastPosition);
            if (t + distancePerDivision / 10 >= 1)
            {
                points[points.Count - 1] = RaycastedPosition(segment.GetChild(0).GetChild(2).position, segment.GetComponent<RoadSegment>());
            }
            else if (calculatedDistance > globalDistancePerDivision)
            {
                lastPosition = position;
                points.Add(RaycastedPosition(position, segment.GetComponent<RoadSegment>()));
            }
        }

        return points.ToArray();
    }

    public Vector3 RaycastedPosition(Vector3 originalPosition, RoadSegment segment)
    {
        if (segment.terrainOption == RoadSegment.TerrainOption.adapt)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(originalPosition + new Vector3(0, 10, 0), Vector3.down, out raycastHit, 100f, ~((1 << globalSettings.ignoreMouseRayLayer) | (1 << globalSettings.roadLayer))))
            {
                return raycastHit.point;
            }
        }

        return originalPosition;
    }

}
