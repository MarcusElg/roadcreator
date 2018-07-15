using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}
