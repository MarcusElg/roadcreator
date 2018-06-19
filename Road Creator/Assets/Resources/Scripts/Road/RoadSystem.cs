using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadSystem : MonoBehaviour {

    Texture createRoadTexture;
    Texture createPrefabLine;
    Texture createSquareIntersection;
    Texture createTriangleIntersection;
    Texture createDiamondIntersection;
    Texture createRoundabout;
    Texture createRoadSplitter;
    Texture createRoadTransition;

    public void ShowCreationButtons ()
    {
        SceneView.lastActiveSceneView.Focus();

        if (createRoadTexture == null)
        {
            createRoadTexture = Resources.Load("Textures/Ui/createroad") as Texture;
            createPrefabLine = Resources.Load("Textures/Ui/createprefabline") as Texture;
            createSquareIntersection = Resources.Load("Textures/Ui/createsquareintersection") as Texture;
            createTriangleIntersection = Resources.Load("Textures/Ui/createtriangleintersection") as Texture;
            createDiamondIntersection = Resources.Load("Textures/Ui/creatediamondintersection") as Texture;
            createRoundabout = Resources.Load("Textures/Ui/createroundabout") as Texture;
            createRoadSplitter = Resources.Load("Textures/Ui/createroadsplitter") as Texture;
            createRoadTransition = Resources.Load("Textures/Ui/createroadtransition") as Texture;
        }

        Rect windowRect = new Rect(SceneView.lastActiveSceneView.position.width - 295, SceneView.lastActiveSceneView.position.height - 50, 285, 40);
        windowRect = GUILayout.Window(0, windowRect, DrawWindow, "");

        // Detect click
        if (ClickedButton(1) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadCreator>();
            gameObject.name = "Road";
            gameObject.transform.SetParent(transform);
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road");
        }
        else if (ClickedButton(2) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<PrefabLineCreator>();
            gameObject.name = "Prefab Line";
            gameObject.transform.SetParent(transform);
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Prefab Line");
        }
        else if (ClickedButton(3) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<SquareIntersection>();
            gameObject.name = "Square Intersection";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Square Intersection");

            SetPosition(gameObject);
        }
        else if (ClickedButton(4) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<TriangleIntersection>();
            gameObject.name = "Triangle Intersection";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Triangle Intersection");

            SetPosition(gameObject);
        }
        else if (ClickedButton(5) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<DiamondIntersection>();
            gameObject.name = "Diamond Intersection";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Diamond Intersection");

            SetPosition(gameObject);
        }
        else if (ClickedButton(6) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<Roundabout>();
            gameObject.name = "Roundabout";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Roundabout");

            SetPosition(gameObject);
        }
        else if (ClickedButton(7) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadTransition>();
            gameObject.name = "Road Transition";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Transition");

            SetPosition(gameObject);
        }
        else if (ClickedButton(8) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadSplitter>();
            gameObject.name = "Road Splitter";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Splitter");

            SetPosition(gameObject);
        }
    }

    void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();

        GUI.DrawTexture(new Rect(5, 5, 30, 30), createRoadTexture);
        GUI.DrawTexture(new Rect(40, 5, 30, 30), createPrefabLine);
        GUI.DrawTexture(new Rect(75, 5, 30, 30), createSquareIntersection);
        GUI.DrawTexture(new Rect(110, 5, 30, 30), createTriangleIntersection);
        GUI.DrawTexture(new Rect(145, 5, 30, 30), createDiamondIntersection);
        GUI.DrawTexture(new Rect(180, 5, 30, 30), createRoundabout);
        GUI.DrawTexture(new Rect(215, 5, 30, 30), createRoadTransition);
        GUI.DrawTexture(new Rect(250, 5, 30, 30), createRoadSplitter);

        GUILayout.EndHorizontal();
    }

    private bool ClickedButton(int i)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            float MouseX = Event.current.mousePosition.x;
            float MouseY = Event.current.mousePosition.y;
            float MinX = 5 + (i - 1) * 35 + SceneView.lastActiveSceneView.position.width - 295;
            float MinY = SceneView.lastActiveSceneView.position.height - 50;

            if (MouseX > MinX && MouseX < MinX + 30 && MouseY > MinY && MouseY < MinY + 30)
            {
                return true;
            }
        }
        return false;
    }

    private void SetPosition(GameObject gameObject)
    {
        RaycastHit raycastHit;
        Ray ray = Camera.current.ScreenPointToRay(new Vector3(Input.mousePosition.x + Camera.current.pixelWidth / 2, Input.mousePosition.y + Camera.current.pixelHeight / 2, Input.mousePosition.z));
        if (Physics.Raycast(ray, out raycastHit, 100))
        {
            gameObject.transform.position = raycastHit.point;
            Selection.activeGameObject = gameObject;
        }
    }

}
