using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// the http client communicates with the api to retrieve assets and global data
public class MainHttpClient : MonoBehaviour
{
    [SerializeField] private MainUdpClient udpClient;
    private readonly string endpoint = "https://api.daimon.world/";
    private readonly HttpClient client = new();

    //the following method requests a resource of given type and given id, and returns it as a json string
    public async Task<string> GetResource(string type, string id)
    {
        string url = endpoint + type + "/" + id;
        HttpResponseMessage response = await client.GetAsync(url);
        _ = response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    //the following method pings the server to check if it is online
    private async Task<bool> Ping()
    {
        HttpResponseMessage response = await client.GetAsync(endpoint);
        _ = response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        // if response body is empty, server is offline
        return responseBody != "";
    }

    public void Connect()
    {
        try
        {
            _ = Ping().ContinueWith(task =>
            {
                print("Attempting to connect to API...");
                if (task.Result)
                {
                    print("Connected to API!");
                    udpClient.Connect();
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
}
