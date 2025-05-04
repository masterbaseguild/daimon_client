using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

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
        public const byte SETBLOCK = 6;
        public const byte SCRIPT = 7;
        public const byte USERCONNECT = 8;
        public const byte USERDISCONNECT = 9;
    }
    public static class Server
    {
        public const byte CONNECT = 0;
        public const byte DISCONNECT = 1;
        public const byte WORLD = 2;
        public const byte NEWPOSITION = 3;
        public const byte KEEPALIVE = 4;
        public const byte CHAT = 5;
        public const byte SETBLOCK = 6;
        public const byte SCRIPT = 7;
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

    public Packet(byte type, string data)
    {
        this.type = type;
        this.data = data.Split('\t');
    }

    // the region data is compressed and encoded, so the packet exposes a method to parse it
    public Region[] ParseWorldData()
    {
        Region[] regions = new Region[data.Length / 4];
        for (int i = 0; i < data.Length; i+=4)
        {
            Vector3Int coordinates = new(int.Parse(data[i]), int.Parse(data[i + 1]), int.Parse(data[i + 2]));
            byte[] bytes = Convert.FromBase64String(data[i + 3]);
            byte[] decompressed = ZlibDecompress(bytes);
            regions[i / 4] = new Region(decompressed, coordinates);
        }
        return regions;
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