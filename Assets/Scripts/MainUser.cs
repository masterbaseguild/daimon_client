using UnityEngine;

public class MainUser : MonoBehaviour
{
    Vector3 spawnPoint = new Vector3(100, 105, 100);
    GameObject playerCamera;
    float xRotation;
    float yRotation;

    void Start()
    {
        playerCamera = transform.GetChild(0).gameObject;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = spawnPoint;
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) transform.Translate(Vector3.forward * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Vector3.back * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Vector3.left * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Vector3.right * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.Space)) transform.Translate(Vector3.up * Time.deltaTime * 10);
        if (Input.GetKey(KeyCode.LeftShift)) transform.Translate(Vector3.down * Time.deltaTime * 10);
        if (Input.GetKeyDown(KeyCode.T)) MainUdpClient.SendChatMessage("Hello World!");
        if (Input.GetKeyDown(KeyCode.Y)) MainUdpClient.LogGameState();

        float mouseX = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 2048;
        float mouseY = Input.GetAxisRaw("Mouse X") * Time.deltaTime * 2048;

        yRotation += mouseY;
        xRotation -= mouseX;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public static Vector3 GetPosition()
    {
        return GameObject.Find("MainUser").transform.position;
    }
}