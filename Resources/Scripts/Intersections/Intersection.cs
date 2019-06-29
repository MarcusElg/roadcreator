﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Intersections")]
public class Intersection : MonoBehaviour
{

    public Material baseMaterial;
    public Material overlayMaterial;
    public PhysicMaterial physicMaterial;
    public List<IntersectionConnection> connections = new List<IntersectionConnection>();
    public float yOffset = 0.02f;
    public SerializedObject settings;
    public GameObject objectToMove;
    public bool stretchTexture = false;
    public float resolutionMultiplier = 1;

    public bool generateBridge = true;
    public BridgeSettings bridgeSettings = new BridgeSettings();

    public bool placePillars = true;
    public GameObject pillarPrefab;
    public float extraPillarHeight = 0.2f;
    public float xzPillarScale = 3;

    public bool outerExtraMeshesAsRoads = false;
    public List<ExtraMesh> extraMeshes = new List<ExtraMesh>();

    // Roundabout
    public bool roundaboutMode = false;
    public float roundaboutRadius = 5f;
    public float roundaboutWidth = 2f;
    public float maxRoundaboutRadius = 100;
    public Material connectionBaseMaterial;
    public Material connectionOverlayMaterial;
    public float textureTilingY = 1;

    public float pillarGap = 5;
    public float pillarPlacementOffset = 5;
    public float xPillarScale = 1;
    public float zPillarScale = 1;
    public PrefabLineCreator.RotationDirection pillarRotationDirection;

    public void Setup()
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();
        gameObject.GetComponent<MeshFilter>().hideFlags = HideFlags.NotEditable;
        gameObject.GetComponent<MeshCollider>().hideFlags = HideFlags.NotEditable;
        gameObject.GetComponent<MeshRenderer>().hideFlags = HideFlags.NotEditable;

        GameObject extraMeshes = new GameObject("Extra Meshes");
        extraMeshes.transform.SetParent(transform, false);

        if (settings.FindProperty("hideNonEditableChildren").boolValue == true)
        {
            extraMeshes.hideFlags = HideFlags.HideInHierarchy;
        }
        else
        {
            extraMeshes.hideFlags = HideFlags.NotEditable;
        }

        if (roundaboutMode == true)
        {
            CreateMesh();
        }
    }

    public void MovePoints(RaycastHit raycastHit, Vector3 position, Event currentEvent)
    {
        if (currentEvent.type == EventType.MouseDown)
        {
            if (currentEvent.button == 0)
            {
                if (objectToMove == null)
                {
                    bool isConnectedPoint = false;
                    if ((raycastHit.transform.name == "Start Point" || raycastHit.transform.name == "End Point") && raycastHit.transform.GetComponent<Point>().roadPoint == true)
                    {
                        for (int i = 0; i < connections.Count; i++)
                        {
                            if (connections[i].road == raycastHit.transform.GetComponent<Point>())
                            {
                                isConnectedPoint = true;
                                break;
                            }
                        }
                    }

                    if ((raycastHit.transform.name == "Connection Point" && raycastHit.transform.parent.gameObject == Selection.activeGameObject) || isConnectedPoint == true)
                    {
                        if (raycastHit.transform.GetComponent<BoxCollider>().enabled == false)
                        {
                            return;
                        }

                        objectToMove = raycastHit.transform.gameObject;
                        objectToMove.GetComponent<BoxCollider>().enabled = false;
                    }
                }
            }
            else if (currentEvent.button == 1)
            {
                // Reset Point
                if (raycastHit.transform.name == "Connection Point")
                {
                    int currentIndex = raycastHit.transform.GetSiblingIndex() - 1;
                    int nextIndex = raycastHit.transform.GetSiblingIndex();

                    if (nextIndex >= connections.Count)
                    {
                        nextIndex = 0;
                    }

                    if (roundaboutMode == true)
                    {
                        if (currentIndex % 2 == 0)
                        {
                            connections[currentIndex / 2].curvePoint = connections[currentIndex / 2].defaultCurvePoint;
                        }
                        else
                        {
                            connections[(currentIndex - 1) / 2].curvePoint2 = connections[(currentIndex - 1) / 2].defaultCurvePoint2;
                        }
                    }
                    else
                    {
                        connections[currentIndex].curvePoint = Misc.GetCenter(connections[currentIndex].leftPoint, connections[nextIndex].rightPoint);
                    }

                    CreateCurvePoints();
                    CreateMesh();
                }
            }
        }
        else if (currentEvent.type == EventType.MouseDrag && objectToMove != null)
        {
            Undo.RecordObject(objectToMove.transform, "Moved Point");
            objectToMove.transform.position = position;
        }
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && objectToMove != null)
        {
            DropMovingPoint();
        }
    }

    private void DropMovingPoint()
    {
        objectToMove.GetComponent<BoxCollider>().enabled = true;

        if (objectToMove.transform.name == "Connection Point")
        {
            int nextIndex = objectToMove.transform.GetSiblingIndex();
            if (nextIndex >= connections.Count)
            {
                nextIndex = 0;
            }

            if (roundaboutMode == true)
            {
                int index = objectToMove.transform.GetSiblingIndex() - 1;

                if (index % 2 == 0)
                {
                    connections[index / 2].curvePoint = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
                }
                else
                {
                    connections[(index - 1) / 2].curvePoint2 = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
                }
            }
            else
            {
                connections[objectToMove.transform.GetSiblingIndex() - 1].curvePoint = new Vector3(objectToMove.transform.position.x, transform.position.y, objectToMove.transform.position.z);
            }

            objectToMove = null;
            CreateMesh();
            CreateCurvePoints();
        }
        else
        {
            if (objectToMove.transform.parent.parent.GetComponent<RoadSegment>().curved == false)
            {
                objectToMove.transform.parent.GetChild(1).position = Misc.GetCenter(objectToMove.transform.parent.GetChild(0).position, objectToMove.transform.parent.GetChild(2).position);
            }

            CreateMesh();
            objectToMove.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh();

            if (objectToMove.transform.name == "Start Point")
            {
                objectToMove.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().UpdateStartConnectionData();
            }
            else
            {
                objectToMove.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().UpdateEndConnectionData();
            }

            objectToMove = null;
            Roundabout.UpdateMaxRadius(this);
        }
    }

    public void CreateMesh(bool fromRoad = false)
    {
        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].road == null)
            {
                connections.RemoveAt(i);
            }
        }

        if (connections.Count < 2 && roundaboutMode == false)
        {
            RemoveIntersection();
        }
        else
        {
            CheckMaterialsAndPrefabs();

            if (roundaboutMode == true)
            {
                Roundabout.GenerateRoundabout(this);
            }
            else
            {
                GenerateNormalMesh();
            }
        }

        if (fromRoad == false)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().CreateMesh(true);
            }
        }
    }

    private void GenerateNormalMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> firstVertexIndexes = new List<int>();
        float[] totalLengths = new float[connections.Count];
        float[] exactLengths = new float[connections.Count];
        int vertexIndex = 0;
        Vector3 lastVertexPosition = Misc.MaxVector3;

        for (int i = 0; i < connections.Count; i++)
        {
            Vector3 firstPoint = connections[i].leftPoint;
            Vector3 firstCenterPoint = connections[i].lastPoint;
            Vector3 nextPoint;
            Vector3 nextCenterPoint;
            totalLengths[i] = connections[i].length;
            firstVertexIndexes.Add(vertexIndex);

            if (i == connections.Count - 1)
            {
                nextPoint = connections[0].rightPoint;
                nextCenterPoint = connections[0].lastPoint;
                totalLengths[i] += connections[0].length;
            }
            else
            {
                nextPoint = connections[i + 1].rightPoint;
                nextCenterPoint = connections[i + 1].lastPoint;
                totalLengths[i] += connections[i + 1].length;
            }

            if (connections[i].curvePoint == null)
            {
                return;
            }

            float segments = totalLengths[i] * settings.FindProperty("resolution").floatValue * resolutionMultiplier * 5;
            segments = Mathf.Max(3, segments);
            float distancePerSegment = 1f / segments;

            for (float t = 0; t <= 1; t += 0.1f)
            {
                Vector3 pos = Misc.Lerp3(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f), Vector3.one, t);
            }

            for (float t = 0; t <= 1 + distancePerSegment; t += distancePerSegment)
            {
                float modifiedT = t;
                if (Mathf.Abs(0.5f - t) < distancePerSegment && t > 0.5f)
                {
                    modifiedT = 0.5f;
                }

                if (t > 1)
                {
                    modifiedT = 1;
                }

                vertices.Add(Misc.Lerp3CenterHeight(firstPoint, connections[i].curvePoint, nextPoint, modifiedT) + new Vector3(0, yOffset, 0) - transform.position);

                if (t > 0)
                {
                    exactLengths[i] += Vector3.Distance(lastVertexPosition, vertices[vertices.Count - 1]);
                    lastVertexPosition = vertices[vertices.Count - 1];
                }
                else
                {
                    lastVertexPosition = vertices[vertices.Count - 1];
                }

                if (modifiedT < 0.5f)
                {
                    Vector3 point = Vector3.Lerp(firstCenterPoint, transform.position, modifiedT * 2) - transform.position;
                    point.y = Mathf.Lerp(firstPoint.y, nextPoint.y, modifiedT) - transform.position.y + yOffset;
                    vertices.Add(point);
                }
                else
                {
                    Vector3 point = Vector3.Lerp(transform.position, nextCenterPoint, 2 * (modifiedT - 0.5f)) - transform.position;
                    point.y = Mathf.Lerp(firstPoint.y, nextPoint.y, modifiedT) - transform.position.y + yOffset;
                    vertices.Add(point);
                }

                uvs.Add(new Vector2(0, modifiedT));

                if (stretchTexture == true)
                {
                    uvs.Add(new Vector2(1, modifiedT));
                }
                else
                {
                    uvs.Add(new Vector2(Vector3.Distance(vertices[vertices.Count - 1], vertices[vertices.Count - 2]), modifiedT));
                }

                if (t < 1)
                {
                    triangles = AddTriangles(triangles, vertexIndex);
                }

                vertexIndex += 2;
            }

            SetupMesh(vertices, triangles, uvs);
        }

        float[] startWidths = new float[firstVertexIndexes.Count];
        float[] endWidths = new float[firstVertexIndexes.Count];
        float[] heights = new float[firstVertexIndexes.Count];

        GenerateIntersectionExtraMeshes(firstVertexIndexes, vertices, exactLengths, totalLengths, ref startWidths, ref endWidths, ref heights);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name == "Bridge")
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
                break;
            }
        }

        if (generateBridge == true)
        {
            BridgeGeneration.GenerateSimpleBridgeIntersection(GetComponent<MeshFilter>().sharedMesh.vertices, this, bridgeSettings.bridgeMaterials, startWidths, endWidths, firstVertexIndexes.ToArray());
        }

        CreateCurvePoints();
    }

    private void SetupMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMaterial = physicMaterial;

        if (overlayMaterial == null)
        {
            GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial };
        }
        else
        {
            GetComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, overlayMaterial };
        }
    }

    private void GenerateIntersectionExtraMeshes(List<int> firstVertexIndexes, List<Vector3> vertices, float[] exactLengths, float[] totalLengths, ref float[] startWidths, ref float[] endWidths, ref float[] heights)
    {
        for (int i = 0; i < extraMeshes.Count; i++)
        {
            int endVertexIndex;
            float currentLength = 0f;
            Vector3 lastPosition = vertices[firstVertexIndexes[extraMeshes[i].index] + 1];

            if (extraMeshes[i].index < firstVertexIndexes.Count - 1)
            {
                endVertexIndex = firstVertexIndexes[extraMeshes[i].index + 1];
            }
            else
            {
                endVertexIndex = vertices.Count;
            }

            List<Vector3> extraMeshVertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            int vertexIndex = 0;

            for (int j = firstVertexIndexes[extraMeshes[i].index]; j < endVertexIndex; j += 2)
            {
                currentLength += Vector3.Distance(lastPosition, vertices[j + 1]);

                Vector3 forward = (vertices[j + 1] - vertices[j]).normalized;
                extraMeshVertices.Add(vertices[j] - forward * (Mathf.Lerp(extraMeshes[i].startWidth, extraMeshes[i].endWidth, currentLength / totalLengths[extraMeshes[i].index]) + Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLengths[extraMeshes[i].index])));
                extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, extraMeshes[i].yOffset + heights[extraMeshes[i].index], 0);
                extraMeshVertices.Add(vertices[j] - forward * Mathf.Lerp(startWidths[extraMeshes[i].index], endWidths[extraMeshes[i].index], currentLength / totalLengths[extraMeshes[i].index]));
                extraMeshVertices[extraMeshVertices.Count - 1] += new Vector3(0, heights[extraMeshes[i].index], 0);

                uvs.Add(new Vector2(0, (currentLength / exactLengths[extraMeshes[i].index])));
                uvs.Add(new Vector2(1, (currentLength / exactLengths[extraMeshes[i].index])));

                if (j < endVertexIndex - 2)
                {
                    triangles = AddTriangles(triangles, vertexIndex);
                    vertexIndex += 2;
                }

                lastPosition = vertices[j + 1];
            }

            Mesh mesh = new Mesh();
            mesh.vertices = extraMeshVertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            vertexIndex = 0;

            transform.GetChild(0).GetChild(i).GetComponent<MeshFilter>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMesh = mesh;
            transform.GetChild(0).GetChild(i).GetComponent<MeshCollider>().sharedMaterial = extraMeshes[i].physicMaterial;

            if (extraMeshes[i].overlayMaterial == null)
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].baseMaterial };
            }
            else
            {
                transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = new Material[] { extraMeshes[i].baseMaterial, extraMeshes[i].overlayMaterial };
            }

            startWidths[extraMeshes[i].index] += extraMeshes[i].startWidth;
            endWidths[extraMeshes[i].index] += extraMeshes[i].endWidth;
            heights[extraMeshes[i].index] += extraMeshes[i].yOffset;
        }
    }

    private void RemoveIntersection()
    {
        RoadCreator[] roads = GameObject.FindObjectsOfType<RoadCreator>();

        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i].startIntersection == this)
            {
                Undo.RecordObject(roads[i], "Remove Intersection");
                roads[i].startIntersection = null;
                roads[i].startIntersectionConnection = null;
            }
            else if (roads[i].endIntersection == this)
            {
                Undo.RecordObject(roads[i], "Remove Intersection");
                roads[i].endIntersection = null;
                roads[i].endIntersectionConnection = null;
            }
        }

        Undo.DestroyObjectImmediate(gameObject);
    }

    private void CheckMaterialsAndPrefabs()
    {
        if (baseMaterial == null)
        {
            baseMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
        }

        if (connectionBaseMaterial == null)
        {
            connectionBaseMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
        }

        if (bridgeSettings.bridgeMaterials == null || bridgeSettings.bridgeMaterials.Length == 0 || bridgeSettings.bridgeMaterials[0] == null)
        {
            Material[] materials = new Material[settings.FindProperty("defaultSimpleBridgeMaterials").arraySize];
            for (int i = 0; i < settings.FindProperty("defaultSimpleBridgeMaterials").arraySize; i++)
            {
                materials[i] = (Material)settings.FindProperty("defaultSimpleBridgeMaterials").GetArrayElementAtIndex(i).objectReferenceValue;
            }

            bridgeSettings.bridgeMaterials = materials;
        }

        for (int i = 0; i < extraMeshes.Count; i++)
        {
            if (extraMeshes[i].baseMaterial == null)
            {
                extraMeshes[i].baseMaterial = (Material)settings.FindProperty("defaultBaseMaterial").objectReferenceValue;
            }
        }

        for (int i = 0; i < extraMeshes.Count; i++)
        {
            if (extraMeshes[i].overlayMaterial == null)
            {
                extraMeshes[i].overlayMaterial = (Material)settings.FindProperty("defaultExtraMeshOverlayMaterial").objectReferenceValue;
            }
        }

        if (pillarPrefab == null || pillarPrefab.GetComponent<MeshFilter>() == null)
        {
            pillarPrefab = (GameObject)settings.FindProperty("defaultBridgePillarPrefab").objectReferenceValue;
        }
    }

    public void CreateCurvePoints()
    {
        RemoveCurvePoints();

        for (int i = 0; i < connections.Count; i++)
        {
            CreateCurvePoint(new Vector3(connections[i].curvePoint.x, yOffset + transform.position.y, connections[i].curvePoint.z));

            if (roundaboutMode == true)
            {
                CreateCurvePoint(new Vector3(connections[i].curvePoint2.x, yOffset + transform.position.y, connections[i].curvePoint2.z));
            }
        }

        if (transform.Find("Bridge") != null)
        {
            transform.Find("Bridge").SetAsLastSibling();
        }
    }

    private void CreateCurvePoint(Vector3 position)
    {
        GameObject curvePoint = null;
        curvePoint = new GameObject("Connection Point");
        curvePoint.transform.SetParent(transform);
        curvePoint.layer = LayerMask.NameToLayer("Ignore Mouse Ray");
        curvePoint.AddComponent<BoxCollider>();
        curvePoint.GetComponent<BoxCollider>().size = new Vector3(settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue, settings.FindProperty("pointSize").floatValue);
        curvePoint.transform.position = position;

        if (settings.FindProperty("hideNonEditableChildren").boolValue == true)
        {
            curvePoint.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    public void ResetCurvePointPositions()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= connections.Count)
            {
                nextIndex = 0;
            }

            if (roundaboutMode == true)
            {
                ResetConnectionCurvePoints(connections[i]);
            }
            else
            {
                connections[i].curvePoint = Misc.GetCenter(connections[i].leftPoint, connections[nextIndex].rightPoint);
            }
        }
    }

    public void ResetConnectionCurvePoints(IntersectionConnection intersectionConnection)
    {
        intersectionConnection.curvePoint = intersectionConnection.defaultCurvePoint;
        intersectionConnection.curvePoint2 = intersectionConnection.defaultCurvePoint2;
    }

    public void RemoveCurvePoints()
    {
        if (gameObject != null)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name == "Connection Point")
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }
    }

    private List<int> AddTriangles(List<int> triangles, int vertexIndex)
    {
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);

        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);

        return triangles;
    }

    public void FixConnectionReferences()
    {
        for (int i = 0; i < connections.Count; i++)
        {
            if (connections[i].road != null)
            {
                if (connections[i].road.Equals(connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection.road))
                {
                    connections[i] = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().endIntersectionConnection;
                }
                else
                {
                    connections[i] = connections[i].road.transform.parent.parent.parent.parent.GetComponent<RoadCreator>().startIntersectionConnection;
                }
            }
        }
    }

    public void CreateExtraMesh()
    {
        GameObject extraMesh = new GameObject("Extra Mesh");
        extraMesh.AddComponent<MeshFilter>();
        extraMesh.AddComponent<MeshRenderer>();
        extraMesh.AddComponent<MeshCollider>();
        extraMesh.transform.SetParent(transform.GetChild(0));
        extraMesh.transform.localPosition = Vector3.zero;
        extraMesh.layer = LayerMask.NameToLayer("Road");
        extraMesh.hideFlags = HideFlags.NotEditable;
    }

}
