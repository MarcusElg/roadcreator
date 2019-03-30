using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Intersection))]
public class IntersectionEditor : Editor
{

    Intersection intersection;
    Tool lastTool;

    public void OnEnable()
    {
        if (intersection == null)
        {
            intersection = (Intersection)target;
        }

        if (GameObject.FindObjectOfType<GlobalSettings>() == null)
        {
            intersection.globalSettings = new GameObject("Global Settings").AddComponent<GlobalSettings>();
            ((Intersection)target).transform.parent.parent.GetComponent<RoadCreator>().globalSettings = intersection.globalSettings;
        }
        else if (intersection.globalSettings == null)
        {
            intersection.globalSettings = GameObject.FindObjectOfType<GlobalSettings>();
        }

        lastTool = Tools.current;
        Tools.current = Tool.None;

        intersection.CreateCurvePoints();
    }

    public void OnDisable()
    {
        intersection.RemoveCurvePoints();

        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        intersection.baseMaterial = (Material)EditorGUILayout.ObjectField("Base Material", intersection.baseMaterial, typeof(Material), false);
        intersection.overlayMaterial = (Material)EditorGUILayout.ObjectField("Overlay Material", intersection.overlayMaterial, typeof(Material), false);
        intersection.physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physics Material", intersection.physicMaterial, typeof(PhysicMaterial), false);
        intersection.yOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", intersection.yOffset));
        intersection.stretchTexture = GUILayout.Toggle(intersection.stretchTexture, "Stretch Texture");

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;

        GUILayout.Space(20);
        GUILayout.Label("Bridge", guiStyle);
        intersection.bridgeGenerator = (RoadSegment.BridgeGenerator)EditorGUILayout.EnumPopup("Generator", intersection.bridgeGenerator);

        if (intersection.bridgeGenerator != RoadSegment.BridgeGenerator.none)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bridgeMaterials"), true);
        }

        if (intersection.bridgeGenerator == RoadSegment.BridgeGenerator.simple)
        {
            intersection.yOffsetFirstStep = Mathf.Clamp(EditorGUILayout.FloatField("Y Offset First Step", intersection.yOffsetFirstStep), 0, 2);
            intersection.yOffsetSecondStep = Mathf.Clamp(EditorGUILayout.FloatField("Y Offset Second Step", intersection.yOffsetSecondStep), 0, 2);
            intersection.widthPercentageFirstStep = Mathf.Clamp(EditorGUILayout.FloatField("Width Percentage First Step", intersection.widthPercentageFirstStep), 0, 1);
            intersection.widthPercentageSecondStep = Mathf.Clamp(EditorGUILayout.FloatField("Width Percentage Second Step", intersection.widthPercentageSecondStep), 0, 1);
            intersection.extraWidth = Mathf.Clamp(EditorGUILayout.FloatField("Extra Width", intersection.extraWidth), 0, 1);

            GUILayout.Space(20);
            GUILayout.Label("Pillar Placement", guiStyle);

            intersection.placePillars = EditorGUILayout.Toggle("Place Pillar", intersection.placePillars);
            if (intersection.placePillars == true)
            {
                intersection.pillarPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", intersection.pillarPrefab, typeof(GameObject), false);
                intersection.extraPillarHeight = EditorGUILayout.FloatField("Extra Pillar Height", intersection.extraPillarHeight);
                intersection.xzPillarScale = EditorGUILayout.FloatField("XZ Pillar Scale", intersection.xzPillarScale);
            }
        }
        GUILayout.Space(20);
        GUILayout.Label("Extra Meshes", guiStyle);
        for (int i = 0; i < intersection.extraMeshes.Count; i++)
        {
            intersection.extraMeshes[i].open = EditorGUILayout.Foldout(intersection.extraMeshes[i].open, "Extra Mesh " + i);
            if (intersection.extraMeshes[i].open == true)
            {
                intersection.extraMeshes[i].index = Mathf.Clamp(EditorGUILayout.IntField("Index", intersection.extraMeshes[i].index), 0, intersection.connections.Count - 1);
                intersection.extraMeshes[i].material = (Material)EditorGUILayout.ObjectField("Material", intersection.extraMeshes[i].material, typeof(Material), false);
                intersection.extraMeshes[i].physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physic Material", intersection.extraMeshes[i].physicMaterial, typeof(PhysicMaterial), false);
                intersection.extraMeshes[i].startWidth = Mathf.Max(EditorGUILayout.FloatField("Start Width", intersection.extraMeshes[i].startWidth), 0);
                intersection.extraMeshes[i].endWidth = Mathf.Max(EditorGUILayout.FloatField("End Width", intersection.extraMeshes[i].endWidth), 0);
                intersection.extraMeshes[i].yOffset = EditorGUILayout.FloatField("Y Offset", intersection.extraMeshes[i].yOffset);

                if (GUILayout.Button("Remove Extra Mesh") == true && intersection.transform.GetChild(0).childCount > 0)
                {
                    intersection.extraMeshes.RemoveAt(i);

                    for (int j = 0; j < targets.Length; j++)
                    {
                        DestroyImmediate(intersection.transform.GetChild(0).GetChild(i).gameObject);
                    }
                }
            }
        }

        if (GUILayout.Button("Add Extra Mesh"))
        {
            intersection.extraMeshes.Add(new ExtraMesh(true, 0, Resources.Load("Materials/Low Poly/Asphalt") as Material, null, 1, 1, 0));

            GameObject extraMesh = new GameObject("Extra Mesh");
            extraMesh.AddComponent<MeshFilter>();
            extraMesh.AddComponent<MeshRenderer>();
            extraMesh.AddComponent<MeshCollider>();
            extraMesh.transform.SetParent(intersection.transform.GetChild(0));
            extraMesh.transform.localPosition = Vector3.zero;
            extraMesh.layer = intersection.globalSettings.roadLayer;
            extraMesh.hideFlags = HideFlags.NotEditable;
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            intersection.CreateMesh();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Reset Curve Points"))
        {
            intersection.ResetCurvePointPositions();
            intersection.CreateCurvePoints();
            intersection.CreateMesh();
        }

        if (GUILayout.Button("Generate Intersection"))
        {
            intersection.CreateMesh();
        }
    }

    private void OnSceneGUI()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit, 100f))
        {
            Vector3 hitPosition = raycastHit.point;
            if (Event.current.control == true)
            {
                hitPosition = Misc.Round(hitPosition);
            }

            intersection.MovePoints(raycastHit, hitPosition, Event.current);

            Handles.color = intersection.globalSettings.intersectionColour;
            for (int i = 1; i < intersection.transform.childCount; i++)
            {
                if (intersection.transform.GetChild(i).name != "Bridge")
                {
                    Handles.CylinderHandleCap(0, intersection.transform.GetChild(i).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
                }
            }

            Handles.color = intersection.globalSettings.cursorColour;
            Handles.CylinderHandleCap(0, raycastHit.point, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
        }

        GameObject.FindObjectOfType<RoadSystem>().ShowCreationButtons();
    }
}
