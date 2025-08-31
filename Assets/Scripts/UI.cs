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
    [SerializeField] private GameObject scrollViewport;

    private void Start()
    {
        sensitivitySlider.value = mainUser.GetComponent<MainUser>().cameraSensitivity / 1000f;
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

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        RectTransform canvasRect = menuRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        menuRect.sizeDelta = new Vector2(canvasRect.rect.width * 0.5f, canvasRect.rect.height * 0.5f);
    }

    public void SetBlockInventory(int count)
    {
        // divide by 10
        int rows = (count + 9) / 10;
        // set slotSize to 1/10 of viewport width
        float slotSize = scrollViewport.GetComponent<RectTransform>().rect.width / 10;
        // set scroll
        scrollViewport.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rows * slotSize);
        // update the UI
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject slot = new GameObject("Slot");
                slot.AddComponent<RectTransform>();
                slot.AddComponent<Image>();
                slot.transform.SetParent(scrollViewport.transform, false);
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(slotSize, slotSize);
                slotRect.anchorMin = new Vector2(0, 1);
                slotRect.anchorMax = new Vector2(0, 1);
                slotRect.pivot = new Vector2(0, 1);
                slotRect.anchoredPosition = new Vector2(j * slotSize, -(i * slotSize));
            }
        }
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