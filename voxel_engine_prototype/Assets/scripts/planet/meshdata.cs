using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshdata
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uv = new List<Vector2>();
    public List<Vector3> collVertices = new List<Vector3>();
    public List<int> collTriangles = new List<int>();
    public meshdata waterMesh;

    public meshdata(bool isMainMesh)
    {
        if(isMainMesh)
        {
            waterMesh = new meshdata(false);
        }
    }

    public void addVertex(Vector3 vertex, bool vertexGensColl)
    {
        vertices.Add(vertex);
        if (vertexGensColl)
        {
            collVertices.Add(vertex);
        }
    }

    public void addQuadTriangles(bool quadGensColl)
    {
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        if(quadGensColl)
        {
            collTriangles.Add(collVertices.Count - 4);
            collTriangles.Add(collVertices.Count - 3);
            collTriangles.Add(collVertices.Count - 2);
            collTriangles.Add(collVertices.Count - 4);
            collTriangles.Add(collVertices.Count - 2);
            collTriangles.Add(collVertices.Count - 1);
        }
    }
}
