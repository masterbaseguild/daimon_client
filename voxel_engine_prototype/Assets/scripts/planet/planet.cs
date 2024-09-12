using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planet : MonoBehaviour
{
    public int mapDimInChunks = 16;
    public int chunkDim = 16;
    public int waterThreshold = 48;
    public float noiseScale = 0.03f;
    public GameObject chunkPrefab;

    public GameObject parentObject;

    Dictionary<Vector3Int, chunkdata> chunkDataDictionary = new Dictionary<Vector3Int, chunkdata>();
    Dictionary<Vector3Int, chunkrenderer> chunkDictionary = new Dictionary<Vector3Int, chunkrenderer>();

    public void generatePlanet()
    {
        chunkDataDictionary.Clear();
        foreach (chunkrenderer chunk in chunkDictionary.Values)
        {
            Destroy(chunk.gameObject);
        }
        chunkDictionary.Clear();

        for (int x = 0; x < mapDimInChunks; x++)
        {
            for (int z = 0; z < mapDimInChunks; z++)
            {

                chunkdata chunkData = new chunkdata(chunkDim, this, new Vector3Int(x * chunkDim, 0, z * chunkDim));
                generateVoxels(chunkData);
                chunkDataDictionary.Add(chunkData.planetPos, chunkData);
            }
        }

        foreach (chunkdata chunkData in chunkDataDictionary.Values)
        {
            meshdata meshData = chunk.getChunkMeshData(chunkData);
            GameObject chunkObject = Instantiate(chunkPrefab, chunkData.planetPos, Quaternion.identity);
            chunkObject.transform.parent = parentObject.transform;
            chunkrenderer chunkrenderer = chunkObject.GetComponent<chunkrenderer>();
            chunkDictionary.Add(chunkData.planetPos, chunkrenderer);
            chunkrenderer.initChunk(chunkData);
            chunkrenderer.updateChunk(meshData);

        }
    }

        private void generateVoxels(chunkdata chunkData)
    {
        for (int x = 0; x < chunkData.chunkDim; x++)
        {
            for (int z = 0; z < chunkData.chunkDim; z++)
            {
                float noiseValue = Mathf.PerlinNoise((chunkData.planetPos.x + x) * noiseScale, (chunkData.planetPos.z + z) * noiseScale);
                int groundPos = Mathf.RoundToInt(noiseValue * chunkDim/2);
                for (int y = 0; y < chunkDim; y++)
                {
                    blocktype voxelType = blocktype.earth;
                    if (y > groundPos)
                    {
                        if (y < waterThreshold)
                        {
                            voxelType = blocktype.water;
                        }
                        else
                        {
                            voxelType = blocktype.air;
                        }

                    }
                    else if (y == groundPos)
                    {
                        voxelType = blocktype.humus;
                    }

                    chunk.setBlock(chunkData, new Vector3Int(x, y, z), voxelType);
                }
            }
        }
    }

    internal blocktype getBlockFromChunkCoords(chunkdata chunkData, int x, int y, int z)
    {
        Vector3Int pos = chunk.chunkPosFromBlockCoords(this, x, y, z);
        chunkdata containerChunk = null;

        chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk == null)
            return blocktype.thevoid;
        Vector3Int blockInChunkCoordinates = chunk.getBlockInChunkCoords(containerChunk, new Vector3Int(x, y, z));
        return chunk.getBlockFromChunkCoords(containerChunk, blockInChunkCoordinates);
    }

    private void Start()
    {
        generatePlanet();
    }
}