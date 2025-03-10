using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

// the udp client communicates with the server to send and receive state data
public class MainUdpClient : MonoBehaviour
{
    // the udp client won't be enabled until the user presses connect
    static bool isEnabled = false;
    static string serverAddress = "arena.projectdaimon.com";
    static int serverPort = 7689;
    static string username = "";
    static int clientPort;
    static int index; // user index in the server user list
    static UdpClient client;
    static IPEndPoint remoteIpEndPoint;
    static bool hasReceivedRegion = false;

    // this must only perform setup independent of the data the user will insert in the login form;
    // all the remaining setup must be done on the connect method, since it is ran on connect button press
    void Start()
    {
        username = "user" + generateUserSuffix();
        clientPort = generateEphemeralPort();
    }

    // send position data to the server every frame
    void Update()
    {
        if (isEnabled)
        {
        Vector3 position = MainUser.GetPosition();
        Vector3 rotation = MainUser.GetRotation();
        Vector3 camera = MainUser.GetCamera();
        Send($"position\t{index}\t{position.x}\t{position.y}\t{position.z}\t{rotation.x}\t{rotation.y}\t{rotation.z}\t{camera.x}");
        }
    }

    public static void SetUsername(string newUsername)
    {
        username = newUsername;
    }

    public static void SetAddress(string newAddress)
    {
        serverAddress = newAddress;
    }

    public static string GetAddress()
    {
        return serverAddress;
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

    // setup the connection and send the connect packet
    public static void Connect()
    {
        remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(Dns.GetHostAddresses(serverAddress)[0].ToString()), serverPort);
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

    // packet handler
    static void HandlePacket(Packet packet)
    {
        // since this function needs to interact with the unity object api, it must be run on the main thread
        // TODO: we probably can only enqueue specific operations, not the whole packet handler
        MainThreadDispatcher.Enqueue(async () =>
        {
            switch (packet.type)
            {
                // set positions of all connected users
                case "allpositions":
                    if (packet.data.Length / 8 != People.GetCount())
                    {
                        return;
                    }
                    for (int i = 0; i < packet.data.Length; i += 8)
                    {
                        int positionIndex = int.Parse(packet.data[i]);
                        if (positionIndex == index)
                        {
                            continue;
                        }
                        float positionX = float.Parse(packet.data[i + 1]);
                        float positionY = float.Parse(packet.data[i + 2]);
                        float positionZ = float.Parse(packet.data[i + 3]);
                        float rotationX = float.Parse(packet.data[i + 4]);
                        float rotationY = float.Parse(packet.data[i + 5]);
                        float rotationZ = float.Parse(packet.data[i + 6]);
                        float cameraX = float.Parse(packet.data[i + 7]);
                        People.setPosition(positionIndex, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, cameraX);
                    }
                    break;
                // a chat message was sent by a user
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
                // the server has confirmed our connection
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
                    // wait 5 seconds, then check if we have received the region data
                    await Task.Delay(5000);
                    if (!hasReceivedRegion)
                    {
                        print("Failed to receive region data, using fallback local packet...");
                        // load packet from binary file in documents folder
                        packet = new Packet("confirmregion\t" + Convert.ToBase64String(System.IO.File.ReadAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/region.dat")));
                        HandlePacket(packet);
                    }
                    break;
                // the server has sent us the region data
                case "confirmregion":
                    hasReceivedRegion = true;
                    // save packet to binary file in documents folder
                    // System.IO.File.WriteAllBytes(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/region.dat", Convert.FromBase64String(packet.data[0]));
                    World.SetRegion(packet.parseRegionData());
                    List<Task<string>> tasks = new List<Task<string>>();
                    int count = World.GetRegion().getHeaderCount();
                    Debug.Log("Count: " + count);
                    for (int i = 0; i < count; i++)
                    {
                        tasks.Add(MainHttpClient.GetResource("item", World.GetRegion().getHeaderLine(i)));
                    }
                    string[] results = await Task.WhenAll(tasks);
                    World.SetBlockPalette(results);
                    World.DisplayWorld();
                    break;
                // we cannot connect because our username is already taken
                case "conflict":
                    PrintChatMessage("Username already taken!");
                    Application.Quit();
                    break;
                // the server has forced us to disconnect
                case "forcedisconnect":
                    Send($"disconnect\t{index}");
                    Application.Quit();
                    break;
                // another user has connected
                case "userconnected":
                    int connectedIndex = int.Parse(packet.data[0]);
                    if(connectedIndex == index)
                    {
                        return;
                    }
                    string connectedUsername = packet.data[1];
                    People.AddUser(connectedIndex, connectedUsername);
                    break;
                // another user has disconnected
                case "userdisconnected":
                    int disconnectedIndex = int.Parse(packet.data[0]);
                    if (disconnectedIndex == index)
                    {
                        return;
                    }
                    People.RemoveUser(disconnectedIndex);
                    break;
                // catch-all: unknown packet type
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

    // debug function to current log game state
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
// - position index x y z rx ry rz cx: this packet is sent to the server when the client moves
// - chat index username message: this packet is sent to the server when the client sends a chat message
// - region: this packet is sent to the server when the client connects, and requests the region data

// current server packet types:

// - confirmconnect index [index username]: this packet is sent to the connecting client, and contains the index of the connecting client, as well as the indices and usernames of all other connected clients
// - conflict: this packet is sent to the connecting client if the username they chose is already taken
// - userconnected index username: this packet is sent to all connected clients when a new client connects
// - userdisconnected index: this packet is sent to all connected clients when a client disconnects
// - allpositions [index x y z rx ry rz cx]: this packet is sent to all connected clients, and contains the positions of all connected clients
// - confirmregion region(base64): this packet is sent to the connecting client, and contains the region data
// - forcedisconnect: this packet is sent to the connecting client when the server wants to disconnect them