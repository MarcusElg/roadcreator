using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareIntersection : MonoBehaviour {

    [HideInInspector]
    public float oldWidth = 3;
    public float width = 3;

    [HideInInspector]
    public float oldHeight = 3;
    public float height = 3;

    [HideInInspector]
    public GlobalSettings globalSettings;

    public List<Key> sideVariables;
    public List<Key> oldSideVariables;

}
