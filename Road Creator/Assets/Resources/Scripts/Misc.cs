using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Misc
{

    public static Vector3 MaxVector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    public static Color lightGreen = new Color(0.34f, 1, 0.44f);
    public static Color darkGreen = new Color(0.11f, 0.35f, 0.13f);

    public static Vector3 Lerp3(Vector3 start, Vector3 middle, Vector3 end, float time)
    {
        Vector3 startToMiddle = Vector3.Lerp(start, middle, time);
        return Vector3.Lerp(startToMiddle, end, time);
    }

    public static Vector3 Round(Vector3 toRound)
    {
        return new Vector3(Mathf.Round(toRound.x), Mathf.Round(toRound.y), Mathf.Round(toRound.z));
    }

    public static float CalculateDistance(Vector3 startPosition, Vector3 controlPosition, Vector3 endPosition)
    {
        float distance = 0;
        for (float t = 0.1f; t <= 1.1f; t += 0.1f)
        {
            distance += Vector3.Distance(Lerp3(startPosition, controlPosition, endPosition, t), Lerp3(startPosition, controlPosition, endPosition, t - 0.1f));
        }

        return distance;
    }

    public static Vector3 CalculateLeft(Vector3 point, Vector3 nextPoint)
    {
        Vector3 forward = (nextPoint - point).normalized;
        return new Vector3(-forward.z, 0, forward.x);
    }

    public static Vector3 CalculateLeft(Vector3[] points, Vector3[] nextSegmentPoints, Vector3 prevoiusPoint, int index, bool circle = false)
    {
        Vector3 forward;
        if (index < points.Length - 1)
        {
            if (index == 0 && prevoiusPoint != MaxVector3)
            {
                forward = points[0] - prevoiusPoint;
            }
            else
            {
                forward = points[index + 1] - points[index];
            }
        }
        else
        {
            if (circle == true)
            {
                forward = points[1] - points[points.Length - 1];
            }
            else
            {
                // Last vertices
                if (nextSegmentPoints != null)
                {
                    if (nextSegmentPoints.Length > 1)
                    {
                        forward = nextSegmentPoints[1] - points[points.Length - 1];
                    }
                    else
                    {
                        forward = nextSegmentPoints[0] - points[points.Length - 1];
                    }
                }
                else
                {
                    forward = points[index] - points[index - 1];
                }
            }
        }
        forward.Normalize();

        return new Vector3(-forward.z, 0, forward.x);
    }

    public static Vector3 GetCenter(Vector3 one, Vector3 two)
    {
        Vector3 difference = two - one;
        return (one + (difference / 2));
    }

    public static float GetPrefabOffset(GameObject prefab, float scale, float offset)
    {
        Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        return ((mesh.bounds.size.x * scale) - offset) / 2;
    }

    public static Vector3 FindPointInCircle(float radius, int i, float degreesPerStep)
    {
        return Quaternion.AngleAxis(degreesPerStep * i, Vector3.up) * (Vector3.right * radius);
    }

    public static Vector3 GetNearestGuidelinePoint(Vector3 hitPosition)
    {
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        Vector3 nearest = MaxVector3;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < roadSegments.Length; i++)
        {
            Vector3[] guidelines = new Vector3[roadSegments[i].startGuidelinePoints.Length + roadSegments[i].centerGuidelinePoints.Length + roadSegments[i].endGuidelinePoints.Length];
            roadSegments[i].startGuidelinePoints.CopyTo(guidelines, 0);
            roadSegments[i].centerGuidelinePoints.CopyTo(guidelines, roadSegments[i].startGuidelinePoints.Length);
            roadSegments[i].endGuidelinePoints.CopyTo(guidelines, roadSegments[i].centerGuidelinePoints.Length);
            Vector3 nearestVector = MaxVector3;
            float nearestDistanceInSegment = float.MaxValue;

            if (guidelines != null)
            {
                for (int j = 0; j < guidelines.Length; j++)
                {
                    float distance = Vector3.Distance(hitPosition, guidelines[j]);
                    if (distance < 1f && distance < nearestDistanceInSegment)
                    {
                        nearestVector = guidelines[j];
                        nearestDistanceInSegment = distance;
                    }
                }
            }

            if (nearestDistanceInSegment < nearestDistance)
            {
                nearestDistance = nearestDistanceInSegment;
                nearest = nearestVector;
            }
        }

        return nearest;
    }

    public static void DrawRoadGuidelines (Vector3 mousePosition, GameObject objectToMove, GameObject extraObjectToMove)
    {
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        for (int i = 0; i < roadSegments.Length; i++)
        {
            if (roadSegments[i].transform.GetChild(0).childCount == 3)
            {
                DrawRoadGuidelines(roadSegments[i].startGuidelinePoints, 0, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
                DrawRoadGuidelines(roadSegments[i].centerGuidelinePoints, 1, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
                DrawRoadGuidelines(roadSegments[i].endGuidelinePoints, 2, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
            }
        }
    }

    private static void DrawRoadGuidelines (Vector3[] guidelines, int child, RoadSegment roadSegment, Vector3 mousePosition, GameObject objectToMove, GameObject extraObjectToMove)
    {
        if (child == 1)
        {
            Handles.color = Misc.darkGreen;
        }
        else
        {
            Handles.color = Misc.lightGreen;
        }
        if (guidelines != null && guidelines.Length > 0 && (Vector3.Distance(mousePosition, roadSegment.transform.GetChild(0).GetChild(child).position) < 10) && roadSegment.transform.GetChild(0).GetChild(child).gameObject != objectToMove && roadSegment.transform.GetChild(0).GetChild(child).gameObject != extraObjectToMove)
        {
            Handles.DrawLine(roadSegment.transform.GetChild(0).GetChild(child).position, guidelines[guidelines.Length - 2]);
            Handles.DrawLine(roadSegment.transform.GetChild(0).GetChild(child).position, guidelines[guidelines.Length - 1]);
            Vector3 left = Misc.CalculateLeft(guidelines[0], guidelines[2]);
            Handles.DrawLine((left * roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.pointSize) + roadSegment.transform.GetChild(0).GetChild(child).position, (-left * roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.pointSize) + roadSegment.transform.GetChild(0).GetChild(child).position);

            for (int j = 0; j < guidelines.Length; j++)
            {
                Handles.DrawSolidDisc(guidelines[j], Vector3.up, roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.pointSize * 0.75f);
            }
        }
    }

}
