using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class blockhelper
{
    private static direction[] directions =
    {
        direction.backwards,
        direction.down,
        direction.forward,
        direction.left,
        direction.right,
        direction.up
    };

    public static meshdata getMeshData(chunkdata mainChunk, int x, int y, int z, meshdata meshData, blocktype blockType)
    {
        if (blockType == blocktype.air || blockType == blocktype.thevoid)
            return meshData;

        foreach (direction direction in directions)
        {
            var neighbourBlockCoords = new Vector3Int(x, y, z) + direction.getVector();
            var neighbourBlockType = chunk.getBlockFromChunkCoords(mainChunk, neighbourBlockCoords);

            if (neighbourBlockType != blocktype.thevoid && blockdatamanager.blockTextureDataDictionary[neighbourBlockType].isSolid == false)
            {

                if (blockType == blocktype.water)
                {
                    if (neighbourBlockType == blocktype.air)
                        meshData.waterMesh = getFaceDataIn(direction, mainChunk, x, y, z, meshData.waterMesh, blockType);
                }
                else
                {
                    meshData = getFaceDataIn(direction, mainChunk, x, y, z, meshData, blockType);
                }

            }
        }

        return meshData;
    }

    public static meshdata getFaceDataIn(direction direction, chunkdata chunk, int x, int y, int z, meshdata meshData, blocktype blockType)
    {
        getFaceVertices(direction, x, y, z, meshData, blockType);
        meshData.addQuadTriangles(blockdatamanager.blockTextureDataDictionary[blockType].gensCollider);
        meshData.uv.AddRange(faceUVs(direction, blockType));


        return meshData;
    }

    public static void getFaceVertices(direction direction, int x, int y, int z, meshdata meshData, blocktype blockType)
    {
        var generatesCollider = blockdatamanager.blockTextureDataDictionary[blockType].gensCollider;
        switch (direction)
        {
            case direction.backwards:
                meshData.addVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                break;
            case direction.forward:
                meshData.addVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                break;
            case direction.left:
                meshData.addVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                break;

            case direction.right:
                meshData.addVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                break;
            case direction.down:
                meshData.addVertex(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f), generatesCollider);
                break;
            case direction.up:
                meshData.addVertex(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                meshData.addVertex(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f), generatesCollider);
                break;
            default:
                break;
        }
    }

    public static Vector2[] faceUVs(direction direction, blocktype blockType)
    {
        Vector2[] uvs = new Vector2[4];
        var tilePos = texturePosition(direction, blockType);

        uvs[0] = new Vector2(blockdatamanager.tileSizeX * tilePos.x + blockdatamanager.tileSizeX - blockdatamanager.textureOffset,
            blockdatamanager.tileSizeY * tilePos.y + blockdatamanager.textureOffset);

        uvs[1] = new Vector2(blockdatamanager.tileSizeX * tilePos.x + blockdatamanager.tileSizeX - blockdatamanager.textureOffset,
            blockdatamanager.tileSizeY * tilePos.y + blockdatamanager.tileSizeY - blockdatamanager.textureOffset);

        uvs[2] = new Vector2(blockdatamanager.tileSizeX * tilePos.x + blockdatamanager.textureOffset,
            blockdatamanager.tileSizeY * tilePos.y + blockdatamanager.tileSizeY - blockdatamanager.textureOffset);

        uvs[3] = new Vector2(blockdatamanager.tileSizeX * tilePos.x + blockdatamanager.textureOffset,
            blockdatamanager.tileSizeY * tilePos.y + blockdatamanager.textureOffset);

        return uvs;
    }

    public static Vector2Int texturePosition(direction direction, blocktype blockType)
    {
        return direction switch
        {
            direction.up => blockdatamanager.blockTextureDataDictionary[blockType].up,
            direction.down => blockdatamanager.blockTextureDataDictionary[blockType].down,
            _ => blockdatamanager.blockTextureDataDictionary[blockType].side
        };
    }
}