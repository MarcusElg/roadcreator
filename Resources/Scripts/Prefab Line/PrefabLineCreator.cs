#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Prefab-Lines")]
public class PrefabLineCreator : MonoBehaviour
{

    public GameObject prefab;
    public GameObject startPrefab;
    public GameObject endPrefab;

    public enum YModification { none, matchTerrain, matchCurve };
    public YModification yModification;
    public float terrainCheckHeight = 1;

    public bool bendObjects = true;
    public bool fillGap = true;
    public float spacing = -1;
    public bool rotateAlongCurve = true;

    public enum RotationDirection { forward, backward, left, right };
    public RotationDirection rotationDirection = RotationDirection.left;
    public float yRotationRandomization;

    public float xScale = 1;
    public float yScale = 1;
    public float zScale = 1;
    public float pointCalculationDivisions = 100;

    // Bridges
    public bool bridgeMode = false;
    public BridgeSettings bridgeSettings;
    public float bridgeOffset;
    public float startWidthLeft;
    public float startWidthRight;
    public float endWidthLeft;
    public float endWidthRight;

    public GameObject objectToMove;
    private bool mouseDown;

#if UNITY_EDITOR
    public SerializedObject settings;
#endif

    public void UndoUpdate()
    {
        PlacePrefabs();
    }

    public void Setup()
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        if (transform.childCount == 0 || transform.GetChild(0).name != "Points")
        {
            GameObject points = new GameObject("Points");
            points.transform.SetParent(transform, false);
            points.transform.SetAsFirstSibling();
            points.hideFlags = HideFlags.NotEditable;
        }

        if (transform.childCount < 2 || transform.GetChild(1).name != "Objects")
        {
            GameObject objects = new GameObject("Objects");
            objects.transform.SetParent(transform, false);

            if (settings.FindProperty("hideNonEditableChildren").boolValue == true)
            {
                objects.hideFlags = HideFlags.HideInHierarchy;
            }
            else
            {
                objects.hideFlags = HideFlags.NotEditable;
            }
        }

        startPrefab = (GameObject)settings.FindProperty("defaultPrefabLineStartPrefab").objectReferenceValue;
        endPrefab = (GameObject)settings.FindProperty("defaultPrefabLineEndPrefab").objectReferenceValue;
    }

    public void CreatePoints(Vector3 hitPosition)
    {
        if (transform.GetChild(0).childCount > 0 && transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).name == "Point")
        {
            if (settings.FindProperty("roadCurved").boolValue == true)
            {
                Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", hitPosition), "Create Point");
            }
            else
            {
                Undo.RegisterCreatedObjectUndo(CreatePoint("Control Point", Misc.GetCenter(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).position, hitPosition)), "Create Point");
                Undo.RegisterCreatedObjectUndo(CreatePoint("Point", hitPosition), "Create Point");
                PlacePrefabs();
            }
        }
        else
        {
            Undo.RegisterCreatedObjectUndo(CreatePoint("Point", hitPosition), "Create Point");
            PlacePrefabs();
        }
    }

    public GameObject CreatePoint(string name, Vector3 raycastHit, bool nonEditable = false)
    {
        GameObject point = new GameObject(name);
        point.AddComponent<BoxCollider>();
        point.GetComponent<BoxCollider>().size = new Vector3(settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue);
        point.GetComponent<BoxCollider>().hideFlags = HideFlags.NotEditable;
        point.transform.SetParent(transform.GetChild(0));
        point.transform.position = raycastHit;
        point.layer = LayerMask.NameToLayer("Ignore Mouse Ray");
        point.AddComponent<Point>();
        point.GetComponent<Point>().roadPoint = false;
        point.GetComponent<Point>().hideFlags = HideFlags.NotEditable;

        if (nonEditable == true)
        {
            point.hideFlags = HideFlags.NotEditable;
            point.GetComponent<BoxCollider>().enabled = false;
        }

        return point;
    }

    public void MovePoints(Vector3 hitPosition, Event guiEvent, RaycastHit raycastHit)
    {
        if (hitPosition == Misc.MaxVector3)
        {
            if (objectToMove != null)
            {
                mouseDown = false;
                objectToMove.GetComponent<BoxCollider>().enabled = true;
                objectToMove = null;
                PlacePrefabs();
            }
        }
        else
        {
            if (mouseDown == true && objectToMove != null)
            {
                if (guiEvent.keyCode == KeyCode.Plus || guiEvent.keyCode == KeyCode.KeypadPlus)
                {
                    Undo.RecordObject(objectToMove.transform, "Moved Point");
                    objectToMove.transform.position += new Vector3(0, 0.2f, 0);

                    if (guiEvent.control == true)
                    {
                        objectToMove.transform.position = new Vector3(objectToMove.transform.position.x, Mathf.Ceil(objectToMove.transform.position.y), objectToMove.transform.position.z);
                    }
                }
                else if (guiEvent.keyCode == KeyCode.Minus || guiEvent.keyCode == KeyCode.KeypadMinus)
                {
                    Vector3 position = objectToMove.transform.position - new Vector3(0, 0.2f, 0);

                    if (guiEvent.control == true)
                    {
                        position = new Vector3(position.x, Mathf.Floor(position.y), position.z);
                    }

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

                if (raycastHit.transform.name.Contains("Point") && raycastHit.collider.transform.parent.parent.GetComponent<PrefabLineCreator>() != null && raycastHit.collider.transform.parent.parent.gameObject == gameObject)
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
                mouseDown = false;
                objectToMove.GetComponent<BoxCollider>().enabled = true;
                objectToMove = null;
                PlacePrefabs();
            }
        }
    }

    public void RemovePoints(bool removeTwo = false)
    {
        if (transform.GetChild(0).childCount > 0)
        {
            Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).gameObject);

            if (removeTwo == true && transform.GetChild(0).childCount > 0)
            {
                Undo.DestroyObjectImmediate(transform.GetChild(0).GetChild(transform.GetChild(0).childCount - 1).gameObject);
            }

            PlacePrefabs();
        }
    }

    public void PlacePrefabs()
    {
        if (prefab == null)
        {
            prefab = (GameObject)settings.FindProperty("defaultPrefabLinePrefab").objectReferenceValue;
        }

        for (int i = transform.GetChild(1).childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(1).GetChild(i).gameObject);
        }

        if (fillGap == true && rotationDirection != PrefabLineCreator.RotationDirection.left && rotationDirection != PrefabLineCreator.RotationDirection.right)
        {
            rotationDirection = PrefabLineCreator.RotationDirection.left;
        }

        if (spacing == -1)
        {
            spacing = prefab.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x * 2 * xScale;
        }

        if (transform.GetChild(0).childCount > 2)
        {
            PointPackage currentPoints = CalculatePoints();
            for (int j = 0; j < currentPoints.prefabPoints.Length; j++)
            {
                float cutoff = 1;
                if (j == currentPoints.prefabPoints.Length - 1)
                {
                    cutoff = (bridgeSettings.sections + bridgeOffset) % 1.0f;

                    if (cutoff == 0)
                    {
                        cutoff = 1;
                    }
                }

                GameObject placedPrefab;

                if (j == 0 && startPrefab != null)
                {
                    placedPrefab = Instantiate(startPrefab);
                }
                else if (j == currentPoints.prefabPoints.Length - 1 && endPrefab != null)
                {
                    placedPrefab = Instantiate(endPrefab);
                }
                else
                {
                    placedPrefab = Instantiate(prefab);
                }

                placedPrefab.transform.SetParent(transform.GetChild(1));
                placedPrefab.name = "Prefab";
                placedPrefab.transform.localScale = new Vector3(xScale, yScale, zScale);

                if (placedPrefab.layer == LayerMask.NameToLayer("Default"))
                {
                    placedPrefab.layer = LayerMask.NameToLayer("Prefab Line");
                }

                if (settings.FindProperty("hideNonEditableChildren").boolValue == true)
                {
                    placedPrefab.hideFlags = HideFlags.HideInHierarchy;
                }
                else
                {
                    placedPrefab.hideFlags = HideFlags.NotEditable;
                }

                Vector3 startPoint = Misc.Lerp3CenterHeight(currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.startTimes[j]) * 3], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.startTimes[j]) * 3 + 1], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.startTimes[j]) * 3 + 2], currentPoints.startTimes[j] - Mathf.FloorToInt(currentPoints.startTimes[j]));
                Vector3 endPoint = Misc.Lerp3CenterHeight(currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.endTimes[j]) * 3], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.endTimes[j]) * 3 + 1], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.endTimes[j]) * 3 + 2], currentPoints.endTimes[j] - Mathf.FloorToInt(currentPoints.endTimes[j]));

                if (bendObjects == true)
                {
                    placedPrefab.transform.position = Misc.GetCenter(startPoint, endPoint);
                }
                else
                {
                    placedPrefab.transform.position = currentPoints.prefabPoints[j];
                }

                Vector3 left = Misc.CalculateLeft(startPoint, endPoint);
                Vector3 forward = new Vector3(left.z, 0, -left.x);

                RotatePrefabs(placedPrefab, forward, left);
                BendPrefabs(placedPrefab, currentPoints, j, cutoff);
                YModifyPrefabs(placedPrefab, currentPoints, j);
                FillGapsBetweenPrefabs(placedPrefab, j);
            }
        }
    }

    private void RotatePrefabs(GameObject placedPrefab, Vector3 forward, Vector3 left)
    {
        if (rotateAlongCurve == true)
        {
            if (rotationDirection == PrefabLineCreator.RotationDirection.forward)
            {
                placedPrefab.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(forward).eulerAngles.x, Quaternion.LookRotation(forward).eulerAngles.y + Random.Range(-yRotationRandomization / 2, yRotationRandomization / 2), Quaternion.LookRotation(forward).eulerAngles.z);
            }
            else if (rotationDirection == PrefabLineCreator.RotationDirection.backward)
            {
                placedPrefab.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(-forward).eulerAngles.x, Quaternion.LookRotation(-forward).eulerAngles.y + Random.Range(-yRotationRandomization / 2, yRotationRandomization / 2), Quaternion.LookRotation(-forward).eulerAngles.z);
            }
            else if (rotationDirection == PrefabLineCreator.RotationDirection.left)
            {
                placedPrefab.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(left).eulerAngles.x, Quaternion.LookRotation(left).eulerAngles.y + Random.Range(-yRotationRandomization / 2, yRotationRandomization / 2), Quaternion.LookRotation(left).eulerAngles.z);
            }
            else if (rotationDirection == PrefabLineCreator.RotationDirection.right)
            {
                placedPrefab.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(-left).eulerAngles.x, Quaternion.LookRotation(-left).eulerAngles.y + Random.Range(-yRotationRandomization / 2, yRotationRandomization / 2), Quaternion.LookRotation(-left).eulerAngles.z);
            }
        }
    }

    private void BendPrefabs(GameObject placedPrefab, PointPackage currentPoints, int j, float cutoff)
    {
        if (bendObjects == true)
        {
            Mesh mesh = GameObject.Instantiate(placedPrefab.GetComponent<MeshFilter>().sharedMesh);
            Vector3[] vertices = mesh.vertices;

            for (var i = 0; i < vertices.Length; i++)
            {
                float distance = Mathf.Abs(vertices[i].x - mesh.bounds.min.x);
                float distanceCovered = (distance / mesh.bounds.size.x);

                if (rotationDirection == RotationDirection.right)
                {
                    distanceCovered = 1 - distanceCovered;
                }

                float unclampedTime = (distanceCovered)/* * (1 / (cutoff - bridgeOffset)) - bridgeOffset*/;
                float currentTime = Mathf.Lerp(currentPoints.startTimes[j], currentPoints.endTimes[j], unclampedTime);

                if (bridgeMode == true)
                {
                    Vector3 lerpedPoint = Misc.Lerp3CenterHeight(currentPoints.lerpPoints[0], currentPoints.lerpPoints[1], currentPoints.lerpPoints[2], currentTime);

                    float currentWidth;
                    if (vertices[i].z > 0)
                    {
                        currentWidth = Mathf.Lerp(startWidthLeft, endWidthLeft, currentTime) + mesh.vertices[0].z - bridgeSettings.xOffset;
                    }
                    else
                    {
                        currentWidth = -Mathf.Lerp(startWidthRight, endWidthRight, currentTime) - mesh.vertices[0].z + bridgeSettings.xOffset;
                    }

                    Vector3 right = Misc.MaxVector3;
                    if (currentTime <= 0.99f)
                    {
                        right = Misc.CalculateLeft(lerpedPoint, Misc.Lerp3CenterHeight(currentPoints.lerpPoints[0], currentPoints.lerpPoints[1], currentPoints.lerpPoints[2], currentTime + 0.01f));
                    }
                    else
                    {
                        right = Misc.CalculateLeft(Misc.Lerp3CenterHeight(currentPoints.lerpPoints[0], currentPoints.lerpPoints[1], currentPoints.lerpPoints[2], currentTime - 0.01f), lerpedPoint);
                    }

                    Vector3 rotatedPoint = Quaternion.Euler(0, -(placedPrefab.transform.rotation.eulerAngles.y), 0) * (lerpedPoint - placedPrefab.transform.position + right * (vertices[i].z + currentWidth));
                    float y = rotatedPoint.y / yScale;

                    /*if (unclampedTime > 1.1f)
                    {
                        y = -vertices[i].y;
                    }*/

                    vertices[i] = new Vector3(rotatedPoint.x / xScale, y, rotatedPoint.z / zScale) + new Vector3(0, vertices[i].y, 0);
                }
                else if (distanceCovered > 0 && distanceCovered < 1)
                {
                    int pointIndex = Mathf.FloorToInt(currentTime);
                    Vector3 lerpedPoint = Misc.Lerp3CenterHeight(currentPoints.lerpPoints[pointIndex * 3], currentPoints.lerpPoints[pointIndex * 3 + 1], currentPoints.lerpPoints[pointIndex * 3 + 2], currentTime - pointIndex);
                    float y = vertices[i].y;
                    Vector3 rotatedPoint = Quaternion.Euler(0, -(placedPrefab.transform.rotation.eulerAngles.y), 0) * (lerpedPoint - placedPrefab.transform.position);
                    vertices[i] += new Vector3(rotatedPoint.x / xScale, rotatedPoint.y / yScale, rotatedPoint.z / zScale) - new Vector3(vertices[i].x, 0, 0);
                    vertices[i].y = y;
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            placedPrefab.GetComponent<MeshFilter>().sharedMesh = mesh;

            // Change collider to match
            System.Type type = placedPrefab.GetComponent<Collider>().GetType();
            if (type != null)
            {
                DestroyImmediate(placedPrefab.GetComponent<Collider>());
                placedPrefab.AddComponent(type);
            }
        }
    }

    private void YModifyPrefabs(GameObject placedPrefab, PointPackage currentPoints, int j)
    {
        if (yModification != PrefabLineCreator.YModification.none)
        {
            Vector3[] vertices = placedPrefab.GetComponent<MeshFilter>().sharedMesh.vertices;
            float startHeight = Misc.Lerp3CenterHeight(currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.startTimes[j]) * 3], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.startTimes[j]) * 3 + 1], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.startTimes[j]) * 3 + 2], currentPoints.startTimes[j] - Mathf.FloorToInt(currentPoints.startTimes[j])).y;
            float endHeight = Misc.Lerp3CenterHeight(currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.endTimes[j]) * 3], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.endTimes[j]) * 3 + 1], currentPoints.lerpPoints[Mathf.FloorToInt(currentPoints.endTimes[j]) * 3 + 2], currentPoints.endTimes[j] - Mathf.FloorToInt(currentPoints.endTimes[j])).y;

            for (var i = 0; i < vertices.Length; i++)
            {
                if (yModification == PrefabLineCreator.YModification.matchTerrain)
                {
                    RaycastHit raycastHit;
                    Vector3 vertexPosition = placedPrefab.transform.rotation * vertices[i];
                    if (Physics.Raycast(placedPrefab.transform.position + (new Vector3(vertexPosition.x * xScale, vertexPosition.y * yScale, vertexPosition.z * zScale)) + new Vector3(0, terrainCheckHeight, 0), Vector3.down, out raycastHit, 100f, ~(1 << LayerMask.NameToLayer("Ignore Mouse Ray") | 1 << LayerMask.NameToLayer("Road") | 1 << LayerMask.NameToLayer("Prefab Line"))))
                    {
                        vertices[i].y += (raycastHit.point.y - placedPrefab.transform.position.y) / yScale;
                    }
                }
                else if (yModification == PrefabLineCreator.YModification.matchCurve)
                {
                    float time = Misc.Remap(vertices[i].x, placedPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.x, placedPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.max.x, 0, 1);

                    if (rotationDirection == PrefabLineCreator.RotationDirection.right)
                    {
                        time = 1 - time;
                    }

                    vertices[i].y += (Mathf.Lerp(startHeight, endHeight, time) - placedPrefab.transform.position.y) / yScale;
                }
            }

            Mesh mesh = Instantiate(placedPrefab.GetComponent<MeshFilter>().sharedMesh);
            mesh.vertices = vertices;
            placedPrefab.GetComponent<MeshFilter>().sharedMesh = mesh;
            placedPrefab.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

            // Change collider to match
            System.Type type = placedPrefab.GetComponent<Collider>().GetType();
            if (type != null)
            {
                DestroyImmediate(placedPrefab.GetComponent<Collider>());
                placedPrefab.AddComponent(type);
            }
        }
        else if (bridgeSettings.adaptToTerrain == true)
        {
            // Change bottom vertices
            RaycastHit raycastHit;
            Vector3[] vertices = placedPrefab.GetComponent<MeshFilter>().sharedMesh.vertices;

            if (Physics.Raycast(placedPrefab.transform.position, Vector3.down, out raycastHit, 100, ~(1 << LayerMask.NameToLayer("Ignore Mouse Ray") | 1 << LayerMask.NameToLayer("Road") | 1 << LayerMask.NameToLayer("Prefab Line"))))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (Mathf.Abs(prefab.GetComponent<MeshFilter>().sharedMesh.vertices[i].y - prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.y) < 0.0001f)
                    {
                        vertices[i] = new Vector3(vertices[i].x, raycastHit.point.y - (placedPrefab.transform.position.y / yScale) - 0.2f, vertices[i].z);
                    }
                }

                Mesh mesh = Instantiate(placedPrefab.GetComponent<MeshFilter>().sharedMesh);
                mesh.vertices = vertices;
                placedPrefab.GetComponent<MeshFilter>().sharedMesh = mesh;
                placedPrefab.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

                // Change collider to match
                System.Type type = placedPrefab.GetComponent<Collider>().GetType();
                if (type != null)
                {
                    DestroyImmediate(placedPrefab.GetComponent<Collider>());
                    placedPrefab.AddComponent(type);
                }
            }
        }
    }

    private void FillGapsBetweenPrefabs(GameObject placedPrefab, int j)
    {
        if (fillGap == true && j > 0)
        {
            // Add last vertices
            List<Vector3> lastVertexPositions = new List<Vector3>();
            Vector3[] lastVertices = transform.GetChild(1).GetChild(j - 1).GetComponent<MeshFilter>().sharedMesh.vertices;
            for (int i = 0; i < lastVertices.Length; i++)
            {
                if (Mathf.Abs(lastVertices[i].x - GetMaxX()) < 0.001f)
                {
                    lastVertexPositions.Add((transform.GetChild(1).GetChild(j - 1).transform.rotation * (new Vector3(xScale * lastVertices[i].x, yScale * lastVertices[i].y, zScale * lastVertices[i].z))) + transform.GetChild(1).GetChild(j - 1).transform.position);
                }
            }

            // Move current vertices to last ones
            Mesh mesh = Instantiate(placedPrefab.GetComponent<MeshFilter>().sharedMesh);
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Mathf.Abs(vertices[i].x - GetMinX()) < 0.001f)
                {
                    Vector3 nearestVertex = Vector3.zero;
                    float currentDistance = float.MaxValue;

                    for (int k = 0; k < lastVertexPositions.Count; k++)
                    {
                        float localZ = (Quaternion.Euler(0, -(transform.GetChild(1).GetChild(j - 1).transform.rotation.eulerAngles.y), 0) * (lastVertexPositions[k] - transform.GetChild(1).GetChild(j - 1).transform.position)).z;
                        float zDifference = Mathf.Abs(localZ - (vertices[i].z * zScale));
                        if (zDifference < 0.001f)
                        {
                            if (yModification == PrefabLineCreator.YModification.none)
                            {
                                if (Mathf.Abs(lastVertexPositions[k].y - ((vertices[i].y * yScale) + placedPrefab.transform.position.y)) < currentDistance)
                                {
                                    nearestVertex = lastVertexPositions[k];
                                    currentDistance = Mathf.Abs(lastVertexPositions[k].y - ((vertices[i].y * yScale) + placedPrefab.transform.position.y));
                                }
                            }
                            else
                            {
                                float calculatedDistance = Vector3.Distance(lastVertexPositions[k], (placedPrefab.transform.rotation * (new Vector3(xScale * vertices[i].x, yScale * vertices[i].y, zScale * vertices[i].z))) + placedPrefab.transform.position);

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
                        Vector3 rotatedPoint = Quaternion.Euler(0, -placedPrefab.transform.rotation.eulerAngles.y, 0) * (nearestVertex - placedPrefab.transform.position);
                        vertices[i] = new Vector3(rotatedPoint.x * (1 / xScale), rotatedPoint.y * (1 / yScale), rotatedPoint.z * (1 / zScale));
                    }
                }
            }

            mesh.vertices = vertices;
            placedPrefab.GetComponent<MeshFilter>().sharedMesh = mesh;
            placedPrefab.GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();

            // Change collider to match
            System.Type type = placedPrefab.GetComponent<Collider>().GetType();
            if (type != null)
            {
                DestroyImmediate(placedPrefab.GetComponent<Collider>());
                placedPrefab.AddComponent(type);
            }
        }
    }

    private float GetMaxX()
    {
        if (rotationDirection == PrefabLineCreator.RotationDirection.left)
        {
            return prefab.GetComponent<MeshFilter>().sharedMesh.bounds.max.x;
        }
        else
        {
            return prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.x;
        }
    }

    private float GetMinX()
    {
        if (rotationDirection == PrefabLineCreator.RotationDirection.left)
        {
            return prefab.GetComponent<MeshFilter>().sharedMesh.bounds.min.x;
        }
        else
        {
            return prefab.GetComponent<MeshFilter>().sharedMesh.bounds.max.x;
        }
    }

    public PointPackage CalculatePoints()
    {
        List<Vector3> prefabPoints = new List<Vector3>();
        List<float> startTimes = new List<float>();
        List<float> endTimes = new List<float>();
        List<Vector3> lerpPoints = new List<Vector3>();

        Vector3 firstPoint = transform.GetChild(0).GetChild(0).position;
        Vector3 controlPoint = transform.GetChild(0).GetChild(1).position;
        Vector3 endPoint = transform.GetChild(0).GetChild(2).position;

        float distance = Misc.CalculateDistance(firstPoint, controlPoint, endPoint);
        startTimes.Add(0);
        Vector3 lastPoint = firstPoint;
        bool endPointAdded = true;

        Vector3 currentPoint = Vector3.zero;

        for (int i = 0; i < transform.GetChild(0).childCount - 2; i += 2)
        {
            distance = Misc.CalculateDistance(firstPoint, controlPoint, endPoint);
            float divisions = distance / spacing;
            divisions = Mathf.Max(2, divisions);
            float distancePerDivision = 1 / divisions;

            firstPoint = transform.GetChild(0).GetChild(i).position;
            controlPoint = transform.GetChild(0).GetChild(i + 1).position;
            endPoint = transform.GetChild(0).GetChild(i + 2).position;

            for (float t = 0; t < 1; t += distancePerDivision / pointCalculationDivisions)
            {
                currentPoint = Misc.Lerp3CenterHeight(firstPoint, controlPoint, endPoint, t);
                float currentDistance = Vector3.Distance(new Vector3(lastPoint.x, 0, lastPoint.z), new Vector3(currentPoint.x, 0, currentPoint.z));

                if ((currentDistance > spacing / 2) && endPointAdded == false)
                {
                    endTimes.Add(i / 2 + t);
                    startTimes.Add(i / 2 + t);
                    endPointAdded = true;
                }

                if (endPointAdded == true && (currentDistance > spacing || (startTimes.Count == 1 && currentDistance > spacing / 2)/* || (t + distancePerDivision > 1 && bridgeMode == true)*/))
                {
                    prefabPoints.Add(currentPoint);
                    lastPoint = currentPoint;
                    endPointAdded = false;
                }
            }

            if (endTimes.Count < prefabPoints.Count && (i + 2) >= transform.GetChild(0).childCount - 2)
            {
                endTimes.Add(i / 2 + 0.999f);
            }

            lerpPoints.Add(firstPoint);
            lerpPoints.Add(controlPoint);
            lerpPoints.Add(endPoint);
        }

        return new PointPackage(prefabPoints.ToArray(), lerpPoints.ToArray(), startTimes.ToArray(), endTimes.ToArray());
    }

    public void CopyPrefabLine(PrefabLineCreator prefabLineToCopy)
    {
        prefab = prefabLineToCopy.prefab;
        startPrefab = prefabLineToCopy.startPrefab;
        endPrefab = prefabLineToCopy.endPrefab;
        yModification = prefabLineToCopy.yModification;
        terrainCheckHeight = prefabLineToCopy.terrainCheckHeight;

        xScale = prefabLineToCopy.xScale;
        yScale = prefabLineToCopy.yScale;
        zScale = prefabLineToCopy.zScale;

        bendObjects = prefabLineToCopy.bendObjects;
        fillGap = prefabLineToCopy.fillGap;
        spacing = prefabLineToCopy.spacing;
        rotateAlongCurve = prefabLineToCopy.rotateAlongCurve;
        rotationDirection = prefabLineToCopy.rotationDirection;
        yRotationRandomization = prefabLineToCopy.yRotationRandomization;
        pointCalculationDivisions = prefabLineToCopy.pointCalculationDivisions;

        bridgeMode = prefabLineToCopy.bridgeMode;
        bridgeSettings = prefabLineToCopy.bridgeSettings;
        startWidthLeft = prefabLineToCopy.startWidthLeft;
        startWidthRight = prefabLineToCopy.startWidthRight;
        endWidthLeft = prefabLineToCopy.endWidthLeft;
        endWidthRight = prefabLineToCopy.endWidthRight;

        PlacePrefabs();
    }

    public void Flip()
    {
        if (transform.GetChild(0).childCount > 0)
        {
            if (transform.GetChild(0).childCount % 2 == 1)
            {
                Undo.RegisterFullObjectHierarchyUndo(gameObject, "Flip Prefab Line");

                // Points
                for (int i = 0; i < transform.GetChild(0).childCount; i++)
                {
                    transform.GetChild(0).GetChild(i).SetAsFirstSibling();
                }

                if (rotationDirection == RotationDirection.left)
                {
                    rotationDirection = RotationDirection.right;
                }
                else if (rotationDirection == RotationDirection.right)
                {
                    rotationDirection = RotationDirection.left;
                }
                else if (rotationDirection == RotationDirection.forward)
                {
                    rotationDirection = RotationDirection.backward;
                }
                else if (rotationDirection == RotationDirection.backward)
                {
                    rotationDirection = RotationDirection.forward;
                }

                PlacePrefabs();
            }
            else
            {
                Debug.Log("Can't flip the prefab line when it ends with a control point");
            }
        }
    }
}
#endif