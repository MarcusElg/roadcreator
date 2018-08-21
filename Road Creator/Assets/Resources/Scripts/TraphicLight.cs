using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TraphicLight : MonoBehaviour
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

    public enum TraphicColour { Green, YellowBeforeRed, Red, YellowBeforeGreen };
    public TraphicColour currentColour;
    public TraphicColour startColour;

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
                if (currentColour == TraphicColour.Green)
                {
                    currentColour = TraphicColour.YellowBeforeRed;
                }
                else if (currentColour == TraphicColour.YellowBeforeRed)
                {
                    currentColour = TraphicColour.Red;
                }
                else if (currentColour == TraphicColour.Red)
                {
                    currentColour = TraphicColour.YellowBeforeGreen;
                }
                else if (currentColour == TraphicColour.YellowBeforeGreen)
                {
                    currentColour = TraphicColour.Green;
                }

                timeSinceLast = 0;
                ModifyChangeTick();
                UpdateMaterials();
            }
        }
    }

    public void ModifyChangeTick()
    {
        if (currentColour == TraphicColour.Green)
        {
            nextChangeTick = greenTime;
        }
        else if (currentColour == TraphicColour.YellowBeforeRed)
        {
            nextChangeTick = yellowBeforeRedTime;
        }
        else if (currentColour == TraphicColour.Red)
        {
            nextChangeTick = redTime;
        }
        else if (currentColour == TraphicColour.YellowBeforeGreen)
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

        if (currentColour == TraphicColour.Green)
        {
            materials[4] = greenActive;
        }
        else
        if (currentColour == TraphicColour.YellowBeforeRed || currentColour == TraphicColour.YellowBeforeGreen)
        {
            materials[3] = yellowActive;
        }
        else
        if (currentColour == TraphicColour.Red)
        {
            materials[2] = redActive;
        }

        transform.GetComponent<MeshRenderer>().sharedMaterials = materials;
    }

}
