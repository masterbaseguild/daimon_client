using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

// the udp client communicates with the server to send and receive state data
public class MainUdpClient : MonoBehaviour
{
    [SerializeField] private GameObject uiGameObject;
    [SerializeField] private World world;
    [SerializeField] private MainHttpClient httpClient;
    [SerializeField] private MainUser user;
    [SerializeField] private People people;
    [SerializeField] private InputManager inputManager;

    [SerializeField] private float lastKeepAlive = 0f;
    [SerializeField] private float keepAliveGracePeriod; // in seconds

    private string lastSentPosition;

    // the udp client won't be enabled until the user presses connect
    private bool isEnabled = false;
    private string serverAddress = "arena.daimon.world";
    private readonly int serverPort = 7689;
    private string username = "";
    private int clientPort;
    private int index; // user index in the server user list
    private UdpClient client;
    private IPEndPoint remoteIpEndPoint;
    private TcpClient tcpClient;
    private NetworkStream tcpStream;
    private CancellationTokenSource cancellationTokenSource;

    // this must only perform setup independent of the data the user will insert in the login form;
    // all the remaining setup must be done on the connect method, since it is ran on connect button press
    private void Start()
    {
        username = "user" + GenerateUserSuffix();
        clientPort = GenerateEphemeralPort();
        // enable UI game object
        uiGameObject.SetActive(true);
    }

    // send position data to the server every frame
    private void FixedUpdate()
    {
        // if udpclient enabled and user gameobject enabled
        if (isEnabled && user.isActiveAndEnabled)
        {
            Vector3 position = user.GetPosition();
            Vector3 rotation = user.GetRotation();
            Vector3 camera = user.GetCamera();
            var newPosition = $"{position.x}\t{position.y}\t{position.z}\t{rotation.x}\t{rotation.y}\t{rotation.z}\t{camera.x}";
            // if the position has changed, send it to the server
            if (lastSentPosition != newPosition)
            {
                lastSentPosition = newPosition;
                Send($"{Packet.Server.NEWPOSITION}\t{index}\t{position.x}\t{position.y}\t{position.z}\t{rotation.x}\t{rotation.y}\t{rotation.z}\t{camera.x}");
            }
            lastSentPosition = newPosition;
        }
        // if the last keep alive packet was received more than x seconds ago, disconnect
        /* if (isEnabled && Time.time - lastKeepAlive > keepAliveGracePeriod)
        {
            Debug.Log("Server timeout, disconnecting...");
            Send($"{Packet.Server.DISCONNECT}\t{index}");
            TcpDisconnect();
            Application.Quit();
        } */
    }

    public void SetUsername(string newUsername)
    {
        username = newUsername;
    }

    public void SetAddress(string newAddress)
    {
        serverAddress = newAddress;
    }

    public string GetUsername()
    {
        return username;
    }

    public string GetAddress()
    {
        return serverAddress;
    }

    private int GenerateEphemeralPort()
    {
        UdpClient tempClient = new(0);
        int port = ((IPEndPoint)tempClient.Client.LocalEndPoint).Port;
        tempClient.Close();
        return port;
    }

    private int GenerateUserSuffix()
    {
        return UnityEngine.Random.Range(1000, 9999);
    }

    public void Send(string data)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            _ = client.Send(bytes, bytes.Length);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void TcpSend(string data)
    {
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            tcpStream.Write(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    // setup the connection and send the connect packet
    public void Connect()
    {
        remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(Dns.GetHostAddresses(serverAddress)[0].ToString()), serverPort);
        try
        {
            client = new UdpClient(clientPort);
            client.Connect(remoteIpEndPoint);
            _ = client.BeginReceive(Get, null);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        Send($"{Packet.Server.CONNECT}\t0\t{username}");
        isEnabled = true;
    }

    private async Task TcpConnect(int index)
    {
        cancellationTokenSource = new CancellationTokenSource();
        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(serverAddress, serverPort);
            tcpStream = tcpClient.GetStream();
            Debug.Log("Connected via TCP");
            // send the connect packet to the server
            TcpSend($"{Packet.Server.CONNECT}\t{index}");
            // start listening for packets from the server
            _ = TcpListen(cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private void TcpDisconnect()
    {
        try
        {
            tcpStream.Close();
            tcpClient.Close();
            cancellationTokenSource.Cancel();
            Debug.Log("Disconnected via TCP");
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    private async Task TcpListen(CancellationToken token)
    {
        try
        {
            byte[] lengthBytes = new byte[4];
            while (!token.IsCancellationRequested)
            {
                int readLengthBytes = await tcpStream.ReadAsync(lengthBytes, 0, lengthBytes.Length, token);
                if (readLengthBytes == 0)
                {
                    break;
                }
                int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                // one byte for packet type
                byte[] typeBytes = new byte[2];
                int readTypeBytes = await tcpStream.ReadAsync(typeBytes, 0, typeBytes.Length, token);
                if (readTypeBytes == 0)
                {
                    break;
                }
                byte[] dataBytes = new byte[messageLength];
                int readDataBytes = 0;
                while (readDataBytes < messageLength)
                {
                    int read = await tcpStream.ReadAsync(dataBytes, readDataBytes, messageLength - readDataBytes, token);
                    if (read == 0)
                    {
                        break;
                    }
                    readDataBytes += read;
                }
                string data = Encoding.UTF8.GetString(dataBytes, 0, readDataBytes);
                Packet message = new(typeBytes[0], data);
                TcpHandlePacket(message);
            }
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    // get callback
    private void Get(IAsyncResult result)
    {
        try
        {
            byte[] bytes = client.EndReceive(result, ref remoteIpEndPoint);
            string data = Encoding.UTF8.GetString(bytes);
            Packet message = new(data);
            HandlePacket(message);
            _ = client.BeginReceive(Get, null);
            return;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return;
        }
    }

    // packet handler
    private void HandlePacket(Packet packet)
    {
        // since this function needs to interact with the unity object api, it must be run on the main thread
        // TODO: we probably can only enqueue specific operations, not the whole packet handler
        MainThreadDispatcher.Enqueue(async () =>
        {
            switch (packet.type)
            {
                // set positions of all connected users
                case Packet.Client.NEWPOSITION:
                    if (packet.data.Length / 8 != people.GetCount())
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
                        float positionX = float.Parse(packet.data[i + 1], CultureInfo.InvariantCulture);
                        float positionY = float.Parse(packet.data[i + 2], CultureInfo.InvariantCulture);
                        float positionZ = float.Parse(packet.data[i + 3], CultureInfo.InvariantCulture);
                        float rotationX = float.Parse(packet.data[i + 4], CultureInfo.InvariantCulture);
                        float rotationY = float.Parse(packet.data[i + 5], CultureInfo.InvariantCulture);
                        float rotationZ = float.Parse(packet.data[i + 6], CultureInfo.InvariantCulture);
                        float cameraX = float.Parse(packet.data[i + 7], CultureInfo.InvariantCulture);
                        people.SetPosition(positionIndex, positionX, positionY, positionZ, rotationX, rotationY, rotationZ, cameraX);
                    }
                    break;
                // a chat message was sent by a user
                case Packet.Client.CHAT:
                    int chatIndex = int.Parse(packet.data[0]);
                    string chatUsername = packet.data[1];
                    string chatMessage = packet.data[2];
                    if (chatIndex == index)
                    {
                        PrintChatMessage($"{chatUsername}: {chatMessage}");
                        return;
                    }
                    GameObject chatUser = people.GetUserGameObject(chatIndex);
                    if (chatUser == null)
                    {
                        PrintChatMessage($"{chatUsername}: {chatMessage}");
                        return;
                    }
                    PrintChatMessage($"{chatUser.GetComponent<User>().username}: {chatMessage}");
                    break;
                // the server has confirmed our connection
                case Packet.Client.CONNECT:
                    Debug.Log("Connected via UDP");
                    index = int.Parse(packet.data[0]);
                    await TcpConnect(index);
                    for (int i = 1; i < packet.data.Length; i += 2)
                    {
                        int firstIndex = int.Parse(packet.data[i]);
                        if (firstIndex == index)
                        {
                            continue;
                        }
                        string firstUsername = packet.data[i + 1];
                        people.AddUser(firstIndex, firstUsername);
                    }
                    lastKeepAlive = Time.time;
                    break;
                // the server has forced us to disconnect
                case Packet.Client.DISCONNECT:
                    Debug.Log("Disconnected via UDP");
                    TcpDisconnect();
                    Application.Quit();
                    break;
                // another user has connected
                case Packet.Client.USERCONNECT:
                    int connectedIndex = int.Parse(packet.data[0]);
                    if (connectedIndex == index)
                    {
                        return;
                    }
                    string connectedUsername = packet.data[1];
                    people.AddUser(connectedIndex, connectedUsername);
                    break;
                // another user has disconnected
                case Packet.Client.USERDISCONNECT:
                    int disconnectedIndex = int.Parse(packet.data[0]);
                    if (disconnectedIndex == index)
                    {
                        return;
                    }
                    people.RemoveUser(disconnectedIndex);
                    break;
                case Packet.Client.KEEPALIVE:
                    // send back a keep alive packet to the server
                    Send($"{Packet.Server.KEEPALIVE}\t{index}");
                    lastKeepAlive = Time.time;
                    break;
                case Packet.Client.SCRIPT:
                    Debug.Log($"Script packet received on MainUdpClient: {packet.data[0]}");
                    inputManager.Receive(packet.data);
                    break;
                // catch-all: unknown packet type
                default:
                    Debug.Log($"Unknown packet type: {packet.type}");
                    break;
            }
        });
    }

    private void TcpHandlePacket(Packet packet)
    {
        MainThreadDispatcher.Enqueue(async () =>
        {
            switch (packet.type)
            {
                // the server has confirmed our connection
                case Packet.Client.CONNECT:
                    int mode = int.Parse(packet.data[0]);
                    if (mode == 0) // edit mode
                    {
                        int blockCount = int.Parse(packet.data[0]);
                        string[] blockList = new string[blockCount];
                        for (int i = 1; i < blockCount + 1; i++)
                        {
                            blockList[i - 1] = packet.data[i];
                        }
                        world.SetBlockList(blockList);
                        int modelCount = int.Parse(packet.data[blockCount + 1]);
                        string[] modelList = new string[modelCount];
                        for (int i = blockCount + 2; i < packet.data.Length; i++)
                        {
                            modelList[i - blockCount - 2] = packet.data[i];
                        }
                        world.SetModelList(modelList);
                    }
                    else if (mode == 1) // play mode
                    {
                        int itemCount = int.Parse(packet.data[0]);
                        string[] itemList = new string[itemCount];
                        for (int i = 1; i < itemCount + 1; i++)
                        {
                            itemList[i - 1] = packet.data[i];
                        }
                        world.SetItemList(itemList);
                    }
                    TcpSend($"{Packet.Server.WORLD}");
                    break;
                // the server has sent us the region data
                case Packet.Client.WORLD:
                    world.SetRegions(packet.ParseWorldData());
                    List<Task<string>> tasks = new();
                    int count = world.GetBlockCount();
                    for (int i = 0; i < count; i++)
                    {
                        tasks.Add(httpClient.GetResource("item", world.GetBlock(i)));
                    }
                    string[] results = await Task.WhenAll(tasks);
                    world.SetBlockPalette(results);
                    world.DisplayWorld();
                    break;
                case Packet.Client.SETBLOCK:
                    int x = int.Parse(packet.data[0]);
                    int y = int.Parse(packet.data[1]);
                    int z = int.Parse(packet.data[2]);
                    int blockIndex = int.Parse(packet.data[3]);
                    world.SetVoxel(x, y, z, blockIndex);
                    break;
                case Packet.Client.SETMINIBLOCK:
                    int minix = int.Parse(packet.data[0]);
                    int miniy = int.Parse(packet.data[1]);
                    int miniz = int.Parse(packet.data[2]);
                    int miniBlockIndex = int.Parse(packet.data[3]);
                    world.SetMiniVoxel(minix, miniy, miniz, miniBlockIndex);
                    break;
                // catch-all: unknown packet type
                default:
                    Debug.Log($"Unknown packet type: {packet.type}");
                    break;
            }
        });
    }

    public void SendChatMessage(string message)
    {
        Send($"{Packet.Server.CHAT}\t{index}\t{message}");
    }

    private void PrintChatMessage(string message)
    {
        Debug.Log(message);
    }

    // debug function to current log game state
    public void LogGameState()
    {
        Debug.Log("Logging game state...");
        string log = "";
        log += $"index: {index}\n";
        log += $"username: {username}\n";
        log += $"connectedUsers: {people.GetCount()}\n";
        List<GameObject> connectedUsers = people.GetUsers();
        foreach (GameObject user in connectedUsers)
        {
            log += $"\t{user.GetComponent<User>().index} {user.GetComponent<User>().username}\n";
        }
        log += $"position: {user.GetPosition()}\n";
        Debug.Log(log);
    }

    // disconnect on application quit
    private void OnApplicationQuit()
    {
        Send($"{Packet.Server.DISCONNECT}\t{index}");
        TcpDisconnect();
    }

    public int GetIndex()
    {
        return index;
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