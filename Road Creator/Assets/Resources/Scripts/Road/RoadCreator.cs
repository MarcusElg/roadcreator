using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;

public class RoadCreator : MonoBehaviour
{

    public RoadSegment currentSegment;
    public float heightOffset = 0.02f;
    public int smoothnessAmount = 3;

    public GlobalSettings globalSettings;

    public Preset segmentPreset;

    public GameObject followObject;
    public Vector3 lastMoveObjectPosition;

    public bool isFollowObject = false;

    public GameObject objectToMove = null;
    public GameObject extraObjectToMove = null;
    private bool mouseDown;

    public void CreateMesh()
    {
        DetectIntersectionConnections();
        Vector3[] currentPoints = null;

        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            if (transform.GetChild(0).GetChild(i).GetChild(0).childCount == 3)
            {
                Vector3 previousPoint = Misc.MaxVector3;

                if (i == 0)
                {
                    currentPoints = CalculatePoints(transform.GetChild(0).GetChild(i));

                    if (transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection != null)
                    {
                        previousPoint = GetIntersectionPoint(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).transform.position + new Vector3(0, heightOffset, 0), transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection.transform.parent.parent.gameObject, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection.name);
                    }
                }

                if (i < transform.GetChild(0).childCount - 1 && transform.GetChild(0).GetChild(i + 1).GetChild(0).childCount == 3)
                {
                    Vector3[] nextPoints = CalculatePoints(transform.GetChild(0).GetChild(i + 1));
                    Vector3 originalControlPoint = currentPoints[currentPoints.Length - 1];

                    int actualSmoothnessAmount = smoothnessAmount;
                    if ((currentPoints.Length / 2) <= actualSmoothnessAmount)
                    {
                        actualSmoothnessAmount = currentPoints.Length / 2 - 1;
                    }

                    if ((nextPoints.Length / 2) <= actualSmoothnessAmount)
                    {
                        actualSmoothnessAmount = nextPoints.Length / 2 - 1;
                    }

                    float distanceSection = 1f / ((actualSmoothnessAmount * 2));
                    for (float t = distanceSection; t < 0.5; t += distanceSection)
                    {
                        // First sectiond
                        currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount + (int)(t * 2 * actualSmoothnessAmount)] = Misc.Lerp3(currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount], originalControlPoint, nextPoints[actualSmoothnessAmount], t);

                        // Second section
                        nextPoints[actualSmoothnessAmount - (int)(t * 2 * actualSmoothnessAmount)] = Misc.Lerp3(currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount], originalControlPoint, nextPoints[actualSmoothnessAmount], 1 - t);
                    }

                    // First and last points
                    currentPoints[currentPoints.Length - 1] = Misc.Lerp3(currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount], originalControlPoint, nextPoints[actualSmoothnessAmount], 0.5f);
                    nextPoints[0] = Misc.Lerp3(currentPoints[currentPoints.Length - 1 - actualSmoothnessAmount], originalControlPoint, nextPoints[actualSmoothnessAmount], 0.5f);
                    transform.GetChild(0).GetChild(i).GetComponent<RoadSegment>().CreateRoadMesh(currentPoints, nextPoints, previousPoint, heightOffset, transform.GetChild(0).GetChild(i), actualSmoothnessAmount, this);
                    StartCoroutine(FixTextureStretch(Misc.CalculateDistance(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(1).position, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).position), i));
                    currentPoints = nextPoints;
                }
                else
                {
                    Vector3[] nextPoints = null;

                    if (transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection != null)
                    {
                        nextPoints = new Vector3[1];
                        nextPoints[0] = GetIntersectionPoint(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).transform.position + new Vector3(0, heightOffset, 0), transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection.transform.parent.parent.gameObject, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection.name);
                    }

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

    private void DetectIntersectionConnections()
    {
        if (transform.GetChild(0).childCount > 0)
        {
            DetectIntersectionConnection(transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject);

            Transform lastSegment = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1);
            if (lastSegment.GetChild(0).childCount == 3)
            {
                DetectIntersectionConnection(lastSegment.transform.GetChild(0).GetChild(2).gameObject);
            }
        }
    }

    private void DetectIntersectionConnection(GameObject gameObject)
    {
        RaycastHit raycastHit2;
        if (Physics.Raycast(gameObject.transform.position + Vector3.up, Vector3.down, out raycastHit2, 100f, ~(1 << globalSettings.ignoreMouseRayLayer)))
        {
            if (raycastHit2.collider.name.Contains("Connection Point"))
            {
                // Change width/height
                SquareIntersection squareIntersection = raycastHit2.collider.transform.parent.parent.parent.GetComponent<SquareIntersection>();
                TriangleIntersection triangleIntersection = raycastHit2.collider.transform.parent.parent.parent.GetComponent<TriangleIntersection>();
                DiamondIntersection diamondIntersection = raycastHit2.collider.transform.parent.parent.parent.GetComponent<DiamondIntersection>();
                Roundabout roundabout = raycastHit2.collider.transform.parent.parent.parent.GetComponent<Roundabout>();
                RoadSplitter roadSplitter = raycastHit2.collider.transform.parent.parent.GetComponent<RoadSplitter>();
                string connectionName = raycastHit2.collider.name;

                if ((roadSplitter != null && raycastHit2.collider.transform.parent.parent.GetChild(1).GetComponent<MeshFilter>().sharedMesh != null) || raycastHit2.collider.transform.parent.GetChild(0).GetComponent<MeshFilter>().sharedMesh != null)
                {
                    gameObject.GetComponent<Point>().intersectionConnection = raycastHit2.collider.gameObject;
                    gameObject.transform.position = raycastHit2.collider.transform.position;

                    float roadWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().startRoadWidth;
                    if (gameObject.name == "End Point")
                    {
                        roadWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().endRoadWidth;
                    }

                    if (squareIntersection != null)
                    {
                        if (connectionName == "Up Connection Point")
                        {
                            squareIntersection.upConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Down Connection Point")
                        {
                            squareIntersection.downConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Left Connection Point")
                        {
                            squareIntersection.leftConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Right Connection Point")
                        {
                            squareIntersection.rightConnectionWidth = roadWidth;
                        }

                        squareIntersection.GenerateMeshes();
                    }
                    else if (triangleIntersection != null)
                    {
                        if (connectionName == "Down Connection Point")
                        {
                            triangleIntersection.downConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Left Connection Point")
                        {
                            triangleIntersection.leftConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Right Connection Point")
                        {
                            triangleIntersection.rightConnectionWidth = roadWidth;
                        }

                        triangleIntersection.GenerateMeshes();
                    }
                    else if (diamondIntersection != null)
                    {
                        if (connectionName == "Upper Left Connection Point")
                        {
                            diamondIntersection.upperLeftConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Upper Right Connection Point")
                        {
                            diamondIntersection.upperRightConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Lower Left Connection Point")
                        {
                            diamondIntersection.lowerLeftConnectionWidth = roadWidth;
                        }
                        else if (connectionName == "Lower Right Connection Point")
                        {
                            diamondIntersection.lowerRightConnectionWidth = roadWidth;
                        }

                        diamondIntersection.GenerateMeshes();
                    }
                    else if (roundabout != null)
                    {
                        roundabout.connectionWidth[raycastHit2.transform.GetSiblingIndex() - 1] = roadWidth;

                        roundabout.GenerateMeshes();
                    }
                    else if (roadSplitter != null)
                    {
                        if (connectionName == "Left Connection Point")
                        {
                            roadSplitter.leftWidth = roadWidth;
                        }
                        else if (connectionName == "Lower Right Connection Point")
                        {
                            roadSplitter.lowerRightXOffset = -roadSplitter.rightWidth + roadWidth;
                        }
                        else if (connectionName == "Upper Right Connection Point")
                        {
                            roadSplitter.upperRightXOffset = roadSplitter.rightWidth - roadWidth;
                        }
                        roadSplitter.GenerateMesh();
                    }
                }
            }
            else
            {
                gameObject.GetComponent<Point>().intersectionConnection = null;
            }
        }
    }

    private Vector3 GetIntersectionPoint(Vector3 position, GameObject intersection, string connectionPointName)
    {
        SquareIntersection squareIntersection = intersection.transform.parent.GetComponent<SquareIntersection>();
        TriangleIntersection triangleIntersection = intersection.transform.parent.GetComponent<TriangleIntersection>();
        DiamondIntersection diamondIntersection = intersection.transform.parent.GetComponent<DiamondIntersection>();
        Roundabout roundabout = intersection.transform.parent.GetComponent<Roundabout>();
        RoadSplitter roadSplitter = intersection.GetComponent<RoadSplitter>();

        if (squareIntersection != null)
        {
            return intersection.transform.position + new Vector3(0, squareIntersection.heightOffset - position.y, 0);
        }
        else if (roundabout != null)
        {
            return roundabout.transform.position + new Vector3(0, roundabout.heightOffset - position.y, 0);
        }
        else if (triangleIntersection != null)
        {
            if (connectionPointName == "Down Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * new Vector3(0, triangleIntersection.heightOffset - position.y, -triangleIntersection.height);
            }
            else if (connectionPointName == "Left Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * Misc.GetCenter(new Vector3(-triangleIntersection.width, triangleIntersection.heightOffset - position.y, -triangleIntersection.height), new Vector3(0, triangleIntersection.heightOffset - position.y, triangleIntersection.height));
            }
            else if (connectionPointName == "Right Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * Misc.GetCenter(new Vector3(triangleIntersection.width, triangleIntersection.heightOffset - position.y, -triangleIntersection.height), new Vector3(0, triangleIntersection.heightOffset - position.y, triangleIntersection.height));
            }
        }
        else if (diamondIntersection != null)
        {
            if (connectionPointName == "Upper Left Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset - position.y, diamondIntersection.height), new Vector3(-diamondIntersection.width, diamondIntersection.heightOffset - position.y, 0));
            }
            else if (connectionPointName == "Upper Right Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset - position.y, diamondIntersection.height), new Vector3(diamondIntersection.width, diamondIntersection.heightOffset - position.y, 0));
            }
            else if (connectionPointName == "Lower Left Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset - position.y, -diamondIntersection.height), new Vector3(-diamondIntersection.width, diamondIntersection.heightOffset - position.y, 0));
            }
            else if (connectionPointName == "Lower Right Connection Point")
            {
                return intersection.transform.position + intersection.transform.rotation * Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset - position.y, -diamondIntersection.height), new Vector3(diamondIntersection.width, diamondIntersection.heightOffset - position.y, 0));
            }
        }
        else if (roadSplitter != null)
        {
            if (connectionPointName == "Left Connection Point")
            {
                return intersection.transform.position + roadSplitter.transform.forward;
            }
            else if (connectionPointName == "Upper Right Connection Point")
            {
                Vector3 up = (roadSplitter.transform.GetChild(0).GetChild(1).position - roadSplitter.transform.GetChild(0).GetChild(2).position).normalized;
                Vector3 left = new Vector3(-up.z, 0, up.x);
                return roadSplitter.transform.GetChild(0).GetChild(1).position + left - new Vector3(0, roadSplitter.heightOffset - position.y, 0);
            }
            else if (connectionPointName == "Lower Right Connection Point")
            {
                Vector3 up = (roadSplitter.transform.GetChild(0).GetChild(1).position - roadSplitter.transform.GetChild(0).GetChild(2).position).normalized;
                Vector3 left = new Vector3(-up.z, 0, up.x);
                return roadSplitter.transform.GetChild(0).GetChild(2).position + left - new Vector3(0, roadSplitter.heightOffset - position.y, 0);
            }
        }

        return Misc.MaxVector3;
    }

    IEnumerator FixTextureStretch(float length, int i)
    {
        yield return new WaitForSeconds(0.01f);

        if (transform.GetChild(0).childCount > i)
        {
            float textureRepeat = length / 4;

            for (int j = 0; j < 3; j++)
            {
                if (transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial != null)
                {
                    Material material = new Material(transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial);
                    material.SetVector("Vector2_79C0D9A3", new Vector2(1, textureRepeat));
                    transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
        }
    }

    public void UndoUpdate()
    {
        if (currentSegment != null && currentSegment.transform.GetChild(0).childCount == 3)
        {
            currentSegment = null;
        }

        if (transform.GetChild(0).childCount > 0)
        {
            Transform lastSegment = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1);
            if (transform.GetChild(0).childCount > 0 && lastSegment.GetChild(0).childCount < 3)
            {
                currentSegment = lastSegment.GetComponent<RoadSegment>();
            }
        }

        CreateMesh();

        if (followObject != null)
        {
            if (followObject.GetComponent<RoadCreator>() != null)
            {
                followObject.GetComponent<RoadCreator>().UndoUpdate();
            } else
            {
                followObject.GetComponent<PrefabLineCreator>().UndoUpdate();
            }
        }
    }

    public void CreatePoints(Vector3 hitPosition)
    {
        if (currentSegment != null)
        {
            if (currentSegment.transform.GetChild(0).childCount == 1)
            {
                if (globalSettings.roadCurved == true)
                {
                    // Create control point
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", currentSegment.transform.GetChild(0), hitPosition), "Created point");
                }
                else
                {
                    // Create control and end points
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", currentSegment.transform.GetChild(0), Misc.GetCenter(currentSegment.transform.GetChild(0).GetChild(0).position, hitPosition)), "Created point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", currentSegment.transform.GetChild(0), hitPosition), "Created point");
                    currentSegment = null;
                    CreateMesh();
                }
            }
            else if (currentSegment.transform.GetChild(0).childCount == 2)
            {
                // Create end point
                Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", currentSegment.transform.GetChild(0), hitPosition), "Created Point");
                currentSegment = null;
                CreateMesh();
            }
        }
        else
        {
            if (transform.GetChild(0).childCount == 0)
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
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", currentSegment.transform.GetChild(0), Misc.GetCenter(currentSegment.transform.GetChild(0).GetChild(0).position, hitPosition)), "Created point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", currentSegment.transform.GetChild(0), hitPosition), "Created point");
                    currentSegment = null;
                    CreateMesh();
                }
            }
        }
    }

    private GameObject CreatePoint(string name, Transform parent, Vector3 position)
    {
        GameObject point = new GameObject(name);
        point.gameObject.AddComponent<BoxCollider>();
        point.GetComponent<BoxCollider>().size = new Vector3(globalSettings.pointSize, globalSettings.pointSize, globalSettings.pointSize);

        if (isFollowObject == true)
        {
            point.GetComponent<BoxCollider>().enabled = false;
        }

        point.transform.SetParent(parent);
        point.transform.position = position;
        point.hideFlags = HideFlags.NotEditable;
        point.layer = globalSettings.ignoreMouseRayLayer;
        point.AddComponent<Point>();
        return point;
    }

    private RoadSegment CreateSegment(Vector3 position)
    {
        RoadSegment segment = new GameObject("Segment").AddComponent<RoadSegment>();
        segment.transform.SetParent(transform.GetChild(0), false);
        segment.transform.position = position;
        segment.transform.hideFlags = HideFlags.NotEditable;

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

                segment.leftShoulderMaterial = oldLastSegment.leftShoulderMaterial;
                segment.leftShoulder = oldLastSegment.leftShoulder;
                segment.leftShoulderWidth = oldLastSegment.leftShoulderWidth;
                segment.leftShoulderHeightOffset = oldLastSegment.leftShoulderHeightOffset;

                segment.rightShoulderMaterial = oldLastSegment.rightShoulderMaterial;
                segment.rightShoulder = oldLastSegment.rightShoulder;
                segment.rightShoulderWidth = oldLastSegment.rightShoulderWidth;
                segment.rightShoulderHeightOffset = oldLastSegment.rightShoulderHeightOffset;
            }
            else
            {
                segment.roadMaterial = Resources.Load("Materials/Roads/2 Lane Roads/2L Road") as Material;
                segment.leftShoulderMaterial = Resources.Load("Materials/Asphalt") as Material;
                segment.rightShoulderMaterial = Resources.Load("Materials/Asphalt") as Material;
            }
        }
        else
        {
            segmentPreset.ApplyTo(segment);
        }

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

        GameObject leftShoulderMesh = new GameObject("Left Shoulder Mesh");
        leftShoulderMesh.transform.SetParent(meshes.transform);
        leftShoulderMesh.transform.localPosition = Vector3.zero;
        leftShoulderMesh.hideFlags = HideFlags.NotEditable;
        leftShoulderMesh.AddComponent<MeshRenderer>();
        leftShoulderMesh.AddComponent<MeshFilter>();
        leftShoulderMesh.AddComponent<MeshCollider>();
        leftShoulderMesh.layer = globalSettings.roadLayer;

        GameObject rightShoulderMesh = new GameObject("Right Shoulder Mesh");
        rightShoulderMesh.transform.SetParent(meshes.transform);
        rightShoulderMesh.transform.localPosition = Vector3.zero;
        rightShoulderMesh.hideFlags = HideFlags.NotEditable;
        rightShoulderMesh.AddComponent<MeshRenderer>();
        rightShoulderMesh.AddComponent<MeshFilter>();
        rightShoulderMesh.AddComponent<MeshCollider>();
        rightShoulderMesh.layer = globalSettings.roadLayer;

        currentSegment = segment;

        return segment;
    }

    public bool IsLastSegmentCurved ()
    {
        if (transform.GetChild(0).childCount > 0)
        {
            return transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved;
        } else
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
                if (currentSegment != null)
                {
                    if (currentSegment.transform.GetChild(0).childCount == 2)
                    {
                        Undo.DestroyObjectImmediate(currentSegment.transform.GetChild(0).GetChild(1).gameObject);
                    }
                    else if (currentSegment.transform.GetChild(0).childCount == 1)
                    {
                        Undo.DestroyObjectImmediate(currentSegment.gameObject);

                        if (transform.GetChild(0).childCount > 0)
                        {
                            if (transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved == false)
                            {
                                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(1).gameObject);
                                currentSegment = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
                                CreateMesh();
                            }
                            else
                            {
                                if (transform.GetChild(0).childCount > 0)
                                {
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshFilter>().sharedMesh = null;
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshCollider>().sharedMesh = null;
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshFilter>().sharedMesh = null;
                                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshCollider>().sharedMesh = null;

                                    Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                                    currentSegment = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
                                }
                            }
                        }
                        else
                        {
                            currentSegment = null;
                        }
                    }
                }
                else
                {
                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshFilter>().sharedMesh = null;
                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshCollider>().sharedMesh = null;
                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshFilter>().sharedMesh = null;
                    transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshCollider>().sharedMesh = null;

                    Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                    currentSegment = transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
                }
            }
            else
            {
                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).gameObject);
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
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            mouseDown = true;
            if (objectToMove == null)
            {
                if (raycastHit.transform.name.Contains("Point") && raycastHit.collider.transform.parent.parent.parent.parent.gameObject != null && raycastHit.collider.transform.parent.parent.parent.parent.GetComponent<RoadCreator>() != null && raycastHit.collider.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().isFollowObject == false)
                {
                    if (raycastHit.collider.gameObject.name == "Control Point")
                    {
                        objectToMove = raycastHit.collider.gameObject;
                        objectToMove.GetComponent<BoxCollider>().enabled = false;

                        if (followObject != null && followObject.GetComponent<RoadCreator>() != null)
                        {
                            followObject.GetComponent<RoadCreator>().objectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex()).GetChild(0).GetChild(1).gameObject;
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

                        if (followObject != null && followObject.GetComponent<RoadCreator>() != null)
                        {
                            followObject.GetComponent<RoadCreator>().objectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex()).GetChild(0).GetChild(0).gameObject;

                            if (extraObjectToMove != null)
                            { 
                                followObject.GetComponent<RoadCreator>().extraObjectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() - 1).GetChild(0).GetChild(2).gameObject;
                            }
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

                        if (followObject != null && followObject.GetComponent<RoadCreator>() != null)
                        {
                            followObject.GetComponent<RoadCreator>().objectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex()).GetChild(0).GetChild(2).gameObject;

                            if (extraObjectToMove != null)
                            {
                                followObject.GetComponent<RoadCreator>().extraObjectToMove = followObject.transform.GetChild(0).GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() + 1).GetChild(0).GetChild(0).gameObject;
                            }
                        }
                    }

                }
            }
        }
        else if (guiEvent.type == EventType.MouseDrag && objectToMove != null)
        {
            {
                Undo.RecordObject(objectToMove.transform, "Moved Point");
                objectToMove.transform.position = hitPosition;

                if (extraObjectToMove != null)
                {
                    Undo.RecordObject(extraObjectToMove.transform, "Moved Point");
                    extraObjectToMove.transform.position = hitPosition;
                }

                if (followObject != null && followObject.GetComponent<RoadCreator>() != null)
                {
                    Undo.RecordObject(followObject.GetComponent<RoadCreator>().objectToMove.transform, "Moved Point");
                    followObject.GetComponent<RoadCreator>().objectToMove.transform.position = hitPosition;

                    if (extraObjectToMove != null)
                    {
                        Undo.RecordObject(followObject.GetComponent<RoadCreator>().extraObjectToMove.transform, "Moved Point");
                        followObject.GetComponent<RoadCreator>().extraObjectToMove.transform.position = hitPosition;
                    }
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

            if (isFollowObject == false)
            {
                objectToMove.GetComponent<BoxCollider>().enabled = true;
            }

            objectToMove = null;

            if (extraObjectToMove != null)
            {
                if (extraObjectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
                {
                    if (extraObjectToMove.transform.parent.childCount == 3)
                    {
                        extraObjectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(extraObjectToMove.transform.parent.GetChild(0).position, extraObjectToMove.transform.parent.GetChild(2).position);
                    }
                }

                if (isFollowObject == false)
                {
                    extraObjectToMove.GetComponent<BoxCollider>().enabled = true;
                    extraObjectToMove = null;
                }
            }

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

        for (float t = 0; t <= 1; t += distancePerDivision / 10)
        {
            if (t > 1 - distancePerDivision)
            {
                t = 1;
            }

            Vector3 position = Misc.Lerp3(segment.GetChild(0).GetChild(0).position, segment.GetChild(0).GetChild(1).position, segment.GetChild(0).GetChild(2).position, t);
            float calculatedDistance = Vector3.Distance(position, lastPosition);
            if (calculatedDistance > globalDistancePerDivision)
            {
                lastPosition = position;

                points.Add(RaycastedPosition(position, segment.GetComponent<RoadSegment>()));
            }
        }

        if (Vector3.Distance(lastPosition, segment.transform.GetChild(0).GetChild(2).position) > (distancePerDivision * divisions) / 2)
        {
            points.Add(RaycastedPosition(segment.transform.GetChild(0).GetChild(2).position, segment.GetComponent<RoadSegment>()));
        }
        else
        {
            points[points.Count - 1] = RaycastedPosition(segment.transform.GetChild(0).GetChild(2).position, segment.GetComponent<RoadSegment>());
        }

        return points.ToArray();
    }

    public Vector3 RaycastedPosition(Vector3 originalPosition, RoadSegment segment)
    {
        if (segment.terrainOption == RoadSegment.TerrainOption.adapt)
        {
            RaycastHit raycastHit;
            if (Physics.Raycast(originalPosition + new Vector3(0, 10, 0), Vector3.down, out raycastHit, 100f, ~((1 << globalSettings.ignoreMouseRayLayer) | (1 << globalSettings.roadLayer) | (1 << globalSettings.intersectionPointsLayer))))
            {
                return raycastHit.point;
            }
        }

        return originalPosition;
    }

}
