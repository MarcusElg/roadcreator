using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoundaboutExtraMesh
{
    public ExtraMesh extraMesh;
    public int listIndex;
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<Vector2> uvs2 = new List<Vector2>();
    public float startOffset;
    public float endOffset;
    public float yOffset;

    public RoundaboutExtraMesh(ExtraMesh extraMesh, int listIndex, float startOffset, float endOffset, float yOffset)
    {
        this.extraMesh = extraMesh;
        this.listIndex = listIndex;
        this.startOffset = startOffset;
        this.endOffset = endOffset;
        this.yOffset = yOffset;
    }
}
