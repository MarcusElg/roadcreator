﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class Misc
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
        Vector3 forward = (nextPoint - point);
        return new Vector3(-forward.z, 0, forward.x).normalized;
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
                        forward = nextSegmentPoints[1] - nextSegmentPoints[0];
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

        return new Vector3(-forward.z, 0, forward.x).normalized;
    }

    public static Vector3 GetCenter(Vector3 one, Vector3 two)
    {
        Vector3 difference = two - one;
        return (one + (difference / 2));
    }

    public static float GetCenter(float one, float two)
    {
        float difference = two - one;
        return (one + (difference / 2));
    }

    public static Vector3 FindPointInCircle(float radius, int i, float degreesPerStep)
    {
        return Quaternion.AngleAxis(degreesPerStep * i, Vector3.up) * (Vector3.right * radius);
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Vector3 GetNearestGuidelinePoint(Vector3 hitPosition)
    {
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        Vector3 nearest = MaxVector3;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < roadSegments.Length; i++)
        {
            if (roadSegments[i].startGuidelinePoints != null && roadSegments[i].centerGuidelinePoints != null && roadSegments[i].endGuidelinePoints != null)
            {
                Vector3[] guidelines = new Vector3[roadSegments[i].startGuidelinePoints.Length + roadSegments[i].centerGuidelinePoints.Length + roadSegments[i].endGuidelinePoints.Length];
                roadSegments[i].startGuidelinePoints.CopyTo(guidelines, 0);
                roadSegments[i].centerGuidelinePoints.CopyTo(guidelines, roadSegments[i].startGuidelinePoints.Length);
                roadSegments[i].endGuidelinePoints.CopyTo(guidelines, roadSegments[i].startGuidelinePoints.Length + roadSegments[i].centerGuidelinePoints.Length);

                Vector3 nearestVector = MaxVector3;
                float nearestDistanceInSegment = float.MaxValue;

                if (guidelines != null)
                {
                    for (int j = 0; j < guidelines.Length; j++)
                    {
                        float distance = Vector3.Distance(new Vector3(hitPosition.x, 0, hitPosition.z), new Vector3(guidelines[j].x, 0, guidelines[j].z));
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
        }

        return nearest;
    }

    public static void DrawRoadGuidelines(Vector3 mousePosition, GameObject objectToMove, GameObject extraObjectToMove)
    {
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        for (int i = 0; i < roadSegments.Length; i++)
        {
            if (roadSegments[i].transform.GetChild(0).childCount == 3)
            {
                if (roadSegments[i].transform.GetSiblingIndex() == 0)
                {
                    DrawRoadGuidelines(roadSegments[i].startGuidelinePoints, 0, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
                }

                DrawRoadGuidelines(roadSegments[i].centerGuidelinePoints, 1, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
                DrawRoadGuidelines(roadSegments[i].endGuidelinePoints, 2, roadSegments[i], mousePosition, objectToMove, extraObjectToMove);
            }
        }
    }

    private static void DrawRoadGuidelines(Vector3[] guidelines, int child, RoadSegment roadSegment, Vector3 mousePosition, GameObject objectToMove, GameObject extraObjectToMove)
    {
        if (roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings == null)
        {
            roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (child == 1)
        {
            Handles.color = roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadControlGuidelinesColour;
        }
        else
        {
            Handles.color = roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadGuidelinesColour;
        }

        if (guidelines != null && guidelines.Length > 0 && (Vector3.Distance(new Vector3(mousePosition.x, 0, mousePosition.z), new Vector3(roadSegment.transform.GetChild(0).GetChild(child).position.x, 0, roadSegment.transform.GetChild(0).GetChild(child).position.z)) < guidelines.Length + 1) && roadSegment.transform.GetChild(0).GetChild(child).gameObject != objectToMove && roadSegment.transform.GetChild(0).GetChild(child).gameObject != extraObjectToMove)
        {
            Handles.DrawLine(roadSegment.transform.GetChild(0).GetChild(child).position, guidelines[guidelines.Length - 2]);
            Handles.DrawLine(roadSegment.transform.GetChild(0).GetChild(child).position, guidelines[guidelines.Length - 1]);
            Vector3 left = Misc.CalculateLeft(guidelines[2], guidelines[0]);
            Handles.DrawLine((left * roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.pointSize) + roadSegment.transform.GetChild(0).GetChild(child).position, (-left * roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.pointSize) + roadSegment.transform.GetChild(0).GetChild(child).position);

            for (int j = 0; j < guidelines.Length; j++)
            {
                Handles.DrawSolidDisc(guidelines[j], Vector3.up, roadSegment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.pointSize * 0.75f);
            }
        }
    }

    public static Vector3 InverseX(Vector3 vector)
    {
        return new Vector3(-vector.x, vector.y, vector.z);
    }

    public static void ConvertToMesh(GameObject gameObject, string name)
    {
        MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        List<Material> materials = new List<Material>();

        if (meshFilters.Length > 0 && meshFilters[0].sharedMesh != null)
        {
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (!materials.Contains(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials[0]))
                {
                    materials.Add(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials[0]);
                }

                if (meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials.Length > 1 && !materials.Contains(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials[1]))
                {
                    materials.Add(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials[1]);
                }
            }

            List<Mesh> subMeshes = new List<Mesh>();
            foreach (Material material in materials)
            {
                List<CombineInstance> combinerInstances = new List<CombineInstance>();
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    Material localMaterial = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial;
                    if (localMaterial != material)
                    {
                        continue;
                    }

                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh = meshFilters[i].sharedMesh;
                    Matrix4x4 matrix = Matrix4x4.identity;
                    matrix.SetTRS(meshFilters[i].transform.position, meshFilters[i].transform.rotation, Vector3.one);
                    combineInstance.transform = matrix;
                    combinerInstances.Add(combineInstance);
                }

                Mesh mesh = new Mesh();
                mesh.CombineMeshes(combinerInstances.ToArray(), true);
                subMeshes.Add(mesh);
            }

            List<CombineInstance> finalCombineInstances = new List<CombineInstance>();
            for (int i = 0; i < subMeshes.Count; i++)
            {
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = subMeshes[i];
                combineInstance.transform = Matrix4x4.identity;
                finalCombineInstances.Add(combineInstance);
            }
            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(finalCombineInstances.ToArray(), false);

            GameObject newMesh = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(newMesh, "Create Combined Mesh");
            newMesh.AddComponent<MeshFilter>();
            newMesh.AddComponent<MeshRenderer>();
            newMesh.AddComponent<MeshCollider>();
            newMesh.GetComponent<MeshFilter>().sharedMesh = finalMesh;
            newMesh.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            newMesh.GetComponent<MeshCollider>().sharedMesh = finalMesh;
            Selection.activeGameObject = newMesh;
            Undo.DestroyObjectImmediate(gameObject.gameObject);
        }
        else
        {
            Debug.Log("There are no meshes to combine");
        }
    }

}
