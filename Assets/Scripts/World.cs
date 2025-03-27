using UnityEngine;
using System.Collections.Generic;

// the world class stores and manages the data of the entire world
// at the moment, the world is composed of a single region

public class World : MonoBehaviour
{
    [SerializeField] private UI ui;
    private BlockPalette blockPalette;

    // all materials are handled and exposed by the world class
    public Material material;
    public Material nonOpaqueMaterial;
    public PhysicsMaterial physicMaterial;

    private Region region; // the single region

    // list of all the chunk meshes in all regions
    private readonly ChunkMesh[] chunkMeshes = new ChunkMesh[Region.REGION_SIZE * Region.REGION_SIZE * Region.REGION_SIZE];

    public void SetTexture(Texture2D texture)
    {
        material.mainTexture = texture;
        nonOpaqueMaterial.mainTexture = texture;
    }

    // set block palette and region
    public void SetBlockPalette(string[] results)
    {
        blockPalette = new BlockPalette(results);
    }

    public void SetRegion(byte[] regionData)
    {
        region = new Region(regionData);
    }

    public Region GetRegion()
    {
        return region;
    }

    // methods to get neighbour voxel data
    public Vector3 GetChunkCoords(Vector3 position)
    {
        return region.GetChunkCoords(position);
    }

    private int GetVoxel(int x, int y, int z)
    {
        return region.GetVoxel(x, y, z);
    }

    private Vector3 GetNeighbourCoords(int x, int y, int z, Direction direction)
    {
        return direction switch
        {
            Direction.backwards => new Vector3(x, y, z - 1),
            Direction.down => new Vector3(x, y - 1, z),
            Direction.foreward => new Vector3(x, y, z + 1),
            Direction.left => new Vector3(x - 1, y, z),
            Direction.right => new Vector3(x + 1, y, z),
            Direction.up => new Vector3(x, y + 1, z),
            _ => new Vector3(x, y, z),
        };

    }

    public int GetNeighbourVoxel(int x, int y, int z, Direction direction)
    {
        Vector3 neighbour = GetNeighbourCoords(x, y, z, direction);
        return GetVoxel((int)neighbour.x, (int)neighbour.y, (int)neighbour.z);
    }

    // methods to render the world
    private void DisplayChunk(int x, int y, int z)
    {
        Chunk chunk = region.GetChunk(x, y, z);
        ChunkMesh chunkMesh = new(true);
        chunkMeshes[x + (y * Region.REGION_SIZE) + (z * Region.REGION_SIZE * Region.REGION_SIZE)] = chunkMesh;
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
        {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++)
            {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++)
                {
                    int voxel = chunk.GetVoxel(i, j, k);
                    if (voxel != 0)
                    {
                        chunkMesh.AddBlockToMesh(i + (x * Chunk.CHUNK_SIZE), j + (y * Chunk.CHUNK_SIZE), k + (z * Chunk.CHUNK_SIZE), voxel, blockPalette);
                    }
                }
            }
        }
    }

    private bool IsChunkEmpty(Chunk chunk)
    {
        return region.IsChunkEmpty(chunk);
    }

    public void DisplayWorld()
    {
        StartCoroutine(DisplayWorldCoroutine());
    }

    private IEnumerator<string> DisplayWorldCoroutine()
    {
        ui.ToggleLoadingText(true);
        List<Vector3> chunkPositions = new();

        for (int x = 0; x < Region.REGION_SIZE; x++)
        {
            for (int y = 0; y < Region.REGION_SIZE; y++)
            {
                for (int z = 0; z < Region.REGION_SIZE; z++)
                {
                    if (!IsChunkEmpty(region.GetChunk(x, y, z)))
                    {
                        chunkPositions.Add(new Vector3(x, y, z));
                    }
                }
            }
        }

        int displayedChunks = 0;
        foreach (Vector3 chunkPos in chunkPositions)
        {
            DisplayChunk((int)chunkPos.x, (int)chunkPos.y, (int)chunkPos.z);
            displayedChunks++;
            string loadingText = "Loading world: " + displayedChunks + "/" + chunkPositions.Count + " chunks";
            ui.SetLoadingText(loadingText);

            yield return null;
        }
        ui.ToggleLoadingText(false);
    }
}