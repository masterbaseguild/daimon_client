using System.Collections.Generic;
using UnityEngine;

public class BlockMesh
{
    public int voxel = 0;
    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2> uvs = new();
    public List<Vector3> colliderVertices = new();
    public List<int> colliderTriangles = new();
}