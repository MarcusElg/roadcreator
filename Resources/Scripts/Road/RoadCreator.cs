using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;

public class RoadCreator : MonoBehaviour
{

    public float heightOffset = 0.02f;
    public int smoothnessAmount = 3;

    public GlobalSettings globalSettings;

    public Preset segmentPreset;

    public GameObject followObject;
    public Vector3 lastMoveObjectPosition;

    public GameObject objectToMove = null;
    public GameObject extraObjectToMove = null;
    private bool mouseDown;

    public Intersection startIntersection = null;
    public Intersection endIntersection = null;
    public IntersectionConnection startIntersectionConnection = null;
    public IntersectionConnection endIntersectionConnection = null;

    public void CreateMesh()
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

                        /*if (transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection != null)
                        {
                            previousPoint = GetIntersectionPoint(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).transform.position + new Vector3(0, heightOffset, 0), transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection.transform.parent.parent.gameObject, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection.name);
                        }*/
                    }

                    if (i < transform.GetChild(0).childCount - 1 && transform.GetChild(0).GetChild(i + 1).GetChild(0).childCount == 3)
                    {
                        Vector3[] nextPoints = CalculatePoints(transform.GetChild(0).GetChild(i + 1));
                        Vector3 originalControlPoint = currentPoints[currentPoints.Length - 1];

                        int actualSmoothnessAmount = smoothnessAmount;

                        if ((currentPoints.Length / 2) <= actualSmoothnessAmount)
                        {
                            actualSmoothnessAmount = currentPoints.Length / 2 - 2;
                        }

                        if ((nextPoints.Length / 2) <= actualSmoothnessAmount)
                        {
                            actualSmoothnessAmount = nextPoints.Length / 2 - 2;
                        }

                        if (actualSmoothnessAmount > 0)
                        {
                            float distanceSection = 1f / (actualSmoothnessAmount * 2);
                            int currentPoint = 0;
                            for (float t = 0; t < 0.4999 + distanceSection; t += distanceSection)
                            {
                                if (t > 0.5f)
                                {
                                    t = 0.5f;
                                }

                                // First section
                                currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount + currentPoint] = Misc.Lerp3(currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount], originalControlPoint, nextPoints[actualSmoothnessAmount], t);

                                // Second section
                                nextPoints[actualSmoothnessAmount - currentPoint] = Misc.Lerp3(currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount], originalControlPoint, nextPoints[actualSmoothnessAmount], 1f - t);

                                currentPoint += 1;
                            }
                        }
                        else
                        {
                            nextPoints[0] = currentPoints[currentPoints.Length - 1];
                        }

                        transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, heightOffset, transform.GetChild(0).GetChild(i), actualSmoothnessAmount, this);
                        StartCoroutine(FixTextureStretch(Misc.CalculateDistance(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).position), i));
                        currentPoints = nextPoints;
                    }
                    else
                    {
                        Vector3[] nextPoints = null;

                        /*if (transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection != null)
                        {
                            nextPoints = new Vector3[1];
                            nextPoints[0] = GetIntersectionPoint(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).transform.position + new Vector3(0, heightOffset, 0), transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection.transform.parent.parent.gameObject, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection.name);
                        }*/

                        transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, heightOffset, transform.GetChild(0).GetChild(i), 0, this);
                        StartCoroutine(FixTextureStretch(Misc.CalculateDistance(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).position), i));
                    }
                }
                else
                {
                    for (int j = 0; j < transform.GetChild(0).GetChild(i).GetChild(1).childCount; j++)
                    {
                        transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshFilter>().sharedMesh = null;
                        transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshCollider>().sharedMesh = null;
                    }
                }
            }
        }
    }

    IEnumerator FixTextureStretch(float length, int i)
    {
        yield return new WaitForSeconds(0.01f);

        if (transform.GetChild(0).childCount > i)
        {
            float textureRepeat = length / 4;

            for (int j = 0; j < transform.GetChild(0).GetChild(i).GetChild(1).childCount; j++)
            {
                if (transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial != null)
                {
                    Material material = new Material(transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial);
                    material.SetVector("Vector2_79C0D9A3", new Vector2(1, textureRepeat * transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().textureTilingY));
                    transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
        }
    }

    public void UndoUpdate()
    {
        CreateMesh();

        if (followObject != null)
        {
            followObject.GetComponent<PrefabLineCreator>().UndoUpdate();
        }
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
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created point");
                }
                else
                {
                    // Create control and end points
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), Misc.GetCenter(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(0).position, hitPosition)), "Created point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created point");
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
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", segment.transform.GetChild(0), hitPosition), "Created Point");
                }
                else
                {
                    segment.curved = false;
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), Misc.GetCenter(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(0).position, hitPosition)), "Created point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0), hitPosition), "Created point");
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
        }
    }

    private GameObject CreatePoint(string name, Transform parent, Vector3 position)
    {
        GameObject point = new GameObject(name);
        point.gameObject.AddComponent<BoxCollider>();
        point.GetComponent<BoxCollider>().size = new Vector3(globalSettings.pointSize, globalSettings.pointSize, globalSettings.pointSize);
        point.transform.SetParent(parent);
        point.transform.position = position;
        point.hideFlags = HideFlags.NotEditable;
        point.layer = globalSettings.ignoreMouseRayLayer;
        point.AddComponent<Point>();

        CheckForIntersectionGeneration(point);

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
                segment.roadMaterial = oldLastSegment.roadMaterial;
                segment.startRoadWidth = oldLastSegment.startRoadWidth;
                segment.endRoadWidth = oldLastSegment.endRoadWidth;
                segment.flipped = oldLastSegment.flipped;
                segment.terrainOption = oldLastSegment.terrainOption;

                for (int i = 0; i < oldLastSegment.extraMeshOpen.Count; i++)
                {
                    GameObject extraMesh = new GameObject("Extra Mesh");
                    extraMesh.AddComponent<MeshFilter>();
                    extraMesh.AddComponent<MeshRenderer>();
                    extraMesh.AddComponent<MeshCollider>();
                    extraMesh.transform.SetParent(segment.transform.GetChild(1));
                    extraMesh.transform.localPosition = Vector3.zero;
                    extraMesh.layer = globalSettings.roadLayer;
                    extraMesh.hideFlags = HideFlags.NotEditable;

                    segment.extraMeshOpen.Add(oldLastSegment.extraMeshOpen[i]);
                    segment.extraMeshLeft.Add(oldLastSegment.extraMeshLeft[i]);
                    segment.extraMeshMaterial.Add(oldLastSegment.extraMeshMaterial[i]);
                    segment.extraMeshPhysicMaterial.Add(oldLastSegment.extraMeshPhysicMaterial[i]);
                    segment.extraMeshYOffset.Add(oldLastSegment.extraMeshYOffset[i]);
                    segment.extraMeshWidth.Add(oldLastSegment.extraMeshWidth[i]);
                }
            }
            else
            {
                segment.roadMaterial = Resources.Load("Materials/Low Poly/Roads/2 Lane Roads/2L Road") as Material;
            }
        }
        else
        {
            segmentPreset.ApplyTo(segment);

            for (int i = 0; i < segment.extraMeshOpen.Count; i++)
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

    public void CheckForIntersectionGeneration(GameObject point)
    {
        RaycastHit[] raycastHits = Physics.RaycastAll(point.transform.position + new Vector3(0, 1, 0), Vector3.down, 100, 1 << globalSettings.ignoreMouseRayLayer);

        if (raycastHits.Length > 0)
        {
            if ((raycastHits[0].transform.parent.parent.parent.parent.gameObject != point.transform.parent.parent.parent.parent.gameObject && raycastHits[0].transform.GetComponent<Point>() != null) || (raycastHits.Length > 1 && raycastHits[1].collider.GetComponent<Point>() != null && raycastHits[1].transform.parent.parent.parent.parent.gameObject != point.transform.parent.parent.parent.parent.gameObject))
            {
                RaycastHit raycastHit;

                if (raycastHits[0].transform.parent.parent.parent.parent.gameObject != point.transform.parent.parent.parent.parent.gameObject && raycastHits[0].transform.GetComponent<Point>() != null)
                {
                    raycastHit = raycastHits[0];
                }
                else
                {
                    raycastHit = raycastHits[1];
                }

                // Create Intersection
                GameObject intersection = new GameObject("Intersection");
                Undo.RegisterCreatedObjectUndo(intersection, "Create Intersection");
                intersection.transform.SetParent(transform.parent);
                intersection.transform.position = raycastHit.point;
                intersection.AddComponent<MeshFilter>();
                intersection.AddComponent<MeshRenderer>();
                intersection.AddComponent<MeshCollider>();
                intersection.GetComponent<MeshFilter>().hideFlags = HideFlags.NotEditable;
                intersection.GetComponent<MeshRenderer>().hideFlags = HideFlags.NotEditable;
                intersection.GetComponent<MeshCollider>().hideFlags = HideFlags.NotEditable;
                intersection.AddComponent<Intersection>();
                intersection.GetComponent<Intersection>().yOffset = heightOffset;

                // First connection
                point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                Vector3[] vertices = point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
                Vector3 forward = Misc.GetCenter(vertices[vertices.Length - 1], vertices[vertices.Length - 2]) - Misc.GetCenter(vertices[vertices.Length - 3], vertices[vertices.Length - 4]);
                point.transform.position += (-forward).normalized * 2;
                point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                IntersectionConnection intersectionConnection = CreateIntersectionConnection(intersection.GetComponent<Intersection>(), point.transform.parent.parent.GetComponent<RoadSegment>().endRoadWidth, point);
                point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection = intersectionConnection;
                point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection = intersection.GetComponent<Intersection>();

                // Second connection
                point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                forward = raycastHit.transform.position - raycastHit.transform.parent.GetChild(0).position;
                raycastHit.transform.position += (-forward).normalized * 2;
                raycastHit.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                intersectionConnection = CreateIntersectionConnection(intersection.GetComponent<Intersection>(), raycastHit.transform.parent.parent.GetComponent<RoadSegment>().endRoadWidth, raycastHit.transform.gameObject);
                raycastHit.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection = intersectionConnection;
                raycastHit.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection = intersection.GetComponent<Intersection>();

                intersection.GetComponent<Intersection>().GenerateMesh();
            }
            else
            {
                RaycastHit raycastHit;

                if (Physics.Raycast(point.transform.position + new Vector3(0, 1, 0), Vector3.down, out raycastHit, 100, globalSettings.ignoreMouseRayLayer))
                {
                    if (raycastHit.transform.name == "Intersection" && raycastHit.transform.GetComponent<Intersection>() != null)
                    {
                        if ((point.transform.GetSiblingIndex() == 2 && point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection != null) || (point.transform.GetSiblingIndex() == 0 && point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection != null))
                        {
                            point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                            Vector3[] vertices = point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices;
                            Vector3 forward = Misc.GetCenter(vertices[vertices.Length - 1], vertices[vertices.Length - 2]) - Misc.GetCenter(vertices[vertices.Length - 3], vertices[vertices.Length - 4]);
                            point.transform.position += (-forward).normalized * 2;
                            point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();
                            IntersectionConnection intersectionConnection = CreateIntersectionConnection(raycastHit.transform.GetComponent<Intersection>(), point.transform.parent.parent.GetComponent<RoadSegment>().endRoadWidth, point);
                            point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection = intersectionConnection;
                            point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection = raycastHit.transform.GetComponent<Intersection>();
                            raycastHit.transform.GetComponent<Intersection>().connections.Sort();
                            raycastHit.transform.GetComponent<Intersection>().GenerateMesh();
                        }
                    }
                }
            }
        }
        else
        {
            if (Event.current.alt == true && endIntersection != null && point.transform.GetSiblingIndex() == 2 && point.transform.parent.parent.GetSiblingIndex() == point.transform.parent.parent.parent.childCount - 1)
            {
                CreateMesh();
                endIntersectionConnection.leftPoint = new SerializedVector3(point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2] + point.transform.parent.parent.position);
                endIntersectionConnection.rightPoint = new SerializedVector3(point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 1] + point.transform.parent.parent.position);
                endIntersectionConnection.lastPoint = new SerializedVector3(Misc.GetCenter(endIntersectionConnection.leftPoint.ToNormalVector3(), endIntersectionConnection.rightPoint.ToNormalVector3()));
                endIntersectionConnection.length = Vector3.Distance(endIntersection.transform.position, point.transform.position);

                Vector3 center = Vector3.zero;
                for (int i = 0; i < endIntersection.connections.Count; i++)
                {
                    center += endIntersection.connections[i].lastPoint.ToNormalVector3();
                }

                endIntersection.transform.position = center / endIntersection.connections.Count;
                endIntersection.GenerateMesh();
            }
            else
            {
                if (point.transform.GetSiblingIndex() == 0 && point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection != null)
                {
                    point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection.connections.Remove(point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnection);
                    point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersection.GenerateMesh();
                    point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnection = null;
                }
                else if (point.transform.GetSiblingIndex() == 2 && point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection != null)
                {
                    for (int i = 0; i < point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection.connections.Count; i++)
                    {
                        if (point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection.connections[i].YRotation == point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection.YRotation)
                        {
                            point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection.connections.RemoveAt(i);
                        }
                    }

                    point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersection.GenerateMesh();
                    point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection = null;
                }
            }
        }
    }

    public IntersectionConnection CreateIntersectionConnection(Intersection intersection, float width, GameObject point)
    {
        IntersectionConnection intersectionConnection = new IntersectionConnection();
        intersection.connections.Add(intersectionConnection);
        
        intersectionConnection.width = width;
        intersectionConnection.leftPoint = new SerializedVector3(point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 2] + point.transform.parent.parent.position);
        intersectionConnection.rightPoint = new SerializedVector3(point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices[point.transform.parent.parent.GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh.vertices.Length - 1] + point.transform.parent.parent.position);
        intersectionConnection.lastPoint = new SerializedVector3(Misc.GetCenter(intersectionConnection.leftPoint.ToNormalVector3(), intersectionConnection.rightPoint.ToNormalVector3()));
        intersectionConnection.YRotation = Quaternion.LookRotation((intersection.transform.position - point.transform.parent.GetChild(0).position).normalized).eulerAngles.y;
        intersectionConnection.length = Vector3.Distance(intersection.transform.position, point.transform.position);

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
        if (mouseDown == true && objectToMove != null)
        {
            if (guiEvent.keyCode == KeyCode.Plus || guiEvent.keyCode == KeyCode.KeypadPlus)
            {
                Undo.RecordObject(objectToMove.transform, "Moved Point");
                objectToMove.transform.position += new Vector3(0, 0.2f, 0);

                if (extraObjectToMove != null)
                {
                    Undo.RecordObject(extraObjectToMove.transform, "Moved Point");
                    extraObjectToMove.transform.position = objectToMove.transform.position;
                }

                if (followObject != null)
                {
                    Undo.RecordObject(followObject.GetComponent<PrefabLineCreator>().objectToMove.transform, "Moved Point");
                    followObject.GetComponent<PrefabLineCreator>().objectToMove.transform.position = objectToMove.transform.position;
                }
            }
            else if (guiEvent.keyCode == KeyCode.Minus || guiEvent.keyCode == KeyCode.KeypadMinus)
            {
                Vector3 position = objectToMove.transform.position - new Vector3(0, 0.2f, 0);

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

                if (followObject != null)
                {
                    Undo.RecordObject(followObject.GetComponent<PrefabLineCreator>().objectToMove.transform, "Moved Point");
                    followObject.GetComponent<PrefabLineCreator>().objectToMove.transform.position = objectToMove.transform.position;
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

                        if (followObject != null)
                        {
                            followObject.GetComponent<PrefabLineCreator>().objectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() * 2 + 1).gameObject;
                        }
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

                        if (followObject != null)
                        {
                            followObject.GetComponent<PrefabLineCreator>().objectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() * 2).gameObject;
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

                        if (followObject != null)
                        {
                            followObject.GetComponent<PrefabLineCreator>().objectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() * 2 + 2).gameObject;
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

            if (followObject != null)
            {
                if (followObject.GetComponent<RoadCreator>() != null)
                {
                    Undo.RecordObject(followObject.GetComponent<RoadCreator>().objectToMove.transform, "Moved Point");
                    followObject.GetComponent<RoadCreator>().objectToMove.transform.position = hitPosition;

                    if (extraObjectToMove != null)
                    {
                        Undo.RecordObject(followObject.GetComponent<RoadCreator>().extraObjectToMove.transform, "Moved Point");
                        followObject.GetComponent<RoadCreator>().extraObjectToMove.transform.position = hitPosition;
                    }
                }
                else
                {
                    Undo.RecordObject(followObject.GetComponent<PrefabLineCreator>().objectToMove.transform, "Moved Point");
                    followObject.GetComponent<PrefabLineCreator>().objectToMove.transform.position = hitPosition;
                }
            }
        }
        else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && objectToMove != null)
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
                if (objectToMove.transform.GetSiblingIndex() == 2 && objectToMove.transform.parent.parent.GetSiblingIndex() == objectToMove.transform.parent.parent.parent.childCount - 1)
                {
                    CheckForIntersectionGeneration(objectToMove);
                }
            }

            objectToMove.GetComponent<BoxCollider>().enabled = true;
            objectToMove = null;

            CreateMesh();
            globalSettings.UpdateRoadGuidelines();
        }
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
            Vector3 position = Misc.Lerp3(segment.GetChild(0).GetChild(0).position, segment.GetChild(0).GetChild(1).position, segment.GetChild(0).GetChild(2).position, t);
            position.y = Mathf.Lerp(segment.GetChild(0).GetChild(0).position.y, segment.GetChild(0).GetChild(2).position.y, t);

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
