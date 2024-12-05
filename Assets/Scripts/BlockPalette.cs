using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine.Networking;

public class BlockPalette
{
    static List<BlockType> blocks = new List<BlockType>();
    static int TEXTURE_SIZE = 16;
    // texture atlas
    static Texture2D textureAtlas;
    public static Material material = new Material(Shader.Find("Unlit/Texture"));

    static Action OnAllTexturesLoaded;

    public BlockPalette(string[] blocks, ChunkMesh[] chunks)
    {
        foreach (string block in blocks)
        {
            BlockPalette.blocks.Add(jsonToBlock(block));
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

    // debug function to save texture to disk
    static void saveTextureToDisk(Texture2D texture)
    {
        string macPath = "/Users/entity/Downloads/texture.png";
        string winPath = "C:\\Users\\entity\\Downloads\\texture.png";
        string path = Application.platform == RuntimePlatform.OSXEditor ? macPath : winPath;
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Saved texture to " + path);
    }

    static void GenerateTextureAtlas(ChunkMesh[] chunks)
    {
        Debug.Log("Generating texture atlas for " + blocks.Count + " blocks");
        textureAtlas = new Texture2D(TEXTURE_SIZE * blocks.Count, TEXTURE_SIZE);
        textureAtlas.filterMode = FilterMode.Point;
        textureAtlas.wrapMode = TextureWrapMode.Clamp;
        Debug.Log("Texture atlas size: " + textureAtlas.width + "x" + textureAtlas.height);

        int xOffset = 0;
        foreach (BlockType block in blocks)
        {
            block.OnTextureLoaded += () =>
            {
                Debug.Log("Adding texture for block " + block.GetTexture());
                textureAtlas.SetPixels(xOffset, 0, TEXTURE_SIZE, TEXTURE_SIZE, block.GetTexture2D().GetPixels());
                xOffset += TEXTURE_SIZE;
                Debug.Log("Done adding texture for block " + block.GetTexture());
            };
        }
        MainThreadDispatcher.Instance.StartCoroutine(AllTexturesLoadedCoroutine());
        OnAllTexturesLoaded += () =>
        {
            Debug.Log("All textures loaded");
            textureAtlas.Apply();
            saveTextureToDisk(textureAtlas);
            material.mainTexture = textureAtlas;
        };
    }

    static IEnumerator AllTexturesLoadedCoroutine() {
        Debug.Log("Waiting for all textures to load");
        while (true)
        {
            bool allLoaded = true;
            foreach (BlockType block in blocks)
            {
                if (block.isLoaded == false)
                {
                    allLoaded = false;
                    break;
                }
            }
            if (allLoaded)
            {
                Debug.Log("All textures loaded");
                OnAllTexturesLoaded?.Invoke();
                break;
            }
            yield return null;
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