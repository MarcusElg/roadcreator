using System;

[System.Serializable]
public class IntersectionConnection  : IComparable<IntersectionConnection> {

    public SerializedVector3 leftPoint;
    public SerializedVector3 rightPoint;
    public SerializedVector3 lastPoint;
    public float YRotation;
    public float width;
    public float length;

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
