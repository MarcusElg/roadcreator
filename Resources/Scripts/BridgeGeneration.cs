using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGeneration
{

    public static void GenerateSimpleBridge(Vector3[] points, Vector3[] nextPoints, Vector3 previousPoint, Transform parent, float startWidth, float endWidth, float extraWidthLeft, float extraWidthRight, float heightOffset, float yOffsetFirstStep, float yOffsetSecondStep, float widthPercentageFirstStep, float widthPercentageSecondStep, Material material)
    {
        Vector3[] vertices = new Vector3[points.Length * 8];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numberTriangles = 2 * (points.Length - 1);
        int[] triangles = new int[numberTriangles * 24];
        int verticeIndex = 0;
        int triangleIndex = 0;
        float totalDistance = 0;
        float currentDistance = 0;

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

            float roadWidth = Mathf.Lerp(startWidth, endWidth, currentDistance / totalDistance);
            float roadWidthLeft = roadWidth + extraWidthLeft;
            float roadWidthRight = roadWidth + extraWidthRight;

            float correctedHeightOffset = heightOffset;
            if (i == 0 && previousPoint != Misc.MaxVector3)
            {
                correctedHeightOffset = previousPoint.y - points[i].y;
            }
            else if (i == points.Length - 1 && nextPoints != null && nextPoints.Length == 1)
            {
                correctedHeightOffset = nextPoints[0].y - points[i].y;
            }

            correctedHeightOffset -= parent.position.y;

            // |_   _|
            //   \_/
            vertices[verticeIndex] = (points[i] - left * roadWidthLeft) - parent.position;
            vertices[verticeIndex].y = points[i].y + correctedHeightOffset;
            vertices[verticeIndex + 1] = (points[i] - left * roadWidthLeft) - parent.position;
            vertices[verticeIndex + 1].y = points[i].y + correctedHeightOffset - yOffsetFirstStep;
            vertices[verticeIndex + 2] = (points[i] - left * roadWidthLeft * widthPercentageFirstStep) - parent.position;
            vertices[verticeIndex + 2].y = points[i].y + correctedHeightOffset - yOffsetFirstStep;
            vertices[verticeIndex + 3] = (points[i] - left * roadWidthLeft * widthPercentageFirstStep * widthPercentageSecondStep) - parent.position;
            vertices[verticeIndex + 3].y = points[i].y + correctedHeightOffset - yOffsetFirstStep - yOffsetSecondStep;

            vertices[verticeIndex + 4] = (points[i] + left * roadWidthRight * widthPercentageFirstStep * widthPercentageSecondStep) - parent.position;
            vertices[verticeIndex + 4].y = points[i].y + correctedHeightOffset - yOffsetFirstStep - yOffsetSecondStep;
            vertices[verticeIndex + 5] = (points[i] + left * roadWidthRight * widthPercentageFirstStep) - parent.position;
            vertices[verticeIndex + 5].y = points[i].y + correctedHeightOffset - yOffsetFirstStep;
            vertices[verticeIndex + 6] = (points[i] + left * roadWidthRight) - parent.position;
            vertices[verticeIndex + 6].y = points[i].y + correctedHeightOffset - yOffsetFirstStep;
            vertices[verticeIndex + 7] = (points[i] + left * roadWidthRight) - parent.position;
            vertices[verticeIndex + 7].y = points[i].y + correctedHeightOffset;

            if (i < points.Length - 1)
            {
                for (int j = 0; j < 7; j += 1)
                {
                    triangles[triangleIndex + j * 6] = verticeIndex + 1 + j;
                    triangles[triangleIndex + 1 + j * 6] = verticeIndex + j;
                    triangles[triangleIndex + 2 + j * 6] = verticeIndex + 8 + j;

                    triangles[triangleIndex + 3 + j * 6] = verticeIndex + 1 + j;
                    triangles[triangleIndex + 4 + j * 6] = verticeIndex + 8 + j;
                    triangles[triangleIndex + 5 + j * 6] = verticeIndex + 9 + j;
                }
            }

            verticeIndex += 8;
            triangleIndex += 48;
        }

        BridgeGeneration.CreateBridge(parent, vertices, triangles, uvs, new Material[] { material });
    }

    public static void GenerateSimpleBridgeIntersection(Vector3[] inputVertices, Transform parent, float heightOffset, float yOffsetFirstStep, float yOffsetSecondStep, float widthPercentageFirstStep, float widthPercentageSecondStep, Material material)
    {
        Vector3[] vertices = new Vector3[inputVertices.Length * 3];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numberTriangles = 2 * (inputVertices.Length / 4 - 1);
        int[] triangles = new int[numberTriangles * 24];
        int verticeIndex = 0;
        int triangleIndex = 0;

        heightOffset -= parent.position.y;

        for (int i = 0; i < inputVertices.Length; i += 2)
        {
            Vector3 verticeDifference = inputVertices[i + 1] - inputVertices[i];

            // |_   _|
            //   \_/
            vertices[verticeIndex] = inputVertices[i];
            vertices[verticeIndex].y = inputVertices[i].y + heightOffset - inputVertices[i].y;
            vertices[verticeIndex + 1] = inputVertices[i];
            vertices[verticeIndex + 1].y = inputVertices[i].y + heightOffset - yOffsetFirstStep - inputVertices[i].y;
            vertices[verticeIndex + 2] = (inputVertices[i] + verticeDifference * widthPercentageFirstStep);
            vertices[verticeIndex + 2].y = inputVertices[i].y + heightOffset - yOffsetFirstStep - inputVertices[i].y;
            vertices[verticeIndex + 3] = inputVertices[i] + verticeDifference * widthPercentageFirstStep * widthPercentageSecondStep;
            vertices[verticeIndex + 3].y = inputVertices[i].y + heightOffset - yOffsetFirstStep - yOffsetSecondStep - inputVertices[i].y;
            vertices[verticeIndex + 4] = Misc.GetCenter(inputVertices[i], inputVertices[i + 1]);
            vertices[verticeIndex + 4].y = inputVertices[i].y + heightOffset - yOffsetFirstStep - yOffsetSecondStep - inputVertices[i].y;

            if (i == 0)
            {
                for (int j = 0; j < 5; j++)
                {
                    //GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //g.transform.position = vertices[j];
                    //g.transform.name = j + "";
                    //g.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                }
            }

            if (i < inputVertices.Length - 1 && i == 0)
            {
                //<5
                for (int j = 0; j < 2; j += 1)
                {
                    triangles[triangleIndex + j * 5] = verticeIndex + 1 + j;
                    triangles[triangleIndex + 1 + j * 5] = verticeIndex + j;
                    triangles[triangleIndex + 2 + j * 5] = verticeIndex + 8 + j;

                    //triangles[triangleIndex + 3 + j * 4] = verticeIndex + 1 + j;
                    //triangles[triangleIndex + 4 + j * 4] = verticeIndex + 8 + j;
                    //triangles[triangleIndex + 5 + j * 4] = verticeIndex + 9 + j;
                }
            }

            verticeIndex += 5;
            triangleIndex += 30;
        }

        BridgeGeneration.CreateBridge(parent, vertices, triangles, uvs, new Material[] { material });
    }

    public static void CreateBridge(Transform parent, Vector3[] vertices, int[] triangles, Vector2[] uvs, Material[] materials)
    {
        GameObject bridge = new GameObject("Bridge");
        bridge.transform.SetParent(parent);
        bridge.transform.localPosition = Vector3.zero;
        bridge.hideFlags = HideFlags.NotEditable;
        bridge.AddComponent<MeshFilter>();
        bridge.AddComponent<MeshRenderer>();
        bridge.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        bridge.GetComponent<MeshFilter>().sharedMesh = mesh;
        bridge.GetComponent<MeshRenderer>().sharedMaterials = materials;
        bridge.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
