using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareIntersection : MonoBehaviour {

    public float width = 4;
    public float height = 4;
    public float heightOffset = 0.02f;

    public bool upConnection = true;
    public float upConnectionWidth = 1.5f;
    public float upConnectionHeight = 1;

    public bool downConnection = true;
    public float downConnectionWidth = 1.5f;
    public float downConnectionHeight = 1;

    public bool leftConnection = true;
    public float leftConnectionWidth = 1.5f;
    public float leftConnectionHeight = 1;

    public bool rightConnection = true;
    public float rightConnectionWidth = 1.5f;
    public float rightConnectionHeight = 1;

    public GlobalSettings globalSettings;

}
