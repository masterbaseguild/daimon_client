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

public class CustomUdpClient : MonoBehaviour
{
    public const string ip = "127.0.0.1";
    public const int serverPort = 4000;
    public const string username = "Entity";
    public const int clientPort = 6000;
    public int index;
    public GameObject userPrefab;
    public List<GameObject> connectedUsers = new List<GameObject>();
    public List<Block> blockPalette = new List<Block>();
    public Region region;
    public ApiClient apiClient = new ApiClient();

    UdpClient client;
    IPEndPoint remoteIpEndPoint;

    void Start()
    {
        try
        {
            client = new UdpClient(clientPort);
            remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(ip), serverPort);
            client.Connect(remoteIpEndPoint);
            client.BeginReceive(Get, null);
            print("Attempting to connect to API...");
            apiClient.Ping().ContinueWith(task =>
            {
                if (task.Result)
                {
                    print("Connected to API!");
                    Send($"connect\t0\t{username}");
                }
                else
                {
                    print("Failed to connect to API!");
                    Application.Quit();
                }
            });
        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.forward * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.back * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * Time.deltaTime * 10);
        if (Input.GetKeyDown(KeyCode.T)) SendChatMessage("Hello World!");
        if (Input.GetKeyDown(KeyCode.Y)) LogGameState();
        Send($"position\t{index}\t{transform.position.x}\t{transform.position.y}\t{transform.position.z}");
    }

    // send function
    void Send(string data)
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

    // get callback
    void Get(IAsyncResult result)
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

    void DisplayBlockTexture(int index)
    {
        Block block = blockPalette[index];
        block.OnTextureLoaded += () =>
        {
            Texture2D texture = block.texture2D;

            if (texture != null)
            {
                // Create a Quad to display the texture
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = new Vector3(0, 100, 0); // Position the Quad as needed
                quad.transform.Rotate(90, 0, 0); // Rotate the Quad as needed
                quad.GetComponent<Renderer>().material.mainTexture = texture;
            }
        };
    }

    void HandlePacket(Packet packet)
    {
        MainThreadDispatcher.Enqueue(async () =>
        {
            switch (packet.type)
            {
                case "allpositions":
                    if (packet.data.Length / 4 != (connectedUsers.Count + 1))
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
                        GameObject positionUser = connectedUsers.Find(user => user.GetComponent<OtherUser>().index == positionIndex);
                        if(positionUser == null)
                        {
                            continue;
                        }
                        positionUser.transform.position = new Vector3(positionX, positionY, positionZ);
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
                    GameObject chatUser = connectedUsers.Find(user => user.GetComponent<OtherUser>().index == chatIndex);
                    if (chatUser == null)
                    {
                        PrintChatMessage($"{chatUsername}: {chatMessage}");
                        return;
                    }
                    PrintChatMessage($"{chatUser.GetComponent<OtherUser>().username}: {chatMessage}");
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
                        GameObject firstUser = Instantiate(userPrefab);
                        firstUser.GetComponent<OtherUser>().index = firstIndex;
                        firstUser.GetComponent<OtherUser>().username = firstUsername;
                        connectedUsers.Add(firstUser);
                    }
                    Send($"region");
                    break;
                case "confirmregion":
                    region = new Region(packet.parseData());
                    print($"First 3 Lines of Region Header:");
                    List<Task<string>> tasks = new List<Task<string>>();
                    for (int i = 0; i < 3; i++)
                    {
                        print(region.Header[i]);
                        tasks.Add(apiClient.GetResource("item", region.Header[i]));
                    }
                    string[] results = await Task.WhenAll(tasks);
                    foreach (string result in results)
                    {
                        blockPalette.Add(jsonToBlock(result));
                    }
                    DisplayBlockTexture(2);
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
                    GameObject connectedUser = Instantiate(userPrefab);
                    connectedUser.GetComponent<OtherUser>().index = connectedIndex;
                    connectedUser.GetComponent<OtherUser>().username = connectedUsername;
                    connectedUsers.Add(connectedUser);
                    break;
                case "userdisconnected":
                    int disconnectedIndex = int.Parse(packet.data[0]);
                    if (disconnectedIndex == index)
                    {
                        return;
                    }
                    GameObject disconnectedUser = connectedUsers.Find(user => user.GetComponent<OtherUser>().index == disconnectedIndex);
                    if (disconnectedUser == null)
                    {
                        return;
                    }
                    connectedUsers.Remove(disconnectedUser);
                    Destroy(disconnectedUser);
                    break;
                default:
                    print($"Unknown packet type: {packet.type}");
                    break;
            }
        });
    }

    void SendChatMessage(string message)
    {
        Send($"chat\t{index}\t{message}");
    }

    void PrintChatMessage(string message)
    {
        print(message);
    }
    
    Block jsonToBlock(string json)
    {
        return JsonConvert.DeserializeObject<Block>(json);
    }

    void LogGameState()
    {
        print("Logging game state...");
        string log = "";
        log += $"index: {index}\n";
        log += $"username: {username}\n";
        log += $"connectedUsers: {connectedUsers.Count}\n";
        foreach (GameObject user in connectedUsers)
        {
            log += $"\t{user.GetComponent<OtherUser>().index} {user.GetComponent<OtherUser>().username}\n";
        }
        log += $"position: {transform.position}\n";
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