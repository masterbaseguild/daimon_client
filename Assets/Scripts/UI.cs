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
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject hotbar;
    [SerializeField] private GameObject menu;
    [SerializeField] private Slider sensitivitySlider;

    private void Start()
    {
        sensitivitySlider.value = mainUser.GetComponent<MainUser>().cameraSensitivity/1000f;
        usernameInput.text = udpClient.GetUsername();
        ipInput.text = udpClient.GetAddress();
        playBtn.onClick.AddListener(() =>
        {
            httpClient.Connect();
            playBtn.gameObject.SetActive(false);
            usernameInput.gameObject.SetActive(false);
            ipInput.gameObject.SetActive(false);
        });
        usernameInput.onValueChanged.AddListener(delegate { EditText(); });
        ipInput.onValueChanged.AddListener(delegate { EditAddressText(); });
        sensitivitySlider.onValueChanged.AddListener(delegate { EditSensitivity(); });
    }

    private void EditText()
    {
        udpClient.SetUsername(usernameInput.text);
    }

    private void EditAddressText()
    {
        udpClient.SetAddress(ipInput.text);
    }

    private void EditSensitivity()
    {
        mainUser.GetComponent<MainUser>().cameraSensitivity = sensitivitySlider.value * 1000f;
    }

    public void ToggleLoadingText(bool toggle)
    {
        loadingText.gameObject.SetActive(toggle);
    }

    public void ToggleBackground(bool toggle)
    {
        backgroundImage.gameObject.SetActive(toggle);
    }

    public void SetLoadingText(string text)
    {
        loadingText.text = text;
    }

    public void ToggleMenu(bool toggle)
    {
        menu.SetActive(toggle);
    }
}