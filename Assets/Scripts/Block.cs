using UnityEngine;
using Newtonsoft.Json;

public class Block
{
    public string id;
    public string display;
    public bool isOpaque;
    public bool isConcrete;
    public Texture2D texture;

    public Block(string id, string display, bool isOpaque, bool isConcrete, Texture2D texture)
    {
        this.id = id;
        this.display = display;
        this.isOpaque = isOpaque;
        this.isConcrete = isConcrete;
        this.texture = texture;
    }

    public Block jsonToBlock(string json)
    {
        return JsonConvert.DeserializeObject<Block>(json);
    }
}