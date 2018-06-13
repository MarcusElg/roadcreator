using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSystem))]
public class RoadSystemEditor : Editor {

    public RoadSystem roadSystem;

    private void OnEnable()
    {
        roadSystem = (RoadSystem)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Create road"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadCreator>();
            gameObject.name = "Road";
            gameObject.transform.parent = roadSystem.transform;
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road");
        }

        if (GUILayout.Button("Create intersection"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<SquareIntersection>();
            gameObject.name = "Square Intersection";
            gameObject.transform.parent = roadSystem.transform;
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Intersection");

            RaycastHit raycastHit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(new Vector2(SceneView.lastActiveSceneView.position.x, SceneView.lastActiveSceneView.position.y)).origin, SceneView.lastActiveSceneView.rotation * Vector3.forward, out raycastHit, 100))
            {
                gameObject.transform.position = raycastHit.point;
            }
        }

        if (GUILayout.Button("Create line of prefabs"))
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<PrefabLineCreator>();
            gameObject.name = "Prefab Line";
            gameObject.transform.parent = roadSystem.transform;
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Prefab Line");
        }
    }

}
