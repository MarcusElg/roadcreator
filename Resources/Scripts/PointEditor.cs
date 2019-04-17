using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Point))]
public class PointEditor : Editor
{
    private void OnSceneGUI()
    {
        Point point = (Point)target;
        RoadCreator roadCreator = null;
        PrefabLineCreator prefabLine = null;

        if (point.roadPoint == true)
        {
            roadCreator = point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>();
        }
        else
        {
            prefabLine = point.transform.parent.parent.GetComponent<PrefabLineCreator>();
        }

        if (point.transform.hasChanged == true)
        {
            if (point.roadPoint == true)
            {
                if (roadCreator != null)
                {
                    roadCreator.CreateMesh();
                }
            }
            else if (prefabLine != null)
            {
                prefabLine.PlacePrefabs();
            }

            point.transform.hasChanged = false;
        }

        // Draw points
        if (point.roadPoint == true)
        {
            if (point.name == "Control Point")
            {
                Handles.color = roadCreator.settings.FindProperty("controlPointColour").colorValue;
            }
            else
            {
                Handles.color = roadCreator.settings.FindProperty("pointColour").colorValue;
            }

            Handles.CylinderHandleCap(0, point.transform.position, Quaternion.Euler(90, 0, 0), roadCreator.settings.FindProperty("pointSize").floatValue, EventType.Repaint);
        }
        else
        {
            if (point.name == "Control Point")
            {
                Handles.color = prefabLine.settings.FindProperty("controlPointColour").colorValue;
            }
            else
            {
                Handles.color = prefabLine.settings.FindProperty("pointColour").colorValue;
            }

            Handles.CylinderHandleCap(0, point.transform.position, Quaternion.Euler(90, 0, 0), prefabLine.settings.FindProperty("pointSize").floatValue, EventType.Repaint);
        }
    }
}
