using System.Collections.Generic;
using System;

// a region is a 16x16x16 grid of chunks

// NOTE: the header data should be moved,
// since it is only needed on region creation
// and all headers should be fused into a single
// block palette for the entire world
public class Region
{
    public static readonly int REGION_SIZE = 16;
    private readonly Chunk[,,] chunks = new Chunk[REGION_SIZE, REGION_SIZE, REGION_SIZE];

    // header data
    private static readonly int HEADER_BLOCK_SIZE = 6;
    private static readonly int HEADER_BLOCK_COUNT = 256;
    private static readonly int HEADER_SIZE = HEADER_BLOCK_SIZE * HEADER_BLOCK_COUNT;
    private readonly List<string> Header = new();

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
            string headerLine = BitConverter.ToString(headerBuffer, i * HEADER_BLOCK_SIZE, HEADER_BLOCK_SIZE).Replace("-", "").ToLower();
            if (headerLine == "000000000000" && i != 0)
            {
                continue;
            }
            Header.Add(headerLine);
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
                                chunk.SetVoxel(voxelX, voxelY, voxelZ, voxelData);
                            }
                        }
                    }
                }
            }
        }
    }

    public string GetHeaderLine(int index)
    {
        return Header[index];
    }

    public int GetHeaderCount()
    {
        return Header.Count;
    }

    public Chunk GetChunk(int chunkX, int chunkY, int chunkZ)
    {
        return chunks[chunkX, chunkY, chunkZ];
    }

    public bool IsChunkEmpty(int x, int y, int z)
    {
        Chunk chunk = chunks[x, y, z];
        return chunk.IsEmpty();
    }

    public int GetVoxel(int x, int y, int z)
    {
        int chunkX = x % REGION_SIZE;
        int chunkY = y % REGION_SIZE;
        int chunkZ = z % REGION_SIZE;
        return chunks[chunkX, chunkY, chunkZ].GetVoxel(x, y, z);
    }

    public void SetVoxel(int x, int y, int z, int block)
    {
        int chunkX = x % REGION_SIZE;
        int chunkY = y % REGION_SIZE;
        int chunkZ = z % REGION_SIZE;
        chunks[chunkX, chunkY, chunkZ].SetVoxel(x, y, z, block);
    }
}