using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabLineCreator))]
public class PrefabLineEditor : Editor
{

    PrefabLineCreator prefabCreator;
    Vector3[] points = null;
    GameObject objectToMove;
    Tool lastTool;
    bool mouseDown;

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

        Undo.undoRedoPerformed += UndoUpdate;
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
                    Undo.DestroyObjectImmediate(prefabCreator.transform.GetChild(i).GetChild(j).gameObject);
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
            PointPackage currentPoints = CalculatePoints();
            for (int j = 0; j < currentPoints.prefabPoints.Length; j++)
            {
                GameObject prefab = Instantiate(prefabCreator.prefab);
                prefab.transform.SetParent(prefabCreator.transform.GetChild(1));
                prefab.transform.position = currentPoints.prefabPoints[j];
                prefab.name = "Prefab";
                prefab.layer = prefabCreator.globalSettings.roadLayer;
                prefab.transform.localScale = new Vector3(prefabCreator.scale, prefabCreator.scale, prefabCreator.scale);
                Vector3 left = Misc.CalculateLeft(currentPoints.startPoints[j], currentPoints.endPoints[j]);
                Vector3 forward = new Vector3(left.z, 0, -left.x);

                if (prefabCreator.rotateAlongCurve == true)
                {
                    if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.forward)
                    {
                        prefab.transform.rotation = Quaternion.LookRotation(forward);
                    }
                    else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.backward)
                    {
                        prefab.transform.rotation = Quaternion.LookRotation(-forward);
                    }
                    else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.left)
                    {
                        prefab.transform.rotation = Quaternion.LookRotation(left);
                    }
                    else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.right)
                    {
                        prefab.transform.rotation = Quaternion.LookRotation(-left);
                    }
                    else if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.randomY)
                    {
                        prefab.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    }
                }

                if (prefabCreator.bendObjects == true)
                {
                    Mesh mesh = GameObject.Instantiate(prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh);
                    Vector3[] vertices = mesh.vertices;

                    // Calculate distance to change
                    Vector3 center = Misc.GetCenter(currentPoints.startPoints[j], currentPoints.endPoints[j]);
                    float distanceToChange = Vector3.Distance(center, currentPoints.prefabPoints[j]);

                    Vector3 controlPoint;
                    float distanceToChangeMultiplied = distanceToChange * prefabCreator.bendMultiplier;
                    if (currentPoints.rotateTowardsLeft[j] == false)
                    {
                        distanceToChangeMultiplied = -distanceToChangeMultiplied;
                    }

                    controlPoint = mesh.bounds.center + new Vector3(0, 0, distanceToChangeMultiplied);

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        float distance = Mathf.Abs(vertices[i].x - mesh.bounds.min.x);
                        float distanceCovered = (distance / mesh.bounds.size.x);
                        Vector3 lerpedPoint = Misc.Lerp3(new Vector3(-mesh.bounds.extents.x, 0, 0), controlPoint, new Vector3(mesh.bounds.extents.x, 0, 0), distanceCovered);
                        vertices[i].z = vertices[i].z - (lerpedPoint).z;
                    }

                    mesh.vertices = vertices;
                    mesh.RecalculateBounds();
                    prefab.GetComponent<MeshFilter>().sharedMesh = mesh;

                    // Change collider to match
                    System.Type type = prefab.GetComponent<Collider>().GetType();
                    if (type != null)
                    {
                        DestroyImmediate(prefab.GetComponent<Collider>());
                        prefab.AddComponent(type);
                    }
                }

                if (prefabCreator.yModification != PrefabLineCreator.YModification.none)
                {
                    Vector3[] vertices = prefab.GetComponent<MeshFilter>().sharedMesh.vertices;
                    float startHeight = currentPoints.startPoints[j].y;
                    float endHeight = currentPoints.endPoints[j].y;

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        if (prefabCreator.yModification == PrefabLineCreator.YModification.matchTerrain)
                        {
                            RaycastHit raycastHit;
                            if (Physics.Raycast(prefab.transform.position + (prefab.transform.rotation * vertices[i] * prefabCreator.scale) + new Vector3(0, prefabCreator.terrainCheckHeight, 0), Vector3.down, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.ignoreMouseRayLayer | 1 << prefabCreator.globalSettings.roadLayer)))
                            {
                                vertices[i].y += (raycastHit.point.y - prefab.transform.position.y) / prefabCreator.scale;
                            }
                        }
                        else if (prefabCreator.yModification == PrefabLineCreator.YModification.matchCurve)
                        {
                            float time = Misc.Remap(vertices[i].x, prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.x, prefab.GetComponent<MeshFilter>().sharedMesh.bounds.max.x, 0, 1);

                            if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.right)
                            {
                                time = 1 - time;
                            }

                            vertices[i].y += (Mathf.Lerp(startHeight, endHeight, time) - prefab.transform.position.y) / prefabCreator.scale;
                        }
                    }

                    Mesh mesh = Instantiate(prefab.GetComponent<MeshFilter>().sharedMesh);
                    mesh.vertices = vertices;
                    prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
                    prefab.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

                    // Change collider to match
                    System.Type type = prefab.GetComponent<Collider>().GetType();
                    if (type != null)
                    {
                        DestroyImmediate(prefab.GetComponent<Collider>());
                        prefab.AddComponent(type);
                    }
                }

                if (prefabCreator.fillGap == true && j > 0)
                {
                    // Add last vertices
                    List<Vector3> lastVertexPositions = new List<Vector3>();
                    Vector3[] lastVertices = prefabCreator.transform.GetChild(1).GetChild(j - 1).GetComponent<MeshFilter>().sharedMesh.vertices;
                    for (int i = 0; i < lastVertices.Length; i++)
                    {
                        if (Mathf.Abs(lastVertices[i].x - GetMaxX()) < 0.001f)
                        {
                            lastVertexPositions.Add((prefabCreator.transform.GetChild(1).GetChild(j - 1).transform.rotation * (prefabCreator.scale * lastVertices[i])) + prefabCreator.transform.GetChild(1).GetChild(j - 1).transform.position);
                        }
                    }

                    // Move current vertices to last ones
                    Mesh mesh = Instantiate(prefab.GetComponent<MeshFilter>().sharedMesh);
                    Vector3[] vertices = mesh.vertices;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        if (Mathf.Abs(vertices[i].x - GetMinX()) < 0.001f)
                        {
                            Vector3 nearestVertex = Vector3.zero;
                            float currentDistance = float.MaxValue;

                            for (int k = 0; k < lastVertexPositions.Count; k++)
                            {
                                float localY = (lastVertexPositions[k] - prefabCreator.transform.GetChild(1).GetChild(j - 1).transform.position).y;
                                float localZ = (Quaternion.Euler(0, -(prefabCreator.transform.GetChild(1).GetChild(j - 1).transform.rotation.eulerAngles.y), 0) * (lastVertexPositions[k] - prefabCreator.transform.GetChild(1).GetChild(j - 1).transform.position)).z;
                                float zDifference = Mathf.Abs(localZ - (vertices[i].z * prefabCreator.scale));
                                if (zDifference < 0.001f)
                                {
                                    if (prefabCreator.yModification == PrefabLineCreator.YModification.none)
                                    {
                                        if (Mathf.Abs(localY - (vertices[i].y * prefabCreator.scale)) < 0.001f)
                                        {
                                            nearestVertex = lastVertexPositions[k];
                                        }
                                    }
                                    else
                                    {
                                        float calculatedDistance = Vector3.Distance(lastVertexPositions[k], (prefab.transform.rotation * (prefabCreator.scale * vertices[i])) + prefab.transform.position);

                                        if (calculatedDistance < currentDistance)
                                        {
                                            currentDistance = calculatedDistance;
                                            nearestVertex = lastVertexPositions[k];
                                        }
                                    }
                                }
                            }

                            if (nearestVertex != Vector3.zero)
                            {
                                float scaleModifier = 1 / prefabCreator.scale;
                                vertices[i] = Quaternion.Euler(0, -prefab.transform.rotation.eulerAngles.y, 0) * (nearestVertex - prefab.transform.position) * scaleModifier;
                            }
                        }
                    }
                    mesh.vertices = vertices;
                    prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
                    prefab.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

                    // Change collider to match
                    System.Type type = prefab.GetComponent<Collider>().GetType();
                    if (type != null)
                    {
                        DestroyImmediate(prefab.GetComponent<Collider>());
                        prefab.AddComponent(type);
                    }
                }
            }
        }
    }

    private float GetMaxX()
    {
        if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.left)
        {
            return prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.max.x;
        }
        else
        {
            return prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.x;
        }
    }

    private float GetMinX()
    {
        if (prefabCreator.rotationDirection == PrefabLineCreator.RotationDirection.left)
        {
            return prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.x;
        }
        else
        {
            return prefabCreator.prefab.GetComponent<MeshFilter>().sharedMesh.bounds.max.x;
        }
    }

    public void OnSceneGUI()
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

        if (Physics.Raycast(ray, out raycastHit, 100f, ~(1 << prefabCreator.globalSettings.roadLayer)))
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
                if (prefabCreator.globalSettings.roadCurved == true)
                {
                    prefabCreator.currentPoint = CreatePoint("Control Point", hitPosition);
                    Undo.RegisterCreatedObjectUndo(prefabCreator.currentPoint, "Create Point");
                }
                else
                {
                    prefabCreator.currentPoint = CreatePoint("Control Point", Misc.GetCenter(prefabCreator.currentPoint.transform.position, hitPosition));
                    Undo.RegisterCreatedObjectUndo(prefabCreator.currentPoint, "Create Point");
                    prefabCreator.currentPoint = CreatePoint("Point", hitPosition);
                    Undo.RegisterCreatedObjectUndo(prefabCreator.currentPoint, "Create Point");
                    PlacePrefabs();
                }
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
        point.hideFlags = HideFlags.NotEditable;
        point.layer = prefabCreator.globalSettings.ignoreMouseRayLayer;
        return point;
    }

    private void MovePoints(Event guiEvent, RaycastHit raycastHit, Vector3 hitPosition)
    {
        if (mouseDown == true && objectToMove != null)
        {
            if (guiEvent.keyCode == KeyCode.Plus || guiEvent.keyCode == KeyCode.KeypadPlus)
            {
                Undo.RecordObject(objectToMove.transform, "Moved Point");
                objectToMove.transform.position += new Vector3(0, 0.2f, 0);
            }
            else if (guiEvent.keyCode == KeyCode.Minus || guiEvent.keyCode == KeyCode.KeypadMinus)
            {
                Vector3 position = objectToMove.transform.position - new Vector3(0, 0.2f, 0);

                if (position.y < raycastHit.point.y)
                {
                    position.y = raycastHit.point.y;
                }

                Undo.RecordObject(objectToMove.transform, "Moved Point");
                objectToMove.transform.position = position;
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && objectToMove == null)
        {
            mouseDown = true;

            if (raycastHit.transform.name.Contains("Point") && raycastHit.collider.transform.parent.parent.GetComponent<PrefabLineCreator>() != null && raycastHit.collider.transform.parent.parent.GetComponent<PrefabLineCreator>() == prefabCreator)
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
        }
        else if (guiEvent.type == EventType.MouseDrag && objectToMove != null)
        {
            Undo.RecordObject(objectToMove.transform, "Moved Point");
            objectToMove.transform.position = hitPosition;
        }
        else if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0 && objectToMove != null)
        {
            mouseDown = false;
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
        Misc.DrawRoadGuidelines(hitPosition, objectToMove, null);

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

    public PointPackage CalculatePoints()
    {
        List<Vector3> prefabPoints = new List<Vector3>();
        List<Vector3> startPoints = new List<Vector3>();
        List<Vector3> endPoints = new List<Vector3>();
        List<bool> rotateTowardsLeft = new List<bool>();

        Vector3 firstPoint = prefabCreator.transform.GetChild(0).GetChild(0).position;
        Vector3 controlPoint = prefabCreator.transform.GetChild(0).GetChild(1).position;
        Vector3 endPoint = prefabCreator.transform.GetChild(0).GetChild(2).position;

        Vector3 lastEndPoint = firstPoint;
        float distance = Misc.CalculateDistance(firstPoint, controlPoint, endPoint);

        startPoints.Add(firstPoint);
        Vector3 lastPoint = firstPoint;
        bool endPointAdded = true;

        Vector3 currentPoint = Vector3.zero;

        for (int i = 0; i < prefabCreator.transform.GetChild(0).childCount - 2; i += 2)
        {
            distance = Misc.CalculateDistance(firstPoint, controlPoint, endPoint);
            float divisions = distance / prefabCreator.spacing;
            divisions = Mathf.Max(2, divisions);
            float distancePerDivision = 1 / divisions;
            bool isSegmentLeft = IsSegmentLeft(i);
            firstPoint = prefabCreator.transform.GetChild(0).GetChild(i).position;
            controlPoint = prefabCreator.transform.GetChild(0).GetChild(i + 1).position;
            endPoint = prefabCreator.transform.GetChild(0).GetChild(i + 2).position;

            if (i == 0)
            {
                rotateTowardsLeft.Add(isSegmentLeft);
            }

            for (float t = 0; t < 1; t += distancePerDivision / 10)
            {
                if (t > 1)
                {
                    t = 1;
                }

                float height = Mathf.Lerp(firstPoint.y, endPoint.y, t);
                currentPoint = Misc.Lerp3(firstPoint, controlPoint, endPoint, t);
                currentPoint.y = height;
                float currentDistance = Vector3.Distance(lastPoint, currentPoint);

                if (currentDistance > prefabCreator.spacing / 2 && endPointAdded == false)
                {
                    endPoints.Add(currentPoint);
                    lastEndPoint = currentPoint;
                    endPointAdded = true;
                }

                if (endPointAdded == true && ((currentDistance > prefabCreator.spacing) || (startPoints.Count == 1 && currentDistance > prefabCreator.spacing / 2)))
                {
                    prefabPoints.Add(currentPoint);
                    lastPoint = currentPoint;
                    startPoints.Add(lastEndPoint);
                    endPointAdded = false;

                    rotateTowardsLeft.Add(isSegmentLeft);
                }
            }

            if (endPoints.Count < prefabPoints.Count && (i + 2) >= prefabCreator.transform.GetChild(0).childCount - 2)
            {
                endPoints.Add(currentPoint);
            }
        }

        return new PointPackage(prefabPoints.ToArray(), startPoints.ToArray(), endPoints.ToArray(), rotateTowardsLeft.ToArray());
    }

    private bool IsSegmentLeft(int startIndex)
    {
        Vector3 forward = (prefabCreator.transform.GetChild(0).GetChild(startIndex).position - prefabCreator.transform.GetChild(0).GetChild(startIndex + 2).position).normalized;
        Vector3 center = Misc.GetCenter(prefabCreator.transform.GetChild(0).GetChild(startIndex).position, prefabCreator.transform.GetChild(0).GetChild(startIndex + 2).position);
        Vector3 right = Vector3.Cross(forward, (prefabCreator.transform.GetChild(0).GetChild(startIndex + 1).position - center).normalized);
        float direction = Vector3.Dot(right, Vector3.up);

        if (direction > 0.0f)
        {
            return false;
        }
        else
        {
            return true;
        }
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
