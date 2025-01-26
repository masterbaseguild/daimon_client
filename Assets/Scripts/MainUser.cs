using UnityEngine;

public class MainUser : MonoBehaviour
{

    float moveSpeed = 10f;
    Vector3 move;

    float sens = 1024f;
    Vector3 spawnPoint = new Vector3(80, 64, 94);
    GameObject playerCamera;
    Rigidbody rigidBody;
    float xRotation;
    float yRotation;
    bool isEnabled = false;

    public float currentVelocity;

    public void Enable()
    {
        playerCamera = transform.GetChild(0).gameObject;
        rigidBody = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = spawnPoint;
        isEnabled = true;
    }

    void FixedUpdate()
    {
        if (!isEnabled) return;
        rigidBody.velocity = (transform.forward * move.z + transform.right * move.x + transform.up * move.y).normalized * moveSpeed;
    }

    void Update()
    {
        if (!isEnabled) return;

        move = Vector3.zero;

        if (Input.GetKey(KeyCode.Space)) move.y += 1f;
        if (Input.GetKey(KeyCode.LeftShift)) move.y += -1f;
        if (Input.GetKey(KeyCode.W)) move.z += 1f;
        if (Input.GetKey(KeyCode.S)) move.z += -1f;
        if (Input.GetKey(KeyCode.A)) move.x += -1f;
        if (Input.GetKey(KeyCode.D)) move.x += 1f;

        if (Input.GetKeyDown(KeyCode.T)) MainUdpClient.SendChatMessage("Hello World!");
        if (Input.GetKeyDown(KeyCode.Y)) MainUdpClient.LogGameState();

        float mouseX = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;
        float mouseY = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;

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