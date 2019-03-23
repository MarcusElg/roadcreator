using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGeneration
{

    public static void GenerateSimpleBridge(Vector3[] points, Vector3[] nextPoints, Vector3 previousPoint, RoadSegment segment, Transform previousSegment, float startExtraWidthLeft, float endExtraWidthLeft, float startExtraWidthRight, float endExtraWidthRight, Material[] materials, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        Vector3[] vertices = new Vector3[points.Length * 8];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numberTriangles = 4 * (points.Length - 1);
        int[] triangles = new int[numberTriangles * 24];
        int verticeIndex = 0;
        int triangleIndex = 0;
        float totalDistance = 0;
        float currentDistance = 0;

        GameObject bridge = new GameObject("Bridge");

        for (int i = 1; i < points.Length; i++)
        {
            totalDistance += Vector3.Distance(points[i - 1], points[i]);
        }

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 left = Misc.CalculateLeft(points, nextPoints, Misc.MaxVector3, i);

            if (i > 0)
            {
                currentDistance += Vector3.Distance(points[i - 1], points[i]);
            }

            float roadWidth = Mathf.Lerp(segment.startRoadWidth, segment.endRoadWidth, currentDistance / totalDistance);

            if (i == 0 && previousSegment != null)
            {
                roadWidth = previousSegment.GetComponent<RoadSegment>().endRoadWidth;
            }

            float roadWidthLeft = roadWidth + Mathf.Lerp(startExtraWidthLeft, endExtraWidthLeft, currentDistance / totalDistance);
            float roadWidthRight = roadWidth + Mathf.Lerp(startExtraWidthRight, endExtraWidthRight, currentDistance / totalDistance);

            float heightOffset = 0;
            if (i == 0 && previousPoint != Misc.MaxVector3)
            {
                heightOffset = previousPoint.y - points[i].y;
            }
            else if (i == points.Length - 1 && nextPoints != null && nextPoints.Length == 1)
            {
                heightOffset = nextPoints[0].y - points[i].y;
            }

            // |_   _|
            //   \_/
            vertices[verticeIndex] = (points[i] - left * roadWidthLeft) - segment.transform.position;
            vertices[verticeIndex].y = points[i].y + heightOffset - segment.transform.position.y;
            vertices[verticeIndex + 1] = (points[i] - left * roadWidthLeft) - segment.transform.position;
            vertices[verticeIndex + 1].y = points[i].y - segment.yOffsetFirstStep + heightOffset - segment.transform.position.y;
            vertices[verticeIndex + 2] = (points[i] - left * roadWidthLeft * segment.widthPercentageFirstStep) - segment.transform.position;
            vertices[verticeIndex + 2].y = points[i].y - segment.yOffsetFirstStep + heightOffset - segment.transform.position.y;
            vertices[verticeIndex + 3] = (points[i] - left * roadWidthLeft * segment.widthPercentageFirstStep * segment.widthPercentageSecondStep) - segment.transform.position;
            vertices[verticeIndex + 3].y = points[i].y - segment.yOffsetFirstStep - segment.yOffsetSecondStep + heightOffset - segment.transform.position.y;

            vertices[verticeIndex + 4] = (points[i] + left * roadWidthRight * segment.widthPercentageFirstStep * segment.widthPercentageSecondStep) - segment.transform.position;
            vertices[verticeIndex + 4].y = points[i].y - segment.yOffsetFirstStep - segment.yOffsetSecondStep + heightOffset - segment.transform.position.y;
            vertices[verticeIndex + 5] = (points[i] + left * roadWidthRight * segment.widthPercentageFirstStep) - segment.transform.position;
            vertices[verticeIndex + 5].y = points[i].y - segment.yOffsetFirstStep + heightOffset - segment.transform.position.y;
            vertices[verticeIndex + 6] = (points[i] + left * roadWidthRight) - segment.transform.position;
            vertices[verticeIndex + 6].y = points[i].y - segment.yOffsetFirstStep + heightOffset - segment.transform.position.y;
            vertices[verticeIndex + 7] = (points[i] + left * roadWidthRight) - segment.transform.position;
            vertices[verticeIndex + 7].y = points[i].y - segment.transform.position.y + heightOffset;

            uvs[verticeIndex] = new Vector2(0, currentDistance / totalDistance);
            uvs[verticeIndex + 1] = new Vector2(1, currentDistance / totalDistance);
            uvs[verticeIndex + 2] = new Vector2(0, currentDistance / totalDistance);
            uvs[verticeIndex + 3] = new Vector2(0.75f, currentDistance / totalDistance);
            uvs[verticeIndex + 4] = new Vector2(0.25f, currentDistance / totalDistance);
            uvs[verticeIndex + 5] = new Vector2(0, currentDistance / totalDistance);
            uvs[verticeIndex + 6] = new Vector2(1, currentDistance / totalDistance);
            uvs[verticeIndex + 7] = new Vector2(0, currentDistance / totalDistance);

            if (i < points.Length - 1)
            {
                for (int j = 0; j < 7; j += 1)
                {
                    triangles[triangleIndex + j * 6] = verticeIndex + 1 + j;
                    triangles[triangleIndex + 1 + j * 6] = verticeIndex + j;
                    triangles[triangleIndex + 2 + j * 6] = verticeIndex + 9 + j;

                    triangles[triangleIndex + 3 + j * 6] = verticeIndex + 0 + j;
                    triangles[triangleIndex + 4 + j * 6] = verticeIndex + 8 + j;
                    triangles[triangleIndex + 5 + j * 6] = verticeIndex + 9 + j;
                }

                triangles[triangleIndex + 42] = verticeIndex + 0;
                triangles[triangleIndex + 43] = verticeIndex + 7;
                triangles[triangleIndex + 44] = verticeIndex + 8;

                triangles[triangleIndex + 45] = verticeIndex + 8;
                triangles[triangleIndex + 46] = verticeIndex + 7;
                triangles[triangleIndex + 47] = verticeIndex + 15;

                GeneratePillars(points, startPoint, controlPoint, endPoint, segment, bridge);
            }

            verticeIndex += 8;
            triangleIndex += 54;
        }

        BridgeGeneration.CreateBridge(bridge, segment.transform, vertices, triangles, uvs, materials);
    }

    public static void GeneratePillars(Vector3[] points, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, RoadSegment segment, GameObject bridge)
    {
        float currentDistance = 0;
        Vector3 lastPosition = startPoint;
        bool placedFirstPillar = false;

        for (float t = 0; t <= 1; t += 0.01f)
        {
            Vector3 currentPosition = Misc.Lerp3(startPoint, controlPoint, endPoint, t);
            currentPosition.y = Mathf.Lerp(Mathf.Lerp(startPoint.y, Misc.GetCenter(endPoint.y, startPoint.y), t), endPoint.y, t);
            currentDistance = Vector3.Distance(lastPosition, currentPosition);

            if (placedFirstPillar == false && currentDistance >= segment.pillarPlacementOffset)
            {
                CreatePillar(bridge.transform, segment.pillarPrefab, currentPosition - new Vector3(0, segment.yOffsetFirstStep + segment.yOffsetSecondStep, 0), segment, (currentPosition - lastPosition).normalized);
                lastPosition = currentPosition;
                placedFirstPillar = true;
            }
            else if (placedFirstPillar == true && currentDistance >= segment.pillarGap)
            {
                CreatePillar(bridge.transform, segment.pillarPrefab, currentPosition - new Vector3(0, segment.yOffsetFirstStep + segment.yOffsetSecondStep, 0), segment, (currentPosition - lastPosition).normalized);
                lastPosition = currentPosition;
            }
        }
    }

    public static void CreatePillar(Transform parent, GameObject prefab, Vector3 position, RoadSegment segment, Vector3 forward)
    {
        GameObject pillar = GameObject.Instantiate(prefab);
        pillar.name = "Pillar";
        pillar.transform.SetParent(parent);
        pillar.hideFlags = HideFlags.NotEditable;

        RaycastHit raycastHit;
        if (Physics.Raycast(position, Vector3.down, out raycastHit, 100, ~(1 << segment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadLayer | 1 << segment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.ignoreMouseRayLayer)))
        {
            Vector3 groundPosition = raycastHit.point;
            Vector3 centerPosition = Misc.GetCenter(position, groundPosition);
            pillar.transform.localPosition = centerPosition - segment.transform.position;

            if (segment.rotationDirection == PrefabLineCreator.RotationDirection.forward)
            {
                pillar.transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y, 0);
            }
            else if (segment.rotationDirection == PrefabLineCreator.RotationDirection.backward)
            {
                pillar.transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(-forward, Vector3.up).eulerAngles.y, 0);
            }
            else if (segment.rotationDirection == PrefabLineCreator.RotationDirection.left)
            {
                pillar.transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(Misc.CalculateLeft(forward), Vector3.up).eulerAngles.y, 0);
            }
            else if (segment.rotationDirection == PrefabLineCreator.RotationDirection.right)
            {
                pillar.transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(-Misc.CalculateLeft(forward), Vector3.up).eulerAngles.y, 0);
            }


            float heightDifference = groundPosition.y - centerPosition.y;
            pillar.transform.localScale = new Vector3(segment.xzPillarScale, -heightDifference + segment.extraPillarHeight, segment.xzPillarScale);
        }
        else
        {
            GameObject.DestroyImmediate(pillar);
        }
    }

    public static void GenerateSimpleBridgeIntersection(Vector3[] inputVertices, Intersection intersection, Material[] materials, float[] startWidths, float[] endWidths, int[] startVertices)
    {
        Vector3[] vertices = new Vector3[inputVertices.Length * 3];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[inputVertices.Length * 30];
        int verticeIndex = 0;
        int triangleIndex = 0;
        int currentSegment = 0;
        Vector3 lastVertexPosition = inputVertices[0];
        float currentDistance = 0f;
        float[] totalDistances = new float[startWidths.Length];

        GameObject bridge = new GameObject("Bridge");

        for (int i = 0; i < inputVertices.Length; i += 2)
        {
            if (currentSegment < startVertices.Length - 1 && i > (startVertices[currentSegment + 1] - 2))
            {
                currentSegment += 1;
                currentDistance = 0;
                lastVertexPosition = inputVertices[i];
            }

            totalDistances[currentSegment] += Vector3.Distance(lastVertexPosition, inputVertices[i]);
            lastVertexPosition = inputVertices[i];
        }

        lastVertexPosition = inputVertices[0];
        currentSegment = 0;

        for (int i = 0; i < inputVertices.Length; i += 2)
        {
            if (currentSegment < startVertices.Length - 1 && i > (startVertices[currentSegment + 1] - 2))
            {
                currentSegment += 1;
                currentDistance = 0;
                lastVertexPosition = inputVertices[i];
            }

            Vector3 verticeDifference = inputVertices[i + 1] - inputVertices[i];
            currentDistance += Vector3.Distance(lastVertexPosition, inputVertices[i]);
            float currentWidth = Mathf.Lerp(startWidths[currentSegment], endWidths[currentSegment], currentDistance / totalDistances[currentSegment]);

            //   _|
            // _/
            vertices[verticeIndex] = inputVertices[i] - verticeDifference.normalized * (intersection.extraWidth + currentWidth);
            vertices[verticeIndex].y = inputVertices[i].y - inputVertices[i].y;
            vertices[verticeIndex + 1] = inputVertices[i] - verticeDifference.normalized * (intersection.extraWidth + currentWidth);
            vertices[verticeIndex + 1].y = inputVertices[i].y - intersection.yOffsetFirstStep - inputVertices[i].y;
            vertices[verticeIndex + 2] = inputVertices[i + 1] - verticeDifference * intersection.widthPercentageFirstStep - verticeDifference.normalized * (intersection.extraWidth + currentWidth);
            vertices[verticeIndex + 2].y = inputVertices[i].y - intersection.yOffsetFirstStep - inputVertices[i].y;
            vertices[verticeIndex + 3] = inputVertices[i + 1] - verticeDifference.normalized * (intersection.extraWidth + currentWidth) - verticeDifference * intersection.widthPercentageFirstStep * intersection.widthPercentageSecondStep;
            vertices[verticeIndex + 3].y = inputVertices[i].y - intersection.yOffsetFirstStep - intersection.yOffsetSecondStep - inputVertices[i].y;
            vertices[verticeIndex + 4] = inputVertices[i + 1];
            vertices[verticeIndex + 4].y = inputVertices[i].y - intersection.yOffsetFirstStep - intersection.yOffsetSecondStep - inputVertices[i].y;
            vertices[verticeIndex + 5] = inputVertices[i + 1];
            vertices[verticeIndex + 5].y = inputVertices[i].y - inputVertices[i].y;

            uvs[verticeIndex] = new Vector2(0, currentDistance / totalDistances[currentSegment]);
            uvs[verticeIndex + 1] = new Vector2(1, currentDistance / totalDistances[currentSegment]);
            uvs[verticeIndex + 2] = new Vector2(0, currentDistance / totalDistances[currentSegment]);
            uvs[verticeIndex + 3] = new Vector2(1, currentDistance / totalDistances[currentSegment]);
            uvs[verticeIndex + 4] = new Vector2(0, currentDistance / totalDistances[currentSegment]);
            uvs[verticeIndex + 5] = new Vector2(1, currentDistance / totalDistances[currentSegment]);

            if (i < inputVertices.Length)
            {
                if (i < inputVertices.Length - 2)
                {
                    for (int j = 0; j < 4; j += 1)
                    {
                        triangles[triangleIndex + j * 6] = verticeIndex + 1 + j;
                        triangles[triangleIndex + 1 + j * 6] = verticeIndex + 6 + j;
                        triangles[triangleIndex + 2 + j * 6] = verticeIndex + j;

                        triangles[triangleIndex + 3 + j * 6] = verticeIndex + 1 + j;
                        triangles[triangleIndex + 4 + j * 6] = verticeIndex + 7 + j;
                        triangles[triangleIndex + 5 + j * 6] = verticeIndex + 6 + j;
                    }

                    // Top cover
                    triangles[triangleIndex + 24] = verticeIndex + 0;
                    triangles[triangleIndex + 25] = verticeIndex + 6;
                    triangles[triangleIndex + 26] = verticeIndex + 5;

                    triangles[triangleIndex + 27] = verticeIndex + 6;
                    triangles[triangleIndex + 28] = verticeIndex + 11;
                    triangles[triangleIndex + 29] = verticeIndex + 5;
                }
                else
                {
                    for (int j = 0; j < 4; j += 1)
                    {
                        triangles[triangleIndex + j * 6] = verticeIndex + 1 + j;
                        triangles[triangleIndex + 1 + j * 6] = j;
                        triangles[triangleIndex + 2 + j * 6] = verticeIndex + j;

                        triangles[triangleIndex + 3 + j * 6] = verticeIndex + 1 + j;
                        triangles[triangleIndex + 4 + j * 6] = j + 1;
                        triangles[triangleIndex + 5 + j * 6] = j;
                    }
                }
            }

            verticeIndex += 6;
            triangleIndex += 30;

            lastVertexPosition = inputVertices[i];
        }

        CreatePillarIntersection(bridge.transform, intersection.pillarPrefab, intersection.transform.position - new Vector3(0, intersection.yOffsetFirstStep + intersection.yOffsetSecondStep, 0), intersection);
        BridgeGeneration.CreateBridge(bridge, intersection.transform, vertices, triangles, uvs, materials);
    }

    public static void CreatePillarIntersection(Transform parent, GameObject prefab, Vector3 position, Intersection intersection)
    {
        GameObject pillar = GameObject.Instantiate(prefab);
        pillar.name = "Pillar";
        pillar.transform.SetParent(parent);
        pillar.hideFlags = HideFlags.NotEditable;

        RaycastHit raycastHit;
        if (Physics.Raycast(position, Vector3.down, out raycastHit, 100, ~(1 << intersection.globalSettings.roadLayer | 1 << intersection.globalSettings.ignoreMouseRayLayer)))
        {
            Vector3 groundPosition = raycastHit.point;
            Vector3 centerPosition = Misc.GetCenter(position, groundPosition);
            pillar.transform.localPosition = new Vector3(0, centerPosition.y - intersection.transform.position.y, 0);

            float heightDifference = groundPosition.y - centerPosition.y;
            pillar.transform.localScale = new Vector3(intersection.xzPillarScale, -heightDifference + intersection.extraPillarHeight, intersection.xzPillarScale);
        }
        else
        {
            GameObject.DestroyImmediate(pillar);
        }
    }

    public static void CreateBridge(GameObject bridge, Transform parent, Vector3[] vertices, int[] triangles, Vector2[] uvs, Material[] materials)
    {
        bridge.transform.SetParent(parent);
        bridge.transform.SetAsLastSibling();
        bridge.transform.localPosition = Vector3.zero;
        bridge.hideFlags = HideFlags.NotEditable;
        bridge.AddComponent<MeshFilter>();
        bridge.AddComponent<MeshRenderer>();
        bridge.AddComponent<MeshCollider>();

        // Flat shaded triangles
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = flatShadedVertices;
        mesh.triangles = triangles;
        mesh.uv = flatShadedUvs;
        mesh.RecalculateNormals();

        bridge.GetComponent<MeshFilter>().sharedMesh = mesh;
        bridge.GetComponent<MeshRenderer>().sharedMaterials = materials;
        bridge.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
