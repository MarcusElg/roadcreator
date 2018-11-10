using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedVector3 {

    public float x;
    public float y;
    public float z;

    public SerializedVector3 (float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializedVector3 (Vector3 vector3)
    {
        this.x = vector3.x;
        this.y = vector3.y;
        this.z = vector3.z;
    }

    public Vector3 ToNormalVector3 ()
    {
        return new Vector3(x, y, z);
    }

}
