using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class WorldData
{
    public List<string> blockmap;
    public List<List<List<int[]>>> chunks;
}