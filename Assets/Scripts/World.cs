using UnityEngine;
using System.Collections.Generic;

// the world class stores and manages the data of the entire world
// at the moment, the world is composed of a single region

// NOTE: the materials are handled badly, we should not
// set both a dynamic and a static reference to them
public class World : MonoBehaviour
{
    static BlockPalette blockPalette;
    static Region region; // the single region

    // all materials are handled and exposed by the world class
    public Material materialPrefab;
    public static Material material;
    public Material nonOpaqueMaterialPrefab;
    public static Material nonOpaqueMaterial;
    public PhysicMaterial physicMaterialPrefab;
    public static PhysicMaterial physicMaterial;

    // list of all the chunk meshes in all regions
    static ChunkMesh[] chunkMeshes = new ChunkMesh[Region.REGION_SIZE * Region.REGION_SIZE * Region.REGION_SIZE];

    // methods to manage materials
    void Start()
    {
        material = materialPrefab;
        nonOpaqueMaterial = nonOpaqueMaterialPrefab;
        physicMaterial = physicMaterialPrefab;
    }

    public static void SetTexture(Texture2D texture)
    {
        material.mainTexture = texture;
        nonOpaqueMaterial.mainTexture = texture;
    }

    public static Material GetMaterial()
    {
        return material;
    }

    public static Material GetNonOpaqueMaterial()
    {
        return nonOpaqueMaterial;
    }

    public static PhysicMaterial GetPhysicMaterial()
    {
        return physicMaterial;
    }

    // set block palette and region
    public static void SetBlockPalette(string[] results)
    {
        blockPalette = new BlockPalette(results, chunkMeshes);
    }

    public static void SetRegion(byte[] regionData)
    {
        region = new Region(regionData);
    }

    public static Region GetRegion()
    {
        return region;
    }

    // methods to get neighbour voxel data
    static public Vector3 GetChunkCoords(Vector3 position)
    {
        return new Vector3(Mathf.FloorToInt(position.x / Chunk.CHUNK_SIZE), Mathf.FloorToInt(position.y / Chunk.CHUNK_SIZE), Mathf.FloorToInt(position.z / Chunk.CHUNK_SIZE));
    }

    public static int GetVoxel(int x, int y, int z)
    {
        Vector3 chunkCoords = GetChunkCoords(new Vector3(x, y, z));
        Chunk chunk = region.getChunk((int)chunkCoords.x, (int)chunkCoords.y, (int)chunkCoords.z);
        return chunk.getVoxel(x % Chunk.CHUNK_SIZE, y % Chunk.CHUNK_SIZE, z % Chunk.CHUNK_SIZE);
    }

    public static Vector3 GetNeighbourCoords(int x, int y, int z, Direction direction)
    {
        switch (direction)
        {
            case Direction.backwards:
                return new Vector3(x, y, z - 1);
            case Direction.down:
                return new Vector3(x, y - 1, z);
            case Direction.foreward:
                return new Vector3(x, y, z + 1);
            case Direction.left:
                return new Vector3(x - 1, y, z);
            case Direction.right:
                return new Vector3(x + 1, y, z);
            case Direction.up:
                return new Vector3(x, y + 1, z);
            default:
                return new Vector3(x, y, z);
        }
    }

    public static int GetNeighbourVoxel(int x, int y, int z, Direction direction)
    {
        Vector3 neighbour = GetNeighbourCoords(x, y, z, direction);
        return GetVoxel((int)neighbour.x, (int)neighbour.y, (int)neighbour.z);
    }

    // methods to render the world
    public static void DisplayChunk(int x, int y, int z)
    {
        Chunk chunk = region.getChunk(x, y, z);
        ChunkMesh chunkMesh = new ChunkMesh(true);
        chunkMeshes[x + y * Region.REGION_SIZE + z * Region.REGION_SIZE * Region.REGION_SIZE] = chunkMesh;
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
}