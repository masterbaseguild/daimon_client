using UnityEngine;
using System.Collections.Generic;

// the world class stores and manages the data of the entire world
// at the moment, the world is composed of a single region

public class World : MonoBehaviour
{
    [SerializeField] private UI ui;
    [SerializeField] private GameObject player;
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
    public Chunk GetChunk(int chunkX, int chunkY, int chunkZ)
    {
        return region.GetChunk(chunkX, chunkY, chunkZ);
    }

    public int GetVoxel(int x, int y, int z)
    {
        return region.GetVoxel(x, y, z);
    }

    public void SetVoxel(int x, int y, int z, int block)
    {
        region.SetVoxel(x, y, z, block);
        UpdateVoxel(x, y, z, block, false);
        // update the neighbouring voxels
        UpdateVoxel(x+1, y, z, block, true);
        UpdateVoxel(x-1, y, z, block, true);
        UpdateVoxel(x, y+1, z, block, true);
        UpdateVoxel(x, y-1, z, block, true);
        UpdateVoxel(x, y, z+1, block, true);
        UpdateVoxel(x, y, z-1, block, true);
    }

    private void UpdateVoxel(int x, int y, int z, int block, bool isNeighbour)
    {
        int chunkX = x / Chunk.CHUNK_SIZE;
        int chunkY = y / Chunk.CHUNK_SIZE;
        int chunkZ = z / Chunk.CHUNK_SIZE;
        ChunkMesh chunkMesh = chunkMeshes[chunkX + (chunkY * Region.REGION_SIZE) + (chunkZ * Region.REGION_SIZE * Region.REGION_SIZE)];
        if (chunkMesh != null)
        {
            int voxelX = x % Chunk.CHUNK_SIZE;
            int voxelY = y % Chunk.CHUNK_SIZE;
            int voxelZ = z % Chunk.CHUNK_SIZE;
            BlockMesh oldBlock = chunkMesh.GetBlockMesh(voxelX, voxelY, voxelZ);
            if (!isNeighbour)
            {
                if (oldBlock != null)
                {
                    chunkMesh.RemoveBlockFromMesh(x, y, z, oldBlock.voxel, blockPalette);
                }
                chunkMesh.AddBlockToMesh(x, y, z, block, blockPalette, false);
            }
            else
            {
                if (oldBlock != null)
                {
                    int voxel = oldBlock.voxel;
                    chunkMesh.RemoveBlockFromMesh(x, y, z, voxel, blockPalette);
                    chunkMesh.AddBlockToMesh(x, y, z, voxel, blockPalette, false);
                }
            }
        }
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
    private void DisplayChunk(int chunkX, int chunkY, int chunkZ)
    {
        Chunk chunk = GetChunk(chunkX, chunkY, chunkZ);
        ChunkMesh chunkMesh = new(0);
        chunkMeshes[chunkX + (chunkY * Region.REGION_SIZE) + (chunkZ * Region.REGION_SIZE * Region.REGION_SIZE)] = chunkMesh;
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++)
        {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++)
            {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++)
                {
                    int voxel = chunk.GetVoxel(i, j, k);
                    if (voxel != 0)
                    {
                        chunkMesh.AddBlockToMesh(i + (chunkX * Chunk.CHUNK_SIZE), j + (chunkY * Chunk.CHUNK_SIZE), k + (chunkZ * Chunk.CHUNK_SIZE), voxel, blockPalette, true);
                    }
                }
            }
        }
        chunkMesh.UpdateAllMeshes();
    }

    private bool IsChunkEmpty(int x, int y, int z)
    {
        return region.IsChunkEmpty(x, y, z);
    }

    public void DisplayWorld()
    {
        StartCoroutine(DisplayWorldCoroutine());
    }

    private IEnumerator<string> DisplayWorldCoroutine()
    {
        ui.ToggleLoadingText(true);
        List<Vector3Int> chunkPositions = new();

        for (int x = 0; x < Region.REGION_SIZE; x++)
        {
            for (int y = 0; y < Region.REGION_SIZE; y++)
            {
                for (int z = 0; z < Region.REGION_SIZE; z++)
                {
                    if (!IsChunkEmpty(x, y, z))
                    {
                        chunkPositions.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        int displayedChunks = 0;
        foreach (Vector3Int chunkPos in chunkPositions)
        {
            DisplayChunk(chunkPos.x, chunkPos.y, chunkPos.z);
            displayedChunks++;
            var displayPercentage = Mathf.RoundToInt((float)displayedChunks / chunkPositions.Count * 100);
            string loadingText = "Loading world: " + displayPercentage + "%";
            ui.SetLoadingText(loadingText);

            yield return null;
        }
        ui.ToggleLoadingText(false);
        ui.ToggleBackground(false);
        player.SetActive(true);
    }
}