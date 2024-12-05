using UnityEngine;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    static BlockPalette blockPalette;
    static Region region;

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

    public static void DisplayBlockTexture(int index)
    {
        BlockType block = blockPalette.GetBlockType(index);
        block.OnTextureLoaded += () =>
        {
            Texture2D texture = block.GetTexture2D();

            if (texture != null)
            {
                // Create a Quad to display the texture
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = new Vector3(0, 100, 0); // Position the Quad as needed
                quad.transform.Rotate(90, 0, 0); // Rotate the Quad as needed
                quad.GetComponent<Renderer>().material.mainTexture = texture;
            }
        };
    }

    public static void DisplayBlock(int x, int y, int z)
    {
        int voxel = region.getVoxel(x, y, z);
        BlockType block = blockPalette.GetBlockType(voxel);
        block.OnTextureLoaded += () =>
        {
            Texture2D texture = block.GetTexture2D();
            // set filter to point to prevent blurring
            texture.filterMode = FilterMode.Point;

            if (texture != null)
            {
                GameObject blockObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blockObject.transform.position = new Vector3(x, y, z);
                blockObject.GetComponent<Renderer>().material.mainTexture = texture;
                blockObject.GetComponent<Renderer>().material.mainTexture = block.GetTexture2D();
            }
        };
    }

    public static void DisplayChunk(int x, int y, int z)
    {
        Chunk chunk = region.getChunk(0, 0, 0);
        ChunkMesh chunkMesh = new ChunkMesh();
        chunks[0] = chunkMesh;
        int voxel = region.getVoxel(x, y, z);
        chunkMesh.AddBlockToMesh(x, y, z, voxel, blockPalette);
    }

    public static void DisplayWorld()
    {
        List<Vector3> coordinates = new List<Vector3>();
        coordinates.Add(new Vector3(100, 100, 100));
        coordinates.Add(new Vector3(100, 100, 101));
        coordinates.Add(new Vector3(100, 101, 100));
        coordinates.Add(new Vector3(100, 101, 101));
        coordinates.Add(new Vector3(101, 100, 100));
        coordinates.Add(new Vector3(101, 100, 101));
        coordinates.Add(new Vector3(101, 101, 100));
        coordinates.Add(new Vector3(101, 101, 101));

        List<Vector3> chunkPositions = new List<Vector3>();

        foreach (Vector3 coordinate in coordinates)
        {
            Vector3 chunkPos = GetChunkCoordsFromBlockCoords(coordinate);
            Debug.Log("Adding chunk at " + chunkPos);
            if (!chunkPositions.Contains(chunkPos))
            {
                chunkPositions.Add(chunkPos);
            }
        }

        foreach (Vector3 chunkPos in chunkPositions)
        {
            DisplayChunk((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);
        }
    }

    public static Vector3 GetChunkCoordsFromBlockCoords(Vector3 blockCoordinates)
    {
        int x = (int)blockCoordinates.x / Chunk.CHUNK_SIZE;
        int y = (int)blockCoordinates.y / Chunk.CHUNK_SIZE;
        int z = (int)blockCoordinates.z / Chunk.CHUNK_SIZE;
        return new Vector3(x, y, z);
    }

    public static void DisplayRegion() {
        for (int x = 0; x < Region.REGION_SIZE; x++)
        {
            for (int y = 0; y < Region.REGION_SIZE; y++)
            {
                for (int z = 0; z < Region.REGION_SIZE; z++)
                {
                    Chunk chunk = region.getChunk(x, y, z);
                    for (int cx = 0; cx < Chunk.CHUNK_SIZE; cx++)
                    {
                        for (int cy = 0; cy < Chunk.CHUNK_SIZE; cy++)
                        {
                            for (int cz = 0; cz < Chunk.CHUNK_SIZE; cz++)
                            {
                                int voxel = chunk.getVoxel(cx, cy, cz);
                                if (voxel == 0)
                                {
                                    continue;
                                }
                                DisplayBlock(x * Chunk.CHUNK_SIZE + cx, y * Chunk.CHUNK_SIZE + cy, z * Chunk.CHUNK_SIZE + cz);
                            }
                        }
                    }
                }
            }
        }
    }
}