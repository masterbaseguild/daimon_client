using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chunkdata
{
    public blocktype[] blocks;
    public int chunkDim = 16;
    public static int chunkDefaultDim = 16;
    public planet planetRef;
    public Vector3Int planetPos;

    public static bool playerEdited = false;

    public chunkdata(int chunkDim, planet planetRef, Vector3Int planetPos)
    {
        this.chunkDim = chunkDim;
        this.planetRef = planetRef;
        this.planetPos = planetPos;
        blocks = new blocktype[chunkDim*chunkDim*chunkDim];
    }
}