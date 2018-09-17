using System.Collections;
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

        if (roadCreator.transform.childCount == 0)
        {
            GameObject segments = new GameObject("Segments");
            segments.transform.SetParent(roadCreator.transform);
            segments.hideFlags = HideFlags.NotEditable;
        }

        if (roadCreator.globalSettings == null)
        {
            roadCreator.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        Undo.undoRedoPerformed += UndoUpdate;
        Undo.undoRedoPerformed += roadCreator.UndoUpdate;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        roadCreator.heightOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", roadCreator.heightOffset));
        roadCreator.smoothnessAmount = Mathf.Max(0, EditorGUILayout.IntField("Smoothness Amount", roadCreator.smoothnessAmount));
        roadCreator.segmentPreset = (Preset)EditorGUILayout.ObjectField("Segment Preset", roadCreator.segmentPreset, typeof(Preset), false);

        if (EditorGUI.EndChangeCheck() == true)
        {
            roadCreator.CreateMesh();
        }

        GameObject lastFollowObject = roadCreator.followObject;

        if (roadCreator.isFollowObject == false)
        {
            roadCreator.followObject = (GameObject)EditorGUILayout.ObjectField("Follow Object", roadCreator.followObject, typeof(GameObject), true);
        }

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        if (roadCreator.followObject != lastFollowObject)
        {
            if (roadCreator.followObject != null)
            {
                if (roadCreator.isFollowObject == false && roadCreator.followObject != null && roadCreator.followObject.GetComponent<RoadCreator>() == null && roadCreator.followObject.GetComponent<PrefabLineCreator>() == null)
                {
                    roadCreator.followObject = null;
                    Debug.Log("Follow object must either be a road or a prefab line");
                }

                if (roadCreator.followObject != null && roadCreator.followObject == roadCreator.gameObject)
                {
                    roadCreator.followObject = null;
                    Debug.Log("Follow object can not be itself");
                }

                if (roadCreator.followObject != null)
                {
                    if (roadCreator.followObject.GetComponent<RoadCreator>() != null)
                    {
                        roadCreator.followObject.GetComponent<RoadCreator>().isFollowObject = true;

                        for (int i = 0; i < roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>().Length; i++)
                        {
                            roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>()[i].enabled = false;

                            if (i == roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>().Length - 1)
                            {
                                int roadCreatorChildCount = 0;

                                if (roadCreator.transform.GetChild(0).childCount > 0)
                                {
                                    roadCreatorChildCount = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).childCount - 1;
                                }

                                int followObjectChildCount = roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>()[i].transform.parent.childCount;

                                if (roadCreatorChildCount > 0 && roadCreatorChildCount < 3)
                                {
                                    DestroyImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).gameObject);
                                }

                                if (followObjectChildCount > 0 && followObjectChildCount < 3)
                                {
                                    DestroyImmediate(roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>()[i].transform.parent.parent.gameObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        roadCreator.followObject.GetComponent<PrefabLineCreator>().isFollowObject = true;

                        for (int i = 0; i < roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>().Length; i++)
                        {
                            roadCreator.followObject.transform.GetComponentsInChildren<BoxCollider>()[i].enabled = false;
                        }
                    }
                }
            }
            else
            {
                if (lastFollowObject.GetComponent<RoadCreator>() != null)
                {
                    lastFollowObject.GetComponent<RoadCreator>().isFollowObject = false;
                }
                else
                {
                    lastFollowObject.GetComponent<PrefabLineCreator>().isFollowObject = false;
                }

                for (int i = 0; i < lastFollowObject.transform.GetComponentsInChildren<BoxCollider>().Length; i++)
                {
                    lastFollowObject.transform.GetComponentsInChildren<BoxCollider>()[i].enabled = true;
                }
            }
        }

        if (roadCreator.globalSettings.debug == true)
        {
            GUILayout.Label("");
            GUILayout.Label("Debug", guiStyle);
            EditorGUILayout.ObjectField(roadCreator.currentSegment, typeof(RoadSegment), true);
            EditorGUILayout.Toggle("Is Follow Object", roadCreator.isFollowObject);
        }

        if (roadCreator.isFollowObject == false)
        {
            if (GUILayout.Button("Reset Road"))
            {
                ResetObject();
            }

            if (GUILayout.Button("Generate Road"))
            {
                roadCreator.CreateMesh();

                if (roadCreator.followObject != null)
                {
                    if (roadCreator.followObject.GetComponent<RoadCreator>() != null)
                    {
                        roadCreator.followObject.GetComponent<RoadCreator>().CreateMesh();
                    }
                    else
                    {
                        roadCreator.followObject.GetComponent<PrefabLineCreator>().PlacePrefabs();
                    }
                }
            }

            if (GUILayout.Button("Convert To Meshes"))
            {
                ConvertToMesh(roadCreator);

                if (roadCreator.followObject != null && roadCreator.followObject.GetComponent<RoadCreator>() != null)
                {
                    ConvertToMesh(roadCreator.followObject.GetComponent<RoadCreator>());
                }
            }
        }
    }

    public void ResetObject()
    {
        if (roadCreator == null)
        {
            roadCreator = (RoadCreator)target;
        }

        roadCreator.currentSegment = null;

        for (int i = roadCreator.transform.GetChild(0).childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(i).gameObject);
        }

        if (roadCreator.followObject != null)
        {
            if (roadCreator.followObject.GetComponent<RoadCreator>() != null)
            {
                roadCreator.followObject.GetComponent<RoadCreator>().currentSegment = null;
            }
            else
            {
                roadCreator.followObject.GetComponent<PrefabLineCreator>().currentPoint = null;
                for (int i = roadCreator.followObject.transform.GetChild(1).childCount - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(roadCreator.followObject.transform.GetChild(1).GetChild(i).gameObject);
                }
            }

            for (int i = roadCreator.followObject.transform.GetChild(0).childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(roadCreator.followObject.transform.GetChild(0).GetChild(i).gameObject);
            }
        }
    }

    public void ConvertToMesh(RoadCreator roadCreator)
    {
        MeshFilter[] meshFilters = roadCreator.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length > 0 && meshFilters[0].sharedMesh != null)
        {
            GameObject roadMesh = new GameObject("Road Mesh");
            Undo.RegisterCreatedObjectUndo(roadMesh, "Created Road Mesh");
            roadMesh.transform.position = meshFilters[0].transform.parent.parent.GetChild(0).GetChild(0).position;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    Undo.SetTransformParent(meshFilters[i].transform, roadMesh.transform, "Created Road Mesh");
                    meshFilters[i].name = "Mesh";
                }
            }

            Undo.DestroyObjectImmediate(roadCreator.gameObject);
            Selection.activeObject = roadMesh;
        }
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

        if (roadCreator.isFollowObject == false)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            guiEvent = Event.current;

            Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << roadCreator.globalSettings.ignoreMouseRayLayer)))
            {
                hitPosition = raycastHit.point;

                if (guiEvent.control == true)
                {
                    Vector3 nearestGuideline = Misc.GetNearestGuidelinePoint(hitPosition);
                    if (nearestGuideline != Misc.MaxVector3)
                    {
                        hitPosition = nearestGuideline;
                    }
                    else
                    {
                        hitPosition = Misc.Round(hitPosition);
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

                if (roadCreator.currentSegment != null && roadCreator.currentSegment.transform.GetChild(0).childCount == 2 && (guiEvent.type == EventType.MouseDrag || guiEvent.type == EventType.MouseMove || guiEvent.type == EventType.MouseDown))
                {
                    points = CalculateTemporaryPoints(hitPosition);
                }

                if (roadCreator.transform.childCount > 0)
                {
                    Draw(hitPosition);
                }
            }

            if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << roadCreator.globalSettings.roadLayer)))
            {
                Vector3 hitPosition = raycastHit.point;

                if (guiEvent.control == true)
                {
                    hitPosition = Misc.Round(hitPosition);
                }

                if (guiEvent.shift == false)
                {
                    MovePoints(raycastHit);
                }
            }

            GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
        }
    }

    private void CreatePoints()
    {
        roadCreator.CreatePoints(hitPosition);

        if (roadCreator.followObject != null)
        {
            if (roadCreator.followObject.GetComponent<RoadCreator>() != null)
            {
                roadCreator.followObject.GetComponent<RoadCreator>().CreatePoints(hitPosition);
            }
            else
            {
                roadCreator.followObject.GetComponent<PrefabLineCreator>().CreatePoints(hitPosition);
            }
        }
    }

    private void MovePoints(RaycastHit raycastHit)
    {
        roadCreator.MovePoints(hitPosition, guiEvent, raycastHit);

        if (roadCreator.followObject != null && roadCreator.followObject.GetComponent<RoadCreator>() != null)
        {
            roadCreator.followObject.GetComponent<RoadCreator>().MovePoints(hitPosition, guiEvent, raycastHit);
        }
    }

    private void RemovePoints()
    {
        if (roadCreator.followObject != null)
        {
            if (roadCreator.followObject.GetComponent<RoadCreator>() != null)
            {
                roadCreator.followObject.GetComponent<RoadCreator>().RemovePoints();
            } else
            {
                roadCreator.followObject.GetComponent<PrefabLineCreator>().RemovePoints(roadCreator.IsLastSegmentCurved());
            }
        }

        roadCreator.RemovePoints();
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
                    Handles.color = Color.red;
                }
                else
                {
                    Handles.color = Color.yellow;
                }

                Handles.CylinderHandleCap(0, roadCreator.transform.GetChild(0).GetChild(i).GetChild(0).GetChild(j).transform.position, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);
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
                    Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(j).GetChild(0).GetChild(1).position, hitPosition);
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
                    Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(0).position, hitPosition);
                }
            }
        }

        if (roadCreator.currentSegment == null && roadCreator.transform.GetChild(0).childCount > 0 && roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).childCount == 3)
        {
            if (guiEvent.shift == true)
            {
                Handles.color = Color.black;
                Handles.DrawLine(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).position, hitPosition);
            }
        }

        // Intersection connections
        Transform[] objects = GameObject.FindObjectsOfType<Transform>();
        Handles.color = Color.green;
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].name.Contains("Intersection") || objects[i].name == "Roundabout")
            {
                for (int j = 0; j < objects[i].GetChild(0).childCount; j++)
                {
                    if (objects[i].GetChild(0).GetChild(j).GetChild(0).GetComponent<MeshFilter>().sharedMesh != null)
                    {
                        Handles.CylinderHandleCap(0, objects[i].GetChild(0).GetChild(j).GetChild(1).position, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);
                    }
                }
            }
            else if (objects[i].name == "Road Splitter")
            {
                for (int j = 0; j < objects[i].GetChild(0).childCount; j++)
                {
                    Handles.CylinderHandleCap(0, objects[i].GetChild(0).GetChild(j).position, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);
                }
            }
        }

        // Mouse position
        Handles.color = Color.blue;
        Handles.CylinderHandleCap(0, hitPosition, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);
    }

    public Vector3[] CalculateTemporaryPoints(Vector3 hitPosition)
    {
        float divisions = Misc.CalculateDistance(roadCreator.currentSegment.transform.GetChild(0).GetChild(0).position, roadCreator.currentSegment.transform.GetChild(0).GetChild(1).position, hitPosition);
        divisions = Mathf.Max(2, divisions);
        List<Vector3> points = new List<Vector3>();
        float distancePerDivision = 1 / divisions;

        for (float t = 0; t <= 1; t += distancePerDivision)
        {
            if (t > 1 - distancePerDivision)
            {
                t = 1;
            }

            Vector3 position = Misc.Lerp3(roadCreator.currentSegment.transform.GetChild(0).GetChild(0).position, roadCreator.currentSegment.transform.GetChild(0).GetChild(1).position, hitPosition, t);

            RaycastHit raycastHit;
            if (Physics.Raycast(position, Vector3.down, out raycastHit, 100f, ~(1 << roadCreator.globalSettings.ignoreMouseRayLayer)))
            {
                position.y = raycastHit.point.y;
            }

            points.Add(position);
        }

        return points.ToArray();
    }
}
