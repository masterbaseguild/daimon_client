// a chunk is a 16x16x16 grid of voxels
// each voxel is stored as a number that maps to a block type in the block palette
using UnityEngine;
using System.Collections.Generic;

public class Chunk
{
    public static readonly int CHUNK_SIZE = 16;
    private readonly int[,,] voxels = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    private readonly int[,,] miniVoxels = new int[CHUNK_SIZE*2, CHUNK_SIZE*2, CHUNK_SIZE*2];
    private readonly List<Model> models = new List<Model>();

    public Chunk()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    voxels[x, y, z] = 0;
                }
            }
        }
        for (int x = 0; x < CHUNK_SIZE*2; x++)
        {
            for (int y = 0; y < CHUNK_SIZE*2; y++)
            {
                for (int z = 0; z < CHUNK_SIZE*2; z++)
                {
                    miniVoxels[x, y, z] = 0;
                }
            }
        }
    }

    public int GetVoxel(int x, int y, int z)
    {
        int voxelX = x % CHUNK_SIZE;
        int voxelY = y % CHUNK_SIZE;
        int voxelZ = z % CHUNK_SIZE;
        return voxels[voxelX, voxelY, voxelZ];
    }

    public int GetMiniVoxel(int x, int y, int z)
    {
        int voxelX = x % (CHUNK_SIZE*2);
        int voxelY = y % (CHUNK_SIZE*2);
        int voxelZ = z % (CHUNK_SIZE*2);
        return miniVoxels[voxelX, voxelY, voxelZ];
    }

    public Model GetModel(int x, int y, int z)
    {
        Vector3 position = new Vector3(x, y, z);
        foreach (Model model in models)
        {
            if (model.position == position)
            {
                return model;
            }
        }
        return null;
    }

    public void SetVoxel(int x, int y, int z, int block)
    {
        int voxelX = x % CHUNK_SIZE;
        int voxelY = y % CHUNK_SIZE;
        int voxelZ = z % CHUNK_SIZE;
        voxels[voxelX, voxelY, voxelZ] = block;
    }

    public void SetMiniVoxel(int x, int y, int z, int block)
    {
        int voxelX = x % (CHUNK_SIZE*2);
        int voxelY = y % (CHUNK_SIZE*2);
        int voxelZ = z % (CHUNK_SIZE*2);
        miniVoxels[voxelX, voxelY, voxelZ] = block;
    }

    public void SetModel(int id, Vector3 position)
    {
        Model model = new Model
        {
            id = id,
            position = position
        };
        models.Add(model);
    }

    public void RemoveModel(int x, int y, int z)
    {
        Model model = GetModel(x, y, z);
        if (model != null)
        {
            models.Remove(model);
        }
    }

    public bool IsEmpty()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (voxels[x, y, z] != 0)
                    {
                        return false;
                    }
                }
            }
        }
        for (int x = 0; x < CHUNK_SIZE*2; x++)
        {
            for (int y = 0; y < CHUNK_SIZE*2; y++)
            {
                for (int z = 0; z < CHUNK_SIZE*2; z++)
                {
                    if (miniVoxels[x, y, z] != 0)
                    {
                        return false;
                    }
                }
            }
        }
        if (models.Count > 0)
        {
            return false;
        }
        return true;
    }
}