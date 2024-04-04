using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

public class ApiClient : MonoBehaviour
{
    public string endpoint = "https://api.projectdaimon.com/";
    public HttpClient client = new HttpClient();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
    public async Task<bool> Ping()
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
}
