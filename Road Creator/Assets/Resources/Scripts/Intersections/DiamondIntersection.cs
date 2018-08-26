using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondIntersection : MonoBehaviour
{

    public float width = 3;
    public float height = 3;
    public float heightOffset = 0.02f;
    public Material centerMaterial;

    public bool upperLeftConnection = true;
    public float upperLeftConnectionWidth = 1.5f;
    public float upperLeftConnectionHeight = 1;
    public Material upperLeftConnectionMaterial;
    public int upperLeftConnectionResolution = 2;

    public bool upperRightConnection = true;
    public float upperRightConnectionWidth = 1.5f;
    public float upperRightConnectionHeight = 1;
    public Material upperRightConnectionMaterial;
    public int upperRightConnectionResolution = 2;

    public bool lowerLeftConnection = true;
    public float lowerLeftConnectionWidth = 1.5f;
    public float lowerLeftConnectionHeight = 1;
    public Material lowerLeftConnectionMaterial;
    public int lowerLeftConnectionResolution = 2;

    public bool lowerRightConnection = true;
    public float lowerRightConnectionWidth = 1.5f;
    public float lowerRightConnectionHeight = 1;
    public Material lowerRightConnectionMaterial;
    public int lowerRightConnectionResolution = 2;

    public GlobalSettings globalSettings;

    public void GenerateMeshes()
    {
        if (centerMaterial == null)
        {
            centerMaterial = Resources.Load("Materials/Intersections/Grid Intersection") as Material;
        }

        if (upperLeftConnectionMaterial == null)
        {
            upperLeftConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        if (upperRightConnectionMaterial == null)
        {
            upperRightConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        if (lowerLeftConnectionMaterial == null)
        {
            lowerLeftConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        if (lowerRightConnectionMaterial == null)
        {
            lowerRightConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        GenerateMesh(transform.GetChild(1), new Vector3(-width, heightOffset, 0), new Vector3(0, heightOffset, -height), new Vector3(0, heightOffset, height), new Vector3(width, heightOffset, 0), centerMaterial);

        if (upperLeftConnection == true)
        {
            transform.GetChild(0).GetChild(0).localPosition = Misc.GetCenter(new Vector3(-width, 0, 0), new Vector3(0, 0, height));
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.FromToRotation(Vector3.left, new Vector3(-width, heightOffset, 0) - new Vector3(0, heightOffset, height));
            transform.GetChild(0).GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, upperLeftConnectionHeight);
            float connectionWidth = Vector3.Distance(new Vector3(-width, heightOffset, 0), new Vector3(0, heightOffset, height)) / 2;
            Misc.GenerateIntersectionConnection(connectionWidth, upperLeftConnectionWidth, upperLeftConnectionResolution * 2, upperLeftConnectionHeight, heightOffset, transform.GetChild(0).GetChild(0).GetChild(0), upperLeftConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (upperRightConnection == true)
        {
            transform.GetChild(0).GetChild(1).localPosition = Misc.GetCenter(new Vector3(width, 0, 0), new Vector3(0, 0, height));
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.FromToRotation(Vector3.right, new Vector3(width, heightOffset, 0) - new Vector3(0, heightOffset, height));
            transform.GetChild(0).GetChild(1).GetChild(1).localPosition = new Vector3(0, 0, upperRightConnectionHeight);
            float connectionWidth = Vector3.Distance(new Vector3(width, heightOffset, 0), new Vector3(0, heightOffset, height)) / 2;
            Misc.GenerateIntersectionConnection(connectionWidth, upperRightConnectionWidth, upperRightConnectionResolution * 2, upperRightConnectionHeight, heightOffset, transform.GetChild(0).GetChild(1).GetChild(0), upperRightConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (lowerLeftConnection == true)
        {
            transform.GetChild(0).GetChild(2).localPosition = Misc.GetCenter(new Vector3(-width, 0, 0), new Vector3(0, 0, -height));
            transform.GetChild(0).GetChild(2).localRotation = Quaternion.FromToRotation(Vector3.left, new Vector3(0, heightOffset, -height) - new Vector3(-width, heightOffset, 0));
            transform.GetChild(0).GetChild(2).GetChild(1).localPosition = new Vector3(0, 0, lowerLeftConnectionHeight);
            float connectionWidth = Vector3.Distance(new Vector3(-width, heightOffset, 0), new Vector3(0, heightOffset, -height)) / 2;
            Misc.GenerateIntersectionConnection(connectionWidth, lowerLeftConnectionWidth, lowerLeftConnectionResolution * 2, lowerLeftConnectionHeight, heightOffset, transform.GetChild(0).GetChild(2).GetChild(0), lowerLeftConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (lowerRightConnection == true)
        {
            transform.GetChild(0).GetChild(3).localPosition = Misc.GetCenter(new Vector3(width, 0, 0), new Vector3(0, 0, -height));
            transform.GetChild(0).GetChild(3).localRotation = Quaternion.FromToRotation(Vector3.right, new Vector3(0, heightOffset, -height) - new Vector3(width, heightOffset, 0));
            transform.GetChild(0).GetChild(3).GetChild(1).localPosition = new Vector3(0, 0, lowerRightConnectionHeight);
            float connectionWidth = Vector3.Distance(new Vector3(width, heightOffset, 0), new Vector3(0, heightOffset, -height)) / 2;
            Misc.GenerateIntersectionConnection(connectionWidth, lowerRightConnectionWidth, lowerRightConnectionResolution * 2, lowerRightConnectionHeight, heightOffset, transform.GetChild(0).GetChild(3).GetChild(0), lowerRightConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }
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
        meshOwner.GetComponent<MeshRenderer>().sharedMaterial = material;
        meshOwner.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
