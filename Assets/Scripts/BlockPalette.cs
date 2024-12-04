using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class BlockPalette
{
    public List<BlockType> blocks = new List<BlockType>();
    public int TEXTURE_SIZE = 16;
    // texture atlas
    public Texture2D textureAtlas;

    public bool isAtlasReady = false;

    public BlockPalette(string[] blocks, ChunkMesh[] chunks)
    {
        foreach (string block in blocks)
        {
            this.blocks.Add(jsonToBlock(block));
        }
        GenerateTextureAtlas(chunks);
    }

    private void GenerateTextureAtlas(ChunkMesh[] chunks)
    {
        textureAtlas = new Texture2D(TEXTURE_SIZE * blocks.Count, TEXTURE_SIZE);
        textureAtlas.filterMode = FilterMode.Point;
        textureAtlas.wrapMode = TextureWrapMode.Clamp;

        int xOffset = 0;
        foreach (BlockType block in blocks)
        {
            if (!string.IsNullOrEmpty(block.texture))
            {
                TextureLoader.LoadTexture(block.texture, (texture2D) =>
                {
                    block.texture2D = texture2D;

                    if (xOffset + TEXTURE_SIZE > textureAtlas.width)
                    {
                        Debug.LogError("Texture atlas is full");
                        return;
                    }

                    textureAtlas.SetPixels(xOffset, 0, TEXTURE_SIZE, TEXTURE_SIZE, texture2D.GetPixels());
                    xOffset += TEXTURE_SIZE;
                    textureAtlas.Apply();

                    if (xOffset >= textureAtlas.width)
                    {
                        isAtlasReady = true;
                        ApplyTextureToMaterial(chunks);
                    }
                });
            }
        }
    }

    private void ApplyTextureToMaterial(ChunkMesh[] chunks)
    {
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = textureAtlas;
        foreach (ChunkMesh chunk in chunks)
        {
            chunk.gameObject.GetComponent<MeshRenderer>().material = material;
        }
    }
    
    private BlockType jsonToBlock(string json)
    {
        return JsonConvert.DeserializeObject<BlockType>(json);
    }

    public BlockType GetBlockType(int index)
    {
        return blocks[index];
    }

    // get texture coordinates for a block
    public Vector2[] GetBlockUVs(BlockType block)
    {
        int x = blocks.IndexOf(block) * TEXTURE_SIZE;
        float atlasWidth = textureAtlas.width;
        float atlasHeight = textureAtlas.height;
        return new Vector2[]
        {
            new Vector2(x / atlasWidth, 0),
            new Vector2((x + TEXTURE_SIZE) / atlasWidth, 0),
            new Vector2((x + TEXTURE_SIZE) / atlasWidth, TEXTURE_SIZE / atlasHeight),
            new Vector2(x / atlasWidth, TEXTURE_SIZE / atlasHeight)
        };
    }

    // get texture coordinates for a block by id

    public Vector2[] GetBlockUVs(string id)
    {
        BlockType block = blocks.Find(b => b.id == id);
        return GetBlockUVs(block);
    }
}