using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoadSystem : MonoBehaviour
{

    Texture createRoad;
    Texture createPrefabLine;

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
            createRoad = Resources.Load("Textures/Low Poly/Ui/createroad") as Texture;
            createPrefabLine = Resources.Load("Textures/Low Poly/Ui/createprefabline") as Texture;

            straightRoad = Resources.Load("Textures/Low Poly/Ui/straightroad") as Texture;
            curvedRoad = Resources.Load("Textures/Low Poly/Ui/curvedroad") as Texture;

            roadGuidelinesOn = Resources.Load("Textures/Low Poly/Ui/roadguidelineson") as Texture;
            roadGuidelinesOff = Resources.Load("Textures/Low Poly/Ui/roadguidelinesoff") as Texture;
        }

        Rect windowRect = new Rect(SceneView.lastActiveSceneView.position.width - 170, SceneView.lastActiveSceneView.position.height - 85, 160, 75);
        windowRect = GUILayout.Window(0, windowRect, DrawWindow, "", (Resources.Load("Textures/Low Poly/Ui/Object Creator Gui Skin") as GUISkin).window);

        // Detect click
        if (ClickedButton((int)(SceneView.lastActiveSceneView.position.width - 80), (int)(SceneView.lastActiveSceneView.position.height - 95)))
        {
            if (globalSettings.roadCurved == true)
            {
                globalSettings.roadCurved = false;
            } else
            {
                globalSettings.roadCurved = true;
            }
        }

        if (ClickedButton((int)(SceneView.lastActiveSceneView.position.width - 45), (int)(SceneView.lastActiveSceneView.position.height - 95)))
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
                GUI.DrawTexture(new Rect(125, 5, 30, 30), roadGuidelinesOn);
            }
            else
            {
                GUI.DrawTexture(new Rect(125, 5, 30, 30), roadGuidelinesOff);
            }
        }

        GUI.DrawTexture(new Rect(55, 40, 30, 30), createRoad);
        GUI.DrawTexture(new Rect(90, 40, 30, 30), createPrefabLine);

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
        return ClickedButton((int)(5 + (i - 1) * 35 + SceneView.lastActiveSceneView.position.width - 120), (int)(SceneView.lastActiveSceneView.position.height - 50 - 15));
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
