using UnityEngine;
using System;

public class BlockType
{
    public string id;
    public string display;
    public bool isOpaque;
    public bool isConcrete;
    public string texture;
    public Texture2D texture2D;
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
            TextureLoader.LoadTexture(texture, (texture2D) =>
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
}