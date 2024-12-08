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

    public BlockType(string id, string display, bool isOpaque, bool isConcrete, string texture)
    {
        this.id = id;
        this.display = display;
        this.isOpaque = isOpaque;
        this.isConcrete = isConcrete;
        this.texture = texture;
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
}