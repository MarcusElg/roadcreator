using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Intersection))]
public class IntersectionEditor : Editor
{

    Intersection intersection;

    public void OnEnable()
    {
        if (intersection == null)
        {
            intersection = (Intersection)target;
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        intersection.material = (Material)EditorGUILayout.ObjectField("Material", intersection.material, typeof(Material), false);
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
    }
}
