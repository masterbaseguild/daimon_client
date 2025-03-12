using UnityEngine;

// the main user controller
public class MainUser : MonoBehaviour
{
    // the user controller won't be enabled until the user presses connect
    bool isEnabled = false;

    // unity api references
    public GameObject playerCamera;
    public Rigidbody rigidBody;
    public LayerMask ground;
    Transform groundCheck;

    // parameters
    public Vector3 spawnPoint;
    public float breakPlaceRange;
    public float movementSpeed;
    public float jumpPower;
    public float jumpCooldown;
    public float groundCheckDistance;
    public float gravityAcceleration;
    public float cameraSensitivity;
    public float delayBetweenDoublePresses;
    public int loopbackY;
    public float groundDrag;
    public float airDrag;
    public float physicsMultiplier;
    public float runningSpeedMultiplier;
    public float flyingSpeedMultiplier;
    public float fallingSpeedMultiplier;
    public float sneakingSpeedMultiplier;

    // user abilities
    bool canFly = true;
    bool canRun = true;
    bool canPhase = true;

    // user state
    bool isReadyToJump = true;
    bool isFlying = true;
    bool isRunning = false;
    bool isPhasing = false;
    bool isGrounded = false;
    bool pressedFirstTimeSpace = false;
    float lastPressedTimeSpace;
    bool pressedFirstTimeW = false;
    float lastPressedTimeW;

    float xRotation;
    float yRotation;
    float movementSpeedMultiplier;
    Vector3 movement;

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
        gameObject.GetComponent<InputManager>().Enable();
    }

    // run input logic every frame
    void Update()
    {
        if (!isEnabled) return;

        HandleMovement();
        HandleSpeedMultipliers();
        HandleSpeedCap();

        HandleGrounded();
        HandleDrag();
        HandleJump();

        HandleRun();
        HandleDoubleWRun();

        HandleDoubleSpaceFlight();
        HandlePhase();

        HandleCamera();
        HandleBreakPlace();

        HandleLoopback();
        HandleDebug();
    }

    // run physics logic at a fixed interval
    void FixedUpdate()
    {
        if (!isEnabled) return;

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        MovementForce();
        GravityForce();
    }

    private void HandleLoopback()
    {
        if (transform.position.y < loopbackY) transform.position = spawnPoint;
    }

    private void HandleGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, ground);
        if (isFlying && isGrounded && !isPhasing) isFlying = false;
    }

    private void HandleMovement()
    {
        movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) movement.z += 1f;
        if (Input.GetKey(KeyCode.S)) movement.z += -1f;
        if (Input.GetKey(KeyCode.A)) movement.x += -1f;
        if (Input.GetKey(KeyCode.D)) movement.x += 1f;
        if (Input.GetKey(KeyCode.Space)&&isFlying) movement.y += 1f;
        if (Input.GetKey(KeyCode.LeftShift)&&isFlying) movement.y += -1f;
    }

    private void HandleSpeedMultipliers()
    {
        movementSpeedMultiplier = 1f;
        if(isRunning) movementSpeedMultiplier *= runningSpeedMultiplier;
        if(isFlying) movementSpeedMultiplier *= flyingSpeedMultiplier;
        else if(!isGrounded) movementSpeedMultiplier *= fallingSpeedMultiplier;
        if(Input.GetKey(KeyCode.LeftShift)&&!isFlying) movementSpeedMultiplier *= sneakingSpeedMultiplier;
    }

    private void HandleSpeedCap()
    {
        if(isFlying)
        {
            Vector3 flatVel = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, rigidBody.velocity.z);
            if(flatVel.magnitude > movementSpeed * movementSpeedMultiplier * physicsMultiplier)
            {
                Vector3 newVel = flatVel.normalized * movementSpeed * movementSpeedMultiplier * physicsMultiplier;
                rigidBody.velocity = new Vector3(newVel.x, newVel.y, newVel.z);
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
            if(flatVel.magnitude > movementSpeed * movementSpeedMultiplier * physicsMultiplier)
            {
                Vector3 newVel = flatVel.normalized * movementSpeed * movementSpeedMultiplier * physicsMultiplier;
                rigidBody.velocity = new Vector3(newVel.x, rigidBody.velocity.y, newVel.z);
            }
        }
    }

    private void HandleDrag()
    {
        if (isGrounded||isFlying) rigidBody.drag = groundDrag;
        else rigidBody.drag = airDrag;
    }

    private void HandleJump()
    {
        if(Input.GetKey(KeyCode.Space)&&isReadyToJump&&isGrounded)
        {
            isReadyToJump = false;
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
            rigidBody.AddForce(Vector3.up * jumpPower * physicsMultiplier, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        isReadyToJump = true;
    }

    private void HandleDoubleSpaceFlight()
    {
        if (Input.GetKeyDown(KeyCode.Space)&&!isPhasing)
        {
            if (pressedFirstTimeSpace&&Time.time - lastPressedTimeSpace <= delayBetweenDoublePresses)
            {
                if(canFly&&!isFlying) isFlying = true;
                else if(isFlying) isFlying = false;
                pressedFirstTimeSpace = false;
            }
            else pressedFirstTimeSpace = true;
    
            lastPressedTimeSpace = Time.time;
        }
        if (pressedFirstTimeSpace && Time.time - lastPressedTimeSpace > delayBetweenDoublePresses) pressedFirstTimeSpace = false;
    }

    private void HandleDoubleWRun()
    {
        if (canRun&&Input.GetKeyDown(KeyCode.W))
        {
            if (pressedFirstTimeW&&Time.time - lastPressedTimeW <= delayBetweenDoublePresses)
            {
                isRunning = true;
                pressedFirstTimeW = false;
            }
            else pressedFirstTimeW = true;
    
            lastPressedTimeW = Time.time;
        }
        if (canRun&&pressedFirstTimeW && Time.time - lastPressedTimeW > delayBetweenDoublePresses) pressedFirstTimeW = false;
    }

    private void HandleBreakPlace()
    {
        if(Input.GetMouseButtonDown(0)) BreakBlock();
        if(Input.GetMouseButtonDown(1)) PlaceBlock();
    }

    // NOTE: not implemented for now
    private void BreakBlock()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, breakPlaceRange))
        {
            Vector3Int placedBlockPos = Vector3Int.RoundToInt(hit.point - hit.normal/2);
            Debug.Log("Break: from "+playerCamera.transform.position+" to "+placedBlockPos+"");
        }
    }

    // NOTE: not implemented for now
    private void PlaceBlock()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, breakPlaceRange))
        {
            Vector3Int placedBlockPos = Vector3Int.RoundToInt(hit.point + hit.normal/2);
            Debug.Log("Place: from "+playerCamera.transform.position+" to "+placedBlockPos+"");
        }
    }

    private void HandleRun()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl)&&canRun) isRunning = true;
        if(Input.GetKeyUp(KeyCode.W)&&isRunning) isRunning = false;
    }

    private void HandleDebug()
    {
        if (Input.GetKeyDown(KeyCode.T)) MainUdpClient.SendChatMessage("Hello World!");
        if (Input.GetKeyDown(KeyCode.Y)) MainUdpClient.LogGameState();
    }

    private void HandlePhase()
    {
        if (Input.GetKeyDown(KeyCode.U)&&canPhase) {
            isPhasing = !isPhasing;
            GetComponent<CapsuleCollider>().enabled = !isPhasing;
            isFlying = true;
        }
    }

    private void HandleCamera()
    {
        yRotation += Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraSensitivity;
        xRotation -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private void MovementForce()
    {
        Vector3 moveVector = (transform.forward * movement.z + transform.right * movement.x + transform.up * movement.y).normalized * movementSpeed * 10f * movementSpeedMultiplier * physicsMultiplier;
        rigidBody.AddForce(moveVector, ForceMode.Force);
    }

    private void GravityForce()
    {
        if (!(isGrounded || isFlying))
        {
            Vector3 gravity = Vector3.up * gravityAcceleration * physicsMultiplier;
            rigidBody.AddForce(gravity, ForceMode.Acceleration);
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