using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Global-Settings")]
public class GlobalSettings : MonoBehaviour
{

    public float pointSize = 0.5f;
    public float resolution = 0.25f;
    public int ignoreMouseRayLayer = 9;
    public int roadLayer = 10;

    public float oldRoadGuidelinesLength = 5;
    public float roadGuidelinesLength = 5;
    public float roadGuidelinesDistance = 10;
    public float roadGuidelinesSnapDistance = 1;

    public Color pointColour = Color.red;
    public Color controlPointColour = Color.yellow;
    public Color intersectionColour = Color.green;
    public Color cursorColour = Color.blue;
    public Color roadGuidelinesColour = Misc.lightGreen;
    public Color roadControlGuidelinesColour = Misc.darkGreen;
    public bool roadCurved = true;

    public void UpdateRoadGuidelines()
    {
        RoadCreator[] objects = GameObject.FindObjectsOfType<RoadCreator>();

        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].CreateMesh();
        }
    }

}
