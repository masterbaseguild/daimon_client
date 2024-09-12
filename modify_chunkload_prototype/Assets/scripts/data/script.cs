/*
    public List<object> worldFile = new List<object>
    {
        new List<object>
        {
            new List<BlockType[]>
            {
                new BlockType[]
                {
                    BlockType.humus
                }
            }
        }
    };


    private void loadChunk(chunkdata chunkData)
    {
        if(chunkData.planetPos.x/chunkData.chunkDim >= planetFile.Count || chunkData.planetPos.y/chunkData.chunkDim >= ((List<object>)planetFile[chunkData.planetPos.x/chunkData.chunkDim]).Count || chunkData.planetPos.z/chunkData.chunkDim >= ((List<blocktype[]>)((List<object>)planetFile[chunkData.planetPos.x/chunkData.chunkDim])[chunkData.planetPos.y/chunkData.chunkDim]).Count)
        {
            generateAirChunk(chunkData);
        }
        else
        {
            blocktype[] chunkFile = ((List<blocktype[]>)((List<object>)planetFile[chunkData.planetPos.x/chunkData.chunkDim])[chunkData.planetPos.y/chunkData.chunkDim])[chunkData.planetPos.z/chunkData.chunkDim];
            for (int x = 0; x < chunkData.chunkDim; x++)
            {
                for (int z = 0; z < chunkData.chunkDim; z++)
                {
                    for (int y = 0; y < chunkData.chunkDim; y++)
                    {
                        blocktype voxelType = blocktype.air;
                        if(x + z * chunkData.chunkDim + y * chunkData.chunkDim * chunkData.chunkDim < chunkFile.Length)
                        voxelType = chunkFile[x + z * chunkData.chunkDim + y * chunkData.chunkDim * chunkData.chunkDim];
                        chunk.setBlock(chunkData, new Vector3Int(x, y, z), voxelType);
                    }
                }
            }
        }
    }

    internal void generateAirChunk(chunkdata chunkData)
    {
        for (int x = 0; x < chunkData.chunkDim; x++)
        {
            for (int z = 0; z < chunkData.chunkDim; z++)
            {
                for (int y = 0; y < chunkData.chunkDim; y++)
                {
                    blocktype voxelType = blocktype.air;
                    chunk.setBlock(chunkData, new Vector3Int(x, y, z), voxelType);
                }
            }
        }
    }

    private void generateVoxels(chunkdata chunkData)
    {
        if(chunkData.planetPos.y == 0)
        for (int x = 0; x < chunkData.chunkDim; x++)
        {
            for (int z = 0; z < chunkData.chunkDim; z++)
            {
                float noiseValue = Mathf.PerlinNoise((chunkData.planetPos.x + x) * 0.03f, (chunkData.planetPos.z + z) * 0.03f);
                int groundPos = Mathf.RoundToInt(noiseValue * chunkDim/2);
                for (int y = 0; y < chunkData.chunkDim; y++)
                {
                    blocktype voxelType = blocktype.earth;
                    if (y > groundPos)
                    {
                        if (y < 48)
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
        else
        {
            for (int x = 0; x < chunkData.chunkDim; x++)
            {
                for (int z = 0; z < chunkData.chunkDim; z++)
                {
                    for (int y = 0; y < chunkData.chunkDim; y++)
                    {
                        blocktype voxelType = blocktype.air;
                        chunk.setBlock(chunkData, new Vector3Int(x, y, z), voxelType);
                    }
                }
            }
        }
    }
*/