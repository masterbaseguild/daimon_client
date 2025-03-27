using UnityEngine;
using UnityEngine.UI;

// a placeholder ui implementation
public class UI : MonoBehaviour
{
    [SerializeField] private MainHttpClient httpClient;
    [SerializeField] private MainUdpClient udpClient;
    [SerializeField] private GameObject mainUser;
    [SerializeField] private Button playBtn;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField ipInput;
    [SerializeField] private Text loadingText;

    private void OnEnable()
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

    public void ToggleLoadingText(bool toggle)
    {
        loadingText.gameObject.SetActive(toggle);
    }

    public void SetLoadingText(string text)
    {
        loadingText.text = text;
    }
}