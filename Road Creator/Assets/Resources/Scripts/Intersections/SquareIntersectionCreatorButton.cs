using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SquareIntersectionCreatorButton : MonoBehaviour
{

    [MenuItem("GameObject/3D Object/Roads/Square Intersection", false, 3)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject roadSystem = GameObject.Find("Road System");

        if (roadSystem != null)
        {
            GameObject gameObject = new GameObject("Square Intersection");
            gameObject.AddComponent<SquareIntersection>();

            if (menuCommand.context != null && (menuCommand.context as GameObject).GetComponent<RoadSystem>() != null)
            {
                GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            }
            else
            {
                gameObject.transform.SetParent(roadSystem.transform);
            }

            Undo.RegisterCreatedObjectUndo(gameObject, "Create Square Intersection");
            Selection.activeObject = gameObject;
        }
        else
        {
            Debug.Log("You must create a road system before creating square intersections");
        }
    }
}
