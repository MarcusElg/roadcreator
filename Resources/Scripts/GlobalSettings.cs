using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{

    public float pointSize = 0.5f;
    public float resolution = 0.25f;
    public int ignoreMouseRayLayer = 9;
    public int roadLayer = 10;
    public int oldAmountRoadGuidelines = 5;
    public int amountRoadGuidelines = 5;

    public bool debug = false;
    public bool roadCurved = true;

    public void UpdateRoadGuidelines()
    {
        RoadCreator[] objects = GameObject.FindObjectsOfType<RoadCreator>();

        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].CreateMesh();
        }
    }

}
