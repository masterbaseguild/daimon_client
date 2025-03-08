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
    LineRenderer lineRendererL1;
    LineRenderer lineRendererR1;
    LineRenderer lineRendererL2;
    LineRenderer lineRendererR2;

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
    public float hookRange;
    public float hookPower;
    public float groundDrag;
    public float airDrag;
    public float physicsMultiplier;
    public float hookedSpeedMultiplier;
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
    bool isHookedL = false;
    bool isHookedR = false;
    bool pressedFirstTimeSpace = false;
    float lastPressedTimeSpace;
    bool pressedFirstTimeW = false;
    float lastPressedTimeW;

    float xRotation;
    float yRotation;
    float movementSpeedMultiplier;
    Vector3 movement;
    Vector3 hookPosL;
    Vector3 hookPosR;

    // setup components and cursor, then enable the user controller
    public void Enable()
    {
        playerCamera = transform.GetChild(0).gameObject;
        lineRendererL1 = transform.Find("HookL1").GetComponent<LineRenderer>();
        lineRendererR1 = transform.Find("HookR1").GetComponent<LineRenderer>();
        lineRendererL2 = transform.Find("HookL2").GetComponent<LineRenderer>();
        lineRendererR2 = transform.Find("HookR2").GetComponent<LineRenderer>();
        groundCheck = transform.GetChild(2).gameObject.transform;
        rigidBody = GetComponent<Rigidbody>();
        ground = LayerMask.GetMask("Ground");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = spawnPoint;
        isEnabled = true;
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
        HandleHooks();

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
        HookForce();
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
        if(isHookedL||isHookedR) movementSpeedMultiplier *= hookedSpeedMultiplier;
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

    private void HandleHooks()
    {
        if (Input.GetKeyDown(KeyCode.Q)) StartHookL();
        if (!Input.GetKey(KeyCode.Q))
        {
            isHookedL = false;
            lineRendererL1.enabled = false;
            lineRendererL2.enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.E)) StartHookR();
        if (!Input.GetKey(KeyCode.E))
        {
            isHookedR = false;
            lineRendererR1.enabled = false;
            lineRendererR2.enabled = false;
        }
        if (isHookedL)
        {
            lineRendererL1.SetPosition(0, transform.position - transform.right * 0.5f + transform.up * 0.5f);
            lineRendererL2.SetPosition(0, transform.position - transform.right * 0.5f - transform.up * 0.5f);
        }
        if (isHookedR)
        {
            lineRendererR1.SetPosition(0, transform.position + transform.right * 0.5f + transform.up * 0.5f);
            lineRendererR2.SetPosition(0, transform.position + transform.right * 0.5f - transform.up * 0.5f);
        }
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

    private void StartHookL()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, hookRange, layerMask: ground))
        {
            hookPosL = hit.point;
            Debug.Log("Hook Left: from "+playerCamera.transform.position+" to "+hookPosL+"");
            isHookedL = true;
            Vector3 startForce = (hookPosL - transform.position).normalized * hookPower * physicsMultiplier;
            if (startForce.y < physicsMultiplier * hookPower * 0.5f) startForce += Vector3.up * (physicsMultiplier * hookPower * 0.5f - startForce.y);
            if (hookPosL.y >= transform.position.y) startForce += Vector3.up * (hookPosL.y - transform.position.y) * physicsMultiplier;
            rigidBody.AddForce(startForce, ForceMode.Force);
            lineRendererL1.enabled = true;
            lineRendererL1.SetPosition(0, transform.position - transform.right * 0.5f + transform.up * 0.5f);
            lineRendererL1.SetPosition(1, hookPosL);
            lineRendererL2.enabled = true;
            lineRendererL2.SetPosition(0, transform.position - transform.right * 0.5f - transform.up * 0.5f);
            lineRendererL2.SetPosition(1, hookPosL);
        }
    }

    private void StartHookR()
    {
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, hookRange, layerMask: ground))
        {
            hookPosR = hit.point;
            Debug.Log("Hook Right: from "+playerCamera.transform.position+" to "+hookPosR+"");
            isHookedR = true;
            Vector3 startForce = (hookPosR - transform.position).normalized * hookPower * physicsMultiplier;
            if (startForce.y < physicsMultiplier * hookPower * 0.5f) startForce += Vector3.up * (physicsMultiplier * hookPower * 0.5f - startForce.y);
            if (hookPosR.y >= transform.position.y) startForce += Vector3.up * (hookPosR.y - transform.position.y) * physicsMultiplier;
            rigidBody.AddForce(startForce, ForceMode.Force);
            lineRendererR1.enabled = true;
            lineRendererR1.SetPosition(0, transform.position + transform.right * 0.5f + transform.up * 0.5f);
            lineRendererR1.SetPosition(1, hookPosR);
            lineRendererR2.enabled = true;
            lineRendererR2.SetPosition(0, transform.position + transform.right * 0.5f - transform.up * 0.5f);
            lineRendererR2.SetPosition(1, hookPosR);
        }
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

    private void HookForce()
    {
        Vector3 currentDirection = rigidBody.velocity.normalized;
        if (isHookedL) {
            Vector3 directionL = (hookPosL - transform.position).normalized;
            Vector3 finalDirection = (directionL + currentDirection).normalized;
            Vector3 forceL = directionL * hookPower * physicsMultiplier;
            rigidBody.AddForce(forceL, ForceMode.Force);
            if (Vector3.Angle(rigidBody.velocity, forceL) > 90f)
                rigidBody.AddForce(-rigidBody.velocity * physicsMultiplier);
        }
        if (isHookedR) {
            Vector3 directionR = (hookPosR - transform.position).normalized;
            Vector3 finalDirection = (directionR + currentDirection).normalized;
            Vector3 forceR = directionR * hookPower * physicsMultiplier;
            rigidBody.AddForce(forceR, ForceMode.Force);
            if (Vector3.Angle(rigidBody.velocity, forceR) > 90f)
                rigidBody.AddForce(-rigidBody.velocity * physicsMultiplier);
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