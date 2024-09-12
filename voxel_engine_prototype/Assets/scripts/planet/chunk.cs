using System;
using UnityEngine;

public static class chunk
{

    public static void loopThroughBlocks(chunkdata chunkData, Action<int, int, int> actionToPerform)
    {
        for (int index = 0; index < chunkData.blocks.Length; index++)
        {
            var position = getPosFromIndex(chunkData, index);
            actionToPerform(position.x, position.y, position.z);
        }
    }

    private static Vector3Int getPosFromIndex(chunkdata chunkData, int index)
    {
        int x = index % chunkData.chunkDim;
        int y = (index / chunkData.chunkDim) % chunkData.chunkDim;
        int z = index / (chunkData.chunkDim * chunkData.chunkDim);
        return new Vector3Int(x, y, z);
    }

    private static int getIndexFromPos(chunkdata chunkData, int x, int y, int z)
    {
        return x + chunkData.chunkDim * y + chunkData.chunkDim * chunkData.chunkDim * z;
    }

    private static bool inRange(chunkdata chunkData, int axisCoord)
    {
        if (axisCoord < 0 || axisCoord >= chunkData.chunkDim)
            return false;

        return true;
    }

    public static blocktype getBlockFromChunkCoords(chunkdata chunkData, Vector3Int chunkCoords)
    {
        return getBlockFromChunkCoords(chunkData, chunkCoords.x, chunkCoords.y, chunkCoords.z);
    }

    public static blocktype getBlockFromChunkCoords(chunkdata chunkData, int x, int y, int z)
    {
        if (inRange(chunkData, x) && inRange(chunkData, y) && inRange(chunkData, z))
        {
            int index = getIndexFromPos(chunkData, x, y, z);
            return chunkData.blocks[index];
        }

        return chunkData.planetRef.getBlockFromChunkCoords(chunkData, chunkData.planetPos.x + x, chunkData.planetPos.y + y, chunkData.planetPos.z + z);
    }

    public static Vector3Int getBlockInChunkCoords(chunkdata chunkData, Vector3Int pos)
    {
        return new Vector3Int
        {
            x = pos.x - chunkData.planetPos.x,
            y = pos.y - chunkData.planetPos.y,
            z = pos.z - chunkData.planetPos.z
        };
    }

    public static void setBlock(chunkdata chunkData, Vector3Int localPos, blocktype block)
    {
        if (inRange(chunkData, localPos.x) && inRange(chunkData, localPos.y) && inRange(chunkData, localPos.z))
        {
            int index = getIndexFromPos(chunkData, localPos.x, localPos.y, localPos.z);
            chunkData.blocks[index] = block;
        }
        else
        {
            throw new Exception("Need to ask Planet for appropriate chunk");
        }
    }

    internal static Vector3Int chunkPosFromBlockCoords(planet planet, int x, int y, int z)
    {
        Vector3Int pos = new Vector3Int
        {
            x = Mathf.FloorToInt(x / (float)planet.chunkDim) * planet.chunkDim,
            y = Mathf.FloorToInt(y / (float)planet.chunkDim) * planet.chunkDim,
            z = Mathf.FloorToInt(z / (float)planet.chunkDim) * planet.chunkDim
        };
        return pos;
    }

    public static meshdata getChunkMeshData(chunkdata chunkData)
    {
        meshdata meshData = new meshdata(true);

        loopThroughBlocks(chunkData, (x, y, z) => meshData = blockhelper.getMeshData(chunkData, x, y, z, meshData, chunkData.blocks[getIndexFromPos(chunkData, x, y, z)]));

        return meshData;
    }
}