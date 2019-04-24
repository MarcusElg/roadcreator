using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Windows;

public class RoadCreatorSettings : ScriptableObject
{

    public float pointSize = 0.5f;
    public float resolution = 0.25f;
    public int ignoreMouseRayLayer = 9;
    public int roadLayer = 10;
    public bool hideNonEditableChildren = true;

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

    private static RoadCreatorSettings GetOrCreateSettings()
    {
        RoadCreatorSettings settings = AssetDatabase.LoadAssetAtPath<RoadCreatorSettings>("Assets/Editor/RoadCreatorSettings.asset");
        if (settings == null)
        {
            if (Directory.Exists("Assets/Editor") == false)
            {
                Directory.CreateDirectory("Assets/Editor");
            }

            settings = ScriptableObject.CreateInstance<RoadCreatorSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Editor/RoadCreatorSettings.asset");
            AssetDatabase.SaveAssets();
        }

        return settings;
    }

    public static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }

    public static void UpdateRoadGuidelines()
    {
        RoadCreator[] objects = GameObject.FindObjectsOfType<RoadCreator>();

        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].CreateMesh();
        }
    }

}
