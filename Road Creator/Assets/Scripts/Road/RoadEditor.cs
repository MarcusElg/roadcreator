using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadCreator))]
public class RoadEditor : Editor
{

    RoadCreator roadCreator;
    Vector3[] points = null;
    GameObject objectToMove, extraObjectToMove;
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
            if (GameObject.FindObjectOfType<GlobalSettings>() == null)
            {
                roadCreator.globalSettings = new GameObject("Global Settings").AddComponent<GlobalSettings>();
            }
            else
            {
                roadCreator.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
            }
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

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("");
        GUILayout.Label("Default segment options", guiStyle);

        roadCreator.defaultRoadMaterial = (Material)EditorGUILayout.ObjectField("Default Road Material", roadCreator.defaultRoadMaterial, typeof(Material), false);
        roadCreator.defaultShoulderMaterial = (Material)EditorGUILayout.ObjectField("Default Shoulder Material", roadCreator.defaultShoulderMaterial, typeof(Material), false);

        if (EditorGUI.EndChangeCheck() == true)
        {
            roadCreator.UpdateMesh();
        }

        if (roadCreator.globalSettings.debug == true)
        {
            GUILayout.Label("");
            GUILayout.Label("Debug", guiStyle);
            EditorGUILayout.ObjectField(roadCreator.currentSegment, typeof(RoadSegment), true);
        }

        if (GUILayout.Button("Reset road"))
        {
            roadCreator.currentSegment = null;

            for (int i = roadCreator.transform.GetChild(0).childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roadCreator.transform.GetChild(0).GetChild(i).gameObject);
            }
        }

        if (GUILayout.Button("Generate road"))
        {
            roadCreator.UpdateMesh();
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
                hitPosition = Misc.Round(hitPosition);
            }

            if (guiEvent.type == EventType.MouseDown)
            {
                if (guiEvent.button == 0)
                {
                    if (guiEvent.shift == true)
                    {
                        if (roadCreator.defaultRoadMaterial == null)
                        {
                            Debug.Log("You have to select a default road material before creating points");
                            return;
                        }

                        if (roadCreator.defaultShoulderMaterial == null)
                        {
                            Debug.Log("You have to select a default shoulder material before creating points");
                            return;
                        }

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
                Draw();
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

        DetectIntersectionConnections();

        roadCreator.UpdateMesh();
    }

    private void CreatePoints()
    {
        if (roadCreator.currentSegment != null)
        {
            if (roadCreator.currentSegment.transform.GetChild(0).childCount == 1)
            {
                // Create control point
                Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", roadCreator.currentSegment.transform.GetChild(0), hitPosition), "Created point");
            }
            else if (roadCreator.currentSegment.transform.GetChild(0).childCount == 2)
            {
                // Create end point
                Undo.RegisterCreatedObjectUndo(CreatePoint("End Point", roadCreator.currentSegment.transform.GetChild(0), hitPosition), "Create Point");
                roadCreator.currentSegment = null;
                roadCreator.UpdateMesh();
                DetectIntersectionConnections();
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
                DetectIntersectionConnections();
            }
            else
            {
                RoadSegment segment = CreateSegment(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).position);
                Undo.RegisterCreatedObjectUndo(segment.gameObject, "Create Point");
                Undo.RegisterCreatedObjectUndo(CreatePoint("Start Point", segment.transform.GetChild(0), segment.transform.position), "Create Point");
                Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", segment.transform.GetChild(0), hitPosition), "Create Point");
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
        segment.roadMaterial = roadCreator.defaultRoadMaterial;
        segment.leftShoulderMaterial = roadCreator.defaultShoulderMaterial;
        segment.rightShoulderMaterial = roadCreator.defaultShoulderMaterial;

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
            objectToMove.GetComponent<BoxCollider>().enabled = true;
            DetectIntersectionConnections();
            objectToMove = null;

            if (extraObjectToMove != null)
            {
                extraObjectToMove.GetComponent<BoxCollider>().enabled = true;
                extraObjectToMove = null;
            }

            roadCreator.UpdateMesh();
        }
    }

    private void DetectIntersectionConnections()
    {
        if (roadCreator.transform.GetChild(0).childCount > 0)
        {
            DetectIntersectionConnection(roadCreator.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject);

            Transform lastSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1);
            if (lastSegment.GetChild(0).childCount == 3)
            {
                DetectIntersectionConnection(lastSegment.transform.GetChild(0).GetChild(2).gameObject);
            }
        }
    }

    private void DetectIntersectionConnection(GameObject gameObject)
    {
        RaycastHit raycastHit2;
        if (Physics.Raycast(new Ray(gameObject.transform.position + Vector3.up, Vector3.down), out raycastHit2, 100f, ~(1 << roadCreator.globalSettings.ignoreMouseRayLayer)))
        {
            if (raycastHit2.collider.name.Contains("Connection Point"))
            {
                gameObject.GetComponent<Point>().intersectionConnection = raycastHit2.collider.gameObject;
                gameObject.transform.position = raycastHit2.collider.transform.position;
            }
            else
            {
                gameObject.GetComponent<Point>().intersectionConnection = null;
            }
        }
    }

    private void RemovePoints()
    {
        if (roadCreator.transform.GetChild(0).childCount > 0)
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
                        roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshFilter>().sharedMesh = null;
                        roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(1).GetChild(0).GetComponent<MeshCollider>().sharedMesh = null;
                        Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                        roadCreator.currentSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
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
                Undo.DestroyObjectImmediate(roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetChild(0).GetChild(2).gameObject);
                roadCreator.currentSegment = roadCreator.transform.GetChild(0).GetChild(roadCreator.transform.GetChild(0).childCount - 1).GetComponent<RoadSegment>();
            }
        }
    }

    private void Draw()
    {
        // Mouse position
        Handles.color = Color.blue;
        Handles.CylinderHandleCap(0, hitPosition, Quaternion.Euler(90, 0, 0), roadCreator.globalSettings.pointSize, EventType.Repaint);

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
