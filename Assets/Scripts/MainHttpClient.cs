using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;

// the http client communicates with the api to retrieve assets and global data
public class MainHttpClient : MonoBehaviour
{
    static string endpoint = "https://api.projectdaimon.com/";
    static HttpClient client = new HttpClient();

    //the following method requests a resource of given type and given id, and returns it as a json string
    public static async Task<string> GetResource(string type, string id)
    {
        string url = endpoint + type + "/" + id;
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    //the following method pings the server to check if it is online
    public static async Task<bool> Ping()
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

    public static void Connect()
    {
        try {
            Ping().ContinueWith(task =>
            {
                print("Attempting to connect to API...");
                if (task.Result)
                {
                    print("Connected to API!");
                    MainUdpClient.Connect();
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
