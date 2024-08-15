using UnityEngine;
using System.Collections.Generic;

public class ChunkMesh
{
    public GameObject gameObject;
    private Mesh mesh = new Mesh();
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    public ChunkMesh()
    {
        gameObject = new GameObject();
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        meshFilter.mesh = mesh;
    }

    public void AddBlockToMesh(int x, int y, int z, string type, BlockPalette BlockPalette)
    {
        Vector3[] cubeVertices = new Vector3[]
        {
            new Vector3(x, y, z),
            new Vector3(x + 1, y, z),
            new Vector3(x + 1, y + 1, z),
            new Vector3(x, y + 1, z),
            new Vector3(x, y, z + 1),
            new Vector3(x + 1, y, z + 1),
            new Vector3(x + 1, y + 1, z + 1),
            new Vector3(x, y + 1, z + 1)
        };

        int[] cubeTriangles = new int[]
        {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            3, 7, 6, 3, 6, 2,
            0, 1, 5, 0, 5, 4,
            0, 4, 7, 0, 7, 3,
            1, 2, 6, 1, 6, 5
        };

        int offset = vertices.Count;
        vertices.AddRange(cubeVertices);

        foreach (int i in cubeTriangles)
        {
            triangles.Add(i + offset);
        }

        // get the block's texture coordinates in the texture atlas
        Vector2[] blockUVs = BlockPalette.GetBlockUVs(type);

        if(blockUVs.Length == 4)
        {
            uvs.Add(blockUVs[0]);
            uvs.Add(blockUVs[1]);
            uvs.Add(blockUVs[2]);
            uvs.Add(blockUVs[3]);
            uvs.Add(blockUVs[0]);
            uvs.Add(blockUVs[1]);
            uvs.Add(blockUVs[2]);
            uvs.Add(blockUVs[3]);
        }
        else
        {
            Debug.LogError("Block " + type + " has invalid UVs");
        }

        UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (vertices.Count != uvs.Count)
        {
            Debug.LogError("Mismatch between vertices and UVs count!");
            return;
        }
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }
}