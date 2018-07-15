using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabLineCreator))]
public class PrefabLineEditor : Editor
{

    PrefabLineCreator prefabCreator;
    Vector3[] points = null;
    GameObject objectToMove;
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
        }

        if (prefabCreator.transform.childCount < 2 || prefabCreator.transform.GetChild(1).name != "Objects")
        {
            GameObject objects = new GameObject("Objects");
            objects.transform.SetParent(prefabCreator.transform);
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
        prefabCreator.spacing = Mathf.Max(0.1f, EditorGUILayout.FloatField("Spacing", prefabCreator.spacing));
        if (GUILayout.Button("Calculate Spacing (X)") == true)
        {
            if (prefabCreator.prefab != null)
            {
                prefabCreator.spacing = prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;
            }
        }

        if (GUILayout.Button("Calculate Spacing (Z)") == true)
        {
            if (prefabCreator.prefab != null)
            {
                prefabCreator.spacing = prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z;
            }
        }

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("");
        GUILayout.Label("Prefab Options", guiStyle);

        prefabCreator.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabCreator.prefab, typeof(GameObject), false);
        prefabCreator.scale = Mathf.Clamp(EditorGUILayout.FloatField("Prefab Scale", prefabCreator.scale), 0, 10);
        prefabCreator.rotateAlongCurve = EditorGUILayout.Toggle("Rotate Alongt Curve", prefabCreator.rotateAlongCurve);
        if (prefabCreator.rotateAlongCurve == true)
        {
            prefabCreator.rotationDirection = (PrefabLineCreator.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", prefabCreator.rotationDirection);
        }

        prefabCreator.offsetPrefabWidth = EditorGUILayout.Toggle("Offset Prefab Width", prefabCreator.offsetPrefabWidth);

        if (EditorGUI.EndChangeCheck() == true)
        {
            PlacePrefabs();
        }

        GUILayout.Label("");

        if (GUILayout.Button("Reset"))
        {
            prefabCreator.currentPoint = null;

            for (int i = 1; i >= 0; i--)
            {
                for (int j = prefabCreator.transform.GetChild(i).childCount - 1; j >= 0; j--)
                {
                    DestroyImmediate(prefabCreator.transform.GetChild(i).GetChild(j).gameObject);
                }
            }
        }

        if (GUILayout.Button("Place Prefabs"))
        {
            PlacePrefabs();
        }
    }

    public void PlacePrefabs()
    {
        for (int i = prefabCreator.transform.GetChild(1).childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(prefabCreator.transform.GetChild(1).GetChild(i).gameObject);
        }

        if (prefabCreator.transform.GetChild(0).childCount > 2)
        {
            Vector3[] currentPoints = null;
            Vector3[] nextPoints;

            for (int i = 0; i < prefabCreator.transform.GetChild(0).childCount; i += 2)
            {
                if (i == 0)
                {
                    if (prefabCreator.offsetPrefabWidth == true)
                    {
                        currentPoints = CalculatePoints(i, Misc.GetPrefabOffset(prefabCreator.prefab, prefabCreator.scale, prefabCreator.globalSettings.pointSize * prefabCreator.scale));
                    } else
                    {
                        currentPoints = CalculatePoints(i, 0);
                    }
                }

                if (prefabCreator.transform.GetChild(0).GetChild(i).name == "Point")
                {
                    if (i < prefabCreator.transform.GetChild(0).childCount - 4)
                    {
                        if (prefabCreator.offsetPrefabWidth == true)
                        {
                            nextPoints = CalculatePoints(i + 2, Misc.GetPrefabOffset(prefabCreator.prefab, prefabCreator.scale, prefabCreator.globalSettings.pointSize * prefabCreator.scale));
                        } else
                        {
                            nextPoints = CalculatePoints(i + 2, 0);
                        }

                        PlacePrefabsInSegment(currentPoints, nextPoints, true);

                        currentPoints = nextPoints;
                    }
                    else if ((i < prefabCreator.transform.GetChild(0).childCount - 1 && prefabCreator.transform.GetChild(0).GetChild(prefabCreator.transform.GetChild(0).childCount - 1).name == "Point") || i < prefabCreator.transform.GetChild(0).childCount - 2)
                    {
                        PlacePrefabsInSegment(currentPoints, null, false);
                    }
                }
            }
        }
    }

    private void PlacePrefabsInSegment(Vector3[] currentPoints, Vector3[] nextPoints, bool removeLastPoint)
    {
        int max = currentPoints.Length;
        if (removeLastPoint == true)
        {
            max -= 1;
        }

        for (int j = 0; j < max; j++)
        {
            GameObject prefab = Instantiate(prefabCreator.prefab);
            prefab.transform.SetParent(prefabCreator.transform.GetChild(1));
            prefab.transform.position = currentPoints[j];
            prefab.name = "Prefab";
            prefab.layer = prefabCreator.globalSettings.ignoreMouseRayLayer;
            prefab.transform.localScale = new Vector3(prefabCreator.scale, prefabCreator.scale, prefabCreator.scale);
            Vector3 left = Misc.CalculateLeft(currentPoints, nextPoints, Misc.MaxVector3, j);
            Vector3 forward = new Vector3(left.z, 0, -left.x);

            if (prefabCreator.rotateAlongCurve == true)
            {
                if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.forward)
                {
                    prefab.transform.rotation = Quaternion.LookRotation(forward);
                } else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.backward)
                {
                    prefab.transform.rotation = Quaternion.LookRotation(-forward);
                } else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.left)
                {
                    prefab.transform.rotation = Quaternion.LookRotation(left);
                } else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.right)
                {
                    prefab.transform.rotation = Quaternion.LookRotation(-left);
                } else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.randomY)
                {
                    prefab.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                }
            }
        }
    }

    public void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Event guiEvent = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);

        RaycastHit raycastHit;
        if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.ignoreMouseRayLayer)))
        {
            Vector3 hitPosition = raycastHit.point;

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
                        CreatePoints(hitPosition);
                    }
                }
                else if (guiEvent.button == 1 && guiEvent.shift == true)
                {
                    RemovePoints();
                }
            }

            if (prefabCreator.currentPoint != null && prefabCreator.transform.GetChild(0).childCount > 1 && (guiEvent.type == EventType.MouseDrag || guiEvent.type == EventType.MouseMove || guiEvent.type == EventType.MouseDown))
            {
                points = CalculatePoints(guiEvent, hitPosition);
            }

            Draw(guiEvent, hitPosition);
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
                MovePoints(guiEvent, raycastHit, hitPosition);
            }
        }

        GameObject.Find("Road System").GetComponent<RoadSystem>().ShowCreationButtons();
    }

    private void UndoUpdate()
    {
        if (prefabCreator.currentPoint == null && prefabCreator.transform.GetChild(0).childCount > 0)
        {
            prefabCreator.currentPoint = prefabCreator.transform.GetChild(0).GetChild(prefabCreator.transform.GetChild(0).childCount - 1).gameObject;
        }

        PlacePrefabs();
    }

    private void CreatePoints(Vector3 hitPosition)
    {
        if (prefabCreator.prefab == null)
        {
            Debug.Log("You must select a prefab to place before creating the line");
        }
        else
        {
            if (prefabCreator.currentPoint != null && prefabCreator.currentPoint.name == "Point")
            {
                prefabCreator.currentPoint = CreatePoint("Control Point", hitPosition);
                Undo.RegisterCreatedObjectUndo(prefabCreator.currentPoint, "Create Point");
            }
            else
            {
                prefabCreator.currentPoint = CreatePoint("Point", hitPosition);
                PlacePrefabs();
                Undo.RegisterCreatedObjectUndo(prefabCreator.currentPoint, "Create Point");
            }
        }
    }

    private GameObject CreatePoint(string name, Vector3 raycastHit)
    {
        GameObject point = new GameObject(name);
        point.AddComponent<BoxCollider>();
        point.GetComponent<BoxCollider>().size = new Vector3(prefabCreator.globalSettings.pointSize, prefabCreator.globalSettings.pointSize, prefabCreator.globalSettings.pointSize);
        point.transform.SetParent(prefabCreator.transform.GetChild(0));
        point.transform.position = raycastHit;
        point.layer = prefabCreator.globalSettings.ignoreMouseRayLayer;
        return point;
    }

    private void MovePoints(Event guiEvent, RaycastHit raycastHit, Vector3 hitPosition)
    {
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && objectToMove == null)
        {
            if (raycastHit.collider.gameObject.name == "Control Point")
            {
                objectToMove = raycastHit.collider.gameObject;
                objectToMove.GetComponent<BoxCollider>().enabled = false;
            }
            else if (raycastHit.collider.gameObject.name == "Point")
            {
                objectToMove = raycastHit.collider.gameObject;
                objectToMove.GetComponent<BoxCollider>().enabled = false;
            }
        }
        else if (guiEvent.type == EventType.MouseDrag && objectToMove != null)
        {
            Undo.RecordObject(objectToMove.transform, "Moved Point");
            objectToMove.transform.position = hitPosition;
        }
        else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && objectToMove != null)
        {
            objectToMove.GetComponent<BoxCollider>().enabled = true;
            objectToMove = null;
            PlacePrefabs();
        }
    }

    private void RemovePoints()
    {
        if (prefabCreator.transform.GetChild(0).childCount > 0)
        {
            if (prefabCreator.currentPoint != null)
            {
                Undo.DestroyObjectImmediate(prefabCreator.currentPoint.gameObject);
                if (prefabCreator.transform.GetChild(0).childCount > 0)
                {
                    prefabCreator.currentPoint = prefabCreator.transform.GetChild(0).GetChild(prefabCreator.transform.GetChild(0).childCount - 1).gameObject;
                }

                PlacePrefabs();
            }
        }
    }

    private void Draw(Event guiEvent, Vector3 hitPosition)
    {
        // Mouse position
        Handles.color = Color.blue;
        Handles.CylinderHandleCap(0, hitPosition, Quaternion.Euler(90, 0, 0), prefabCreator.globalSettings.pointSize, EventType.Repaint);

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
    }

    public Vector3[] CalculatePoints(int i, float offset)
    {
        List<Vector3> points = new List<Vector3>();
        float distance = Misc.CalculateDistance(prefabCreator.transform.GetChild(0).GetChild(i).position, prefabCreator.transform.GetChild(0).GetChild(i + 1).position, prefabCreator.transform.GetChild(0).GetChild(i + 2).position);

        float divisions = distance / prefabCreator.spacing;
        divisions = Mathf.Max(2, divisions);

        float distancePerDivision = 1 / divisions;
        offset /= distance;

        for (float t = offset; t < 1 - offset; t += distancePerDivision)
        {
            if (t > 1)
            {
                t = 1;
            }

            Vector3 position = Misc.Lerp3(prefabCreator.transform.GetChild(0).GetChild(i).position, prefabCreator.transform.GetChild(0).GetChild(i + 1).position, prefabCreator.transform.GetChild(0).GetChild(i + 2).position, t);

            RaycastHit raycastHit;
            if (Physics.Raycast(position, Vector3.down, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.ignoreMouseRayLayer)))
            {
                position.y = raycastHit.point.y;
            }

            points.Add(position);
        }
        return points.ToArray();
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
            if (Physics.Raycast(position, Vector3.down, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.ignoreMouseRayLayer)))
            {
                position.y = raycastHit.point.y;
            }

            points.Add(position);
        }

        return points.ToArray();
    }
}
