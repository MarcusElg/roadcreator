using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TriangleIntersectionCreatorButton : MonoBehaviour
{

    [MenuItem("GameObject/3D Object/Roads/Triangle Intersection", false, 4)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject roadSystem = GameObject.Find("Road System");

        if (roadSystem != null)
        {
            GameObject gameObject = new GameObject("Triangle Intersection");
            gameObject.AddComponent<TriangleIntersection>();

            if (menuCommand.context != null && (menuCommand.context as GameObject).GetComponent<RoadSystem>() != null)
            {
                GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            }
            else
            {
                gameObject.transform.SetParent(roadSystem.transform);
            }

            Undo.RegisterCreatedObjectUndo(gameObject, "Create Triangle Intersection");
            Selection.activeObject = gameObject;
        }
        else
        {
            Debug.Log("You must create a road system before creating triangle intersections");
        }
    }

}
