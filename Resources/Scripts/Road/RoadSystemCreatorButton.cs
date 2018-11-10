using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadRootCreator : MonoBehaviour
{

    [MenuItem("GameObject/3D Object/Road System", false, 0)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        GameObject gameObject = new GameObject("Road System");
        gameObject.AddComponent<RoadSystem>();
        GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(gameObject, "Create Road System");
        Selection.activeObject = gameObject;
    }
}
