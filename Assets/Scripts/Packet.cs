public class Packet
{
    public string type;
    public string[] data;

    public Packet(string type, string[] data)
    {
        this.type = type;
        this.data = data;
    }
}