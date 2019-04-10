using UnityEngine;

public class PointPackage
{

    public Vector3[] prefabPoints;
    public Vector3[] lerpPoints;
    public float[] startTimes;
    public float[] endTimes;

    public PointPackage(Vector3[] prefabPoints, Vector3[] lerpPoints, float[] startTimes, float[] endTimes)
    {
        this.prefabPoints = prefabPoints;
        this.lerpPoints = lerpPoints;
        this.startTimes = startTimes;
        this.endTimes = endTimes;
    }

}
