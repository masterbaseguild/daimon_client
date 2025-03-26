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

    private void Awake()
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
        usernameInput.onValueChanged.AddListener(delegate { EditText(); });
        ipInput.onValueChanged.AddListener(delegate { EditAddressText(); });
    }

    private void EditText()
    {
        udpClient.SetUsername(usernameInput.text);
    }

    private void EditAddressText()
    {
        udpClient.SetAddress(ipInput.text);
    }
}