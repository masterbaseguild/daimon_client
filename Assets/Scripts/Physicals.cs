using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

public class Voxel
{
    public string id { get; set; }
    public Voxel(string id)
    {
        this.id = id;
    }
}

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

public class Region
{
    public const int REGION_SIZE = 16;
    public const int HEADER_BLOCK_SIZE = 6;
    public const int HEADER_BLOCK_COUNT = 256;
    public const int HEADER_SIZE = HEADER_BLOCK_SIZE * HEADER_BLOCK_COUNT;
    public Chunk[,,] chunks = new Chunk[REGION_SIZE, REGION_SIZE, REGION_SIZE];
    public List<string> Header { get; } = new List<string>();

    public Region()
    {

        for (int x = 0; x < REGION_SIZE; x++)
        {
            for (int y = 0; y < REGION_SIZE; y++)
            {
                for (int z = 0; z < REGION_SIZE; z++)
                {
                    chunks[x, y, z] = new Chunk();
                }
            }
        }
    }

    public Region(byte[] data)
    {
        // split header and content
        byte[] headerBuffer = new byte[HEADER_SIZE];
        byte[] contentBuffer = new byte[data.Length - HEADER_SIZE];
        Array.Copy(data, headerBuffer, headerBuffer.Length);
        Array.Copy(data, HEADER_SIZE, contentBuffer, 0, contentBuffer.Length);

        // parse header
        for (int i = 0; i < HEADER_BLOCK_COUNT; i++)
        {
            Header.Add(BitConverter.ToString(headerBuffer, i * HEADER_BLOCK_SIZE, HEADER_BLOCK_SIZE).Replace("-", ""));
        }

        // parse chunks
        int index = 0;
        for (int chunkX = 0; chunkX < REGION_SIZE; chunkX++)
        {
            for (int chunkY = 0; chunkY < REGION_SIZE; chunkY++)
            {
                for (int chunkZ = 0; chunkZ < REGION_SIZE; chunkZ++)
                {
                    chunks[chunkX, chunkY, chunkZ] = new Chunk();
                    Chunk chunk = chunks[chunkX, chunkY, chunkZ];
                    for (int voxelX = 0; voxelX < Chunk.CHUNK_SIZE; voxelX++)
                    {
                        for (int voxelY = 0; voxelY < Chunk.CHUNK_SIZE; voxelY++)
                        {
                            for (int voxelZ = 0; voxelZ < Chunk.CHUNK_SIZE; voxelZ++)
                            {
                                byte voxelData = contentBuffer[index++];
                                chunk.setVoxel(voxelX, voxelY, voxelZ, new Voxel(Header[voxelData]));
                            }
                        }
                    }
                }
            }
        }
    }

    public Chunk getChunk(int x, int y, int z)
    {
        return chunks[x, y, z];
    }

    public void setChunk(int x, int y, int z, Chunk chunk)
    {
        chunks[x, y, z] = chunk;
    }

    public Voxel getVoxel(int x, int y, int z)
    {
        int chunkX = x / Chunk.CHUNK_SIZE;
        int chunkY = y / Chunk.CHUNK_SIZE;
        int chunkZ = z / Chunk.CHUNK_SIZE;
        int voxelX = x % Chunk.CHUNK_SIZE;
        int voxelY = y % Chunk.CHUNK_SIZE;
        int voxelZ = z % Chunk.CHUNK_SIZE;
        return chunks[chunkX, chunkY, chunkZ].getVoxel(voxelX, voxelY, voxelZ);
    }

    public void setVoxel(int x, int y, int z, Voxel voxel)
    {
        int chunkX = x / Chunk.CHUNK_SIZE;
        int chunkY = y / Chunk.CHUNK_SIZE;
        int chunkZ = z / Chunk.CHUNK_SIZE;
        int voxelX = x % Chunk.CHUNK_SIZE;
        int voxelY = y % Chunk.CHUNK_SIZE;
        int voxelZ = z % Chunk.CHUNK_SIZE;
        chunks[chunkX, chunkY, chunkZ].setVoxel(voxelX, voxelY, voxelZ, voxel);
    }
}