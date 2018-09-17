using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabLineCreator))]
public class PrefabLineEditor : Editor
{

    PrefabLineCreator prefabCreator;
    Vector3[] points = null;
    Tool lastTool;

    private void OnEnable()
    {
        prefabCreator = (PrefabLineCreator)target;

        if (prefabCreator.globalSettings == null)
        {
            prefabCreator.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        if (prefabCreator.transform.childCount == 0 || prefabCreator.transform.GetChild(0).name != "Points")
        {
            GameObject points = new GameObject("Points");
            points.transform.SetParent(prefabCreator.transform);
            points.transform.SetAsFirstSibling();
            points.hideFlags = HideFlags.NotEditable;
        }

        if (prefabCreator.transform.childCount < 2 || prefabCreator.transform.GetChild(1).name != "Objects")
        {
            GameObject objects = new GameObject("Objects");
            objects.transform.SetParent(prefabCreator.transform);
            objects.hideFlags = HideFlags.NotEditable;
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        Undo.undoRedoPerformed += prefabCreator.UndoUpdate;
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        prefabCreator.bendObjects = GUILayout.Toggle(prefabCreator.bendObjects, "Bend Objects");
        prefabCreator.fillGap = GUILayout.Toggle(prefabCreator.fillGap, "Fill Gap");

        if (prefabCreator.fillGap == false && prefabCreator.bendObjects == false)
        {
            prefabCreator.spacing = Mathf.Max(0.1f, EditorGUILayout.FloatField("Spacing", prefabCreator.spacing));

            if (GUILayout.Button("Calculate Spacing (X)") == true)
            {
                if (prefabCreator.prefab != null)
                {
                    prefabCreator.spacing = prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x * 2 * prefabCreator.scale;
                }
            }

            if (GUILayout.Button("Calculate Spacing (Z)") == true)
            {
                if (prefabCreator.prefab != null)
                {
                    prefabCreator.spacing = prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z * 2 * prefabCreator.scale;
                }
            }
        }

        if (prefabCreator.bendObjects == true)
        {
            prefabCreator.bendMultiplier = Mathf.Max(0, EditorGUILayout.FloatField("Bend Multiplier", prefabCreator.bendMultiplier));
        }

        prefabCreator.yModification = (PrefabLineCreator.YModification)EditorGUILayout.EnumPopup("Vertex Y Modification", prefabCreator.yModification);

        if (prefabCreator.yModification == PrefabLineCreator.YModification.matchTerrain)
        {
            prefabCreator.terrainCheckHeight = Mathf.Max(0, EditorGUILayout.FloatField("Check Terrain Height", prefabCreator.terrainCheckHeight));
        }

        prefabCreator.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabCreator.prefab, typeof(GameObject), false);
        prefabCreator.scale = Mathf.Clamp(EditorGUILayout.FloatField("Prefab Scale", prefabCreator.scale), 0.1f, 10);
        prefabCreator.rotateAlongCurve = EditorGUILayout.Toggle("Rotate Alongst Curve", prefabCreator.rotateAlongCurve);
        if (prefabCreator.rotateAlongCurve == true)
        {
            prefabCreator.rotationDirection = (PrefabLineCreator.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", prefabCreator.rotationDirection);
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            if (prefabCreator.prefab.GetComponent<MeshFilter>() == null)
            {
                prefabCreator.prefab = null;
                Debug.Log("Selected prefab must have a mesh filter attached");
                return;
            }

            if (prefabCreator.fillGap == true || prefabCreator.bendObjects == true)
            {
                prefabCreator.spacing = prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x * 2 * prefabCreator.scale;

                if (prefabCreator.rotationDirection != PrefabLineCreator.RotationDirection.left && prefabCreator.rotationDirection != PrefabLineCreator.RotationDirection.right)
                {
                    prefabCreator.rotationDirection = PrefabLineCreator.RotationDirection.left;
                }
            }

            prefabCreator.PlacePrefabs();
        }

        if (prefabCreator.globalSettings.debug == true)
        {
            EditorGUILayout.Toggle("Is Follow Object", prefabCreator.isFollowObject);
        }

        if (prefabCreator.isFollowObject == false)
        {
            GUILayout.Label("");

            if (GUILayout.Button("Reset"))
            {
                prefabCreator.currentPoint = null;

                for (int i = 1; i >= 0; i--)
                {
                    for (int j = prefabCreator.transform.GetChild(i).childCount - 1; j >= 0; j--)
                    {
                        Undo.DestroyObjectImmediate(prefabCreator.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }

            if (GUILayout.Button("Place Prefabs"))
            {
                prefabCreator.PlacePrefabs();
            }
        }
    }

    public void OnSceneGUI()
    {
        if (prefabCreator.isFollowObject == false)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event guiEvent = Event.current;

            Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.ignoreMouseRayLayer | 1 << prefabCreator.globalSettings.roadLayer)))
            {
                Vector3 hitPosition = raycastHit.point;

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
                            prefabCreator.CreatePoints(hitPosition);
                        }
                    }
                    else if (guiEvent.button == 1 && guiEvent.shift == true)
                    {
                        prefabCreator.RemovePoints();
                    }
                }

                if (prefabCreator.currentPoint != null && prefabCreator.transform.GetChild(0).childCount > 1 && (guiEvent.type == EventType.MouseDrag || guiEvent.type == EventType.MouseMove || guiEvent.type == EventType.MouseDown))
                {
                    points = CalculatePoints(guiEvent, hitPosition);
                }

                Draw(guiEvent, hitPosition);
            }

            if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.roadLayer)))
            {
                Vector3 hitPosition = raycastHit.point;

                if (guiEvent.control == true)
                {
                    hitPosition = Misc.Round(hitPosition);
                }

                if (guiEvent.shift == false)
                {
                    prefabCreator.MovePoints(guiEvent, raycastHit, hitPosition);
                }
            }

            GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
        }
    }

    private void Draw(Event guiEvent, Vector3 hitPosition)
    {
        Misc.DrawRoadGuidelines(hitPosition, prefabCreator.objectToMove, null);

        for (int i = 0; i < prefabCreator.transform.GetChild(0).childCount; i++)
        {
            if (prefabCreator.transform.GetChild(0).GetChild(i).name == "Point")
            {
                Handles.color = Color.red;
                Handles.CylinderHandleCap(0, prefabCreator.transform.GetChild(0).GetChild(i).position, Quaternion.Euler(90, 0, 0), prefabCreator.globalSettings.pointSize, EventType.Repaint);
            }
            else
            {
                Handles.color = Color.yellow;
                Handles.CylinderHandleCap(0, prefabCreator.transform.GetChild(0).GetChild(i).position, Quaternion.Euler(90, 0, 0), prefabCreator.globalSettings.pointSize, EventType.Repaint);
            }
        }

        for (int j = 1; j < prefabCreator.transform.GetChild(0).childCount; j += 2)
        {
            Handles.color = Color.white;
            Handles.DrawLine(prefabCreator.transform.GetChild(0).GetChild(j - 1).position, prefabCreator.transform.GetChild(0).GetChild(j).position);

            if (j < prefabCreator.transform.GetChild(0).childCount - 1)
            {
                Handles.DrawLine(prefabCreator.transform.GetChild(0).GetChild(j).position, prefabCreator.transform.GetChild(0).GetChild(j + 1).position);
            }
            else if (guiEvent.shift == true)
            {
                Handles.DrawLine(prefabCreator.transform.GetChild(0).GetChild(j).position, hitPosition);
            }
        }

        if (prefabCreator.currentPoint != null)
        {
            if (guiEvent.shift == true)
            {
                if (prefabCreator.transform.GetChild(0).childCount > 1 && prefabCreator.currentPoint.name == "Control Point")
                {
                    Handles.color = Color.black;
                    Handles.DrawPolyLine(points);
                }
                else
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(prefabCreator.currentPoint.transform.position, hitPosition);
                }
            }
        }

        // Mouse position
        Handles.color = Color.blue;
        Handles.CylinderHandleCap(0, hitPosition, Quaternion.Euler(90, 0, 0), prefabCreator.globalSettings.pointSize, EventType.Repaint);
    }

    private Vector3[] CalculatePoints(Event guiEvent, Vector3 hitPosition)
    {
        float divisions;
        int lastIndex = prefabCreator.currentPoint.transform.GetSiblingIndex();
        if (prefabCreator.transform.GetChild(0).GetChild(lastIndex).name == "Point")
        {
            lastIndex -= 1;
        }

        divisions = Misc.CalculateDistance(prefabCreator.transform.GetChild(0).GetChild(lastIndex - 1).position, prefabCreator.transform.GetChild(0).GetChild(lastIndex).position, hitPosition);

        divisions = Mathf.Max(2, divisions);
        List<Vector3> points = new List<Vector3>();
        float distancePerDivision = 1 / divisions;

        for (float t = 0; t <= 1; t += distancePerDivision)
        {
            if (t > 1 - distancePerDivision)
            {
                t = 1;
            }

            Vector3 position = Misc.Lerp3(prefabCreator.transform.GetChild(0).GetChild(lastIndex - 1).position, prefabCreator.transform.GetChild(0).GetChild(lastIndex).position, hitPosition, t);

            RaycastHit raycastHit;
            if (Physics.Raycast(position, Vector3.down, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.ignoreMouseRayLayer | 1 << prefabCreator.globalSettings.roadLayer)))
            {
                position.y = raycastHit.point.y;
            }

            points.Add(position);
        }

        return points.ToArray();
    }
}
