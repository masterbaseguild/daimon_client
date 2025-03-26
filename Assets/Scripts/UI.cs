using UnityEngine;
using UnityEngine.UI;

// a placeholder ui implementation
public class UI : MonoBehaviour
{
    public MainHttpClient httpClient;
    public MainUdpClient udpClient;
    public GameObject mainUser;
    public Button playBtn;
    public InputField usernameInput;
    public InputField ipInput;
    public Text ChunkData;

    void Awake()
    {
        usernameInput.text = udpClient.GetUsername();
        ipInput.text = udpClient.GetAddress();
        playBtn.onClick.AddListener(() =>
        {
            httpClient.Connect();
            mainUser.GetComponent<MainUser>().Enable();
            playBtn.gameObject.SetActive(false);
            usernameInput.gameObject.SetActive(false);
            ipInput.gameObject.SetActive(false);
        });
        usernameInput.onValueChanged.AddListener(delegate{editText();});
        ipInput.onValueChanged.AddListener(delegate{editAddressText();});
    }

    void editText()
    {
        udpClient.SetUsername(usernameInput.text);
    }

    void editAddressText()
    {
        udpClient.SetAddress(ipInput.text);
    }
}