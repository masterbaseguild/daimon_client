using UnityEngine;
using System;

public class BlockType
{
    private readonly string id;
    private readonly string display; // display name
    private readonly bool isOpaque;
    private readonly bool isConcrete;
    private readonly string texture; // texture url
    private readonly Action OnTextureLoaded;
    private Texture2D texture2D;
    public bool isLoaded = false;

    public BlockType(string id, string display, int type)
    {
        this.id = id;
        this.display = display;
        // type 1: opaque concrete (e.g. minecraft stone)
        // type 2: transparent concrete (e.g. minecraft glass)
        // type 3: transparent non-concrete (e.g. minecraft water)
        // type 4: opaque non-concrete (e.g. minecraft lava)

        // NOTE: transparent does not mean it has no texture, it just means the texture has transparency
        isOpaque = type is 1 or 4;
        isConcrete = type is 1 or 2;
        //set texture to https://media.daimon.world/public/blocks/{id}.png
        texture = $"https://media.daimon.world/public/blocks/{this.id}.png";
        if (!string.IsNullOrEmpty(texture))
        {
            BlockPalette.LoadTexture(texture, (texture2D) =>
            {
                this.texture2D = texture2D;
                OnTextureLoaded?.Invoke();
                isLoaded = true;
            });
        }
        else
        {
            texture2D = null;
            isLoaded = true;
        }
    }

    public Texture2D GetTexture2D()
    {
        return texture2D;
    }

    public bool IsOpaque()
    {
        return isOpaque;
    }

    public bool IsConcrete()
    {
        return isConcrete;
    }
}