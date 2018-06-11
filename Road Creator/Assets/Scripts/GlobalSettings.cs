using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{

    [HideInInspector]
    public float oldPointSize = 0.2f;
    public float pointSize = 0.2f;

    [HideInInspector]
    public float oldResolution = 0.5f;
    public float resolution = 0.5f;

    public int layer = 9;

    public Material defaultRoadMaterial;
    public Material defaultShoulderMaterial;

}
