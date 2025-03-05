using UnityEngine;

// the main user controller
public class MainUser : MonoBehaviour
{
    // the user controller won't be enabled until the user presses connect
    bool isEnabled = false;

    // unity api references
    GameObject playerCamera;
    Rigidbody rigidBody;
    Transform groundCheck;
    LayerMask ground;

    Vector3 spawnPoint = new Vector3(80, 64, 94);
    float range = 5f; // break and place block range
    float moveSpeed = 7.5f;
    float jumpHeight = 1f;
    float groundDistance = 0.25f; // ground check distance
    float gravityAcceleration = -25f;
    float sens = 750f; // mouse sensitivity
    float delayBetweenPresses = 0.25f; // grace period for detecting double presses
    int lowestY = -100; // if the player falls below this y value, they will be teleported to spawn
    float hookRange = 125f; // hook range
    float hookPower = 25f;

    // user abilities
    bool canFly = true;
    bool canRun = true;
    bool canPhase = true;

    // user state
    bool isFlying = true;
    bool isRunning = false;
    bool isPhasing = false;
    bool isGrounded = false;
    bool isHookedL = false;
    bool isHookedR = false;
    bool pressedFirstTimeSpace = false;
    float lastPressedTimeSpace;
    bool pressedFirstTimeW = false;
    float lastPressedTimeW;

    float xRotation; // camera x rotation
    float yRotation; // body y rotation
    float gravityVelocity = 0f;
    float moveSpeedMultiplier = 1f;
    Vector3 hookPosL;
    Vector3 hookPosR;
    Vector3 move = Vector3.zero; // current movement vector

    // setup components and cursor, then enable the user controller
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

    // run physics logic at a fixed interval
    void FixedUpdate()
    {
        if (!isEnabled) return;

        // ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, ground);

        // movement
        move = (transform.forward * move.z + transform.right * move.x + transform.up * move.y).normalized * moveSpeed * moveSpeedMultiplier;
        move.y += gravityVelocity;
        rigidBody.AddForce(move, ForceMode.VelocityChange);

        // hook movement
        Vector3 forceL = Vector3.zero;
        Vector3 forceR = Vector3.zero;
        if (isHookedL) {
            Vector3 directionL = hookPosL - transform.position;
            forceL = directionL.normalized * hookPower;
        }
        if (isHookedR) {
            Vector3 directionR = hookPosR - transform.position;
            forceR = directionR.normalized * hookPower;
        }
        rigidBody.AddForce(forceL, ForceMode.VelocityChange);
        rigidBody.AddForce(forceR, ForceMode.VelocityChange);
    }

    // run input logic every frame
    void Update()
    {
        if (!isEnabled) return;

        gravityVelocity += gravityAcceleration * Time.deltaTime;
        if (isGrounded || isFlying || isHookedL || isHookedR) gravityVelocity = 0f;
        if (transform.position.y < lowestY) transform.position = spawnPoint;
        if (isFlying && isGrounded && !isPhasing) isFlying = false;

        move = Vector3.zero;
        moveSpeedMultiplier = 1f;

        if (Input.GetKey(KeyCode.Space)&&isFlying) move.y += 1f;
        if (Input.GetKey(KeyCode.LeftShift)&&isFlying) move.y += -1f;
        if(Input.GetKey(KeyCode.Space)&&isGrounded) gravityVelocity = Mathf.Sqrt(jumpHeight * -2f * gravityAcceleration);

        checkDoublePressSpace();
        checkDoublePressW();

        if(Input.GetMouseButtonDown(0)) breakBlock();
        if(Input.GetMouseButtonDown(1)) placeBlock();

        if(Input.GetKeyDown(KeyCode.LeftControl)&&canRun) isRunning = true;
        if(Input.GetKeyUp(KeyCode.W)&&isRunning) isRunning = false;
        if(isHookedL||isHookedR) moveSpeedMultiplier *= 2f;
        if(isRunning) moveSpeedMultiplier *= 2f;
        if(isFlying) moveSpeedMultiplier *= 2f;
        if(Input.GetKey(KeyCode.LeftShift)&&!isFlying) moveSpeedMultiplier /= 2f;

        if (Input.GetKey(KeyCode.W)) move.z += 1f;
        if (Input.GetKey(KeyCode.S)) move.z += -1f;
        if (Input.GetKey(KeyCode.A)) move.x += -1f;
        if (Input.GetKey(KeyCode.D)) move.x += 1f;

        if (Input.GetKeyDown(KeyCode.Q)) startHookL();
        if (!Input.GetKey(KeyCode.Q)) isHookedL = false;
        if (Input.GetKeyDown(KeyCode.E)) startHookR();
        if (!Input.GetKey(KeyCode.E)) isHookedR = false;

        if (Input.GetKeyDown(KeyCode.T)) MainUdpClient.SendChatMessage("Hello World!");
        if (Input.GetKeyDown(KeyCode.Y)) MainUdpClient.LogGameState();
        if (Input.GetKeyDown(KeyCode.U)&&canPhase) {
            isPhasing = !isPhasing;
            GetComponent<CapsuleCollider>().enabled = !isPhasing;
            isFlying = true;
        }

        yRotation += Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
        xRotation -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // double press logic
    private void checkDoublePressSpace()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&!isPhasing)
        {
            if (pressedFirstTimeSpace&&Time.time - lastPressedTimeSpace <= delayBetweenPresses)
            {
                if(canFly&&!isFlying) isFlying = true;
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
        if (canRun&&Input.GetKeyDown(KeyCode.W))
        {
            if (pressedFirstTimeW&&Time.time - lastPressedTimeW <= delayBetweenPresses)
            {
                isRunning = true;
                pressedFirstTimeW = false;
            }
            else pressedFirstTimeW = true;
    
            lastPressedTimeW = Time.time;
        }
        if (canRun&&pressedFirstTimeW && Time.time - lastPressedTimeW > delayBetweenPresses) pressedFirstTimeW = false;
    }

    // block placement and breaking logic
    // NOTE: not implemented for now
    private void breakBlock()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            Vector3Int placedBlockPos = Vector3Int.RoundToInt(hit.point - hit.normal/2);
            Debug.Log("Break: from "+playerCamera.transform.position+" to "+placedBlockPos+"");
        }
    }

    private void placeBlock()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, range))
        {
            Vector3Int placedBlockPos = Vector3Int.RoundToInt(hit.point + hit.normal/2);
            Debug.Log("Place: from "+playerCamera.transform.position+" to "+placedBlockPos+"");
        }
    }

    // hook logic
    private void startHookL()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, hookRange))
        {
            hookPosL = hit.point;
            Debug.Log("Hook Left: from "+playerCamera.transform.position+" to "+hookPosL+"");
            isHookedL = true;
        }
    }
    private void startHookR()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, hookRange))
        {
            hookPosR = hit.point;
            Debug.Log("Hook Right: from "+playerCamera.transform.position+" to "+hookPosR+"");
            isHookedR = true;
        }
    }

    // public methods
    public static Vector3 GetPosition()
    {
        return GameObject.Find("MainUser").transform.position;
    }

    public static Vector3 GetRotation()
    {
        return GameObject.Find("MainUser").transform.eulerAngles;
    }

    public static Vector3 GetCamera()
    {
        return GameObject.Find("MainUser").transform.GetChild(0).gameObject.transform.eulerAngles;
    }
}