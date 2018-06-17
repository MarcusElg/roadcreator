using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleIntersection : MonoBehaviour {

    public float width = 4;
    public float height = 4;
    public float heightOffset = 0.02f;

    public Material centerMaterial;
    public Material connectionMaterial;

    public bool downConnection = true;
    public float downConnectionWidth = 1.5f;
    public float downConnectionHeight = 1;

    public bool leftConnection = true;
    public float leftConnectionWidth = 1.5f;
    public float leftConnectionHeight = 1;

    public bool rightConnection = true;
    public float rightConnectionWidth = 1.5f;
    public float rightConnectionHeight = 1;

    public GlobalSettings globalSettings;

    public void GenerateMeshes()
    {
        if (centerMaterial == null)
        {
            Debug.Log("You have to select a center material before generating the intersection");
            return;
        }

        if (connectionMaterial == null)
        {
            Debug.Log("You have to select a connection material before generating the intersection");
            return;
        }

        GenerateCenterMesh();

        if (downConnection == true)
        {
            transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, -height);
            transform.GetChild(0).GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, downConnectionHeight);
            GenerateMesh(transform.GetChild(0).GetChild(0).GetChild(0), new Vector3(-width, heightOffset, 0), new Vector3(width, heightOffset, 0), new Vector3(-downConnectionWidth, heightOffset, downConnectionHeight), new Vector3(downConnectionWidth, heightOffset, downConnectionHeight), connectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (leftConnection == true)
        {
            transform.GetChild(0).GetChild(1).localPosition = Misc.GetCenter(new Vector3(-width, heightOffset, -height), new Vector3(0, heightOffset, height));
            transform.GetChild(0).GetChild(1).rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(0, heightOffset, height) - new Vector3(-width, heightOffset, -height));
            transform.GetChild(0).GetChild(1).GetChild(1).localPosition = new Vector3(0, 0, leftConnectionHeight);
            float connectionHeight = Vector3.Distance(new Vector3(-width, heightOffset, -height), new Vector3(0, heightOffset, height)) / 2;
            GenerateMesh(transform.GetChild(0).GetChild(1).GetChild(0), new Vector3(-connectionHeight, heightOffset, 0), new Vector3(connectionHeight, heightOffset, 0), new Vector3(-leftConnectionWidth, heightOffset, leftConnectionHeight), new Vector3(leftConnectionWidth, heightOffset, leftConnectionHeight), connectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (rightConnection == true)
        {
            transform.GetChild(0).GetChild(2).localPosition = Misc.GetCenter(new Vector3(width, heightOffset, -height), new Vector3(0, heightOffset, height));
            transform.GetChild(0).GetChild(2).rotation = Quaternion.FromToRotation(Vector3.left, new Vector3(0, heightOffset, height) - new Vector3(width, heightOffset, -height));
            transform.GetChild(0).GetChild(2).GetChild(1).localPosition = new Vector3(0, 0, rightConnectionHeight);
            float connectionHeight = Vector3.Distance(new Vector3(-width, heightOffset, -height), new Vector3(0, heightOffset, height)) / 2;
            GenerateMesh(transform.GetChild(0).GetChild(2).GetChild(0), new Vector3(-connectionHeight, heightOffset, 0), new Vector3(connectionHeight, heightOffset, 0), new Vector3(-rightConnectionWidth, heightOffset, rightConnectionHeight), new Vector3(rightConnectionWidth, heightOffset, rightConnectionHeight), connectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }
    }

    private void GenerateCenterMesh()
    {
        //GenerateMesh(new Vector3(-width, heightOffset, -height), new Vector3(width, heightOffset, -height), new Vector3(-width, heightOffset, height), new Vector3(width, heightOffset, height));
        Vector3[] vertices = new Vector3[3];
        Vector2[] uvs = new Vector2[3];

        vertices[0] = new Vector3(-width, heightOffset, -height);
        vertices[1] = new Vector3(width, heightOffset, -height);
        vertices[2] = new Vector3(0, heightOffset, height);

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0.5f, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0 };
        mesh.uv = uvs;

        transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = mesh;
        Material newMaterial = centerMaterial;
        Texture texture = newMaterial.mainTexture;
        texture.wrapMode = TextureWrapMode.Clamp;
        newMaterial.mainTexture = texture;
        transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = centerMaterial;
        transform.GetChild(1).GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void GenerateMesh(Transform meshOwner, Vector3 pointOne, Vector3 pointTwo, Vector3 pointThree, Vector3 pointFour, Material material)
    {
        Vector3[] vertices = new Vector3[4];
        Vector2[] uvs = new Vector2[4];

        vertices[0] = pointOne;
        vertices[1] = pointTwo;
        vertices[2] = pointThree;
        vertices[3] = pointFour;

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[] { 2, 1, 0, 1, 2, 3 };
        mesh.uv = uvs;

        meshOwner.GetComponent<MeshFilter>().sharedMesh = mesh;
        Material newMaterial = material;
        Texture texture = newMaterial.mainTexture;
        texture.wrapMode = TextureWrapMode.Clamp;
        newMaterial.mainTexture = texture;
        meshOwner.GetComponent<MeshRenderer>().sharedMaterial = material;
        meshOwner.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
