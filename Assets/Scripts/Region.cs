using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class Region
{
    public static int REGION_SIZE = 16;
    static int HEADER_BLOCK_SIZE = 6;
    static int HEADER_BLOCK_COUNT = 256;
    static int HEADER_SIZE = HEADER_BLOCK_SIZE * HEADER_BLOCK_COUNT;
    Chunk[,,] chunks = new Chunk[REGION_SIZE, REGION_SIZE, REGION_SIZE];
    List<string> Header = new List<string>();

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
                                chunk.setVoxel(voxelX, voxelY, voxelZ, voxelData);
                            }
                        }
                    }
                }
            }
        }
    }

    public string getHeaderLine(int index)
    {
        return Header[index];
    }

    public int getHeaderCount()
    {
        return Header.Count;
    }

    public Chunk getChunk(int x, int y, int z)
    {
        return chunks[x, y, z];
    }

    public int getVoxel(int x, int y, int z)
    {
        int chunkX = x / Chunk.CHUNK_SIZE;
        int chunkY = y / Chunk.CHUNK_SIZE;
        int chunkZ = z / Chunk.CHUNK_SIZE;
        int voxelX = x % Chunk.CHUNK_SIZE;
        int voxelY = y % Chunk.CHUNK_SIZE;
        int voxelZ = z % Chunk.CHUNK_SIZE;
        return chunks[chunkX, chunkY, chunkZ].getVoxel(voxelX, voxelY, voxelZ);
    }

    void setVoxel(int x, int y, int z, int voxel)
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