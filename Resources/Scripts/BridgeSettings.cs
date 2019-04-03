using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BridgeSettings
{

    public Material[] bridgeMaterials;
    public float yOffsetFirstStep = 0.25f;
    public float yOffsetSecondStep = 0.5f;
    public float widthPercentageFirstStep = 0.6f;
    public float widthPercentageSecondStep = 0.6f;
    public float extraWidth = 0.2f;

    // Suspension bridge
    public GameObject cablePrefab;
    public int sections = 1;
    public float cableScale = 1;
    public float topCableScale = 1.5f;
    public float cableGap = 1;
    public float height = 10f;
    public float widthOffset = 0f;

}
