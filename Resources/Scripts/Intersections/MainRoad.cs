using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MainRoad
{
    public bool open = true;
    public int startIndex;
    public int endIndex;
    public bool flipTexture;

    public MainRoad(int startIndex, int endIndex, bool flipTexture)
    {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.flipTexture = flipTexture;
    }
}
