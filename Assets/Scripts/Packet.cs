using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class Packet
{
    public string type;
    public string[] data;

    public Packet(string message)
    {
        string[] split = message.Split('\t');
        type = split[0];
        data = split.Skip(1).ToArray();
    }

    // the region data is compressed and encoded, so the packet exposes a method to parse it
    public byte[] ParseRegionData()
    {
        string data = this.data[0];
        byte[] bytes = Convert.FromBase64String(data);
        byte[] decompressed = ZlibDecompress(bytes);
        return decompressed;
    }

    byte[] ZlibDecompress(byte[] data)
    {
        using MemoryStream input = new MemoryStream(data);
        // skip zlib header
        input.Seek(2, SeekOrigin.Begin);
        using MemoryStream output = new MemoryStream();
        using DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress);
        dstream.CopyTo(output);
        return output.ToArray();
    }
}