using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlobalSettings))]
public class GlobalSettingsEditor : Editor
{

    GlobalSettings settings;

    private void OnEnable()
    {
        settings = ((GlobalSettings)target);
        settings.transform.hideFlags = HideFlags.NotEditable;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        settings.pointSize = Mathf.Max(0.1f, EditorGUILayout.FloatField("Point Size", settings.pointSize));
        if (EditorGUI.EndChangeCheck() == true)
        {
            Transform[] objects = GameObject.FindObjectsOfType<Transform>();

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].name.Contains("Connection Point") || objects[i].name == "Start Point" || objects[i].name == "Control Point" || objects[i].name == "End Point")
                {
                    objects[i].GetComponent<BoxCollider>().size = new Vector3(settings.pointSize, settings.pointSize, settings.pointSize);
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        settings.resolution = Mathf.Clamp(EditorGUILayout.FloatField("Resolution", settings.resolution), 0.01f, 2f);

        if (EditorGUI.EndChangeCheck() == true)
        {
            RoadCreator[] roads = GameObject.FindObjectsOfType<RoadCreator>();
            for (int i = 0; i < roads.Length; i++)
            {
                roads[i].CreateMesh();
            }

            Intersection[] intersections = GameObject.FindObjectsOfType<Intersection>();
            for (int i = 0; i < intersections.Length; i++)
            {
                intersections[i].GenerateMesh();
            }
        }

        settings.ignoreMouseRayLayer = Mathf.Clamp(EditorGUILayout.IntField("Ignore Mouse Ray Layer", settings.ignoreMouseRayLayer), 9, 31);
        settings.roadLayer = Mathf.Clamp(EditorGUILayout.IntField("Road Layer", settings.roadLayer), 9, 31);

        EditorGUI.BeginChangeCheck();
        settings.amountRoadGuidelines = Mathf.Clamp(EditorGUILayout.IntField("Amount Of Road Guidelines (each side)", settings.amountRoadGuidelines), 0, 15);
        if (EditorGUI.EndChangeCheck() == true)
        {
            settings.UpdateRoadGuidelines();
        }

        settings.debug = EditorGUILayout.Toggle("Debug", settings.debug);

    }
}
