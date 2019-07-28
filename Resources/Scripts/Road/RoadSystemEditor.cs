#if UNITY_EDITOR
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

        if (roadSystem.settings == null)
        {
            roadSystem.settings = RoadCreatorSettings.GetSerializedSettings();
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
                Undo.DestroyObjectImmediate(roadSystem.transform.GetChild(i).gameObject);
            }
        }

        if (GUILayout.Button("Convert To Mesh"))
        {
            Misc.ConvertToMesh(roadSystem.gameObject, "Road System Mesh");
        }
    }

    private void OnSceneGUI()
    {
        roadSystem.ShowCreationButtons();
    }

}
#endif