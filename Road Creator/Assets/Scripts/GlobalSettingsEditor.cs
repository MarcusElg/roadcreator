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

    private void OnSceneGUI()
    {
        if (settings.pointSize != settings.oldPointSize)
        {
            Transform[] objects = GameObject.FindObjectsOfType<Transform>();

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].name.Contains("Connection Point") || objects[i].name == "Start Point" || objects[i].name == "Control Point" || objects[i].name == "End Point")
                {
                    objects[i].GetComponent<BoxCollider>().size = new Vector3(settings.pointSize, settings.pointSize, settings.pointSize);
                }
            }

            settings.oldPointSize = settings.pointSize;
        }
    }

}
