#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

[CustomEditor(typeof(RoadCreator))]
public class RoadEditor : Editor
{

    RoadCreator roadCreator;
    Vector3[] points = null;
    Tool lastTool;
    Event guiEvent;
    Vector3 hitPosition;

    private void OnEnable()
    {
        roadCreator = (RoadCreator)target;
        roadCreator.Setup();
        roadCreator.aDown = false;
        roadCreator.pDown = false;
        roadCreator.sDown = false;

        lastTool = Tools.current;
        Tools.current = Tool.None;

        Undo.undoRedoPerformed += UndoUpdate;
        Undo.undoRedoPerformed += roadCreator.UndoUpdate;

        if (roadCreator.startIntersection != null)
        {
            roadCreator.startIntersection.FixConnectionReferences();
        }

        if (roadCreator.endIntersection != null)
        {
            roadCreator.endIntersection.FixConnectionReferences();
        }
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        roadCreator.heightOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", roadCreator.heightOffset));
        roadCreator.segmentPreset = (Preset)EditorGUILayout.ObjectField("Segment Preset", roadCreator.segmentPreset, typeof(Preset), false);

        if (roadCreator.segmentPreset == null)
        {
            roadCreator.segmentToCopy = (RoadSegment)EditorGUILayout.ObjectField("Segment To Copy", roadCreator.segmentToCopy, typeof(RoadSegment), true);
        }

        roadCreator.resolutionMultiplier = Mathf.Clamp(EditorGUILayout.FloatField("Resoltion Multiplier", roadCreator.resolutionMultiplier), 0.01f, 10f);
        roadCreator.createIntersections = EditorGUILayout.Toggle("Create Intersections", roadCreator.createIntersections);
        roadCreator.generateCollider = EditorGUILayout.Toggle("Generate Collider", roadCreator.generateCollider);

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        if (roadCreator.startIntersection != null)
        {
            GUILayout.Space(20);
            GUILayout.Label("Start Intersection Lane Markings", guiStyle);
            roadCreator.startLanes = Mathf.Clamp(EditorGUILayout.IntField("Lanes", roadCreator.startLanes), 0, 10);

            if (roadCreator.startLanes > 0)
            {
                roadCreator.startMarkersScale = Mathf.Clamp(EditorGUILayout.FloatField("Scale", roadCreator.startMarkersScale), 0.1f, 5f);
                roadCreator.startMarkersRepeations = Mathf.Clamp(EditorGUILayout.IntField("Repeations", roadCreator.startMarkersRepeations), 1, 5);
                roadCreator.startMarkersStartIntersectionOffset = Mathf.Clamp(EditorGUILayout.FloatField("Start Intersection Offset", roadCreator.startMarkersStartIntersectionOffset), 0.7f * roadCreator.startMarkersScale, 5);

                if (roadCreator.startMarkersRepeations > 1)
                {
                    roadCreator.startMarkersContinuousIntersectionOffset = Mathf.Clamp(EditorGUILayout.FloatField("Continuous Offset", roadCreator.startMarkersContinuousIntersectionOffset), 1.4f * roadCreator.startMarkersScale, 25);
                }

                roadCreator.startMarkersYOffset = Mathf.Max(EditorGUILayout.FloatField("Y Offset", roadCreator.startMarkersYOffset), 0f);

                GUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Index");
                GUILayout.Label("Left");
                GUILayout.Label("Forwards");
                GUILayout.Label("Right");
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < roadCreator.startLanes; i++)
                {
                    if (i > roadCreator.startLaneMarkers.Count - 1)
                    {
                        roadCreator.startLaneMarkers.Add(new Vector3Bool(false, true, false));

                        if (i == roadCreator.startLanes - 1)
                        {
                            roadCreator.GenerateLaneMarkings();
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("#" + i);
                    GUILayout.Space(Screen.width / 4 - 20);
                    roadCreator.startLaneMarkers[i].one = EditorGUILayout.Toggle(roadCreator.startLaneMarkers[i].one);
                    roadCreator.startLaneMarkers[i].two = EditorGUILayout.Toggle(roadCreator.startLaneMarkers[i].two);
                    roadCreator.startLaneMarkers[i].three = EditorGUILayout.Toggle(roadCreator.startLaneMarkers[i].three);
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Reset Lane Markers"))
                {
                    for (int i = 0; i < roadCreator.startLanes; i++)
                    {
                        roadCreator.startLaneMarkers[i] = new Vector3Bool(false, true, false);
                    }
                }
            }

            GUILayout.Space(20);
        }

        if (roadCreator.endIntersection != null)
        {
            GUILayout.Space(20);
            GUILayout.Label("End Intersection Lane Markings", guiStyle);
            roadCreator.endLanes = Mathf.Clamp(EditorGUILayout.IntField("Lanes", roadCreator.endLanes), 0, 10);

            if (roadCreator.endLanes > 0)
            {
                roadCreator.endMarkersScale = Mathf.Clamp(EditorGUILayout.FloatField("Scale", roadCreator.endMarkersScale), 0.1f, 5f);
                roadCreator.endMarkersRepeations = Mathf.Clamp(EditorGUILayout.IntField("Repeations", roadCreator.endMarkersRepeations), 1, 5);
                roadCreator.endMarkersStartIntersectionOffset = Mathf.Clamp(EditorGUILayout.FloatField("Start Intersection Offset", roadCreator.endMarkersStartIntersectionOffset), 0.7f * roadCreator.endMarkersScale, 5);

                if (roadCreator.endMarkersRepeations > 1)
                {
                    roadCreator.endMarkersContinuousIntersectionOffset = Mathf.Clamp(EditorGUILayout.FloatField("Continuous Offset", roadCreator.endMarkersContinuousIntersectionOffset), 1.4f * roadCreator.endMarkersScale, 25);
                }

                roadCreator.endMarkersYOffset = Mathf.Max(EditorGUILayout.FloatField("Y Offset", roadCreator.endMarkersYOffset), 0f);

                GUILayout.Space(20);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Index");
                GUILayout.Label("Left");
                GUILayout.Label("Forwards");
                GUILayout.Label("Right");
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < roadCreator.endLanes; i++)
                {
                    if (i > roadCreator.endLaneMarkers.Count - 1)
                    {
                        roadCreator.endLaneMarkers.Add(new Vector3Bool(false, true, false));

                        if (i == roadCreator.endLanes - 1)
                        {
                            roadCreator.GenerateLaneMarkings();
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("#" + i);
                    GUILayout.Space(Screen.width / 4 - 20);
                    roadCreator.endLaneMarkers[i].one = EditorGUILayout.Toggle(roadCreator.endLaneMarkers[i].one);
                    roadCreator.endLaneMarkers[i].two = EditorGUILayout.Toggle(roadCreator.endLaneMarkers[i].two);
                    roadCreator.endLaneMarkers[i].three = EditorGUILayout.Toggle(roadCreator.endLaneMarkers[i].three);
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Reset Lane Markers"))
                {
                    for (int i = 0; i < roadCreator.endLanes; i++)
                    {
                        roadCreator.endLaneMarkers[i] = new Vector3Bool(false, true, false);
                    }
                }
            }

            GUILayout.Space(20);
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            roadCreator.CreateMesh();
        }

        if (GUILayout.Button("Flip Road"))
        {
            roadCreator.Flip();
        }

        if (GUILayout.Button("Reset Road"))
        {
            ResetObject();
        }

        if (GUILayout.Button("Generate Road"))
        {
            roadCreator.CreateMesh();
        }
    }

    public void ResetObject()
    {
        if (roadCreator == null)
        {
            roadCreator = (RoadCreator)target;
        }

        for (int i = roadCreator.transform.GetChild(0).childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(i).gameObject);
        }

        roadCreator.startIntersection = null;
        roadCreator.endIntersection = null;
        roadCreator.startIntersectionConnection = null;
        roadCreator.endIntersectionConnection = null;
        roadCreator.RemoveLaneMarkings();
    }

    public void UndoUpdate()
    {
        if (roadCreator == null)
        {
            roadCreator = (RoadCreator)target;
        }
    }

    public void OnSceneGUI()
    {
        if (roadCreator == null)
        {
            roadCreator = (RoadCreator)target;
        }

        if (roadCreator.transform.hasChanged)
        {
            roadCreator.transform.rotation = Quaternion.identity;
            roadCreator.transform.localScale = Vector3.one;
            roadCreator.CreateMesh();
            roadCreator.transform.hasChanged = false;
        }

        guiEvent = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << LayerMask.NameToLayer("Ignore Mouse Ray"))))
        {
            hitPosition = raycastHit.point;

            if (guiEvent.control == true)
            {
                Vector3 nearestGuideline = Misc.GetNearestGuidelinePoint(hitPosition);
                if (nearestGuideline != Misc.MaxVector3)
                {
                    hitPosition = new Vector3(nearestGuideline.x, hitPosition.y, nearestGuideline.z);
                }
                else
                {
                    hitPosition = new Vector3(Mathf.Round(hitPosition.x), hitPosition.y, Mathf.Round(hitPosition.z));
                }
            }

            if (guiEvent.type == EventType.MouseDown)
            {
                if (guiEvent.button == 0)
                {
                    if (guiEvent.shift == true)
                    {
                        CreatePoints();
                    }
                }
                else if (guiEvent.button == 1 && guiEvent.shift == true)
                {
                    RemovePoints();
                }
            }
            else if (guiEvent.type == EventType.KeyDown)
            {
                if (guiEvent.keyCode == KeyCode.S)
                {
                    roadCreator.sDown = true;
                }
                else if (guiEvent.keyCode == KeyCode.P)
                {
                    roadCreator.pDown = true;
                }
                else if (guiEvent.keyCode == KeyCode.A)
                {
                    roadCreator.aDown = true;
                }
            }
            else if (guiEvent.type == EventType.KeyUp)
            {
                if (guiEvent.keyCode == KeyCode.S)
                {
                    roadCreator.sDown = false;
                }
                else if (guiEvent.keyCode == KeyCode.P)
                {
                    roadCreator.pDown = false;
                }
                else if (guiEvent.keyCode == KeyCode.A)
                {
                    roadCreator.aDown = false;
                }
            }

            if (roadCreator.transform.GetChild(0).childCount > 0 && roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).childCount == 2 && (guiEvent.type == EventType.MouseDrag || guiEvent.type == EventType.MouseMove || guiEvent.type == EventType.MouseDown))
            {
                points = CalculateTemporaryPoints(hitPosition);
            }

            if (roadCreator.transform.childCount > 0)
            {
                Draw(hitPosition);
            }
        }

        if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << LayerMask.NameToLayer("Road") | 1 << LayerMask.NameToLayer("Prefab Line"))))
        {
            hitPosition = raycastHit.point;

            if (guiEvent.control == true)
            {
                Vector3 nearestGuideline = Misc.GetNearestGuidelinePoint(hitPosition);
                if (nearestGuideline != Misc.MaxVector3)
                {
                    hitPosition = new Vector3(nearestGuideline.x, hitPosition.y, nearestGuideline.z);
                }
                else
                {
                    hitPosition = new Vector3(Mathf.Round(hitPosition.x), hitPosition.y, Mathf.Round(hitPosition.z));
                }
            }

            if (guiEvent.shift == false)
            {
                MovePoints(raycastHit);
            }
            else
            {
                hitPosition = Misc.MaxVector3;
                MovePoints(raycastHit);
            }
        }

        if (guiEvent.type == EventType.MouseDown && Physics.Raycast(ray, out raycastHit, 100f, (1 << LayerMask.NameToLayer("Road"))))
        {
            hitPosition = raycastHit.point;

            if (guiEvent.button == 2)
            {
                if (raycastHit.transform.parent.parent.GetComponent<RoadSegment>() != null)
                {
                    roadCreator.segmentToCopy = raycastHit.transform.parent.parent.GetComponent<RoadSegment>();
                }
            }
            else if (guiEvent.button == 0)
            {
                if (guiEvent.control == true)
                {
                    Vector3 nearestGuideline = Misc.GetNearestGuidelinePoint(hitPosition);
                    if (nearestGuideline != Misc.MaxVector3)
                    {
                        hitPosition = new Vector3(nearestGuideline.x, hitPosition.y, nearestGuideline.z);
                    }
                    else
                    {
                        hitPosition = new Vector3(Mathf.Round(hitPosition.x), hitPosition.y, Mathf.Round(hitPosition.z));
                    }
                }

                roadCreator.SplitSegment(hitPosition, raycastHit);
            }
        }

        roadCreator.transform.parent.GetComponent<RoadSystem>().ShowCreationButtons();
        SceneView.currentDrawingSceneView.Repaint();
    }

    private void CreatePoints()
    {
        if (roadCreator.endIntersection == null)
        {
            roadCreator.CreatePoints(hitPosition);
        }
    }

    private void MovePoints(RaycastHit raycastHit)
    {
        roadCreator.MovePoints(hitPosition, guiEvent, raycastHit);
    }

    private void RemovePoints()
    {
        if (roadCreator.endIntersection == null)
        {
            roadCreator.RemovePoints();
        }
    }

    private void Draw(Vector3 mousePosition)
    {
        Misc.DrawRoadGuidelines(mousePosition, roadCreator.objectToMove, roadCreator.extraObjectToMove);

        for (int i = 0; i < roadCreator.transform.GetChild(0).childCount; i++)
        {
            for (int j = 0; j < roadCreator.transform.GetChild(0).GetChild(i).GetChild(0).childCount; j++)
            {
                if (roadCreator.transform.GetChild(0).GetChild(i).GetChild(0).GetChild(j).name != "Control Point")
                {
                    Handles.color = roadCreator.settings.FindProperty("pointColour").colorValue;
                }
                else
                {
                    Handles.color = roadCreator.settings.FindProperty("controlPointColour").colorValue;
                }

                Misc.DrawPoint((RoadCreatorSettings.PointShape)roadCreator.settings.FindProperty("pointShape").intValue, roadCreator.transform.GetChild(0).GetChild(i).GetChild(0).GetChild(j).transform.position, roadCreator.settings.FindProperty("pointSize").floatValue);
            }
        }

        for (int j = 0; j < roadCreator.transform.GetChild(0).childCount; j++)
        {
            if (roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).childCount == 3)
            {
                Handles.color = Color.white;
                Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(0).position, roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(1).position);
                Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(1).position, roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(2).position);
            }
            else if (roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).childCount == 2)
            {
                Handles.color = Color.white;
                Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(0).position, roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(1).position);

                if (guiEvent.shift == true)
                {
                    Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(1).position, mousePosition);
                }

                if (points != null && guiEvent.shift == true)
                {
                    Handles.color = Color.black;
                    Handles.DrawPolyLine(points);
                }
            }
            else
            {
                if (guiEvent.shift == true)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(0).position, mousePosition);
                }
            }
        }

        if (roadCreator.transform.GetChild(0).childCount > 0 && roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).childCount == 3)
        {
            if (guiEvent.shift == true && roadCreator.endIntersection == null)
            {
                Handles.color = Color.black;
                Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).position, mousePosition);
            }
        }

        // Mouse position
        Handles.color = roadCreator.settings.FindProperty("cursorColour").colorValue;
        Misc.DrawPoint((RoadCreatorSettings.PointShape)roadCreator.settings.FindProperty("pointShape").intValue, mousePosition, roadCreator.settings.FindProperty("pointSize").floatValue);
    }

    public Vector3[] CalculateTemporaryPoints(Vector3 hitPosition)
    {
        float distance = Misc.CalculateDistance(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).transform.GetChild(0).GetChild(0).position, roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).transform.GetChild(0).GetChild(1).position, hitPosition);
        float divisions = roadCreator.settings.FindProperty("resolution").floatValue * roadCreator.resolutionMultiplier * 4 * distance;
        divisions = Mathf.Max(3, divisions);
        List<Vector3> points = new List<Vector3>();
        float distancePerDivision = 1 / divisions;

        for (float t = 0; t <= 1; t += distancePerDivision)
        {
            if (t > 1 - distancePerDivision)
            {
                t = 1;
            }

            Vector3 position = Misc.Lerp3CenterHeight(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).transform.GetChild(0).GetChild(0).position, roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).transform.GetChild(0).GetChild(1).position, hitPosition, t);
            points.Add(position);
        }

        return points.ToArray();
    }
}
#endif