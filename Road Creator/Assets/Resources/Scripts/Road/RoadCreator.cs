using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadCreator : MonoBehaviour
{

    public RoadSegment currentSegment;
    public float heightOffset = 0.02f;
    public int smoothnessAmount = 3;

    public GlobalSettings globalSettings;

    public void UpdateMesh()
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
                        previousPoint = GetIntersectionPoint(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection.transform.parent.parent.gameObject, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<Point>().intersectionConnection.name);
                    }
                }
                else
                {
                    previousPoint = Misc.MaxVector3;
                }

                if (i < transform.GetChild(0).childCount - 1 && transform.GetChild(0).GetChild(i + 1).GetChild(0).childCount == 3)
                {
                    Vector3[] nextPoints = CalculatePoints(transform.GetChild(0).GetChild(i + 1));
                    Vector3 originalControlPoint = currentPoints[currentPoints.Length - 1];

                    int actualSmoothnessAmount = smoothnessAmount;
                    if ((currentPoints.Length / 2) < actualSmoothnessAmount)
                    {
                        actualSmoothnessAmount = currentPoints.Length / 2;
                    }

                    if ((nextPoints.Length / 2) < actualSmoothnessAmount)
                    {
                        actualSmoothnessAmount = nextPoints.Length / 2;
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
                    //currentPoints[currentPoints.Length - actualSmoothnessAmount - 2] = Misc.GetCenter(currentPoints[currentPoints.Length - actualSmoothnessAmount - 3], currentPoints[currentPoints.Length - actualSmoothnessAmount - 1]);
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
                        nextPoints[0] = GetIntersectionPoint(transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection.transform.parent.parent.gameObject, transform.GetChild(0).GetChild(i).GetChild(0).GetChild(2).GetComponent<Point>().intersectionConnection.name);
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
        if (Physics.Raycast(new Ray(gameObject.transform.position + Vector3.up, Vector3.down), out raycastHit2, 100f, ~(1 << globalSettings.ignoreMouseRayLayer)))
        {
            if (raycastHit2.collider.name.Contains("Connection Point"))
            {
                gameObject.GetComponent<Point>().intersectionConnection = raycastHit2.collider.gameObject;
                gameObject.transform.position = raycastHit2.collider.transform.position;

                // Change width/height
                SquareIntersection squareIntersection = raycastHit2.collider.transform.parent.parent.parent.GetComponent<SquareIntersection>();
                TriangleIntersection triangleIntersection = raycastHit2.collider.transform.parent.parent.parent.GetComponent<TriangleIntersection>();
                DiamondIntersection diamondIntersection = raycastHit2.collider.transform.parent.parent.parent.GetComponent<DiamondIntersection>();
                Roundabout roundabout = raycastHit2.collider.transform.parent.parent.parent.GetComponent<Roundabout>();
                RoadSplitter roadSplitter = raycastHit2.collider.transform.parent.parent.GetComponent<RoadSplitter>();
                RoadTransition roadTransition = raycastHit2.collider.transform.parent.parent.GetComponent<RoadTransition>();
                string connectionName = raycastHit2.collider.name;

                if (squareIntersection != null)
                {
                    if (connectionName == "Up Connection Point")
                    {
                        squareIntersection.upConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Down Connection Point")
                    {
                        squareIntersection.downConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Left Connection Point")
                    {
                        squareIntersection.leftConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Right Connection Point")
                    {
                        squareIntersection.rightConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }

                    squareIntersection.GenerateMeshes();
                }
                else if (triangleIntersection != null)
                {
                    if (connectionName == "Down Connection Point")
                    {
                        triangleIntersection.downConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Left Connection Point")
                    {
                        triangleIntersection.leftConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Right Connection Point")
                    {
                        triangleIntersection.rightConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }

                    triangleIntersection.GenerateMeshes();
                }
                else if (diamondIntersection != null)
                {
                    if (connectionName == "Upper Left Connection Point")
                    {
                        diamondIntersection.upperLeftConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Upper Right Connection Point")
                    {
                        diamondIntersection.upperRightConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Lower Left Connection Point")
                    {
                        diamondIntersection.lowerLeftConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }
                    else if (connectionName == "Lower Right Connection Point")
                    {
                        diamondIntersection.lowerRightConnectionWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }

                    diamondIntersection.GenerateMeshes();
                }
                else if (roundabout != null)
                {
                    roundabout.connectionWidth[raycastHit2.transform.GetSiblingIndex() - 1] = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;

                    roundabout.GenerateMeshes();
                } else if (roadSplitter != null && connectionName == "Left Connection Point")
                {
                    roadSplitter.leftWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;

                    roadSplitter.GenerateMesh();
                } else if (roadTransition != null)
                {
                    if (connectionName == "Left Connection Point")
                    {
                        roadTransition.leftWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    } else if (connectionName == "Right Connection Point")
                    {
                        roadTransition.rightWidth = gameObject.transform.parent.parent.GetComponent<RoadSegment>().roadWidth;
                    }

                    roadTransition.GenerateMesh();
                }
            }
            else
            {
                gameObject.GetComponent<Point>().intersectionConnection = null;
            }
        }
    }

    private Vector3 GetIntersectionPoint(GameObject intersection, string connectionPointName)
    {
        SquareIntersection squareIntersection = intersection.transform.parent.GetComponent<SquareIntersection>();
        TriangleIntersection triangleIntersection = intersection.transform.parent.GetComponent<TriangleIntersection>();
        DiamondIntersection diamondIntersection = intersection.transform.parent.GetComponent<DiamondIntersection>();
        Roundabout roundabout = intersection.transform.parent.GetComponent<Roundabout>();
        RoadSplitter roadSplitter = intersection.GetComponent<RoadSplitter>();
        RoadTransition roadTransition = intersection.GetComponent<RoadTransition>();

        if (squareIntersection != null || roundabout != null)
        {
            return intersection.transform.position;
        }
        else if (triangleIntersection != null)
        {
            if (connectionPointName == "Down Connection Point")
            {
                return intersection.transform.position + new Vector3(0, 0, -triangleIntersection.height);
            }
            else if (connectionPointName == "Left Connection Point")
            {
                return intersection.transform.position + Misc.GetCenter(new Vector3(-triangleIntersection.width, triangleIntersection.heightOffset, -triangleIntersection.height), new Vector3(0, triangleIntersection.heightOffset, triangleIntersection.height));
            }
            else if (connectionPointName == "Right Connection Point")
            {
                return intersection.transform.position + Misc.GetCenter(new Vector3(triangleIntersection.width, triangleIntersection.heightOffset, -triangleIntersection.height), new Vector3(0, triangleIntersection.heightOffset, triangleIntersection.height));
            }
        }
        else if (diamondIntersection != null)
        {
            if (connectionPointName == "Upper Left Connection Point")
            {
                return intersection.transform.position + Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset, diamondIntersection.height), new Vector3(-diamondIntersection.width, diamondIntersection.heightOffset, 0));
            }
            else if (connectionPointName == "Upper Right Connection Point")
            {
                return intersection.transform.position + Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset, diamondIntersection.height), new Vector3(diamondIntersection.width, diamondIntersection.heightOffset, 0));
            }
            else if (connectionPointName == "Lower Left Connection Point")
            {
                return intersection.transform.position + Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset, -diamondIntersection.height), new Vector3(-diamondIntersection.width, diamondIntersection.heightOffset, 0));
            }
            else if (connectionPointName == "Lower Right Connection Point")
            {
                return intersection.transform.position + Misc.GetCenter(new Vector3(0, diamondIntersection.heightOffset, -diamondIntersection.height), new Vector3(diamondIntersection.width, diamondIntersection.heightOffset, 0));
            }
        } else if (roadSplitter != null)
        {
            if (connectionPointName == "Left Connection Point")
            {
                Vector3 center = roadSplitter.transform.forward * roadSplitter.height * 0.5f;
                return intersection.transform.position + center;
            } else if (connectionPointName == "Upper Right Connection Point")
            {
                Vector3 center = roadSplitter.transform.forward * roadSplitter.height * 0.5f;
                Vector3 right = roadSplitter.transform.right * roadSplitter.rightWidth * 0.5f;
                return intersection.transform.position + center - right + new Vector3(0, heightOffset, 0);
            } else if (connectionPointName == "Lower Right Connection Point")
            {
                Vector3 center = roadSplitter.transform.forward * roadSplitter.height * 0.5f;
                Vector3 right = roadSplitter.transform.right * roadSplitter.rightWidth * 0.5f;
                return intersection.transform.position + center + right + new Vector3(0, heightOffset, 0);
            }
        } else if (roadTransition != null) {
            Vector3 center = roadTransition.transform.forward * roadTransition.height * 0.5f;
            return intersection.transform.position + center;
        }

        return Misc.MaxVector3;
    }

    IEnumerator FixTextureStretch(float length, int i)
    {
        yield return new WaitForSeconds(0.01f);

        if (transform.GetChild(0).childCount > i)
        {
            float textureRepeat = length * globalSettings.resolution;

            for (int j = 0; j < 3; j++)
            {
                Material material = new Material(transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial);
                material.mainTextureScale = new Vector2(1, textureRepeat);
                transform.GetChild(0).GetChild(i).GetChild(1).GetChild(j).GetComponent<MeshRenderer>().sharedMaterial = material;
            }
        }
    }

    public Vector3[] CalculatePoints(Transform segment)
    {
        float divisions = Misc.CalculateDistance(segment.GetChild(0).GetChild(0).position, segment.GetChild(0).GetChild(1).position, segment.GetChild(0).GetChild(2).position);
        divisions = Mathf.Max(2, divisions);
        List<Vector3> points = new List<Vector3>();
        float distancePerDivision = 1 / divisions;

        for (float t = 0; t <= 1; t += distancePerDivision)
        {
            if (t > 1 - distancePerDivision)
            {
                t = 1;
            }

            Vector3 position = Misc.Lerp3(segment.GetChild(0).GetChild(0).position, segment.GetChild(0).GetChild(1).position, segment.GetChild(0).GetChild(2).position, t);

            if (segment.GetComponent<RoadSegment>().terrainOption == RoadSegment.TerrainOption.adapt)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(position + new Vector3(0, 10, 0), Vector3.down, out raycastHit, 100f, ~((1 << globalSettings.ignoreMouseRayLayer) | (1 << globalSettings.roadLayer) | (1 << globalSettings.intersectionPointsLayer))))
                {
                    position.y = raycastHit.point.y;
                }
            }

            points.Add(position);
        }

        return points.ToArray();
    }

}
