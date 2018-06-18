using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadSplitterCreator : MonoBehaviour {

    [MenuItem("GameObject/3D Object/Roads/Road Splitter", false, 6)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject roadSystem = GameObject.Find("Road System");

        if (roadSystem != null)
        {
            GameObject gameObject = new GameObject("Road Splitter");
            gameObject.AddComponent<RoadSplitter>();

            if (menuCommand.context != null && (menuCommand.context as GameObject).GetComponent<RoadSystem>() != null)
            {
                GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
            }
            else
            {
                gameObject.transform.SetParent(roadSystem.transform);
            }

            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Splitter");
            Selection.activeObject = gameObject;
        }
        else
        {
            Debug.Log("You must create a road system before creating road splitters");
        }
    }
}
