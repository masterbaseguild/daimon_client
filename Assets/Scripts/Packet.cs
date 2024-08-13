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
        this.type = split[0];
        this.data = split.Skip(1).ToArray();
    }

    public byte[] parseData()
    {
        string data = this.data[0];
        byte[] bytes = Convert.FromBase64String(data);
        byte[] decompressed = zlibDecompress(bytes);
        return decompressed;
    }

    private byte[] zlibDecompress(byte[] data)
    {
        using (MemoryStream input = new MemoryStream(data))
        {
            // skip zlib header
            input.Seek(2, SeekOrigin.Begin);
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    dstream.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }
}