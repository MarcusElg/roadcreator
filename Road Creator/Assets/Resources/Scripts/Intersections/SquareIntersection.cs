using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareIntersection : MonoBehaviour {

    public float width = 3;
    public float height = 3;
    public float heightOffset = 0.02f;
    public Material centerMaterial;

    public bool upConnection = true;
    public float upConnectionWidth = 1.5f;
    public float upConnectionHeight = 1;
    public Material upConnectionMaterial;
    public int upConnectionResolution = 2;

    public bool downConnection = true;
    public float downConnectionWidth = 1.5f;
    public float downConnectionHeight = 1;
    public Material downConnectionMaterial;
    public int downConnectionResolution = 2;

    public bool leftConnection = true;
    public float leftConnectionWidth = 1.5f;
    public float leftConnectionHeight = 1;
    public Material leftConnectionMaterial;
    public int leftConnectionResolution = 2;

    public bool rightConnection = true;
    public float rightConnectionWidth = 1.5f;
    public float rightConnectionHeight = 1;
    public Material rightConnectionMaterial;
    public int rightConnectionResolution = 2;

    public GlobalSettings globalSettings;

    public void GenerateMeshes()
    {
        if (centerMaterial == null)
        {
            centerMaterial = Resources.Load("Materials/Intersections/Grid Intersection") as Material;
        }

        if (upConnectionMaterial == null)
        {
            upConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        if (downConnectionMaterial == null)
        {
            downConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        if (leftConnectionMaterial == null)
        {
            leftConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        if (rightConnectionMaterial == null)
        {
            rightConnectionMaterial = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        Misc.GenerateSquareMesh(transform.GetChild(1), new Vector3(-width, heightOffset, -height), new Vector3(width, heightOffset, -height), new Vector3(-width, heightOffset, height), new Vector3(width, heightOffset, height), centerMaterial);

        if (upConnection == true)
        {
            transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, height);
            transform.GetChild(0).GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, upConnectionHeight);
            Misc.GenerateIntersectionConnection(width, upConnectionWidth, upConnectionResolution * 2, upConnectionHeight, heightOffset, transform.GetChild(0).GetChild(0).GetChild(0), upConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (downConnection == true)
        {
            transform.GetChild(0).GetChild(1).localPosition = new Vector3(0, 0, -height);
            transform.GetChild(0).GetChild(1).GetChild(1).localPosition = new Vector3(0, 0, downConnectionHeight);
            Misc.GenerateIntersectionConnection(width, downConnectionWidth, downConnectionResolution * 2, downConnectionHeight, heightOffset, transform.GetChild(0).GetChild(1).GetChild(0), downConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (leftConnection == true)
        {
            transform.GetChild(0).GetChild(2).localPosition = new Vector3(-width, 0, 0);
            transform.GetChild(0).GetChild(2).GetChild(1).localPosition = new Vector3(0, 0, leftConnectionHeight);
            Misc.GenerateIntersectionConnection(height, leftConnectionWidth, leftConnectionResolution * 2, leftConnectionHeight, heightOffset, transform.GetChild(0).GetChild(2).GetChild(0), leftConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }

        if (rightConnection == true)
        {
            transform.GetChild(0).GetChild(3).localPosition = new Vector3(width, 0, 0);
            transform.GetChild(0).GetChild(3).GetChild(1).localPosition = new Vector3(0, 0, rightConnectionHeight);
            Misc.GenerateIntersectionConnection(height, rightConnectionWidth, rightConnectionResolution * 2, rightConnectionHeight, heightOffset, transform.GetChild(0).GetChild(3).GetChild(0), rightConnectionMaterial);
        }
        else
        {
            transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
            transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
        }
    }

}
