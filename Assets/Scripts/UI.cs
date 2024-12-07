using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Button playBtn;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField ipInput;
    public GameObject mainUser;

    private void Awake()
    {
        ipInput.text = MainUdpClient.GetIP();
        playBtn.onClick.AddListener(() =>
        {
            MainHttpClient.Connect();
            mainUser.GetComponent<MainUser>().Enable();
            playBtn.gameObject.SetActive(false);
            usernameInput.gameObject.SetActive(false);
            ipInput.gameObject.SetActive(false);
        });
        usernameInput.onValueChanged.AddListener(delegate{editText();});
        ipInput.onValueChanged.AddListener(delegate{editIpText();});
    }

    private void editText()
    {
        MainUdpClient.SetUsername(usernameInput.text);
    }

    private void editIpText()
    {
        MainUdpClient.SetIP(ipInput.text);
    }
}