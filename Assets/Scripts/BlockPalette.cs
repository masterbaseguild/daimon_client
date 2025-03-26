using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine.Networking;

// the block palette stores and handles the complete list of block types
// TODO: i don't remember how the async part of this works, need to document it and eventually refactor
public class BlockPalette
{
    World world;
    List<BlockType> blocks = new List<BlockType>();
    int TEXTURE_SIZE = 16;
    // texture atlas: a single big image containing all block textures,
    // which the game extracts via UV mapping
    Texture2D textureAtlas;
    Action OnAllTexturesLoaded;

    public BlockPalette(string[] blocks, ChunkMesh[] chunks)
    {
        world = GameObject.Find("World").GetComponent<World>();
        foreach (string block in blocks)
        {
            this.blocks.Add(jsonToBlock(block));
        }
        GenerateTextureAtlas(chunks);
    }

    public static void LoadTexture(string url, Action<Texture2D> onTextureLoaded)
    {
        // since this coroutine uses the unity networking api, it must be run on the main thread
        MainThreadDispatcher.Instance.StartCoroutine(LoadTextureCoroutine(url, onTextureLoaded));
    }

    static IEnumerator LoadTextureCoroutine(string url, Action<Texture2D> onTextureLoaded)
    {
        using UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
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

    // debug function to save texture to disk
    void saveTextureToDisk(Texture2D texture)
    {
        string macPath = "/Users/entity/Downloads/texture.png";
        string winPath = "C:\\Users\\Dario\\Downloads\\texture.png";
        string path = Application.platform == RuntimePlatform.OSXEditor ? macPath : winPath;
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("Saved texture to " + path);
    }

    void GenerateTextureAtlas(ChunkMesh[] chunks)
    {
        textureAtlas = new Texture2D(TEXTURE_SIZE * blocks.Count, TEXTURE_SIZE);
        textureAtlas.filterMode = FilterMode.Point;
        textureAtlas.wrapMode = TextureWrapMode.Clamp;

        int xOffset = 0;
        MainThreadDispatcher.Instance.StartCoroutine(AllTexturesLoadedCoroutine());
        OnAllTexturesLoaded += () =>
        {
            foreach (BlockType block in blocks)
            {
                if (block.GetTexture2D() != null)
                {
                    textureAtlas.SetPixels(xOffset, 0, TEXTURE_SIZE, TEXTURE_SIZE, block.GetTexture2D().GetPixels());
                }
                xOffset += TEXTURE_SIZE;
            }
            textureAtlas.Apply();
            //saveTextureToDisk(textureAtlas);
            world.SetTexture(textureAtlas);
        };
    }

    IEnumerator AllTexturesLoadedCoroutine() {
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

    public Vector2[] GetBlockUVs(int index)
    {
        return GetBlockUVs(blocks[index]);
    }

    // get texture coordinates for a block
    public Vector2[] GetBlockUVs(BlockType block)
    {
        int x = blocks.IndexOf(block) * TEXTURE_SIZE;
        float atlasWidth = textureAtlas.width;
        float atlasHeight = textureAtlas.height;
        return new Vector2[]
        {
            new Vector2(x / atlasWidth, 0), // 0
            new Vector2(x / atlasWidth, TEXTURE_SIZE / atlasHeight), // 3
            new Vector2((x + TEXTURE_SIZE) / atlasWidth, TEXTURE_SIZE / atlasHeight), // 2
            new Vector2((x + TEXTURE_SIZE) / atlasWidth, 0), // 1
        };
    }
}