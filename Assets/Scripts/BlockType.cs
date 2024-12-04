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
            });
        }
        else
        {
            this.texture2D = null;
        }
    }

    public string GetTexture()
    {
        return texture;
    }

    public void SetTexture2D(Texture2D texture2D)
    {
        this.texture2D = texture2D;
    }

    public Texture2D GetTexture2D()
    {
        return texture2D;
    }
}