using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSystem))]
public class RoadSystemEditor : Editor
{

    public RoadSystem roadSystem;
    Tool lastTool;

    private void OnEnable()
    {
        roadSystem = (RoadSystem)target;

        if (GameObject.FindObjectOfType<GlobalSettings>() == null)
        {
            new GameObject("Global Settings").AddComponent<GlobalSettings>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;
    }

    private void OnDestroy()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Reset"))
        {
            for (int i = roadSystem.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roadSystem.transform.GetChild(i).gameObject);
            }
        }
    }

    private void OnSceneGUI()
    {
        roadSystem.ShowCreationButtons();
    }

}
