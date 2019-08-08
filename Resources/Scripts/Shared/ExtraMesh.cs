using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ExtraMesh
{
    public bool open;
    public int index;
    public bool left;
    public Material baseMaterial;
    public Material overlayMaterial;
    public PhysicMaterial physicMaterial;
    public float startWidth;
    public float endWidth;
    public bool flipped;
    public float yOffset;

    public ExtraMesh(bool open, bool left, Material baseMaterial, Material overlayMaterial, PhysicMaterial physicMaterial, float startWidth, float endWidth, bool flipped, float yOffset)
    {
        this.open = open;
        this.left = left;
        this.baseMaterial = baseMaterial;
        this.overlayMaterial = overlayMaterial;
        this.physicMaterial = physicMaterial;
        this.startWidth = startWidth;
        this.endWidth = endWidth;
        this.flipped = flipped;
        this.yOffset = yOffset;
    }

    public ExtraMesh(bool open, int index, Material baseMaterial, Material overlayMaterial, PhysicMaterial physicMaterial, float startWidth, float endWidth, bool flipped, float yOffset)
    {
        this.open = open;
        this.index = index;
        this.baseMaterial = baseMaterial;
        this.overlayMaterial = overlayMaterial;
        this.physicMaterial = physicMaterial;
        this.startWidth = startWidth;
        this.endWidth = endWidth;
        this.flipped = flipped;
        this.yOffset = yOffset;
    }

    public ExtraMesh Copy()
    {
        ExtraMesh extraMesh = new ExtraMesh(open, index, baseMaterial, overlayMaterial, physicMaterial, startWidth, endWidth, flipped, yOffset);
        extraMesh.left = left;
        return extraMesh;
    }

}
