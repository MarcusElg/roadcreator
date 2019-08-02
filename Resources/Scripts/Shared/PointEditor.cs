#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Point))]
public class PointEditor : Editor
{
    private Tool lastTool;

    private void OnEnable()
    {
        lastTool = Tools.current;
        Tools.current = Tool.Move;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    private void OnSceneGUI()
    {
        Point point = (Point)target;
        RoadCreator roadCreator = null;
        PrefabLineCreator prefabLine = null;

        if (point.roadPoint == true)
        {
            roadCreator = point.transform.parent.parent.parent.parent.GetComponent<RoadCreator>();

            if (roadCreator.settings == null)
            {
                roadCreator.settings = RoadCreatorSettings.GetSerializedSettings();
            }
        }
        else
        {
            prefabLine = point.transform.parent.parent.GetComponent<PrefabLineCreator>();

            if (prefabLine.settings == null)
            {
                prefabLine.settings = RoadCreatorSettings.GetSerializedSettings();
            }
        }

        if (point.transform.hasChanged == true)
        {
            point.transform.rotation = Quaternion.identity;
            point.transform.localScale = Vector3.one;

            if (point.roadPoint == true)
            {
                if (point.name == "Control Point")
                {
                    point.transform.parent.parent.GetComponent<RoadSegment>().curved = true;
                }
                else
                {
                    if (point.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
                    {
                        point.transform.parent.GetChild(1).position = Misc.GetCenter(point.transform.parent.GetChild(0).position, point.transform.parent.GetChild(2).position);
                    }

                    if (point.name == "Start Point" && point.transform.parent.parent.GetSiblingIndex() > 0)
                    {
                        point.transform.parent.parent.parent.GetChild(point.transform.parent.parent.GetSiblingIndex() - 1).GetChild(0).GetChild(2).position = point.transform.position;
                    }
                    else if (point.name == "End Point" && point.transform.parent.parent.GetSiblingIndex() < point.transform.parent.parent.parent.childCount - 1)
                    {
                        point.transform.parent.parent.parent.GetChild(point.transform.parent.parent.GetSiblingIndex() + 1).GetChild(0).GetChild(0).position = point.transform.position;
                    }
                }

                roadCreator.CreateMesh();
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

            Misc.DrawPoint((RoadCreatorSettings.PointShape)roadCreator.settings.FindProperty("pointShape").intValue, point.transform.position, roadCreator.settings.FindProperty("pointSize").floatValue);
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

            Misc.DrawPoint((RoadCreatorSettings.PointShape)prefabLine.settings.FindProperty("pointShape").intValue, point.transform.position, prefabLine.settings.FindProperty("pointSize").floatValue);
        }
    }
}
#endif