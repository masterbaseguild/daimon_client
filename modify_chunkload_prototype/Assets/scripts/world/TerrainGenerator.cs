using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public ChunkData GenerateChunkData(ChunkData data)
    {
        if(data.worldPosition.y == 0)
        {
            for (int x = 0; x < data.chunkSize; x++)
            {
                for (int z = 0; z < data.chunkSize; z++)
                {
                    for (int y = 0; y < data.chunkSize; y++)
                    {
                        float noiseValue = Mathf.PerlinNoise((data.worldPosition.x + x) * 0.03f, (data.worldPosition.z + z) * 0.03f);
                        int groundPos = Mathf.RoundToInt(noiseValue * data.chunkSize/2);
                        BlockType voxelType = BlockType.Earth;
                        if (y > groundPos)
                        {
                            if (y < 5)
                            {
                                voxelType = BlockType.Water;
                            }
                            else
                            {
                                voxelType = BlockType.Air;
                            }

                        }
                        else if (y == groundPos)
                        {
                            voxelType = BlockType.Humus;
                        }

                        Chunk.SetBlock(data, new Vector3Int(x, y, z), voxelType);
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < data.chunkSize; x++)
            {
                for (int z = 0; z < data.chunkSize; z++)
                {
                    for (int y = 0; y < data.chunkSize; y++)
                    {
                        Chunk.SetBlock(data, new Vector3Int(x, y, z), BlockType.Air);
                    }
                }
            }
        }
        return data;
    }
}