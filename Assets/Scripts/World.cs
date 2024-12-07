using UnityEngine;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    static BlockPalette blockPalette;
    static Region region;
    public Material materialPrefab;
    public static Material material;

    void Start()
    {
        material = materialPrefab;
    }

    public static void SetTexture(Texture2D texture)
    {
        material.mainTexture = texture;
    }

    public static Material GetMaterial()
    {
        return material;
    }

    static ChunkMesh[] chunks = new ChunkMesh[Region.REGION_SIZE * Region.REGION_SIZE * Region.REGION_SIZE];

    public static void SetBlockPalette(string[] results)
    {
        blockPalette = new BlockPalette(results, chunks);
    }

    public static void SetRegion(byte[] regionData)
    {
        region = new Region(regionData);
    }

    public static Region GetRegion()
    {
        return region;
    }

    public static void DisplayChunk(int x, int y, int z)
    {
        Chunk chunk = region.getChunk(x, y, z);
        ChunkMesh chunkMesh = new ChunkMesh();
        chunks[x + y * Region.REGION_SIZE + z * Region.REGION_SIZE * Region.REGION_SIZE] = chunkMesh;
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
        {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++)
            {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++)
                {
                    int voxel = chunk.getVoxel(i, j, k);
                    if (voxel != 0)
                    {
                        chunkMesh.AddBlockToMesh(i + x * Chunk.CHUNK_SIZE, j + y * Chunk.CHUNK_SIZE, k + z * Chunk.CHUNK_SIZE, voxel, blockPalette);
                    }
                }
            }
        }
    }

    public static void DisplayWorld()
    {
        List<Vector3> chunkPositions = new List<Vector3>();

        for (int x = 0; x < Region.REGION_SIZE; x++)
        {
            for (int y = 0; y < Region.REGION_SIZE; y++)
            {
                for (int z = 0; z < Region.REGION_SIZE; z++)
                {
                    if (!isChunkEmpty(region.getChunk(x, y, z)))
                    {
                        chunkPositions.Add(new Vector3(x, y, z));
                    }
                }
            }
        }

        foreach (Vector3 chunkPos in chunkPositions)
        {
            DisplayChunk((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);
        }
    }

    static bool isChunkEmpty(Chunk chunk)
    {
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    if (chunk.getVoxel(x, y, z) != 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}