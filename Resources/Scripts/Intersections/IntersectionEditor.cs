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

        if (intersection.globalSettings == null)
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

        GUILayout.Label("");
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

            GUILayout.Label("");
            GUILayout.Label("Pillar Placement", guiStyle);

            intersection.placePillars = EditorGUILayout.Toggle("Place Pillar", intersection.placePillars);
            if (intersection.placePillars == true)
            {
                intersection.pillarPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", intersection.pillarPrefab, typeof(GameObject), false);
                intersection.extraPillarHeight = EditorGUILayout.FloatField("Extra Pillar Height", intersection.extraPillarHeight);
                intersection.xzPillarScale = EditorGUILayout.FloatField("XZ Pillar Scale", intersection.xzPillarScale);
            }
        }

        if (EditorGUI.EndChangeCheck() == true)
        {
            intersection.CreateMesh();
        }

        GUILayout.Label("");

        if (GUILayout.Button("Generate Intersection"))
        {
            intersection.CreateMesh();
        }
    }

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
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

            Handles.color = Color.green;
            for (int i = 0; i < intersection.transform.childCount; i++)
            {
                if (intersection.transform.GetChild(i).name != "Bridge")
                {
                    Handles.CylinderHandleCap(0, intersection.transform.GetChild(i).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
                }
            }

            Handles.color = Color.blue;
            Handles.CylinderHandleCap(0, raycastHit.point, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
        }

        GameObject.FindObjectOfType<RoadSystem>().ShowCreationButtons();
    }
}
