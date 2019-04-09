using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadGuideline
{

    public Vector3 startPoint;
    public Vector3 centerPoint;
    public Vector3 endPoint;

    public RoadGuideline (Vector3 startPoint, Vector3 centerPoint, Vector3 endPoint)
    {
        this.startPoint = startPoint;
        this.centerPoint = centerPoint;
        this.endPoint = endPoint;
    }

}
