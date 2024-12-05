using UnityEngine;
using System.Collections.Generic;

public class ChunkMesh
{
    GameObject gameObject;
    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public ChunkMesh()
    {
        gameObject = new GameObject();
        gameObject.transform.parent = GameObject.Find("World").transform;
        gameObject.name = "ChunkMesh";
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = BlockPalette.material;
        meshFilter.mesh = mesh;
    }

    public void SetMaterial(Material material)
    {
        gameObject.GetComponent<MeshRenderer>().material = material;
    }

    public void AddBlockToMesh(int x, int y, int z, int voxel, BlockPalette BlockPalette)
    {
        BlockType type = BlockPalette.GetBlockType(voxel);
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

    void UpdateMesh()
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