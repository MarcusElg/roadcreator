using System;
using UnityEngine;

[System.Serializable]
public class IntersectionConnection  : IComparable<IntersectionConnection> {

    public Vector3 leftPoint;
    public Vector3 rightPoint;
    public Vector3 lastPoint;
    public Vector3 curvePoint;
    public float YRotation;
    public float length;
    public Point road;

    // Roundabout connection
    public Vector3 defaultCurvePoint;
    public Vector3 defaultCurvePoint2;
    public Vector3 curvePoint2;

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
