using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine.Networking;

public class BlockPalette
{
    List<BlockType> blocks = new List<BlockType>();
    static int TEXTURE_SIZE = 16;
    // texture atlas
    Texture2D textureAtlas;

    public BlockPalette(string[] blocks, ChunkMesh[] chunks)
    {
        foreach (string block in blocks)
        {
            this.blocks.Add(jsonToBlock(block));
        }
        GenerateTextureAtlas(chunks);
    }

    public static void LoadTexture(string url, Action<Texture2D> onTextureLoaded)
    {
        MainThreadDispatcher.Instance.StartCoroutine(LoadTextureCoroutine(url, onTextureLoaded));
    }

    static IEnumerator LoadTextureCoroutine(string url, Action<Texture2D> onTextureLoaded)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onTextureLoaded?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                onTextureLoaded?.Invoke(texture);
            }
        }
    }

    void GenerateTextureAtlas(ChunkMesh[] chunks)
    {
        textureAtlas = new Texture2D(TEXTURE_SIZE * blocks.Count, TEXTURE_SIZE);
        textureAtlas.filterMode = FilterMode.Point;
        textureAtlas.wrapMode = TextureWrapMode.Clamp;

        int xOffset = 0;
        foreach (BlockType block in blocks)
        {
            if (!string.IsNullOrEmpty(block.GetTexture()))
            {
                LoadTexture(block.GetTexture(), (texture2D) =>
                {
                    block.SetTexture2D(texture2D);

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
                        ApplyTextureToMaterial(chunks);
                    }
                });
            }
        }
    }

    void ApplyTextureToMaterial(ChunkMesh[] chunks)
    {
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = textureAtlas;
        foreach (ChunkMesh chunk in chunks)
        {
            chunk.SetMaterial(material);
        }
    }
    
    BlockType jsonToBlock(string json)
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
}