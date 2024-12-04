using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

public class Chunk
{
    public static int CHUNK_SIZE = 16;
    int[,,] voxels = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

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
    }

    public int getVoxel(int x, int y, int z)
    {
        return voxels[x, y, z];
    }

    public void setVoxel(int x, int y, int z, int voxel)
    {
        voxels[x, y, z] = voxel;
    }
}