using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class MainUdpClient : MonoBehaviour
{
    static string ip = "arena.masterbaseguild.it";
    static int serverPort = 7689;
    static string username = "";
    static int clientPort;
    static int index;
    static UdpClient client;
    static IPEndPoint remoteIpEndPoint;
    static bool isEnabled = false;

    void Start()
    {
        username = "user" + generateUserSuffix();
        clientPort = generateEphemeralPort();
    }

    public static void SetUsername(string newUsername)
    {
        username = newUsername;
    }

    public static void SetIP(string newIP)
    {
        ip = newIP;
    }

    public static string GetIP()
    {
        return ip;
    }

    void Update()
    {
        if (isEnabled)
        {
        Vector3 position = MainUser.GetPosition();
        Send($"position\t{index}\t{position.x}\t{position.y}\t{position.z}");
        }
    }

    int generateEphemeralPort()
    {
        UdpClient tempClient = new UdpClient(0);
        int port = ((IPEndPoint)tempClient.Client.LocalEndPoint).Port;
        tempClient.Close();
        return port;
    }

    int generateUserSuffix()
    {
        return UnityEngine.Random.Range(1000, 9999);
    }

    // send function
    static void Send(string data)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            client.Send(bytes, bytes.Length);
        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }

    public static void Connect()
    {
        remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(ip), serverPort);
        try {
            client = new UdpClient(clientPort);
            client.Connect(remoteIpEndPoint);
            client.BeginReceive(Get, null);
        }
        catch (Exception e)
        {
            print(e.ToString());
        }
        Send($"connect\t0\t{username}");
        isEnabled = true;
    }

    // get callback
    static void Get(IAsyncResult result)
    {
        try
        {
            byte[] bytes = client.EndReceive(result, ref remoteIpEndPoint);
            string data = Encoding.UTF8.GetString(bytes);
            Packet message = new Packet(data);
            HandlePacket(message);
            client.BeginReceive(Get, null);
            return;
        }
        catch (Exception e)
        {
            print(e.ToString());
            return;
        }
    }

    static void HandlePacket(Packet packet)
    {
        MainThreadDispatcher.Enqueue(async () =>
        {
            switch (packet.type)
            {
                case "allpositions":
                    if (packet.data.Length / 4 != (People.GetCount()))
                    {
                        return;
                    }
                    for (int i = 0; i < packet.data.Length; i += 4)
                    {
                        int positionIndex = int.Parse(packet.data[i]);
                        if (positionIndex == index)
                        {
                            continue;
                        }
                        float positionX = float.Parse(packet.data[i + 1]);
                        float positionY = float.Parse(packet.data[i + 2]);
                        float positionZ = float.Parse(packet.data[i + 3]);
                        People.setPosition(positionIndex, positionX, positionY, positionZ);
                    }
                    break;
                case "chatmessage":
                    int chatIndex = int.Parse(packet.data[0]);
                    string chatUsername = packet.data[1];
                    string chatMessage = packet.data[2];
                    if (chatIndex == index)
                    {
                        PrintChatMessage($"{chatUsername}: {chatMessage}");
                        return;
                    }
                    GameObject chatUser = People.GetUserGameObject(chatIndex);
                    if (chatUser == null)
                    {
                        PrintChatMessage($"{chatUsername}: {chatMessage}");
                        return;
                    }
                    PrintChatMessage($"{chatUser.GetComponent<User>().username}: {chatMessage}");
                    break;
                case "confirmconnect":
                    print($"Connected with index: {packet.data[0]}");
                    index = int.Parse(packet.data[0]);
                    for (int i = 1; i < packet.data.Length; i += 2)
                    {
                        int firstIndex = int.Parse(packet.data[i]);
                        if(firstIndex == index)
                        {
                            continue;
                        }
                        string firstUsername = packet.data[i + 1];
                        People.AddUser(firstIndex, firstUsername);
                    }
                    Send($"region");
                    break;
                case "confirmregion":
                    World.SetRegion(packet.parseRegionData());
                    List<Task<string>> tasks = new List<Task<string>>();
                    int count = World.GetRegion().getHeaderCount();
                    for (int i = 0; i < count; i++)
                    {
                        tasks.Add(MainHttpClient.GetResource("item", World.GetRegion().getHeaderLine(i)));
                    }
                    string[] results = await Task.WhenAll(tasks);
                    World.SetBlockPalette(results);
                    World.DisplayWorld();
                    break;
                case "conflict":
                    PrintChatMessage("Username already taken!");
                    Application.Quit();
                    break;
                case "forcedisconnect":
                    Send($"disconnect\t{index}");
                    Application.Quit();
                    break;
                case "userconnected":
                    int connectedIndex = int.Parse(packet.data[0]);
                    if(connectedIndex == index)
                    {
                        return;
                    }
                    string connectedUsername = packet.data[1];
                    People.AddUser(connectedIndex, connectedUsername);
                    break;
                case "userdisconnected":
                    int disconnectedIndex = int.Parse(packet.data[0]);
                    if (disconnectedIndex == index)
                    {
                        return;
                    }
                    People.RemoveUser(disconnectedIndex);
                    break;
                default:
                    print($"Unknown packet type: {packet.type}");
                    break;
            }
        });
    }

    public static void SendChatMessage(string message)
    {
        Send($"chat\t{index}\t{message}");
    }

    static void PrintChatMessage(string message)
    {
        print(message);
    }

    public static void LogGameState()
    {
        print("Logging game state...");
        string log = "";
        log += $"index: {index}\n";
        log += $"username: {username}\n";
        log += $"connectedUsers: {People.GetCount()}\n";
        List<GameObject> connectedUsers = People.GetUsers();
        foreach (GameObject user in connectedUsers)
        {
            log += $"\t{user.GetComponent<User>().index} {user.GetComponent<User>().username}\n";
        }
        log += $"position: {MainUser.GetPosition()}\n";
        print(log);
    }

    // disconnect on application quit
    void OnApplicationQuit()
    {
        Send($"disconnect\t{index}");
    }
}

// current client packet types:

// - connect username: this packet is sent to the server when the client connects
// - disconnect index: this packet is sent to the server when the client disconnects
// - position index x y z: this packet is sent to the server when the client moves
// - chat index username message: this packet is sent to the server when the client sends a chat message
// - region: this packet is sent to the server when the client connects, and requests the region data

// current server packet types:

// - confirmconnect index [index username]: this packet is sent to the connecting client, and contains the index of the connecting client, as well as the indices and usernames of all other connected clients
// - conflict: this packet is sent to the connecting client if the username they chose is already taken
// - userconnected index username: this packet is sent to all connected clients when a new client connects
// - userdisconnected index: this packet is sent to all connected clients when a client disconnects
// - allpositions [index x y z]: this packet is sent to all connected clients, and contains the positions of all connected clients
// - confirmregion region(base64): this packet is sent to the connecting client, and contains the region data
// - forcedisconnect: this packet is sent to the connecting client when the server wants to disconnect them