using UnityEngine;

public class PointPackage {

    public Vector3[] prefabPoints;
    public Vector3[] startPoints;
    public Vector3[] endPoints;
    public bool[] rotateTowardsLeft;

    public PointPackage (Vector3[] prefabPoints, Vector3[] startPoints, Vector3[] endPoints, bool[] rotateTowardsLeft = null)
    {
        this.prefabPoints = prefabPoints;
        this.startPoints = startPoints;
        this.endPoints = endPoints;
        this.rotateTowardsLeft = rotateTowardsLeft;
    }

}
