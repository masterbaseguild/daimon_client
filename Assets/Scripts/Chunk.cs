using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

public class Chunk
{
    public const int CHUNK_SIZE = 16;
    public Voxel[,,] voxels = new Voxel[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

    public Chunk()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    voxels[x, y, z] = new Voxel("air");
                }
            }
        }
    }

    public Voxel getVoxel(int x, int y, int z)
    {
        return voxels[x, y, z];
    }

    public void setVoxel(int x, int y, int z, Voxel voxel)
    {
        voxels[x, y, z] = voxel;
    }
}