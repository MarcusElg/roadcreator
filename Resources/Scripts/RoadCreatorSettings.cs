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

    public Material defaultBaseMaterial;
    public Material defaultRoadOverlayMaterial;
    public Material defaultExtraMeshMaterial;
    public Material defaultIntersectionOverlayMaterial;
    public Material[] defaultSimpleBridgeMaterials;
    public GameObject defaultPillarPrefab;
    public GameObject defaultCustomBridgePrefab;
    public GameObject defaultPrefabLinePrefab;

    public Color pointColour = Color.red;
    public Color controlPointColour = Color.yellow;
    public Color intersectionColour = Color.green;
    public Color cursorColour = Color.blue;
    public Color roadGuidelinesColour = Misc.lightGreen;
    public Color roadControlGuidelinesColour = Misc.darkGreen;
    public bool roadCurved = true;

    private void OnEnable()
    {
        CheckDefaults();
    }

    public static RoadCreatorSettings GetOrCreateSettings()
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

    public void CheckDefaults()
    {
        if (defaultBaseMaterial == null)
        {
            defaultBaseMaterial = Resources.Load("Materials/Asphalt") as Material;
        }

        if (defaultRoadOverlayMaterial == null)
        {
            defaultRoadOverlayMaterial = Resources.Load("Materials/Roads/2 Lane Roads/2L Road") as Material;
        }

        if (defaultExtraMeshMaterial == null)
        {
            defaultExtraMeshMaterial = Resources.Load("Materials/Asphalt") as Material;
        }

        if (defaultIntersectionOverlayMaterial == null)
        {
            defaultIntersectionOverlayMaterial = Resources.Load("Materials/Intersections/Asphalt Intersection") as Material;
        }

        if (defaultSimpleBridgeMaterials == null || defaultSimpleBridgeMaterials.Length == 0)
        {
            defaultSimpleBridgeMaterials = new Material[] { Resources.Load("Materials/Concrete") as Material };
        }

        if (defaultPillarPrefab == null)
        {
            defaultPillarPrefab = Resources.Load("Prefabs/Bridges/Pillars/Oval Bridge Pillar") as GameObject;
        }

        if (defaultCustomBridgePrefab == null)
        {
            defaultCustomBridgePrefab = Resources.Load("Prefabs/Bridges/Complete/Suspension Bridge") as GameObject;
        }

        if (defaultPrefabLinePrefab == null)
        {
            defaultPrefabLinePrefab = Resources.Load("Prefabs/Concrete Barrier") as GameObject;
        }
    }

}
