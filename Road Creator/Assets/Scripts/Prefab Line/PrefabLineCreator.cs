using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLineCreator : MonoBehaviour {

    public GameObject prefab;
    public GameObject currentPoint;

    [HideInInspector]
    public float oldSpacing = 1;
    public float spacing = 1;

    [HideInInspector]
    public bool oldRotateAlongCurve = true;
    public bool rotateAlongCurve = true;

    [HideInInspector]
    public float oldScale = 1;
    public float scale = 1;

    public int smoothnessAmount = 3;

    [HideInInspector]
    public GlobalSettings globalSettings;

}
