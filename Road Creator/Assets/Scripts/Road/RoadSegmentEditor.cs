using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSegment))]
public class RoadSegmentEditor : Editor {

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
        {
            ((RoadSegment)target).transform.parent.parent.GetComponent<RoadCreator>().roadEditor.UpdateMesh();
        }
    }

}
