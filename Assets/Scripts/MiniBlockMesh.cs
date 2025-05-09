using System.Collections.Generic;
using UnityEngine;

public class MiniBlockMesh
{
    public int voxel = 0;
    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2> uvs = new();
    public List<Vector3> colliderVertices = new();
    public List<int> colliderTriangles = new();

    public void AddVertices(Direction direction, int x, int y, int z)
    {
        // order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                break;
            case Direction.foreward:
                vertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f));
                break;
            case Direction.left:
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                break;

            case Direction.right:
                vertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f));
                break;
            case Direction.down:
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f));
                break;
            case Direction.up:
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f));
                vertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f-0.5f));
                vertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f-0.5f));
                break;
            default:
                break;
        }
    }

    public void AddColliderVertices(Direction direction, int x, int y, int z)
    {
        // order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                break;
            case Direction.foreward:
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f));
                break;
            case Direction.left:
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                break;

            case Direction.right:
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f));
                break;
            case Direction.down:
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f-0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f-0.5f, z*0.5f));
                break;
            case Direction.up:
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f));
                colliderVertices.Add(new Vector3(x*0.5f, y*0.5f, z*0.5f-0.5f));
                colliderVertices.Add(new Vector3(x*0.5f-0.5f, y*0.5f, z*0.5f-0.5f));
                break;
            default:
                break;
        }
    }

    public void AddQuadTriangles()
    {
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
    }

    public void AddColliderQuadTriangles()
    {
        colliderTriangles.Add(colliderVertices.Count - 4);
        colliderTriangles.Add(colliderVertices.Count - 3);
        colliderTriangles.Add(colliderVertices.Count - 2);

        colliderTriangles.Add(colliderVertices.Count - 4);
        colliderTriangles.Add(colliderVertices.Count - 2);
        colliderTriangles.Add(colliderVertices.Count - 1);
    }

    public void AddUvs(Vector2[] uvs)
    {
        this.uvs.AddRange(uvs);
    }
}