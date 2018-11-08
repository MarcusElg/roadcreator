using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadSystem : MonoBehaviour
{

    Texture createRoad;
    Texture createPrefabLine;
    Texture createSquareIntersection;
    Texture createTriangleIntersection;
    Texture createDiamondIntersection;
    Texture createRoundabout;
    Texture createRoadSplitter;

    Texture straightRoad;
    Texture curvedRoad;

    Texture roadGuidelinesOn;
    Texture roadGuidelinesOff;

    public GlobalSettings globalSettings;
    public GUIStyle largeBoldText;

    public void ShowCreationButtons()
    {
        SceneView.lastActiveSceneView.Focus();
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (largeBoldText == null)
        {
            largeBoldText = new GUIStyle();
            largeBoldText.fontStyle = FontStyle.Bold;
            largeBoldText.fontSize = 15;
        }

        if (createRoad == null)
        {
            createRoad = Resources.Load("Textures/Ui/createroad") as Texture;
            createPrefabLine = Resources.Load("Textures/Ui/createprefabline") as Texture;
            createSquareIntersection = Resources.Load("Textures/Ui/createsquareintersection") as Texture;
            createTriangleIntersection = Resources.Load("Textures/Ui/createtriangleintersection") as Texture;
            createDiamondIntersection = Resources.Load("Textures/Ui/creatediamondintersection") as Texture;
            createRoundabout = Resources.Load("Textures/Ui/createroundabout") as Texture;
            createRoadSplitter = Resources.Load("Textures/Ui/createroadsplitter") as Texture;

            straightRoad = Resources.Load("Textures/Ui/straightroad") as Texture;
            curvedRoad = Resources.Load("Textures/Ui/curvedroad") as Texture;

            roadGuidelinesOn = Resources.Load("Textures/Ui/roadguidelineson") as Texture;
            roadGuidelinesOff = Resources.Load("Textures/Ui/roadguidelinesoff") as Texture;
        }

        Rect windowRect = new Rect(SceneView.lastActiveSceneView.position.width - 260, SceneView.lastActiveSceneView.position.height - 85, 250, 75);
        windowRect = GUILayout.Window(0, windowRect, DrawWindow, "", (Resources.Load("Textures/Ui/Object Creator Gui Skin") as GUISkin).window);

        // Detect click
        if (ClickedButton((int)(SceneView.lastActiveSceneView.position.width - 175), (int)(SceneView.lastActiveSceneView.position.height - 95)))
        {
            if (globalSettings.roadCurved == true)
            {
                globalSettings.roadCurved = false;
            } else
            {
                globalSettings.roadCurved = true;
            }
        }

        if (ClickedButton((int)(SceneView.lastActiveSceneView.position.width - 135), (int)(SceneView.lastActiveSceneView.position.height - 95)))
        {
            if (globalSettings.amountRoadGuidelines > 0)
            {
                globalSettings.oldAmountRoadGuidelines = globalSettings.amountRoadGuidelines;
                globalSettings.amountRoadGuidelines = 0;
            }
            else
            {
                globalSettings.amountRoadGuidelines = globalSettings.oldAmountRoadGuidelines;
            }

            globalSettings.UpdateRoadGuidelines();
        }

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
            gameObject.name = "Square Intersection";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Square Intersection");

            SetPosition(gameObject);
        }
        else if (ClickedButton(4) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Triangle Intersection";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Triangle Intersection");

            SetPosition(gameObject);
        }
        else if (ClickedButton(5) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Diamond Intersection";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Diamond Intersection");

            SetPosition(gameObject);
        }
        else if (ClickedButton(6) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Roundabout";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Roundabout");

            SetPosition(gameObject);
        }
        else if (ClickedButton(7) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Road Splitter";
            gameObject.transform.SetParent(transform);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Splitter");

            SetPosition(gameObject);
        }
    }

    void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();

        GUI.Label(new Rect(5, 10, 100, 40), "Settings:", largeBoldText);

        if (globalSettings != null)
        {
            if (globalSettings.roadCurved == true)
            {
                GUI.DrawTexture(new Rect(90, 5, 30, 30), curvedRoad);
            }
            else
            {
                GUI.DrawTexture(new Rect(90, 5, 30, 30), straightRoad);
            }

            if (globalSettings.amountRoadGuidelines > 0)
            {
                GUI.DrawTexture(new Rect(130, 5, 30, 30), roadGuidelinesOn);
            }
            else
            {
                GUI.DrawTexture(new Rect(130, 5, 30, 30), roadGuidelinesOff);
            }
        }

        GUI.DrawTexture(new Rect(5, 40, 30, 30), createRoad);
        GUI.DrawTexture(new Rect(40, 40, 30, 30), createPrefabLine);
        GUI.DrawTexture(new Rect(75, 40, 30, 30), createSquareIntersection);
        GUI.DrawTexture(new Rect(110, 40, 30, 30), createTriangleIntersection);
        GUI.DrawTexture(new Rect(145, 40, 30, 30), createDiamondIntersection);
        GUI.DrawTexture(new Rect(180, 40, 30, 30), createRoundabout);
        GUI.DrawTexture(new Rect(215, 40, 30, 30), createRoadSplitter);

        GUILayout.EndHorizontal();
    }

    private bool ClickedButton(int minX, int minY)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            float MouseX = Event.current.mousePosition.x;
            float MouseY = Event.current.mousePosition.y;

            if (MouseX > minX && MouseX < minX + 30 && MouseY > minY && MouseY < minY + 30)
            {
                return true;
            }
        }
        return false;
    }

    private bool ClickedButton(int i)
    {
        return ClickedButton((int)(5 + (i - 1) * 35 + SceneView.lastActiveSceneView.position.width - 260), (int)(SceneView.lastActiveSceneView.position.height - 50 - 15));
    }

    private void SetPosition(GameObject gameObject)
    {
        RaycastHit raycastHit;

        Ray ray = Camera.current.ScreenPointToRay(new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, 0));
        if (Physics.Raycast(ray, out raycastHit, 100))
        {
            gameObject.transform.position = raycastHit.point;
            gameObject.transform.rotation = Quaternion.Euler(0, Camera.current.transform.rotation.eulerAngles.y, 0);
            Selection.activeGameObject = gameObject;
        }
    }

}
