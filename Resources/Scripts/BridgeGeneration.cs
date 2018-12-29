using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGeneration
{

    public static void GenerateSimpleBridge(Vector3[] points, Vector3[] nextPoints, Vector3 previousPoint, Transform parent, float startWidth, float endWidth, float extraWidthLeft, float extraWidthRight, float yOffsetFirstStep, float yOffsetSecondStep, float widthPercentageFirstStep, float widthPercentageSecondStep, float inputHeightOffset, Material[] materials)
    {
        Vector3[] vertices = new Vector3[points.Length * 8];
        Vector2[] uvs = new Vector2[vertices.Length];
        int numberTriangles = 4 * (points.Length - 1);
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
            vertices[verticeIndex] = (points[i] - left * roadWidthLeft) - parent.position;
            vertices[verticeIndex].y = points[i].y + heightOffset - parent.transform.position.y;
            vertices[verticeIndex + 1] = (points[i] - left * roadWidthLeft) - parent.position;
            vertices[verticeIndex + 1].y = points[i].y - yOffsetFirstStep + heightOffset - parent.transform.position.y;
            vertices[verticeIndex + 2] = (points[i] - left * roadWidthLeft * widthPercentageFirstStep) - parent.position;
            vertices[verticeIndex + 2].y = points[i].y - yOffsetFirstStep + heightOffset - parent.transform.position.y;
            vertices[verticeIndex + 3] = (points[i] - left * roadWidthLeft * widthPercentageFirstStep * widthPercentageSecondStep) - parent.position;
            vertices[verticeIndex + 3].y = points[i].y - yOffsetFirstStep - yOffsetSecondStep + heightOffset - parent.transform.position.y;

            vertices[verticeIndex + 4] = (points[i] + left * roadWidthRight * widthPercentageFirstStep * widthPercentageSecondStep) - parent.position;
            vertices[verticeIndex + 4].y = points[i].y - yOffsetFirstStep - yOffsetSecondStep + heightOffset - parent.transform.position.y;
            vertices[verticeIndex + 5] = (points[i] + left * roadWidthRight * widthPercentageFirstStep) - parent.position;
            vertices[verticeIndex + 5].y = points[i].y - yOffsetFirstStep + heightOffset - parent.transform.position.y;
            vertices[verticeIndex + 6] = (points[i] + left * roadWidthRight) - parent.position;
            vertices[verticeIndex + 6].y = points[i].y - yOffsetFirstStep + heightOffset - parent.transform.position.y;
            vertices[verticeIndex + 7] = (points[i] + left * roadWidthRight) - parent.position;
            vertices[verticeIndex + 7].y = points[i].y - parent.transform.position.y + heightOffset;

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
            }

            verticeIndex += 8;
            triangleIndex += 54;
        }

        BridgeGeneration.CreateBridge(parent, vertices, triangles, uvs, materials);
    }

    public static void GenerateSimpleBridgeIntersection(Vector3[] inputVertices, Transform parent, float extraWidth, float heightOffset, float yOffsetFirstStep, float yOffsetSecondStep, float widthPercentageFirstStep, float widthPercentageSecondStep, Material[] materials)
    {
        Vector3[] vertices = new Vector3[inputVertices.Length * 3];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[inputVertices.Length * 30];
        int verticeIndex = 0;
        int triangleIndex = 0;

        for (int i = 0; i < inputVertices.Length; i += 2)
        {
            Vector3 verticeDifference = inputVertices[i + 1] - inputVertices[i];

            // |_   _|
            //   \_/
            vertices[verticeIndex] = inputVertices[i] - verticeDifference.normalized * extraWidth;
            vertices[verticeIndex].y = inputVertices[i].y - inputVertices[i].y;
            vertices[verticeIndex + 1] = inputVertices[i] - verticeDifference.normalized * extraWidth;
            vertices[verticeIndex + 1].y = inputVertices[i].y - yOffsetFirstStep - inputVertices[i].y;
            vertices[verticeIndex + 2] = inputVertices[i + 1] - verticeDifference * widthPercentageFirstStep - verticeDifference.normalized * extraWidth;
            vertices[verticeIndex + 2].y = inputVertices[i].y - yOffsetFirstStep - inputVertices[i].y;
            vertices[verticeIndex + 3] = inputVertices[i + 1] - verticeDifference.normalized * extraWidth - verticeDifference * widthPercentageFirstStep * widthPercentageSecondStep;
            vertices[verticeIndex + 3].y = inputVertices[i].y - yOffsetFirstStep - yOffsetSecondStep - inputVertices[i].y;
            vertices[verticeIndex + 4] = inputVertices[i + 1];
            vertices[verticeIndex + 4].y = inputVertices[i].y - yOffsetFirstStep - yOffsetSecondStep - inputVertices[i].y;
            vertices[verticeIndex + 5] = inputVertices[i + 1];
            vertices[verticeIndex + 5].y = inputVertices[i].y - inputVertices[i].y;

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

                triangles[triangleIndex + 24] = verticeIndex + 0;
                triangles[triangleIndex + 25] = verticeIndex + 6;
                triangles[triangleIndex + 26] = verticeIndex + 5;

                triangles[triangleIndex + 27] = verticeIndex + 6;
                triangles[triangleIndex + 28] = verticeIndex + 11;
                triangles[triangleIndex + 29] = verticeIndex + 5;
            }

            verticeIndex += 6;
            triangleIndex += 30;
        }

        BridgeGeneration.CreateBridge(parent, vertices, triangles, uvs, materials);
    }

    public static void CreateBridge(Transform parent, Vector3[] vertices, int[] triangles, Vector2[] uvs, Material[] materials)
    {
        GameObject bridge = new GameObject("Bridge");
        bridge.transform.SetParent(parent);
        bridge.transform.SetAsLastSibling();
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
