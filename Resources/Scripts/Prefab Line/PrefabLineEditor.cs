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
        prefabCreator.pointCalculationDivisions = Mathf.Clamp(EditorGUILayout.FloatField("Point Calculation Divisions", prefabCreator.pointCalculationDivisions), 1, 1000);
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

        prefabCreator.yModification = (PrefabLineCreator.YModification)EditorGUILayout.EnumPopup("Vertex Y Modification", prefabCreator.yModification);

        if (prefabCreator.yModification == PrefabLineCreator.YModification.matchTerrain)
        {
            prefabCreator.terrainCheckHeight = Mathf.Max(0, EditorGUILayout.FloatField("Check Terrain Height", prefabCreator.terrainCheckHeight));
        }

        prefabCreator.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabCreator.prefab, typeof(GameObject), false);
        prefabCreator.startPrefab = (GameObject)EditorGUILayout.ObjectField("Start Prefab", prefabCreator.startPrefab, typeof(GameObject), false);
        prefabCreator.endPrefab = (GameObject)EditorGUILayout.ObjectField("End Prefab", prefabCreator.endPrefab, typeof(GameObject), false);
        prefabCreator.scale = Mathf.Clamp(EditorGUILayout.FloatField("Prefab Scale", prefabCreator.scale), 0.1f, 10);
        prefabCreator.rotateAlongCurve = EditorGUILayout.Toggle("Rotate Alongst Curve", prefabCreator.rotateAlongCurve);
        if (prefabCreator.rotateAlongCurve == true)
        {
            prefabCreator.rotationDirection = (PrefabLineCreator.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", prefabCreator.rotationDirection);

            if (prefabCreator.fillGap == false)
            {
                prefabCreator.yRotationRandomization = Mathf.Clamp(EditorGUILayout.FloatField("Y Rotation Randomization", prefabCreator.yRotationRandomization), 0, 360);
            } else
            {
                prefabCreator.yRotationRandomization = 0;
            }
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            if (prefabCreator.prefab == null)
            {
                prefabCreator.prefab = Resources.Load("Prefabs/Low Poly/Concrete Barrier") as GameObject;
            }
            else if (prefabCreator.prefab.GetComponent<MeshFilter>() == null)
            {
                prefabCreator.prefab = Resources.Load("Prefabs/Low Poly/Concrete Barrier") as GameObject;
                Debug.Log("Selected prefab must have a mesh filter attached. Prefab has been changed to the concrete barrier");
                return;
            }

            if (prefabCreator.fillGap == true || prefabCreator.bendObjects == true)
            {
                prefabCreator.spacing = prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x * 2 * prefabCreator.scale;
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

            if (GUILayout.Button("Convert To Mesh"))
            {
                Misc.ConvertToMesh(prefabCreator.gameObject, "Prefab Line Mesh");
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
                            prefabCreator.CreatePoints(hitPosition);
                        }
                    }
                    else if (guiEvent.button == 1 && guiEvent.shift == true)
                    {
                        prefabCreator.RemovePoints();
                    }
                }

                if (prefabCreator.transform.childCount > 0 && prefabCreator.transform.GetChild(0).childCount > 1 && (guiEvent.type == EventType.MouseDrag || guiEvent.type == EventType.MouseMove || guiEvent.type == EventType.MouseDown))
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
                    prefabCreator.MovePoints(hitPosition, guiEvent, raycastHit);
                }
            }

            GameObject.FindObjectOfType<RoadSystem>().ShowCreationButtons();
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

        if (prefabCreator.transform.childCount > 0)
        {
            if (guiEvent.shift == true)
            {
                Handles.color = Color.black;
                if (prefabCreator.transform.GetChild(0).childCount > 1 && prefabCreator.transform.GetChild(0).GetChild(prefabCreator.transform.GetChild(0).childCount - 1).name == "Control Point")
                {
                    Handles.DrawPolyLine(points);
                }
                else if (prefabCreator.transform.GetChild(0).childCount > 0)
                {
                    Handles.DrawLine(prefabCreator.transform.GetChild(0).GetChild(prefabCreator.transform.GetChild(0).childCount - 1).position, hitPosition);
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
        int lastIndex = prefabCreator.transform.GetChild(0).GetChild(prefabCreator.transform.GetChild(0).childCount - 1).GetSiblingIndex();
        if (prefabCreator.transform.GetChild(0).GetChild(lastIndex).name == "Point")
        {
            lastIndex -= 1;
        }

        divisions = Misc.CalculateDistance(prefabCreator.transform.GetChild(0).GetChild(lastIndex - 1).position, prefabCreator.transform.GetChild(0).GetChild(lastIndex).position, hitPosition);

        divisions = Mathf.Max(2, divisions);
        List<Vector3> points = new List<Vector3>();
        float distancePerDivision = 1 / divisions;

        for (float t = 0; t <= 1; t += distancePerDivision / prefabCreator.pointCalculationDivisions)
        {
            if (t > 1 - distancePerDivision / prefabCreator.pointCalculationDivisions)
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
