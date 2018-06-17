using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabLineCreatorButton : MonoBehaviour
{

    [MenuItem("GameObject/3D Object/Roads/Prefab Line", false, 2)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject roadSystem = GameObject.Find("Road System");

        if (roadSystem != null)
        {
            GameObject gameObject = new GameObject("Prefab Line");
            gameObject.AddComponent<PrefabLineCreator>();

            if (menuCommand.context != null && (menuCommand.context as GameObject).GetComponent<RoadSystem>() != null)
            {
                GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            }
            else
            {
                gameObject.transform.SetParent(roadSystem.transform);
            }

            Undo.RegisterCreatedObjectUndo(gameObject, "Create Prefab Line");
            Selection.activeObject = gameObject;
        }
        else
        {
            Debug.Log("You must create a road system before creating prefab lines");
        }
    }
}
