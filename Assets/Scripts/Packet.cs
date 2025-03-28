using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class Packet
{
    public static class Client
    {
        public const byte CONNECT = 0;
        public const byte DISCONNECT = 1;
        public const byte WORLD = 2;
        public const byte NEWPOSITION = 3;
        public const byte KEEPALIVE = 4;
        public const byte CHAT = 5;
        public const byte USERCONNECT = 6;
        public const byte USERDISCONNECT = 7;
    }
    public static class Server
    {
        public const byte CONNECT = 0;
        public const byte DISCONNECT = 1;
        public const byte WORLD = 2;
        public const byte NEWPOSITION = 3;
        public const byte KEEPALIVE = 4;
        public const byte CHAT = 5;
    }
    public byte type;
    public string[] data;

    public Packet(string message)
    {
        string[] split = message.Split('\t');
        var typeString = split[0];
        type = byte.Parse(typeString);
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

    private byte[] ZlibDecompress(byte[] data)
    {
        using MemoryStream input = new(data);
        // skip zlib header
        _ = input.Seek(2, SeekOrigin.Begin);
        using MemoryStream output = new();
        using DeflateStream dstream = new(input, CompressionMode.Decompress);
        dstream.CopyTo(output);
        return output.ToArray();
    }
}