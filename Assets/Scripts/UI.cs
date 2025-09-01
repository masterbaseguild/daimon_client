using UnityEngine;
using UnityEngine.UI;

// a placeholder ui implementation
public class UI : MonoBehaviour
{
    [SerializeField] private MainHttpClient httpClient;
    [SerializeField] private MainUdpClient udpClient;
    [SerializeField] private MainUser mainUser;
    [SerializeField] private Button playBtn;
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField ipInput;
    [SerializeField] private Text loadingText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject hotbar;
    [SerializeField] private GameObject menu;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private GameObject scrollViewport;
    [SerializeField] private Font mainFont;
    private Image hotbarSelectedSlot;
    private float slotSize;
    [SerializeField] private int blockCount;
    private Texture2D textureAtlas;
    [SerializeField] private Transform inHandSlot;
    [SerializeField] private int inHandBlockIndex;
    [SerializeField] private GameObject Tutorial;

    private void Start()
    {
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

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        RectTransform canvasRect = menuRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        menuRect.sizeDelta = new Vector2(canvasRect.rect.width * 0.5f, canvasRect.rect.height * 0.5f);
        slotSize = scrollViewport.GetComponent<RectTransform>().rect.width / 10;

        inHandSlot.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize - 20, slotSize - 20);

        menu.transform.Find("ResetButton").GetComponent<Button>().onClick.AddListener(() => { ResetHotbar(); });

        // set hotbar size
        RectTransform hotbarRect = hotbar.GetComponent<RectTransform>();
        hotbarRect.sizeDelta = new Vector2((slotSize * 10) + (20 * 11), slotSize + 40);
        hotbarRect.anchoredPosition = new Vector2(0, hotbarRect.sizeDelta.y / 2);

        // create and place hotbar selected slot
        hotbarSelectedSlot = new GameObject("HotbarSelectedSlot").AddComponent<Image>();
        hotbarSelectedSlot.transform.SetParent(hotbar.transform, false);
        hotbarSelectedSlot.color = new Color32(255, 255, 255, 128);
        RectTransform selectedSlotRect = hotbarSelectedSlot.GetComponent<RectTransform>();
        selectedSlotRect.sizeDelta = new Vector2(slotSize + 40, slotSize + 40);
        selectedSlotRect.anchorMin = new Vector2(0, 0.5f);
        selectedSlotRect.anchorMax = new Vector2(0, 0.5f);
        selectedSlotRect.pivot = new Vector2(0, 0.5f);
        selectedSlotRect.anchoredPosition = new Vector2(0, 0);

        // populate slots in hotbar

        for (int i = 0; i < 10; i++)
        {
            GameObject slot = new GameObject("Slot");
            slot.transform.SetParent(hotbar.transform, false);
            slot.AddComponent<RectTransform>();
            slot.AddComponent<Image>();
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(slotSize, slotSize);
            slotRect.anchorMin = new Vector2(0, 0.5f);
            slotRect.anchorMax = new Vector2(0, 0.5f);
            slotRect.pivot = new Vector2(0, 0.5f);
            slotRect.anchoredPosition = new Vector2((i * slotSize) + (20 * (i + 1)), 0);

            // add smaller image in the slot
            GameObject slotImage = new GameObject("SlotImage");
            slotImage.transform.SetParent(slot.transform, false);
            Image image = slotImage.AddComponent<Image>();
            RectTransform imageRect = slotImage.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(slotSize - 20, slotSize - 20);
            imageRect.anchoredPosition = new Vector2(0, 0);
            image.raycastTarget = false;

            // add keyboard number in the middle of the slot
            GameObject slotText = new GameObject("SlotText");
            slotText.transform.SetParent(slot.transform, false);
            Text text = slotText.AddComponent<Text>();
            text.text = ((i + 1) % 10).ToString();
            text.font = mainFont;
            text.raycastTarget = false;
            text.fontSize = 24;
            text.color = new Color32(0, 68, 255, 128);
            text.alignment = TextAnchor.MiddleCenter;

            // add button to slot
            Button button = slot.AddComponent<Button>();
            int index = i;
            button.onClick.AddListener(delegate { OnHotbarSlotClicked(index); });
        }
    }

    private void Update()
    {
        inHandSlot.transform.position = Input.mousePosition;
    }

    public void InitSensitivitySlider()
    {
        sensitivitySlider.value = mainUser.cameraSensitivity / 1000f;
        sensitivitySlider.onValueChanged.AddListener(delegate { EditSensitivity(); });
    }

    public void SetHotbarSelectedSlot(int index)
    {
        RectTransform selectedSlotRect = hotbarSelectedSlot.GetComponent<RectTransform>();
        selectedSlotRect.anchoredPosition = new Vector2(index * (slotSize + 20), 0);
    }

    public void SetBlockCount(int count)
    {
        blockCount = count * 2;
        int rows = (blockCount + 9) / 10;
        scrollViewport.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rows * slotSize);

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

                // add button to slot
                Button button = slot.AddComponent<Button>();
                int row = i;
                int col = j;
                button.onClick.AddListener(delegate { OnBlockSlotClicked(row * 10 + col); });

                // add smaller image in the slot
                GameObject slotImage = new GameObject("SlotImage");
                slotImage.transform.SetParent(slot.transform, false);
                Image image = slotImage.AddComponent<Image>();
                RectTransform imageRect = slotImage.GetComponent<RectTransform>();
                imageRect.sizeDelta = new Vector2(slotSize - 20, slotSize - 20);
                imageRect.anchoredPosition = new Vector2(0, 0);
            }
        }
    }

    private void OnBlockSlotClicked(int index)
    {
        if (inHandSlot.gameObject.activeSelf)
        {
            inHandSlot.gameObject.SetActive(false);
            inHandSlot.GetChild(0).GetComponent<Image>().sprite = null;
            inHandBlockIndex = 0;
            return;
        }
        Image image = inHandSlot.GetChild(0).GetComponent<Image>();
        Rect rect = new Rect(index % (blockCount / 2) * textureAtlas.height, 0, textureAtlas.height, textureAtlas.height);
        Sprite sprite = Sprite.Create(textureAtlas, rect, new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        image.rectTransform.localScale = index < blockCount / 2 ? new Vector3(1f, 1f, 1f) : new Vector3(0.5f, 0.5f, 0.5f);
        image.preserveAspect = true;
        inHandSlot.gameObject.SetActive(true);
        inHandBlockIndex = index;
    }

    private void OnHotbarSlotClicked(int index)
    {
        bool isHotbarSlotBlockFull = mainUser.hotbarSlots[index].isFullVoxel;
        bool isInHandBlockFull = inHandBlockIndex < blockCount / 2;
        Image hotbarSlot = hotbar.transform.GetChild(index + 1).Find("SlotImage").GetComponent<Image>();
        if (inHandSlot.gameObject.activeSelf)
        {
            Sprite hotbarSlotBlockSprite = hotbarSlot.sprite;
            int hotbarSlotBlockId = mainUser.hotbarSlots[index].id + (isHotbarSlotBlockFull ? 0 : (blockCount / 2));
            hotbarSlot.sprite = inHandSlot.GetChild(0).GetComponent<Image>().sprite;
            if (isInHandBlockFull)
            {
                mainUser.SetHotbarSlot(index, inHandBlockIndex, true);
                hotbarSlot.rectTransform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                mainUser.SetHotbarSlot(index, inHandBlockIndex - (blockCount / 2), false);
                hotbarSlot.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            if (hotbarSlotBlockSprite == null)
            {
                inHandSlot.gameObject.SetActive(false);
                inHandSlot.GetChild(0).GetComponent<Image>().sprite = null;
                inHandBlockIndex = 0;
            }
            else
            {
                inHandSlot.GetChild(0).GetComponent<Image>().sprite = hotbarSlotBlockSprite;
                inHandBlockIndex = hotbarSlotBlockId;
                inHandSlot.GetChild(0).GetComponent<Image>().rectTransform.localScale = isHotbarSlotBlockFull ? new Vector3(1f, 1f, 1f) : new Vector3(0.5f, 0.5f, 0.5f);
            }
            return;
        }
        Image image = inHandSlot.GetChild(0).GetComponent<Image>();
        image.sprite = hotbarSlot.sprite;
        image.preserveAspect = true;
        inHandSlot.gameObject.SetActive(true);
        inHandBlockIndex = mainUser.hotbarSlots[index].id + (isHotbarSlotBlockFull ? 0 : (blockCount / 2));
        mainUser.SetHotbarSlot(index, 0, true);
        hotbarSlot.sprite = null;
        hotbarSlot.rectTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void SetBlockInventory(Texture2D textureAtlas)
    {
        this.textureAtlas = textureAtlas;
        for (int i = 0; i < blockCount / 2; i++)
        {
            Transform slot = scrollViewport.transform.GetChild(i);
            Image image = slot.GetChild(0).GetComponent<Image>();
            Rect rect = new Rect(i * textureAtlas.height, 0, textureAtlas.height, textureAtlas.height);
            Sprite sprite = Sprite.Create(textureAtlas, rect, new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
            image.preserveAspect = true;
        }
        for (int i = blockCount / 2; i < blockCount; i++)
        {
            Transform slot = scrollViewport.transform.GetChild(i);
            Image image = slot.GetChild(0).GetComponent<Image>();
            Rect rect = new Rect((i - (blockCount / 2)) * textureAtlas.height, 0, textureAtlas.height, textureAtlas.height);
            Sprite sprite = Sprite.Create(textureAtlas, rect, new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
            image.preserveAspect = true;
            image.rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
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
        mainUser.cameraSensitivity = sensitivitySlider.value * 1000f;
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
        Tutorial.SetActive(toggle);
        menu.SetActive(toggle);
        if (!toggle)
        {
            inHandSlot.gameObject.SetActive(false);
            inHandSlot.GetChild(0).GetComponent<Image>().sprite = null;
            inHandBlockIndex = 0;
        }
    }

    private void ResetHotbar()
    {
        for (int i = 0; i < mainUser.hotbarSlots.Length; i++)
        {
            mainUser.SetHotbarSlot(i, 0, true);
            Image image = hotbar.transform.GetChild(i + 1).Find("SlotImage").GetComponent<Image>();
            image.sprite = null;
            image.rectTransform.localScale = new Vector3(1f, 1f, 1f);
        }
        inHandSlot.gameObject.SetActive(false);
        inHandSlot.GetChild(0).GetComponent<Image>().sprite = null;
        inHandBlockIndex = 0;
    }
}