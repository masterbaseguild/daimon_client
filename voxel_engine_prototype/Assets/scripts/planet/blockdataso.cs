using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="BlockData" ,menuName ="Data/BlockData")]
public class blockdataso : ScriptableObject
{
    public float textureSizeX, textureSizeY;
    public List<textureData> textureDataList;
}

[Serializable]
public class textureData
{
    public blocktype blockType;
    public Vector2Int up, down, side;
    public bool isSolid = true;
    public bool gensCollider = true;
}