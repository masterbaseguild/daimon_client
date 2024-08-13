using UnityEngine;

public class Block
{
    public string id;
    public string display;
    public bool isOpaque;
    public bool isConcrete;
    public string texture;
    public Texture2D texture2D;

    public Block(string id, string display, bool isOpaque, bool isConcrete, string texture)
    {
        this.id = id;
        this.display = display;
        this.isOpaque = isOpaque;
        this.isConcrete = isConcrete;
        // if texture is not null, set it
        if (texture != null)
        {
            this.texture = texture;
            this.texture2D = parseTexture(texture);
        }
    }

    private Texture2D parseTexture(string texture)
    {
        byte[] imageBytes = System.Text.Encoding.UTF8.GetBytes(texture);
        Texture2D texture2D = new Texture2D(16, 16);
        texture2D.LoadImage(imageBytes);
        return texture2D;
    }
}