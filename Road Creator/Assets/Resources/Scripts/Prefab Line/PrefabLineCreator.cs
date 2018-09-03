using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLineCreator : MonoBehaviour
{

    public GameObject prefab;
    public GameObject currentPoint;

    public bool bendObjects = true;
    public bool modifyY = true;
    public float bendMultiplier = 1;
    public bool fillGap = true;
    public float spacing = 1;
    public bool rotateAlongCurve = true;

    public enum RotationDirection { forward, backward, left, right, randomY };
    public RotationDirection rotationDirection;

    public float scale = 1;
    public bool offsetPrefabWidth = true;

    public GlobalSettings globalSettings;

}
