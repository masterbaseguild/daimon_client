using System.Collections.Generic;
using System;
using UnityEngine;

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
    private static readonly int HEADER_SIZE_BUFFER_SIZE = 4; // in bytes
    private static readonly int HEADER_BLOCK_SIZE = 6;
    private readonly List<string> Header = new();

    private readonly Vector3Int coordinates;

    // list of all the chunk meshes in all regions
    private readonly ChunkMesh[] chunkMeshes = new ChunkMesh[REGION_SIZE * REGION_SIZE * REGION_SIZE];

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

    public Region(byte[] data, Vector3Int coordinates)
    {
        this.coordinates = coordinates;

        // split header and content
        byte[] headerSizeBuffer = new byte[HEADER_SIZE_BUFFER_SIZE];
        Array.Copy(data, headerSizeBuffer, HEADER_SIZE_BUFFER_SIZE);
        int headerSize = BitConverter.ToInt32(headerSizeBuffer, 0);

        int byteWidth;
        if (headerSize <= 0xFF)
        {
            byteWidth = 1;
        }
        else if (headerSize <= 0xFFFF)
        {
            byteWidth = 2;
        }
        else
        {
            byteWidth = 4;
        }

        byte[] headerBuffer = new byte[headerSize * HEADER_BLOCK_SIZE];
        byte[] contentBuffer = new byte[data.Length - headerSize * HEADER_BLOCK_SIZE - HEADER_SIZE_BUFFER_SIZE];
        Array.Copy(data, HEADER_SIZE_BUFFER_SIZE, headerBuffer, 0, headerSize * HEADER_BLOCK_SIZE);
        Array.Copy(data, HEADER_SIZE_BUFFER_SIZE + headerSize * HEADER_BLOCK_SIZE, contentBuffer, 0, contentBuffer.Length);

        // parse header
        for (int i = 0; i < headerSize; i++)
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
                                byte[] voxelData = new byte[byteWidth];
                                Array.Copy(contentBuffer, index * byteWidth, voxelData, 0, byteWidth);
                                int voxelValue = byteWidth switch
                                {
                                    1 => voxelData[0],
                                    2 => BitConverter.ToInt16(voxelData, 0),
                                    4 => BitConverter.ToInt32(voxelData, 0),
                                    _ => throw new InvalidOperationException($"Unsupported byteWidth: {byteWidth}")
                                };
                                chunk.SetVoxel(voxelX, voxelY, voxelZ, voxelValue);
                                index++;
                            }
                        }
                    }
                }
            }
        }
        // parse mini blocks
        for (int chunkX = 0; chunkX < REGION_SIZE; chunkX++)
        {
            for (int chunkY = 0; chunkY < REGION_SIZE; chunkY++)
            {
                for (int chunkZ = 0; chunkZ < REGION_SIZE; chunkZ++)
                {
                    Chunk chunk = chunks[chunkX, chunkY, chunkZ];
                    for (int voxelX = 0; voxelX < Chunk.CHUNK_SIZE*2; voxelX++)
                    {
                        for (int voxelY = 0; voxelY < Chunk.CHUNK_SIZE*2; voxelY++)
                        {
                            for (int voxelZ = 0; voxelZ < Chunk.CHUNK_SIZE*2; voxelZ++)
                            {
                                byte[] voxelData = new byte[byteWidth];
                                Array.Copy(contentBuffer, index * byteWidth, voxelData, 0, byteWidth);
                                int voxelValue = byteWidth switch
                                {
                                    1 => voxelData[0],
                                    2 => BitConverter.ToInt16(voxelData, 0),
                                    4 => BitConverter.ToInt32(voxelData, 0),
                                    _ => throw new InvalidOperationException($"Unsupported byteWidth: {byteWidth}")
                                };
                                chunk.SetMiniVoxel(voxelX, voxelY, voxelZ, voxelValue);
                                index++;
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
        return chunks[chunkX % REGION_SIZE, chunkY % REGION_SIZE, chunkZ % REGION_SIZE];
    }

    public bool IsChunkEmpty(int x, int y, int z)
    {
        Chunk chunk = chunks[x, y, z];
        return chunk.IsEmpty();
    }

    public int GetVoxel(int x, int y, int z)
    {
        int chunkX = x / Chunk.CHUNK_SIZE;
        int chunkY = y / Chunk.CHUNK_SIZE;
        int chunkZ = z / Chunk.CHUNK_SIZE;
        return chunks[chunkX % REGION_SIZE, chunkY % REGION_SIZE, chunkZ % REGION_SIZE].GetVoxel(x, y, z);
    }

    public int GetMiniVoxel(int x, int y, int z)
    {
        int chunkX = x / (Chunk.CHUNK_SIZE*2);
        int chunkY = y / (Chunk.CHUNK_SIZE*2);
        int chunkZ = z / (Chunk.CHUNK_SIZE*2);
        return chunks[chunkX % REGION_SIZE, chunkY % REGION_SIZE, chunkZ % REGION_SIZE].GetMiniVoxel(x, y, z);
    }

    public void SetVoxel(int x, int y, int z, string block)
    {
        int blockIdIndex = Header.IndexOf(block);
        if (blockIdIndex == -1)
        {
            // add blockId to header
            blockIdIndex = Header.Count;
            Header.Add(block);
        }
        int chunkX = x / Chunk.CHUNK_SIZE;
        int chunkY = y / Chunk.CHUNK_SIZE;
        int chunkZ = z / Chunk.CHUNK_SIZE;
        chunks[chunkX % REGION_SIZE, chunkY % REGION_SIZE, chunkZ % REGION_SIZE].SetVoxel(x, y, z, blockIdIndex);
    }

    public void SetMiniVoxel(int x, int y, int z, string block)
    {
        int blockIdIndex = Header.IndexOf(block);
        if (blockIdIndex == -1)
        {
            // add blockId to header
            blockIdIndex = Header.Count;
            Header.Add(block);
        }
        int chunkX = x / (Chunk.CHUNK_SIZE*2);
        int chunkY = y / (Chunk.CHUNK_SIZE*2);
        int chunkZ = z / (Chunk.CHUNK_SIZE*2);
        chunks[chunkX % REGION_SIZE, chunkY % REGION_SIZE, chunkZ % REGION_SIZE].SetMiniVoxel(x, y, z, blockIdIndex);
    }

    public Vector3Int GetCoordinates()
    {
        return coordinates;
    }
    public ChunkMesh[] GetChunkMeshes()
    {
        return chunkMeshes;
    }
}