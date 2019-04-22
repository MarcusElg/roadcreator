using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoadCreatorProjectSettings
{
    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        SettingsProvider settingsProvider = new SettingsProvider("Project/RoadCreator", SettingsScope.Project)
        {
            label = "Road Creator",

            guiHandler = (searchContext) =>
            {
                SerializedObject settings = RoadCreatorSettings.GetSerializedSettings();

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("pointSize").floatValue = Mathf.Max(0.1f, EditorGUILayout.FloatField("Point Size", settings.FindProperty("pointSize").floatValue));
                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    Transform[] objects = GameObject.FindObjectsOfType<Transform>();

                    for (int i = 0; i < objects.Length; i++)
                    {
                        if (objects[i].name.Contains("Connection Point") || objects[i].name == "Start Point" || objects[i].name == "Control Point" || objects[i].name == "End Point")
                        {
                            objects[i].GetComponent<BoxCollider>().size = new Vector3(settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue);
                        }
                    }

                    UpdateSettings();
                }

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("resolution").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Resolution", settings.FindProperty("resolution").floatValue), 0.01f, 2f);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();

                    RoadCreator[] roads = GameObject.FindObjectsOfType<RoadCreator>();
                    for (int i = 0; i < roads.Length; i++)
                    {
                        roads[i].CreateMesh();
                    }

                    Intersection[] intersections = GameObject.FindObjectsOfType<Intersection>();
                    for (int i = 0; i < intersections.Length; i++)
                    {
                        intersections[i].CreateMesh();
                    }
                }

                EditorGUI.BeginChangeCheck();
                settings.FindProperty("ignoreMouseRayLayer").intValue = Mathf.Clamp(EditorGUILayout.IntField("Ignore Mouse Ray Layer", settings.FindProperty("ignoreMouseRayLayer").intValue), 9, 31);
                settings.FindProperty("roadLayer").intValue = Mathf.Clamp(EditorGUILayout.IntField("Road Layer", settings.FindProperty("roadLayer").intValue), 9, 31);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                }

                GUIStyle guiStyle = new GUIStyle();
                guiStyle.fontStyle = FontStyle.Bold;

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                GUILayout.Label("Road Guidelines", guiStyle);
                settings.FindProperty("roadGuidelinesLength").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Road Guidelines Length (each side)", settings.FindProperty("roadGuidelinesLength").floatValue), 0, 15);
                settings.FindProperty("roadGuidelinesDistance").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Road Guidelines Display Distance", settings.FindProperty("roadGuidelinesDistance").floatValue), 1, 50);
                settings.FindProperty("roadGuidelinesSnapDistance").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Road Guidelines Snap Distance", settings.FindProperty("roadGuidelinesSnapDistance").floatValue), 0.1f, 5);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                    RoadCreatorSettings.UpdateRoadGuidelines();
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                GUILayout.Label("Colours", guiStyle);
                settings.FindProperty("pointColour").colorValue = EditorGUILayout.ColorField("Point Colour", settings.FindProperty("pointColour").colorValue);
                settings.FindProperty("controlPointColour").colorValue = EditorGUILayout.ColorField("Control Point Colour", settings.FindProperty("controlPointColour").colorValue);
                settings.FindProperty("intersectionColour").colorValue = EditorGUILayout.ColorField("Intersection Point Colour", settings.FindProperty("intersectionColour").colorValue);
                settings.FindProperty("cursorColour").colorValue = EditorGUILayout.ColorField("Cursor Colour", settings.FindProperty("cursorColour").colorValue);
                settings.FindProperty("roadGuidelinesColour").colorValue = EditorGUILayout.ColorField("Road Guidelines Colour", settings.FindProperty("roadGuidelinesColour").colorValue);
                settings.FindProperty("roadControlGuidelinesColour").colorValue = EditorGUILayout.ColorField("Road Control Guidelines Colour", settings.FindProperty("roadControlGuidelinesColour").colorValue);

                if (GUILayout.Button("Reset Colours"))
                {
                    settings.FindProperty("pointColour").colorValue = Color.red;
                    settings.FindProperty("controlPointColour").colorValue = Color.yellow;
                    settings.FindProperty("intersectionColour").colorValue = Color.green;
                    settings.FindProperty("cursorColour").colorValue = Color.blue;
                    settings.FindProperty("roadGuidelinesColour").colorValue = Misc.lightGreen;
                    settings.FindProperty("roadControlGuidelinesColour").colorValue = Misc.darkGreen;
                }

                if (EditorGUI.EndChangeCheck() == true)
                {
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    UpdateSettings();
                }
            }
        };

        return settingsProvider;
    }

    private static void UpdateSettings()
    {
        RoadSystem[] roadSystems = GameObject.FindObjectsOfType<RoadSystem>();
        for (int i = 0; i < roadSystems.Length; i++)
        {
            for (int j = 0; j < roadSystems[i].transform.childCount; j++)
            {
                Transform transform = roadSystems[i].transform.GetChild(j);

                if (transform.GetComponent<RoadCreator>() != null)
                {
                    transform.GetComponent<RoadCreator>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<RoadSegment>() != null)
                {
                    transform.GetComponent<RoadSegment>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<PrefabLineCreator>() != null)
                {
                    transform.GetComponent<PrefabLineCreator>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<Intersection>() != null)
                {
                    transform.GetComponent<Intersection>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
                else if (transform.GetComponent<RoadSystem>() != null)
                {
                    transform.GetComponent<RoadSystem>().settings = RoadCreatorSettings.GetSerializedSettings();
                }
            }
        }
    }

}
