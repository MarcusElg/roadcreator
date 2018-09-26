using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSplitter : MonoBehaviour
{

    public float leftWidth = 2;
    public float rightWidth = 2;
    public float height = 3;
    public float heightOffset = 0.02f;

    public float rightXOffset = 0;
    public float lowerRightXOffset = -0.75f;
    public float upperRightXOffset = 0.75f;

    public Material material;

    public GlobalSettings globalSettings;

    public void GenerateMesh()
    {
        transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, heightOffset, 0);
        transform.GetChild(0).GetChild(1).transform.localPosition = new Vector3(rightXOffset - upperRightXOffset, heightOffset, height);
        transform.GetChild(0).GetChild(2).transform.localPosition = new Vector3(rightXOffset - lowerRightXOffset, heightOffset, height);

        Point[] objects = GameObject.FindObjectsOfType<Point>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].intersectionConnection != null)
            {
                if (objects[i].intersectionConnection == transform.GetChild(0).GetChild(1).gameObject)
                {
                    objects[i].transform.position = transform.GetChild(0).GetChild(1).transform.position;
                }
                else if (objects[i].intersectionConnection == transform.GetChild(0).GetChild(2).gameObject)
                {
                    objects[i].transform.position = transform.GetChild(0).GetChild(2).transform.position;
                }
            }
        }

        if (material == null)
        {
            material = Resources.Load("Materials/Intersections/Intersection Connections/2L Connection") as Material;
        }

        Misc.GenerateSquareMesh(transform.GetChild(1), new Vector3(-leftWidth, heightOffset, 0), new Vector3(leftWidth, heightOffset, 0), new Vector3(-rightWidth + rightXOffset, heightOffset, height), new Vector3(rightWidth + rightXOffset, heightOffset, height), material);
    }

}
