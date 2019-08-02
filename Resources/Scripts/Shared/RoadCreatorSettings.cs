#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Windows;

public class RoadCreatorSettings : ScriptableObject
{

    public float pointSize = 0.5f;
    public float resolution = 0.25f;
    public bool hideNonEditableChildren = true;
    public int roundaboutConnectionIndexOffset = 1;
    public enum PointShape { Cylinder, Sphere, Cube, Cone };
    public PointShape pointShape;

    public float oldRoadGuidelinesLength = 5;
    public float roadGuidelinesLength = 5;
    public float roadGuidelinesDistance = 10;
    public float roadGuidelinesSnapDistance = 1;

    public int defaultLanes = 2;
    public Material defaultBaseMaterial;
    public Material defaultRoadOverlayMaterial;
    public Material defaultExtraMeshOverlayMaterial;
    public Material defaultIntersectionOverlayMaterial;
    public Material[] defaultSimpleBridgeMaterials;
    public GameObject defaultPillarPrefab;
    public GameObject defaultBridgePillarPrefab;
    public GameObject defaultCustomBridgePrefab;
    public GameObject defaultPrefabLinePrefab;
    public GameObject defaultPrefabLineStartPrefab;
    public GameObject defaultPrefabLineEndPrefab;

    public GameObject leftTurnMarking;
    public GameObject forwardTurnMarking;
    public GameObject rightTurnMarking;
    public GameObject leftForwardTurnMarking;
    public GameObject rightForwardTurnMarking;
    public GameObject leftRightTurnMarking;
    public GameObject leftRightForwardTurnMarking;

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

        if (defaultBridgePillarPrefab == null)
        {
            defaultBridgePillarPrefab = Resources.Load("Prefabs/Bridges/Pillars/Cylinder Bridge Pillar") as GameObject;
        }

        if (defaultCustomBridgePrefab == null)
        {
            defaultCustomBridgePrefab = Resources.Load("Prefabs/Bridges/Complete/Suspension Bridge") as GameObject;
        }

        if (defaultPrefabLinePrefab == null)
        {
            defaultPrefabLinePrefab = Resources.Load("Prefabs/Concrete Barrier") as GameObject;
        }

        if (leftTurnMarking == null)
        {
            leftTurnMarking = Resources.Load("Prefabs/Turn Markings/Left Arrow") as GameObject;
        }

        if (forwardTurnMarking == null)
        {
            forwardTurnMarking = Resources.Load("Prefabs/Turn Markings/Forward Arrow") as GameObject;
        }

        if (rightTurnMarking == null)
        {
            rightTurnMarking = Resources.Load("Prefabs/Turn Markings/Right Arrow") as GameObject;
        }

        if (leftForwardTurnMarking == null)
        {
            leftForwardTurnMarking = Resources.Load("Prefabs/Turn Markings/Forward And Left Arrow") as GameObject;
        }

        if (rightForwardTurnMarking == null)
        {
            rightForwardTurnMarking = Resources.Load("Prefabs/Turn Markings/Forward And Right Arrow") as GameObject;
        }

        if (leftRightTurnMarking == null)
        {
            leftRightTurnMarking = Resources.Load("Prefabs/Turn Markings/Left And Right Arrow") as GameObject;
        }

        if (leftRightForwardTurnMarking == null)
        {
            leftRightForwardTurnMarking = Resources.Load("Prefabs/Turn Markings/Forward, Left And Right Arrow") as GameObject;
        }
    }

}
#endif