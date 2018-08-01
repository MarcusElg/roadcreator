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
    GameObject objectToMove = null;
    GameObject extraObjectToMove = null;
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
        }

        if (roadCreator.globalSettings == null)
        {
            roadCreator.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        Undo.undoRedoPerformed += UndoUpdate;
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
        roadCreator.segmentPreset = (Preset)EditorGUILayout.ObjectField("Segment Preset", serializedObject.FindProperty("segmentPreset").objectReferenceValue, typeof(Preset), false);

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        if (EditorGUI.EndChangeCheck() == true)
        {
            roadCreator.CreateMesh();
        }

        if (roadCreator.globalSettings.debug == true)
        {
            GUILayout.Label("");
            GUILayout.Label("Debug", guiStyle);
            EditorGUILayout.ObjectField(roadCreator.currentSegment, typeof(RoadSegment), true);
        }

        if (GUILayout.Button("Reset Road"))
        {
            roadCreator.currentSegment = null;

            for (int i = roadCreator.transform.GetChild(0).childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roadCreator.transform.GetChild(0).GetChild(i).gameObject);
            }
        }

        if (GUILayout.Button("Generate Road"))
        {
            roadCreator.CreateMesh();
        }
    }

    public void OnSceneGUI()
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
                bool snapToGuidelines = false;
                RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
                for (int i = 0; i < roadSegments.Length; i++)
                {
                    if (roadSegments[i].startGuidelinePoints != null)
                    {
                        for (int j = 0; j < roadSegments[i].startGuidelinePoints.Length; j++)
                        {
                            if (Vector3.Distance(hitPosition, roadSegments[i].startGuidelinePoints[j]) < 1f)
                            {
                                hitPosition = roadSegments[i].startGuidelinePoints[j];
                                snapToGuidelines = true;
                                continue;
                            }
                        }
                    }

                    if (roadSegments[i].centerGuidelinePoints != null)
                    {
                        for (int j = 0; j < roadSegments[i].centerGuidelinePoints.Length; j++)
                        {
                            if (Vector3.Distance(hitPosition, roadSegments[i].centerGuidelinePoints[j]) < 1f)
                            {
                                hitPosition = roadSegments[i].centerGuidelinePoints[j];
                                snapToGuidelines = true;
                                continue;
                            }
                        }
                    }

                    if (roadSegments[i].endGuidelinePoints != null)
                    {
                        for (int j = 0; j < roadSegments[i].endGuidelinePoints.Length; j++)
                        {
                            if (Vector3.Distance(hitPosition, roadSegments[i].endGuidelinePoints[j]) < 1f)
                            {
                                hitPosition = roadSegments[i].endGuidelinePoints[j];
                                snapToGuidelines = true;
                                continue;
                            }
                        }
                    }
                }

                if (snapToGuidelines == false)
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

        if (Physics.Raycast(ray, out raycastHit, 100f))
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

    private void UndoUpdate()
    {
        if (roadCreator.currentSegment != null && roadCreator.currentSegment.transform.GetChild(0).childCount == 3)
        {
            roadCreator.currentSegment = null;
        }

        Transform lastSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1);
        if (roadCreator.transform.GetChild(0).childCount > 0 && lastSegment.GetChild(0).childCount < 3)
        {
            roadCreator.currentSegment = lastSegment.GetComponent<RoadSegment>();
        }

        roadCreator.CreateMesh();
    }

    private void CreatePoints()
    {
        if (roadCreator.currentSegment != null)
        {
            if (roadCreator.currentSegment.transform.GetChild(0).childCount == 1)
            {
                if (roadCreator.globalSettings.roadCurved == true)
                {
                    // Create control point
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", roadCreator.currentSegment.transform.GetChild(0), hitPosition), "Created point");
                }
                else
                {
                    // Create control and end points
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", roadCreator.currentSegment.transform.GetChild(0), Misc.GetCenter(roadCreator.currentSegment.transform.GetChild(0).GetChild(0).position, hitPosition)), "Created point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", roadCreator.currentSegment.transform.GetChild(0), hitPosition), "Created point");
                    roadCreator.currentSegment = null;
                    roadCreator.CreateMesh();
                }
            }
            else if (roadCreator.currentSegment.transform.GetChild(0).childCount == 2)
            {
                // Create end point
                Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", roadCreator.currentSegment.transform.GetChild(0), hitPosition), "Create Point");
                roadCreator.currentSegment = null;
                roadCreator.CreateMesh();
            }
        }
        else
        {
            if (roadCreator.transform.GetChild(0).childCount == 0)
            {
                // Create first segment
                RoadSegment segment = CreateSegment(hitPosition);
                Undo.RegisterCreatedObjectUndo(segment.gameObject, "Create Point");
                Undo.RegisterCreatedObjectUndo(CreatePoint("Start Point", segment.transform.GetChild(0), hitPosition), "Create Point");

                if (roadCreator.globalSettings.roadCurved == false)
                {
                    segment.curved = false;
                }
            }
            else
            {
                RoadSegment segment = CreateSegment(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).position);
                Undo.RegisterCreatedObjectUndo(segment.gameObject, "Create Point");
                Undo.RegisterCreatedObjectUndo(CreatePoint("Start Point", segment.transform.GetChild(0), segment.transform.position), "Create Point");

                if (roadCreator.globalSettings.roadCurved == true)
                {
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", segment.transform.GetChild(0), hitPosition), "Create Point");
                }
                else
                {
                    segment.curved = false;
                    Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", roadCreator.currentSegment.transform.GetChild(0), Misc.GetCenter(roadCreator.currentSegment.transform.GetChild(0).GetChild(0).position, hitPosition)), "Created point");
                    Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", roadCreator.currentSegment.transform.GetChild(0), hitPosition), "Created point");
                    roadCreator.currentSegment = null;
                    roadCreator.CreateMesh();
                }
            }
        }
    }

    private GameObject CreatePoint(string name, Transform parent, Vector3 position)
    {
        GameObject point = new GameObject(name);
        point.gameObject.AddComponent<BoxCollider>();
        point.GetComponent<BoxCollider>().size = new Vector3(roadCreator.globalSettings.pointSize, roadCreator.globalSettings.pointSize, roadCreator.globalSettings.pointSize);
        point.transform.SetParent(parent);
        point.transform.position = position;
        point.layer = roadCreator.globalSettings.ignoreMouseRayLayer;
        point.AddComponent<Point>();
        return point;
    }

    private RoadSegment CreateSegment(Vector3 position)
    {
        RoadSegment segment = new GameObject("Segment").AddComponent<RoadSegment>();
        segment.transform.SetParent(roadCreator.transform.GetChild(0), false);
        segment.transform.position = position;

        if (roadCreator.segmentPreset == null)
        {
            if (roadCreator.transform.GetChild(0).childCount > 1)
            {
                RoadSegment oldLastSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 2).GetComponent<RoadSegment>();
                segment.roadMaterial = oldLastSegment.roadMaterial;
                segment.startRoadWidth = oldLastSegment.startRoadWidth;
                segment.endRoadWidth = oldLastSegment.endRoadWidth;
                segment.flipped = oldLastSegment.flipped;
                segment.terrainOption = oldLastSegment.terrainOption;

                segment.leftShoulderMaterial = oldLastSegment.leftShoulderMaterial;
                segment.leftShoulder = oldLastSegment.leftShoulder;
                segment.leftShoulderWidth = oldLastSegment.leftShoulderWidth;
                segment.leftShoulderHeightOffset = oldLastSegment.leftShoulderHeightOffset;

                segment.rightShoulderMaterial = oldLastSegment.rightShoulderMaterial;
                segment.rightShoulder = oldLastSegment.rightShoulder;
                segment.rightShoulderWidth = oldLastSegment.rightShoulderWidth;
                segment.rightShoulderHeightOffset = oldLastSegment.rightShoulderHeightOffset;
            }
            else
            {
                segment.roadMaterial = Resources.Load("Materials/Roads/2 Lane Roads/2L Road") as Material;
                segment.leftShoulderMaterial = Resources.Load("Materials/Asphalt") as Material;
                segment.rightShoulderMaterial = Resources.Load("Materials/Asphalt") as Material;
            }
        }
        else
        {
            roadCreator.segmentPreset.ApplyTo(segment);
        }

        GameObject points = new GameObject("Points");
        points.transform.SetParent(segment.transform);
        points.transform.localPosition = Vector3.zero;

        GameObject meshes = new GameObject("Meshes");
        meshes.transform.SetParent(segment.transform);
        meshes.transform.localPosition = Vector3.zero;

        GameObject mainMesh = new GameObject("Main Mesh");
        mainMesh.transform.SetParent(meshes.transform);
        mainMesh.transform.localPosition = Vector3.zero;
        mainMesh.AddComponent<MeshRenderer>();
        mainMesh.AddComponent<MeshFilter>();
        mainMesh.AddComponent<MeshCollider>();
        mainMesh.layer = roadCreator.globalSettings.roadLayer;

        GameObject leftShoulderMesh = new GameObject("Left Shoulder Mesh");
        leftShoulderMesh.transform.SetParent(meshes.transform);
        leftShoulderMesh.transform.localPosition = Vector3.zero;
        leftShoulderMesh.AddComponent<MeshRenderer>();
        leftShoulderMesh.AddComponent<MeshFilter>();
        leftShoulderMesh.AddComponent<MeshCollider>();
        leftShoulderMesh.layer = roadCreator.globalSettings.roadLayer;

        GameObject rightShoulderMesh = new GameObject("Right Shoulder Mesh");
        rightShoulderMesh.transform.SetParent(meshes.transform);
        rightShoulderMesh.transform.localPosition = Vector3.zero;
        rightShoulderMesh.AddComponent<MeshRenderer>();
        rightShoulderMesh.AddComponent<MeshFilter>();
        rightShoulderMesh.AddComponent<MeshCollider>();
        rightShoulderMesh.layer = roadCreator.globalSettings.roadLayer;

        roadCreator.currentSegment = segment;

        return segment;
    }

    private void MovePoints(RaycastHit raycastHit)
    {
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && objectToMove == null)
        {
            if (raycastHit.collider.gameObject.name == "Control Point")
            {
                objectToMove = raycastHit.collider.gameObject;
                objectToMove.GetComponent<BoxCollider>().enabled = false;
            }
            else if (raycastHit.collider.gameObject.name == "Start Point")
            {
                objectToMove = raycastHit.collider.gameObject;
                objectToMove.GetComponent<BoxCollider>().enabled = false;

                if (objectToMove.transform.parent.parent.GetSiblingIndex() > 0)
                {
                    extraObjectToMove = raycastHit.collider.gameObject.transform.parent.parent.parent.GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() - 1).GetChild(0).GetChild(2).gameObject;
                    extraObjectToMove.GetComponent<BoxCollider>().enabled = false;
                }
            }
            else if (raycastHit.collider.gameObject.name == "End Point")
            {
                objectToMove = raycastHit.collider.gameObject;
                objectToMove.GetComponent<BoxCollider>().enabled = false;

                if (objectToMove.transform.parent.parent.GetSiblingIndex() < objectToMove.transform.parent.parent.parent.childCount - 1 && raycastHit.collider.gameObject.transform.parent.parent.parent.GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() + 1).GetChild(0).childCount == 3)
                {
                    extraObjectToMove = raycastHit.collider.gameObject.transform.parent.parent.parent.GetChild(objectToMove.transform.parent.parent.GetSiblingIndex() + 1).GetChild(0).GetChild(0).gameObject;
                    extraObjectToMove.GetComponent<BoxCollider>().enabled = false;
                }
            }
        }
        else if (guiEvent.type == EventType.MouseDrag && objectToMove != null)
        {
            Undo.RecordObject(objectToMove.transform, "Moved Point");
            objectToMove.transform.position = hitPosition;

            if (extraObjectToMove != null)
            {
                Undo.RecordObject(extraObjectToMove.transform, "Moved Point");
                extraObjectToMove.transform.position = hitPosition;
            }
        }
        else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && objectToMove != null)
        {
            if (objectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
            {
                if (objectToMove.transform.GetSiblingIndex() == 1)
                {
                    objectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved = true;
                }
                else
                {
                    if (objectToMove.transform.parent.childCount == 3)
                    {
                        objectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(objectToMove.transform.parent.GetChild(0).position, objectToMove.transform.parent.GetChild(2).position);
                    }
                }
            }

            objectToMove.GetComponent<BoxCollider>().enabled = true;
            objectToMove = null;

            if (extraObjectToMove != null)
            {
                if (extraObjectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
                {
                    if (extraObjectToMove.transform.parent.childCount == 3)
                    {
                        extraObjectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(extraObjectToMove.transform.parent.GetChild(0).position, extraObjectToMove.transform.parent.GetChild(2).position);
                    }
                }

                extraObjectToMove.GetComponent<BoxCollider>().enabled = true;
                extraObjectToMove = null;
            }

            roadCreator.CreateMesh();
            roadCreator.globalSettings.UpdateRoadGuidelines();
        }
    }

    private void RemovePoints()
    {
        if (roadCreator.transform.GetChild(0).childCount > 0)
        {
            if (roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved == true)
            {
                if (roadCreator.currentSegment != null)
                {
                    if (roadCreator.currentSegment.transform.GetChild(0).childCount == 2)
                    {
                        Undo.DestroyObjectImmediate(roadCreator.currentSegment.transform.GetChild(0).GetChild(1).gameObject);
                    }
                    else if (roadCreator.currentSegment.transform.GetChild(0).childCount == 1)
                    {
                        Undo.DestroyObjectImmediate(roadCreator.currentSegment.gameObject);

                        if (roadCreator.transform.GetChild(0).childCount > 0)
                        {
                            if (roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>().curved == false)
                            {
                                Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                                Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(1).gameObject);
                                roadCreator.currentSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
                                roadCreator.CreateMesh();
                            }
                            else
                            {
                                if (roadCreator.transform.GetChild(0).childCount > 0)
                                {
                                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
                                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
                                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshFilter>().sharedMesh = null;
                                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshCollider>().sharedMesh = null;
                                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshFilter>().sharedMesh = null;
                                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshCollider>().sharedMesh = null;

                                    Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                                    roadCreator.currentSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
                                }
                            }
                        }
                        else
                        {
                            roadCreator.currentSegment = null;
                        }
                    }
                }
                else
                {
                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshFilter>().sharedMesh = null;
                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(1).GetComponent<MeshCollider>().sharedMesh = null;
                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshFilter>().sharedMesh = null;
                    roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(2).GetComponent<MeshCollider>().sharedMesh = null;

                    Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                    roadCreator.currentSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
                }
            }
            else
            {
                Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).gameObject);
                roadCreator.CreateMesh();
            }
        }
    }

    private void Draw(Vector3 mousePosition)
    {
        // Guidelines
        RoadSegment[] roadSegments = GameObject.FindObjectsOfType<RoadSegment>();
        for (int i = 0; i < roadSegments.Length; i++)
        {
            if (roadSegments[i].transform.GetChild(0).childCount == 3)
            {
                Handles.color = Misc.lightGreen;
                if (roadSegments[i].startGuidelinePoints != null && roadSegments[i].startGuidelinePoints.Length > 0 && (Vector3.Distance(mousePosition, roadSegments[i].transform.GetChild(0).GetChild(0).position) < 10) && roadSegments[i].transform.GetChild(0).GetChild(0).gameObject != objectToMove && roadSegments[i].transform.GetChild(0).GetChild(0).gameObject != extraObjectToMove)
                {
                    Handles.DrawLine(roadSegments[i].transform.GetChild(0).GetChild(0).position, roadSegments[i].startGuidelinePoints[roadSegments[i].startGuidelinePoints.Length - 2]);
                    Handles.DrawLine(roadSegments[i].transform.GetChild(0).GetChild(0).position, roadSegments[i].startGuidelinePoints[roadSegments[i].startGuidelinePoints.Length - 1]);

                    for (int j = 0; j < roadSegments[i].startGuidelinePoints.Length; j++)
                    {
                        Handles.DrawSolidDisc(roadSegments[i].startGuidelinePoints[j], Vector3.up, roadCreator.globalSettings.pointSize * 0.75f);
                    }
                }

                Handles.color = Misc.darkGreen;
                if (roadSegments[i].centerGuidelinePoints != null && roadSegments[i].centerGuidelinePoints.Length > 0 && (Vector3.Distance(mousePosition, roadSegments[i].transform.GetChild(0).GetChild(1).position) < 10) && roadSegments[i].transform.GetChild(0).GetChild(1).gameObject != objectToMove && roadSegments[i].transform.GetChild(0).GetChild(1).gameObject != extraObjectToMove)
                {
                    Handles.DrawLine(roadSegments[i].transform.GetChild(0).GetChild(1).position, roadSegments[i].centerGuidelinePoints[roadSegments[i].centerGuidelinePoints.Length - 2]);
                    Handles.DrawLine(roadSegments[i].transform.GetChild(0).GetChild(1).position, roadSegments[i].centerGuidelinePoints[roadSegments[i].centerGuidelinePoints.Length - 1]);

                    for (int j = 0; j < roadSegments[i].centerGuidelinePoints.Length; j++)
                    {
                        Handles.DrawSolidDisc(roadSegments[i].centerGuidelinePoints[j], Vector3.up, roadCreator.globalSettings.pointSize * 0.75f);
                    }
                }

                Handles.color = Misc.lightGreen;
                if (roadSegments[i].endGuidelinePoints != null && roadSegments[i].endGuidelinePoints.Length > 0 && (Vector3.Distance(mousePosition, roadSegments[i].transform.GetChild(0).GetChild(2).position) < 10) && roadSegments[i].transform.GetChild(0).GetChild(2).gameObject != objectToMove && roadSegments[i].transform.GetChild(0).GetChild(2).gameObject != extraObjectToMove)
                {
                    Handles.DrawLine(roadSegments[i].transform.GetChild(0).GetChild(2).position, roadSegments[i].endGuidelinePoints[roadSegments[i].endGuidelinePoints.Length - 2]);
                    Handles.DrawLine(roadSegments[i].transform.GetChild(0).GetChild(2).position, roadSegments[i].endGuidelinePoints[roadSegments[i].endGuidelinePoints.Length - 1]);

                    for (int j = 0; j < roadSegments[i].endGuidelinePoints.Length; j++)
                    {
                        Handles.DrawSolidDisc(roadSegments[i].endGuidelinePoints[j], Vector3.up, roadCreator.globalSettings.pointSize * 0.75f);
                    }
                }
            }
        }

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
                    Handles.CylinderHandleCap(0, objects[i].GetChild(0).GetChild(j).GetChild(1).position, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);
                }
            }
            else if (objects[i].name == "Road Splitter" || objects[i].name == "Road Transition")
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
