using System;
using UnityEngine;

[System.Serializable]
public class IntersectionConnection  : IComparable<IntersectionConnection> {

    public SerializedVector3 leftPoint;
    public SerializedVector3 rightPoint;
    public SerializedVector3 lastPoint;
    public SerializedVector3 curvePoint;
    public float YRotation;
    public float length;
    public Point road;

    public int CompareTo (IntersectionConnection intersectionConnection)
    {
        if (intersectionConnection == null)
        {
            return 1;
        } else
        {
            return this.YRotation.CompareTo(intersectionConnection.YRotation);
        }
    }

}
