using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadCreatorButton : MonoBehaviour
{

    [MenuItem("GameObject/3D Object/Roads/Road", false, 1)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject roadSystem = GameObject.Find("Road System");

        if (roadSystem != null)
        {
            GameObject gameObject = new GameObject("Road");
            gameObject.AddComponent<RoadCreator>();

            if (menuCommand.context != null && (menuCommand.context as GameObject).GetComponent<RoadSystem>() != null)
            {
                GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            }
            else
            {
                gameObject.transform.SetParent(roadSystem.transform);
            }

            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road");
            Selection.activeObject = gameObject;
        }
        else
        {
            Debug.Log("You must create a road system before creating roads");
        }
    }
}
