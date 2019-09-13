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

    // Custom mesh
    public GameObject bridgeMesh;
    public float sections = 1;
    public float yScale = 1;
    public float xOffset = 0;
    public bool adaptToTerrain = false;

    public BridgeSettings()
    {

    }

    public BridgeSettings(BridgeSettings bridgeSettings)
    {
        bridgeMaterials = bridgeSettings.bridgeMaterials;
        yOffsetFirstStep = bridgeSettings.yOffsetFirstStep;
        yOffsetSecondStep = bridgeSettings.yOffsetSecondStep;
        widthPercentageFirstStep = bridgeSettings.widthPercentageFirstStep;
        widthPercentageSecondStep = bridgeSettings.widthPercentageSecondStep;
        extraWidth = bridgeSettings.extraWidth;

        bridgeMesh = bridgeSettings.bridgeMesh;
        sections = bridgeSettings.sections;
        yScale = bridgeSettings.yScale;
        xOffset = bridgeSettings.xOffset;
        adaptToTerrain = bridgeSettings.adaptToTerrain;
    }
}
