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
        RoadCreator roadCreator = point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>();
        PrefabLineCreator prefabLine = point.transform.parent.parent.GetComponent<PrefabLineCreator>();

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
                Handles.color = roadCreator.globalSettings.controlPointColour;
            } else
            {
                Handles.color = roadCreator.globalSettings.pointColour;
            }

            Handles.CylinderHandleCap(0, point.transform.position, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);
        }
        else
        {
            if (point.name == "Control Point")
            {
                Handles.color = prefabLine.globalSettings.controlPointColour;
            }
            else
            {
                Handles.color = prefabLine.globalSettings.pointColour;
            }

            Handles.CylinderHandleCap(0, point.transform.position, Quaternion.Euler(90, 0, 0), prefabLine.globalSettings.pointSize, EventType.Repaint);
        }
    }
}
