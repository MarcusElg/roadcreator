using System.Collections.Generic;
using UnityEngine;

public class BridgeGeneration
{

    public static GameObject GenerateSimpleBridge(Vector3[] points, Vector3[] nextPoints, Vector3 previousPoint, RoadSegment segment, Transform previousSegment, float startExtraWidthLeft, float endExtraWidthLeft, float startExtraWidthRight, float endExtraWidthRight, Material[] materials, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> extraUvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        int verticeIndex = 0;
        float totalDistance = 0;
        float currentDistance = 0;

        GameObject bridge = new GameObject("Bridge Base");

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
            vertices.Add((points[i] - left * roadWidthRight) - segment.transform.position);
            vertices[verticeIndex] = new Vector3(vertices[verticeIndex].x, points[i].y + heightOffset - segment.transform.position.y, vertices[verticeIndex].z);
            vertices.Add((points[i] - left * roadWidthRight) - segment.transform.position);
            vertices[verticeIndex + 1] = new Vector3(vertices[verticeIndex + 1].x, points[i].y - segment.bridgeSettings.yOffsetFirstStep + heightOffset - segment.transform.position.y, vertices[verticeIndex + 1].z);
            vertices.Add((points[i] - left * roadWidthRight * segment.bridgeSettings.widthPercentageFirstStep) - segment.transform.position);
            vertices[verticeIndex + 2] = new Vector3(vertices[verticeIndex + 2].x, points[i].y - segment.bridgeSettings.yOffsetFirstStep + heightOffset - segment.transform.position.y, vertices[verticeIndex + 2].z);
            vertices.Add((points[i] - left * roadWidthRight * segment.bridgeSettings.widthPercentageFirstStep * segment.bridgeSettings.widthPercentageSecondStep) - segment.transform.position);
            vertices[verticeIndex + 3] = new Vector3(vertices[verticeIndex + 3].x, points[i].y - segment.bridgeSettings.yOffsetFirstStep - segment.bridgeSettings.yOffsetSecondStep + heightOffset - segment.transform.position.y, vertices[verticeIndex + 3].z);
            vertices.Add((points[i] + left * roadWidthLeft * segment.bridgeSettings.widthPercentageFirstStep * segment.bridgeSettings.widthPercentageSecondStep) - segment.transform.position);
            vertices[verticeIndex + 4] = new Vector3(vertices[verticeIndex + 4].x, points[i].y - segment.bridgeSettings.yOffsetFirstStep - segment.bridgeSettings.yOffsetSecondStep + heightOffset - segment.transform.position.y, vertices[verticeIndex + 4].z);
            vertices.Add((points[i] + left * roadWidthLeft * segment.bridgeSettings.widthPercentageFirstStep) - segment.transform.position);
            vertices[verticeIndex + 5] = new Vector3(vertices[verticeIndex + 5].x, points[i].y - segment.bridgeSettings.yOffsetFirstStep + heightOffset - segment.transform.position.y, vertices[verticeIndex + 5].z);
            vertices.Add((points[i] + left * roadWidthLeft) - segment.transform.position);
            vertices[verticeIndex + 6] = new Vector3(vertices[verticeIndex + 6].x, points[i].y - segment.bridgeSettings.yOffsetFirstStep + heightOffset - segment.transform.position.y, vertices[verticeIndex + 6].z);
            vertices.Add((points[i] + left * roadWidthLeft) - segment.transform.position);
            vertices[verticeIndex + 7] = new Vector3(vertices[verticeIndex + 7].x, points[i].y - segment.transform.position.y + heightOffset, vertices[verticeIndex + 7].z);

            uvs.Add(new Vector2(0, currentDistance / totalDistance));
            uvs.Add(new Vector2(1, currentDistance / totalDistance));
            uvs.Add(new Vector2(0, currentDistance / totalDistance));
            uvs.Add(new Vector2(1, currentDistance / totalDistance));
            uvs.Add(new Vector2(0, currentDistance / totalDistance));
            uvs.Add(new Vector2(1, currentDistance / totalDistance));
            uvs.Add(new Vector2(0, currentDistance / totalDistance));
            uvs.Add(new Vector2(1, currentDistance / totalDistance));

            if (i < points.Length - 1)
            {
                for (int j = 0; j < 7; j += 1)
                {
                    triangles.Add(verticeIndex + 1 + j);
                    triangles.Add(verticeIndex + j);
                    triangles.Add(verticeIndex + 9 + j);

                    triangles.Add(verticeIndex + 0 + j);
                    triangles.Add(verticeIndex + 8 + j);
                    triangles.Add(verticeIndex + 9 + j);
                }

                triangles.Add(verticeIndex + 0);
                triangles.Add(verticeIndex + 7);
                triangles.Add(verticeIndex + 8);

                triangles.Add(verticeIndex + 8);
                triangles.Add(verticeIndex + 7);
                triangles.Add(verticeIndex + 15);

                if (i == points.Length - 2)
                {
                    // Start cap
                    triangles.Add(0);
                    triangles.Add(1);
                    triangles.Add(7);

                    triangles.Add(1);
                    triangles.Add(6);
                    triangles.Add(7);

                    triangles.Add(2);
                    triangles.Add(3);
                    triangles.Add(5);

                    triangles.Add(3);
                    triangles.Add(4);
                    triangles.Add(5);

                    for (int j = 0; j < 2; j++)
                    {
                        extraUvs.Add(new Vector2(1, 0));
                        extraUvs.Add(new Vector2(0, 0));
                        extraUvs.Add(new Vector2(1, 1));

                        extraUvs.Add(new Vector2(0, 0));
                        extraUvs.Add(new Vector2(0, 1));
                        extraUvs.Add(new Vector2(1, 1));
                    }

                    // End cap
                    triangles.Add(verticeIndex + 15);
                    triangles.Add(verticeIndex + 9);
                    triangles.Add(verticeIndex + 8);

                    triangles.Add(verticeIndex + 15);
                    triangles.Add(verticeIndex + 14);
                    triangles.Add(verticeIndex + 9);

                    triangles.Add(verticeIndex + 13);
                    triangles.Add(verticeIndex + 11);
                    triangles.Add(verticeIndex + 10);

                    triangles.Add(verticeIndex + 13);
                    triangles.Add(verticeIndex + 12);
                    triangles.Add(verticeIndex + 11);

                    for (int j = 0; j < 2; j++)
                    {
                        extraUvs.Add(new Vector2(1, 0));
                        extraUvs.Add(new Vector2(0, 1));
                        extraUvs.Add(new Vector2(1, 1));

                        extraUvs.Add(new Vector2(1, 0));
                        extraUvs.Add(new Vector2(0, 0));
                        extraUvs.Add(new Vector2(0, 1));
                    }
                }

                GeneratePillars(points, startPoint, controlPoint, endPoint, segment, bridge);
            }

            verticeIndex += 8;
        }

        return BridgeGeneration.CreateBridge(bridge, segment.transform, vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), extraUvs.ToArray(), materials);
    }

    public static void GenerateSuspensionBridge(Vector3[] points, Vector3[] nextPoints, Vector3 previousPoint, RoadSegment segment, Transform previousSegment, float startExtraWidthLeft, float endExtraWidthLeft, float startExtraWidthRight, float endExtraWidthRight, Material[] materials, Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint)
    {
        GameObject bridge = GenerateSimpleBridge(points, nextPoints, previousPoint, segment, previousSegment, startExtraWidthLeft, endExtraWidthLeft, startExtraWidthRight, endExtraWidthRight, materials, startPoint, controlPoint, endPoint);

        float totalDistance = 0;
        float currentDistance = 0;
        float currentTotalDistance = 0;
        Vector3 lastPosition = startPoint;
        Vector3 lastCablePosition = startPoint;
        Vector3 currentLeftCablePosition = Vector3.zero;
        Vector3 currentRightCablePosition = Vector3.zero;
        Vector3 leftCablePosition = startPoint + Misc.CalculateLeft(points[0], points[1]) * (segment.startRoadWidth + startExtraWidthLeft - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset);
        Vector3 rightCablePosition = startPoint - Misc.CalculateLeft(points[0], points[1]) * (segment.startRoadWidth + startExtraWidthRight - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset);

        for (int i = 1; i < points.Length; i++)
        {
            totalDistance += Vector3.Distance(points[i - 1], points[i]);
        }

        bool upwards = true;
        float distancePerSegment = 1f / segment.bridgeSettings.sections;
        float currentSegmentDistance = 0;

        for (float t = 0; t <= 1; t += 0.001f)
        {
            Vector3 currentPosition = Misc.Lerp3(startPoint, controlPoint, endPoint, t);
            currentPosition.y = Mathf.Lerp(Mathf.Lerp(startPoint.y, Misc.GetCenter(endPoint.y, startPoint.y), t), endPoint.y, t);
            currentDistance = Vector3.Distance(lastCablePosition, currentPosition);
            currentTotalDistance += Vector3.Distance(lastPosition, currentPosition);

            if (upwards == true)
            {
                currentSegmentDistance += Vector3.Distance(lastPosition, currentPosition);
            }
            else
            {
                currentSegmentDistance -= Vector3.Distance(lastPosition, currentPosition);
            }

            if (currentDistance >= segment.bridgeSettings.cableGap)
            {
                if (currentSegmentDistance > distancePerSegment * totalDistance / 2)
                {
                    upwards = false;
                    currentSegmentDistance = distancePerSegment * totalDistance / 2;
                }
                else if (currentSegmentDistance <= 0)
                {
                    upwards = true;
                    currentSegmentDistance = 0;
                }

                Vector3 left = Misc.CalculateLeft(lastPosition, currentPosition);
                float height = segment.bridgeSettings.height * currentSegmentDistance / (distancePerSegment * totalDistance);
                float roadWidth = Mathf.Lerp(segment.startRoadWidth, segment.endRoadWidth, currentTotalDistance / totalDistance);
                float roadWidthLeft = roadWidth + Mathf.Lerp(startExtraWidthLeft, endExtraWidthLeft, currentTotalDistance / totalDistance) - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset;
                float roadWidthRight = roadWidth + Mathf.Lerp(startExtraWidthRight, endExtraWidthRight, currentTotalDistance / totalDistance) - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset;

                // Left cable
                GameObject cable = GameObject.Instantiate(segment.bridgeSettings.cablePrefab);
                cable.name = "Cable";
                cable.hideFlags = HideFlags.NotEditable;
                cable.layer = segment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadLayer;
                cable.transform.SetParent(bridge.transform);
                cable.transform.position = currentPosition + roadWidthLeft * left + new Vector3(0, height / 2, 0);
                cable.transform.rotation = Quaternion.LookRotation(left, Vector3.up);
                cable.transform.localScale = new Vector3(segment.bridgeSettings.cableScale, height, segment.bridgeSettings.cableScale);
                currentLeftCablePosition = cable.transform.position + new Vector3 (0, height / 2, 0);

                // Right cable
                cable = GameObject.Instantiate(segment.bridgeSettings.cablePrefab);
                cable.name = "Cable";
                cable.hideFlags = HideFlags.NotEditable;
                cable.layer = segment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadLayer;
                cable.transform.SetParent(bridge.transform);
                cable.transform.position = currentPosition + roadWidthRight * -left + new Vector3(0, height / 2, 0);
                cable.transform.rotation = Quaternion.LookRotation(-left, Vector3.up);
                cable.transform.localScale = new Vector3(segment.bridgeSettings.cableScale, height, segment.bridgeSettings.cableScale);
                currentRightCablePosition = cable.transform.position + new Vector3(0, height / 2, 0);

                // Top Cables
                GenerateTopCable(segment, bridge.transform, Misc.GetCenter(leftCablePosition, currentLeftCablePosition), (leftCablePosition - currentLeftCablePosition).normalized, Vector3.Distance(currentLeftCablePosition, leftCablePosition));
                GenerateTopCable(segment, bridge.transform, Misc.GetCenter(rightCablePosition, currentRightCablePosition), (rightCablePosition - currentRightCablePosition).normalized, Vector3.Distance(currentRightCablePosition, rightCablePosition));

                if (currentTotalDistance + segment.bridgeSettings.cableGap > totalDistance)
                {
                    // Top Cables
                    left = Misc.CalculateLeft(points[points.Length - 2], points[points.Length - 1]);
                    GenerateTopCable(segment, bridge.transform, Misc.GetCenter(endPoint + left * (segment.endRoadWidth + endExtraWidthLeft - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset), currentLeftCablePosition), ((endPoint + left * (segment.endRoadWidth + endExtraWidthLeft - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset)) - currentLeftCablePosition).normalized, Vector3.Distance(currentLeftCablePosition, endPoint + left * (segment.endRoadWidth + endExtraWidthLeft - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset)));
                    GenerateTopCable(segment, bridge.transform, Misc.GetCenter(endPoint - left * (segment.endRoadWidth + endExtraWidthRight - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset), currentRightCablePosition), ((endPoint - left * (segment.endRoadWidth + endExtraWidthRight - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset)) - currentRightCablePosition).normalized, Vector3.Distance(currentRightCablePosition, endPoint - left * (segment.endRoadWidth + endExtraWidthRight - segment.bridgeSettings.extraWidth + segment.bridgeSettings.widthOffset)));
                }

                leftCablePosition = currentLeftCablePosition;
                rightCablePosition = currentRightCablePosition;
                lastCablePosition = currentPosition;
            }

            lastPosition = currentPosition;
        }
    }

    public static GameObject GenerateTopCable(RoadSegment segment, Transform parent, Vector3 position, Vector3 lookDirection, float height)
    {
        GameObject cable = GameObject.Instantiate(segment.bridgeSettings.cablePrefab);
        cable.name = "Top Cable";
        cable.hideFlags = HideFlags.NotEditable;
        cable.layer = segment.transform.parent.parent.GetComponent<RoadCreator>().globalSettings.roadLayer;
        cable.transform.SetParent(parent);
        cable.transform.position = position;
        cable.transform.rotation = Quaternion.LookRotation(lookDirection);
        cable.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
        cable.transform.localScale = new Vector3(segment.bridgeSettings.topCableScale, height, segment.bridgeSettings.topCableScale);

        return cable;
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
                CreatePillar(bridge.transform, segment.pillarPrefab, currentPosition - new Vector3(0, segment.bridgeSettings.yOffsetFirstStep + segment.bridgeSettings.yOffsetSecondStep, 0), segment, (currentPosition - lastPosition).normalized);
                lastPosition = currentPosition;
                placedFirstPillar = true;
            }
            else if (placedFirstPillar == true && currentDistance >= segment.pillarGap)
            {
                CreatePillar(bridge.transform, segment.pillarPrefab, currentPosition - new Vector3(0, segment.bridgeSettings.yOffsetFirstStep + segment.bridgeSettings.yOffsetSecondStep, 0), segment, (currentPosition - lastPosition).normalized);
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
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        int verticeIndex = 0;
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
            vertices.Add(inputVertices[i] - verticeDifference.normalized * (intersection.bridgeSettings.extraWidth + currentWidth));
            vertices[verticeIndex] = new Vector3(vertices[verticeIndex].x, inputVertices[i].y - inputVertices[i].y, vertices[verticeIndex].z);
            vertices.Add(inputVertices[i] - verticeDifference.normalized * (intersection.bridgeSettings.extraWidth + currentWidth));
            vertices[verticeIndex + 1] = new Vector3(vertices[verticeIndex + 1].x, inputVertices[i].y - intersection.bridgeSettings.yOffsetFirstStep - inputVertices[i].y, vertices[verticeIndex + 1].z);
            vertices.Add(inputVertices[i + 1] - verticeDifference * intersection.bridgeSettings.widthPercentageFirstStep - verticeDifference.normalized * (intersection.bridgeSettings.extraWidth + currentWidth));
            vertices[verticeIndex + 2] = new Vector3(vertices[verticeIndex + 2].x, inputVertices[i].y - intersection.bridgeSettings.yOffsetFirstStep - inputVertices[i].y, vertices[verticeIndex + 2].z);
            vertices.Add(inputVertices[i + 1] - verticeDifference.normalized * (intersection.bridgeSettings.extraWidth + currentWidth) - verticeDifference * intersection.bridgeSettings.widthPercentageFirstStep * intersection.bridgeSettings.widthPercentageSecondStep);
            vertices[verticeIndex + 3] = new Vector3(vertices[verticeIndex + 3].x, inputVertices[i].y - intersection.bridgeSettings.yOffsetFirstStep - intersection.bridgeSettings.yOffsetSecondStep - inputVertices[i].y, vertices[verticeIndex + 3].z);
            vertices.Add(inputVertices[i + 1]);
            vertices[verticeIndex + 4] = new Vector3(vertices[verticeIndex + 4].x, inputVertices[i].y - intersection.bridgeSettings.yOffsetFirstStep - intersection.bridgeSettings.yOffsetSecondStep - inputVertices[i].y, vertices[verticeIndex + 4].z);
            vertices.Add(inputVertices[i + 1]);
            vertices[verticeIndex + 5] = new Vector3(vertices[verticeIndex + 5].x, inputVertices[i].y - inputVertices[i].y, vertices[verticeIndex + 5].z);

            uvs.Add(new Vector2(0, currentDistance / totalDistances[currentSegment]));
            uvs.Add(new Vector2(1, currentDistance / totalDistances[currentSegment]));
            uvs.Add(new Vector2(0, currentDistance / totalDistances[currentSegment]));
            uvs.Add(new Vector2(1, currentDistance / totalDistances[currentSegment]));
            uvs.Add(new Vector2(0, currentDistance / totalDistances[currentSegment]));
            uvs.Add(new Vector2(1, currentDistance / totalDistances[currentSegment]));

            if (i < inputVertices.Length)
            {
                if (i < inputVertices.Length - 2)
                {
                    for (int j = 0; j < 4; j += 1)
                    {
                        triangles.Add(verticeIndex + 1 + j);
                        triangles.Add(verticeIndex + 6 + j);
                        triangles.Add(verticeIndex + j);

                        triangles.Add(verticeIndex + 1 + j);
                        triangles.Add(verticeIndex + 7 + j);
                        triangles.Add(verticeIndex + 6 + j);
                    }

                    // Top cover
                    triangles.Add(verticeIndex + 0);
                    triangles.Add(verticeIndex + 6);
                    triangles.Add(verticeIndex + 5);

                    triangles.Add(verticeIndex + 6);
                    triangles.Add(verticeIndex + 11);
                    triangles.Add(verticeIndex + 5);
                }
                else
                {
                    for (int j = 0; j < 4; j += 1)
                    {
                        triangles.Add(verticeIndex + 1 + j);
                        triangles.Add(j);
                        triangles.Add(verticeIndex + j);

                        triangles.Add(verticeIndex + 1 + j);
                        triangles.Add(j + 1);
                        triangles.Add(j);
                    }
                }
            }

            verticeIndex += 6;
            lastVertexPosition = inputVertices[i];
        }

        CreatePillarIntersection(bridge.transform, intersection.pillarPrefab, intersection.transform.position - new Vector3(0, intersection.bridgeSettings.yOffsetFirstStep + intersection.bridgeSettings.yOffsetSecondStep, 0), intersection);
        BridgeGeneration.CreateBridge(bridge, intersection.transform, vertices.ToArray(), triangles.ToArray(), uvs.ToArray(), null, materials);
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

    public static GameObject CreateBridge(GameObject bridge, Transform parent, Vector3[] vertices, int[] triangles, Vector2[] uvs, Vector2[] extraUvs, Material[] materials)
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

            if (extraUvs != null && i >= triangles.Length - 24)
            {
                flatShadedUvs[i] = extraUvs[i - triangles.Length + 24];
            }
            else
            {
                flatShadedUvs[i] = uvs[triangles[i]];
            }

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

        return bridge;
    }

}
