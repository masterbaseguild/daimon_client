using UnityEngine;
using UnityEngine.UI;

// a placeholder ui implementation
public class UI : MonoBehaviour
{
    [SerializeField] private Button playBtn;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField ipInput;
    [SerializeField] private Text ChunkData;
    public GameObject mainUser;

    private void Awake()
    {
        ipInput.text = MainUdpClient.GetAddress();
        playBtn.onClick.AddListener(() =>
        {
            MainHttpClient.Connect();
            mainUser.GetComponent<MainUser>().Enable();
            playBtn.gameObject.SetActive(false);
            usernameInput.gameObject.SetActive(false);
            ipInput.gameObject.SetActive(false);
        });
        usernameInput.onValueChanged.AddListener(delegate{editText();});
        ipInput.onValueChanged.AddListener(delegate{editAddressText();});
    }

    void Update()
    {
        Vector3 coords = mainUser.transform.position;
        var Xtext = "X: " + (int)coords.x;
        var Ytext = "Y: " + (int)coords.y;
        var Ztext = "Z: " + (int)coords.z;
        Vector3 chunkCoords = World.GetChunkCoords(mainUser.transform.position);
        var chunkXtext = "ChunkX: " + chunkCoords.x;
        var chunkYtext = "ChunkY: " + chunkCoords.y;
        var chunkZtext = "ChunkZ: " + chunkCoords.z;
        ChunkData.text = Xtext + "\n" + Ytext + "\n" + Ztext + "\n" + chunkXtext + "\n" + chunkYtext + "\n" + chunkZtext;
    }

    private void editText()
    {
        MainUdpClient.SetUsername(usernameInput.text);
    }

    private void editAddressText()
    {
        MainUdpClient.SetAddress(ipInput.text);
    }
}