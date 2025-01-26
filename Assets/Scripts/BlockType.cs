using UnityEngine;
using System;

public class BlockType
{
    string id;
    string display;
    bool isOpaque;
    bool isConcrete;
    string texture;
    Texture2D texture2D;
    public Action OnTextureLoaded;
    public bool isLoaded = false;

    public BlockType(string id, string display, int type)
    {
        this.id = id;
        this.display = display;
        // type 1: opaque concrete
        // type 2: transparent concrete
        // type 3: transparent non-concrete
        this.isOpaque = type == 1;
        this.isConcrete = type == 1 || type == 2;
        //set texture to https://media.projectdaimon.com/public/blocks/{id}.png
        texture = $"https://media.projectdaimon.com/public/blocks/{this.id}.png";
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
            this.texture2D = null;
            isLoaded = true;
        }
    }

    public string GetTexture()
    {
        return texture;
    }

    public Texture2D GetTexture2D()
    {
        return texture2D;
    }

    public string GetDisplay()
    {
        return display;
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