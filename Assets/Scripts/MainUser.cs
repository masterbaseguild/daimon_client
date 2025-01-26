using UnityEngine;

public class MainUser : MonoBehaviour
{
    bool isEnabled = false;

    Vector3 spawnPoint = new Vector3(80, 64, 94);
    float moveSpeed = 5f;
    float jumpHeight = 1f;
    float groundDistance = 0.25f;
    float gravity = -30f;
    float sens = 1024f;
    float delayBetweenPresses = 0.25f;
    int lowestY = -100;

    bool canFly = true;
    bool isFlying = true;
    bool isRunning = false;
    bool isGrounded = false;
    bool pressedFirstTimeSpace = false;
    float lastPressedTimeSpace;
    bool pressedFirstTimeW = false;
    float lastPressedTimeW;

    GameObject playerCamera;
    Rigidbody rigidBody;
    Transform groundCheck;
    LayerMask ground;

    float xRotation;
    float yRotation;
    float gravityAcceleration = 0f;
    float moveSpeedMultiplier = 1f;
    Vector3 move = Vector3.zero;

    public void Enable()
    {
        playerCamera = transform.GetChild(0).gameObject;
        groundCheck = transform.GetChild(2).gameObject.transform;
        rigidBody = GetComponent<Rigidbody>();
        ground = LayerMask.GetMask("Ground");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = spawnPoint;
        isEnabled = true;
    }

    void FixedUpdate()
    {
        if (!isEnabled) return;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, ground);
        move = (transform.forward * move.z + transform.right * move.x + transform.up * move.y).normalized * moveSpeed * moveSpeedMultiplier;
        move.y += gravityAcceleration;
        rigidBody.velocity = move;
    }

    void Update()
    {
        if (!isEnabled) return;

        gravityAcceleration += gravity * Time.deltaTime;
        if (isGrounded || isFlying) gravityAcceleration = 0;
        if (transform.position.y < lowestY) transform.position = spawnPoint;
        if (isFlying && isGrounded) isFlying = false;

        move = Vector3.zero;
        moveSpeedMultiplier = 1f;

        if (Input.GetKey(KeyCode.Space)&&isFlying) move.y += 1f;
        if (Input.GetKey(KeyCode.LeftShift)&&isFlying) move.y += -1f;
        if(Input.GetKey(KeyCode.Space)&&isGrounded) gravityAcceleration = Mathf.Sqrt(jumpHeight * -2f * gravity);

        checkDoublePressSpace();
        checkDoublePressW();

        if(Input.GetKeyDown(KeyCode.LeftControl)&&Input.GetKey(KeyCode.W)) isRunning = true;
        if(Input.GetKeyUp(KeyCode.W)&&isRunning) isRunning = false;
        if(isRunning) moveSpeedMultiplier *= 2f;
        if(isFlying) moveSpeedMultiplier *= 1.5f;
        if(Input.GetKey(KeyCode.LeftShift)&&!isFlying) moveSpeedMultiplier /= 2f;

        if (Input.GetKey(KeyCode.W)) move.z += 1f;
        if (Input.GetKey(KeyCode.S)) move.z += -1f;
        if (Input.GetKey(KeyCode.A)) move.x += -1f;
        if (Input.GetKey(KeyCode.D)) move.x += 1f;

        if (Input.GetKeyDown(KeyCode.T)) MainUdpClient.SendChatMessage("Hello World!");
        if (Input.GetKeyDown(KeyCode.Y)) MainUdpClient.LogGameState();

        yRotation += Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
        xRotation -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void checkDoublePressSpace()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pressedFirstTimeSpace&&Time.time - lastPressedTimeSpace <= delayBetweenPresses)
            {
                if(!isFlying&&canFly) isFlying = true;
                else if(isFlying) isFlying = false;
                pressedFirstTimeSpace = false;
            }
            else pressedFirstTimeSpace = true;
    
            lastPressedTimeSpace = Time.time;
        }
        if (pressedFirstTimeSpace && Time.time - lastPressedTimeSpace > delayBetweenPresses) pressedFirstTimeSpace = false;
    }
    private void checkDoublePressW()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (pressedFirstTimeW&&Time.time - lastPressedTimeW <= delayBetweenPresses)
            {
                isRunning = true;
                pressedFirstTimeW = false;
            }
            else pressedFirstTimeW = true;
    
            lastPressedTimeW = Time.time;
        }
        if (pressedFirstTimeW && Time.time - lastPressedTimeW > delayBetweenPresses) pressedFirstTimeW = false;
    }

    public static Vector3 GetPosition()
    {
        return GameObject.Find("MainUser").transform.position;
    }
}