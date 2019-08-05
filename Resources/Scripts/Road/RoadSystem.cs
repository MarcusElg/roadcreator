#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Road-Systems")]
public class RoadSystem : MonoBehaviour
{

    Texture createRoad;
    Texture createRoundabout;
    Texture createPrefabLine;
    Texture createTrafficLight;

    Texture straightRoad;
    Texture curvedRoad;

    Texture roadGuidelinesOn;
    Texture roadGuidelinesOff;

    public SerializedObject settings;
    public GUIStyle largeBoldText;

    public void ShowCreationButtons()
    {
        HandleUtility.nearestControl = GUIUtility.GetControlID(FocusType.Passive);

        if (settings == null)
        {
            settings = RoadCreatorSettings.GetSerializedSettings();
        }

        if (largeBoldText == null)
        {
            largeBoldText = new GUIStyle();
            largeBoldText.fontStyle = FontStyle.Bold;
            largeBoldText.fontSize = 15;
        }

        if (createRoad == null)
        {
            createRoad = Resources.Load("Textures/Ui/createroad") as Texture;
            createRoundabout = Resources.Load("Textures/Ui/createroundabout") as Texture;
            createPrefabLine = Resources.Load("Textures/Ui/createprefabline") as Texture;
            createTrafficLight = Resources.Load("Textures/Ui/createtrafficlight") as Texture;

            straightRoad = Resources.Load("Textures/Ui/straightroad") as Texture;
            curvedRoad = Resources.Load("Textures/Ui/curvedroad") as Texture;

            roadGuidelinesOn = Resources.Load("Textures/Ui/roadguidelineson") as Texture;
            roadGuidelinesOff = Resources.Load("Textures/Ui/roadguidelinesoff") as Texture;
        }

        Rect windowRect = new Rect(SceneView.lastActiveSceneView.position.width - 170, SceneView.lastActiveSceneView.position.height - 85, 160, 75);
        windowRect = GUILayout.Window(0, windowRect, DrawWindow, "", (Resources.Load("Textures/Ui/Object Creator Gui Skin") as GUISkin).window);

        // Detect click
        if (ClickedButton((int)(SceneView.lastActiveSceneView.position.width - 80), (int)(SceneView.lastActiveSceneView.position.height - 95)))
        {
            if (settings.FindProperty("roadCurved").boolValue == true)
            {
                settings.FindProperty("roadCurved").boolValue = false;
            }
            else
            {
                settings.FindProperty("roadCurved").boolValue = true;
            }

            settings.ApplyModifiedPropertiesWithoutUndo();
            RoadCreatorProjectSettings.UpdateSettings();
        }

        if (ClickedButton((int)(SceneView.lastActiveSceneView.position.width - 45), (int)(SceneView.lastActiveSceneView.position.height - 95)))
        {
            if (settings.FindProperty("roadGuidelinesLength").floatValue > 0)
            {
                settings.FindProperty("oldRoadGuidelinesLength").floatValue = settings.FindProperty("roadGuidelinesLength").floatValue;
                settings.FindProperty("roadGuidelinesLength").floatValue = 0;
            }
            else
            {
                settings.FindProperty("roadGuidelinesLength").floatValue = settings.FindProperty("oldRoadGuidelinesLength").floatValue;
            }

            settings.ApplyModifiedPropertiesWithoutUndo();
            RoadCreatorSettings.UpdateRoadGuidelines();
        }

        if (ClickedButton(0) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<RoadCreator>();
            gameObject.name = "Road";
            gameObject.transform.SetParent(transform);
            gameObject.GetComponent<RoadCreator>().startLanes = settings.FindProperty("defaultLanes").intValue;
            gameObject.GetComponent<RoadCreator>().endLanes = settings.FindProperty("defaultLanes").intValue;
            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road");
        }
        else if (ClickedButton(1) == true)
        {
            GameObject gameObject = new GameObject();
            gameObject.layer = LayerMask.NameToLayer("Intersection");
            gameObject.AddComponent<Intersection>();
            gameObject.GetComponent<Intersection>().roundaboutMode = true;
            gameObject.GetComponent<Intersection>().Setup();
            gameObject.transform.hideFlags = HideFlags.None;
            gameObject.GetComponent<Intersection>().generateBridge = false;
            gameObject.GetComponent<Intersection>().placePillars = false;
            gameObject.GetComponent<Intersection>().overlayMaterial = (Material)settings.FindProperty("defaultRoadOverlayMaterial").objectReferenceValue;
            gameObject.GetComponent<Intersection>().connectionOverlayMaterial = (Material)settings.FindProperty("defaultIntersectionOverlayMaterial").objectReferenceValue;
            gameObject.GetComponent<Intersection>().placePillars = true;
            gameObject.GetComponent<Intersection>().CreateMesh();

            gameObject.name = "Roundabout";
            gameObject.transform.SetParent(transform);
            SetPosition(gameObject);
            gameObject.transform.localRotation = Quaternion.identity;

            Selection.activeGameObject = gameObject;
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Roundabout");
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
            GameObject gameObject = Instantiate(Resources.Load("Prefabs/Traffic Light") as GameObject);
            gameObject.name = "Traffic Light";
            gameObject.transform.SetParent(transform);
            Selection.activeGameObject = gameObject;
            SetPosition(gameObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Traffic Light");
        }
    }

    void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();

        GUI.Label(new Rect(5, 10, 100, 40), "Settings:", largeBoldText);

        if (settings != null)
        {
            if (settings.FindProperty("roadCurved").boolValue == true)
            {
                GUI.DrawTexture(new Rect(90, 5, 30, 30), curvedRoad);
            }
            else
            {
                GUI.DrawTexture(new Rect(90, 5, 30, 30), straightRoad);
            }

            if (settings.FindProperty("roadGuidelinesLength").floatValue > 0)
            {
                GUI.DrawTexture(new Rect(125, 5, 30, 30), roadGuidelinesOn);
            }
            else
            {
                GUI.DrawTexture(new Rect(125, 5, 30, 30), roadGuidelinesOff);
            }
        }

        GUI.DrawTexture(new Rect(5, 40, 30, 30), createRoad);
        GUI.DrawTexture(new Rect(45, 40, 30, 30), createRoundabout);
        GUI.DrawTexture(new Rect(85, 40, 30, 30), createPrefabLine);
        GUI.DrawTexture(new Rect(125, 40, 30, 30), createTrafficLight);

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
        return ClickedButton((int)(5 + (i - 1) * 40 + SceneView.lastActiveSceneView.position.width - 130), (int)(SceneView.lastActiveSceneView.position.height - 50 - 15));
    }

    private void SetPosition(GameObject gameObject)
    {
        RaycastHit raycastHit;

        Ray ray = Camera.current.ScreenPointToRay(new Vector3(Camera.current.pixelWidth / 2, Camera.current.pixelHeight / 2, 0));
        if (Physics.Raycast(ray, out raycastHit, 100))
        {
            gameObject.transform.position = raycastHit.point;
            gameObject.transform.rotation = Quaternion.Euler(0, Camera.current.transform.rotation.eulerAngles.y + 180, 0);
            Selection.activeGameObject = gameObject;
        }
    }

}
#endif