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

        if (GameObject.FindObjectOfType<GlobalSettings>() == null)
        {
            new GameObject("Global Settings").AddComponent<GlobalSettings>();
        }
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

            if (roadSystem.intersectionType == RoadSystem.IntersectionType.square)
            {
                gameObject.AddComponent<SquareIntersection>();
                gameObject.name = "Square Intersection";
            }
            else if (roadSystem.intersectionType == RoadSystem.IntersectionType.triangle)
            {
                gameObject.AddComponent<TriangleIntersection>();
                gameObject.name = "Triangle Intersection";
            }
            else if (roadSystem.intersectionType == RoadSystem.IntersectionType.diamond)
            {
                gameObject.AddComponent<DiamondIntersection>();
                gameObject.name = "Diamond Intersection";
            }
            else if (roadSystem.intersectionType == RoadSystem.IntersectionType.roundabout)
            {
                gameObject.AddComponent<Roundabout>();
                gameObject.name = "Roundabout";
            }

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

        if (GUILayout.Button("Create Road Splitter"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadSplitter>();
            gameObject.name = "Road Splitter";
            gameObject.transform.SetParent(roadSystem.transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Splitter");

            lastPlacedObject = gameObject;
        }

        if (GUILayout.Button("Create Road Transition"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadTransition>();
            gameObject.name = "Road Transition";
            gameObject.transform.SetParent(roadSystem.transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Transition");

            lastPlacedObject = gameObject;
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
            RaycastHit raycastHit;
            Ray ray = Camera.current.ScreenPointToRay(new Vector3(Input.mousePosition.x + Camera.current.pixelWidth / 2, Input.mousePosition.y + Camera.current.pixelHeight / 2, Input.mousePosition.z));
            if (Physics.Raycast(ray, out raycastHit, 100))
            {
                lastPlacedObject.transform.position = raycastHit.point;
                Selection.activeGameObject = lastPlacedObject;
                lastPlacedObject = null;
            }
        }
    }

}
