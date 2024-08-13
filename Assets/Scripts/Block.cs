using UnityEngine;
using System;

public class Block
{
    public string id;
    public string display;
    public bool isOpaque;
    public bool isConcrete;
    public string texture;
    public Texture2D texture2D;
    public Action OnTextureLoaded;

    public Block(string id, string display, bool isOpaque, bool isConcrete, string texture)
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
                Debug.Log("Texture loaded: " + texture);
                this.texture2D = texture2D;
                Debug.Log(texture2D);
                OnTextureLoaded?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Block " + id + " has no texture");
            this.texture2D = null;
        }
    }
}