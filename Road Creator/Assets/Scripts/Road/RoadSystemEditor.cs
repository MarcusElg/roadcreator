using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSystem))]
public class RoadSystemEditor : Editor
{

    public RoadSystem roadSystem;
    GameObject lastPlacedObject;

    private void OnEnable()
    {
        roadSystem = (RoadSystem)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Create Road"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadCreator>();
            gameObject.name = "Road";
            gameObject.transform.SetParent(roadSystem.transform);
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road");
        }
        GUILayout.Label("");

        roadSystem.intersectionType = (RoadSystem.IntersectionType)EditorGUILayout.EnumPopup("Intersection Type", (RoadSystem.IntersectionType)Enum.GetValues(typeof(RoadSystem.IntersectionType)).GetValue((int)roadSystem.intersectionType));

        if (GUILayout.Button("Create Intersection"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<SquareIntersection>();
            gameObject.name = "Square Intersection";
            gameObject.transform.SetParent(roadSystem.transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Intersection");

            lastPlacedObject = gameObject;
        }

        GUILayout.Label("");

        if (GUILayout.Button("Create Line Of Prefabs"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<PrefabLineCreator>();
            gameObject.name = "Prefab Line";
            gameObject.transform.SetParent(roadSystem.transform);
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Prefab Line");
        }

        GUILayout.Label("");

        if (GUILayout.Button("Reset"))
        {
            for (int i = roadSystem.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roadSystem.transform.GetChild(i).gameObject);
            }
        }
    }

    private void OnSceneGUI()
    {
        // Fix position
        if (lastPlacedObject != null && lastPlacedObject.transform.position == Vector3.zero)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(Camera.current.transform.position + Vector3.up, Vector3.Scale(Vector3.down, Camera.current.transform.rotation.eulerAngles), out raycastHit, 100))
                {
                    lastPlacedObject.transform.position = raycastHit.point;
                    Selection.activeGameObject = lastPlacedObject;
                    lastPlacedObject = null;
                }
            }
            else
            {
                Debug.Log("Last active scene view is null, placing object at 0, 0, 0");
            }
        }
    }

}
