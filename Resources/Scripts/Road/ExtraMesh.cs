using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExtraMesh
{
    public bool open;
    public int index;
    public bool left;
    public Material material;
    public PhysicMaterial physicMaterial;
    public float startWidth;
    public float endWidth;
    public float yOffset;
    
    public ExtraMesh (bool open, bool left, Material material, PhysicMaterial physicMaterial, float startWidth, float endWidth, float yOffset)
    {
        this.open = open;
        this.left = left;
        this.material = material;
        this.physicMaterial = physicMaterial;
        this.startWidth = startWidth;
        this.endWidth = endWidth;
        this.yOffset = yOffset;
    }

    public ExtraMesh(bool open, int index, Material material, PhysicMaterial physicMaterial, float startWidth, float endWidth, float yOffset)
    {
        this.open = open;
        this.index = index;
        this.material = material;
        this.physicMaterial = physicMaterial;
        this.startWidth = startWidth;
        this.endWidth = endWidth;
        this.yOffset = yOffset;
    }

}
