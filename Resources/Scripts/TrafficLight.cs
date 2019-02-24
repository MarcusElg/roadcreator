using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[HelpURL("https://github.com/MCrafterzz/roadcreator/wiki/Traffic-Lights")]
public class TrafficLight : MonoBehaviour
{

    public Material greenActive;
    public Material greenNonActive;
    public Material yellowActive;
    public Material yellowNonActive;
    public Material redActive;
    public Material redNonActive;

    public float greenTime = 30;
    public float yellowBeforeRedTime = 5;
    public float redTime = 60;
    public float yellowBeforeGreenTime = 1;
    public float startTime = 0;

    public float timeSinceLast = 0;
    public float nextChangeTick = 0;

    public enum TrafficColour { Green, YellowBeforeRed, Red, YellowBeforeGreen };
    public TrafficColour currentColour;
    public TrafficColour startColour;

    public bool paused = false;

    public void Start()
    {
        ModifyChangeTick();
    }

    public void Update()
    {
        if (paused == false)
        {
            timeSinceLast += Time.deltaTime;

            if (timeSinceLast >= nextChangeTick)
            {
                if (currentColour == TrafficColour.Green)
                {
                    currentColour = TrafficColour.YellowBeforeRed;
                }
                else if (currentColour == TrafficColour.YellowBeforeRed)
                {
                    currentColour = TrafficColour.Red;
                }
                else if (currentColour == TrafficColour.Red)
                {
                    currentColour = TrafficColour.YellowBeforeGreen;
                }
                else if (currentColour == TrafficColour.YellowBeforeGreen)
                {
                    currentColour = TrafficColour.Green;
                }

                timeSinceLast = 0;
                ModifyChangeTick();
                UpdateMaterials();
            }
        }
    }

    public void ModifyChangeTick()
    {
        if (currentColour == TrafficColour.Green)
        {
            nextChangeTick = greenTime;
        }
        else if (currentColour == TrafficColour.YellowBeforeRed)
        {
            nextChangeTick = yellowBeforeRedTime;
        }
        else if (currentColour == TrafficColour.Red)
        {
            nextChangeTick = redTime;
        }
        else if (currentColour == TrafficColour.YellowBeforeGreen)
        {
            nextChangeTick = yellowBeforeGreenTime;
        }
    }

    public void UpdateMaterials()
    {
        Material[] materials = transform.GetComponent<MeshRenderer>().sharedMaterials;
        materials[4] = greenNonActive;
        materials[3] = yellowNonActive;
        materials[2] = redNonActive;

        if (currentColour == TrafficColour.Green)
        {
            materials[4] = greenActive;
        }
        else
        if (currentColour == TrafficColour.YellowBeforeRed || currentColour == TrafficColour.YellowBeforeGreen)
        {
            materials[3] = yellowActive;
        }
        else
        if (currentColour == TrafficColour.Red)
        {
            materials[2] = redActive;
        }

        transform.GetComponent<MeshRenderer>().sharedMaterials = materials;
    }

}
