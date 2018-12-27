using System.Collections;
using System.Collections.Generic;
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

        for (int i = 0; i < intersection.connections.Count; i++)
        {
            GameObject curvePoint = new GameObject("Connection Point");
            curvePoint.transform.SetParent(intersection.transform);
            curvePoint.hideFlags = HideFlags.NotEditable;
            curvePoint.layer = intersection.globalSettings.ignoreMouseRayLayer;
            curvePoint.AddComponent<BoxCollider>();
            curvePoint.GetComponent<BoxCollider>().size = new Vector3(intersection.globalSettings.pointSize, intersection.globalSettings.pointSize, intersection.globalSettings.pointSize);

            int nextIndex = i + 1;
            if (nextIndex >= intersection.connections.Count)
            {
                nextIndex = 0;
            }

            curvePoint.transform.position = Misc.GetCenter(intersection.connections[i].leftPoint.ToNormalVector3(), intersection.connections[nextIndex].rightPoint.ToNormalVector3()) - Misc.CalculateLeft(intersection.connections[i].leftPoint.ToNormalVector3(), intersection.connections[nextIndex].rightPoint.ToNormalVector3()) * intersection.connections[i].curviness;
        }
    }

    public void OnDisable()
    {
        if (intersection != null)
        {
            for (int i = intersection.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(intersection.transform.GetChild(i).gameObject);
            }
        }

        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        intersection.baseMaterial = (Material)EditorGUILayout.ObjectField("Base Material", intersection.baseMaterial, typeof(Material), false);
        intersection.overlayMaterial = (Material)EditorGUILayout.ObjectField("Overlay Material", intersection.overlayMaterial, typeof(Material), false);
        intersection.physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physics Material", intersection.physicMaterial, typeof(PhysicMaterial), false);
        intersection.yOffset = Mathf.Max(0, EditorGUILayout.FloatField("Y Offset", intersection.yOffset));

        if (EditorGUI.EndChangeCheck() == true)
        {
            intersection.GenerateMesh();
        }

        GUILayout.Label("");

        if (GUILayout.Button("Generate Intersection"))
        {
            intersection.GenerateMesh();
        }

        if (GameObject.FindObjectOfType<GlobalSettings>().debug == true)
        {
            for (int i = 0; i < intersection.connections.Count; i++)
            {
                GUILayout.Label(intersection.connections[i].curviness.ToString());
            }
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
                Handles.CylinderHandleCap(0, intersection.transform.GetChild(i).position, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
            }

            Handles.color = Color.blue;
            Handles.CylinderHandleCap(0, raycastHit.point, Quaternion.Euler(90, 0, 0), intersection.globalSettings.pointSize, EventType.Repaint);
        }

        GameObject.FindObjectOfType<RoadSystem>().ShowCreationButtons();
    }
}
