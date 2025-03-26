using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// the http client communicates with the api to retrieve assets and global data
public class MainHttpClient : MonoBehaviour
{
    public MainUdpClient udpClient;
    readonly string endpoint = "https://api.daimon.world/";
    readonly HttpClient client = new HttpClient();

    //the following method requests a resource of given type and given id, and returns it as a json string
    public async Task<string> GetResource(string type, string id)
    {
        string url = endpoint + type + "/" + id;
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    //the following method pings the server to check if it is online
    async Task<bool> Ping()
    {
        HttpResponseMessage response = await client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        // if response body is empty, server is offline
        if (responseBody == "")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Connect()
    {
        try {
            Ping().ContinueWith(task =>
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
