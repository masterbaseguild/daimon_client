using System.Collections.Generic;

public class Region
{
    public List<string> Header { get; } = new List<string>();
    public List<List<List<List<List<List<byte>>>>>> Data { get; } = new List<List<List<List<List<List<byte>>>>>>();
}