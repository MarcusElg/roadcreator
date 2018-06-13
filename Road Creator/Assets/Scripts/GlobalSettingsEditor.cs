using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlobalSettings))]
public class GlobalSettingsEditor : Editor {

    GlobalSettings settings;

    private void OnEnable()
    {
        settings = ((GlobalSettings)target);
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        settings.pointSize = Mathf.Max(0.2f, EditorGUILayout.FloatField("Point Size", settings.pointSize));
        settings.resolution = Mathf.Max(0.2f, EditorGUILayout.FloatField("Resolution", settings.resolution));
        settings.layer = Mathf.Clamp(EditorGUILayout.IntField("Ignore Mouse Ray Layer", settings.layer), 9, 31);
        settings.debug = EditorGUILayout.Toggle("Debug", settings.debug);

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
    }

}
