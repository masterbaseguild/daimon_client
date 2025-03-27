using UnityEngine;

// the main user controller
public class MainUser : MonoBehaviour
{
    [SerializeField] private MainUdpClient udpClient;

    // the user controller won't be enabled until the user presses connect
    private bool isEnabled = false;

    // unity api references
    [SerializeField] public GameObject playerCamera;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] public LayerMask ground;
    [SerializeField] private Transform groundCheck;

    // parameters
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private float breakPlaceRange;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpPower;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float gravityAcceleration;
    [SerializeField] private float cameraSensitivity;
    [SerializeField] private float delayBetweenDoublePresses;
    [SerializeField] private int loopbackY;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    [SerializeField] public float physicsMultiplier;
    [SerializeField] private float runningSpeedMultiplier;
    [SerializeField] private float walkingSpeedMultiplier;
    [SerializeField] private float flyingSpeedMultiplier;
    [SerializeField] private float fallingSpeedMultiplier;
    [SerializeField] private float sneakingSpeedMultiplier;

    // user abilities
    private readonly bool canFly = true;
    private readonly bool canRun = true;
    private readonly bool canPhase = true;

    // user state
    private bool isReadyToJump = true;
    private bool isFlying = true;
    private bool isRunning = false;
    private bool isPhasing = false;
    private bool isGrounded = false;
    private bool pressedFirstTimeSpace = false;
    private float lastPressedTimeSpace;
    private bool pressedFirstTimeW = false;
    private float lastPressedTimeW;

    private float xRotation;
    private float yRotation;
    private float movementSpeedMultiplier;
    private Vector3 movement;

    // setup components and cursor, then enable the user controller
    public void Enable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = spawnPoint;
        isEnabled = true;
        gameObject.GetComponent<InputManager>().Enable();
    }

    // run input logic every frame
    private void Update()
    {
        if (!isEnabled)
        {
            return;
        }


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
    private void FixedUpdate()
    {
        if (!isEnabled)
        {
            return;
        }


        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        MovementForce();
        GravityForce();
    }

    private void HandleLoopback()
    {
        if (transform.position.y < loopbackY)
        {
            transform.position = spawnPoint;
        }

    }

    private void HandleGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, ground);
        if (isFlying && isGrounded && !isPhasing)
        {
            isFlying = false;
        }

    }

    private void HandleMovement()
    {
        movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            movement.z += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            movement.z += -1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            movement.x += -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movement.x += 1f;
        }

        if (Input.GetKey(KeyCode.Space) && isFlying)
        {
            movement.y += 1f;
        }

        if (Input.GetKey(KeyCode.LeftShift) && isFlying)
        {
            movement.y += -1f;
        }

    }

    private void HandleSpeedMultipliers()
    {
        movementSpeedMultiplier = 1f;
        if (isRunning)
        {
            movementSpeedMultiplier *= runningSpeedMultiplier;
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            movementSpeedMultiplier *= walkingSpeedMultiplier;
        }

        if (isFlying)
        {
            movementSpeedMultiplier *= flyingSpeedMultiplier;
        }
        else if (!isGrounded)
        {
            movementSpeedMultiplier *= fallingSpeedMultiplier;
        }

        if (Input.GetKey(KeyCode.LeftShift) && !isFlying)
        {
            movementSpeedMultiplier *= sneakingSpeedMultiplier;
        }

    }

    private void HandleSpeedCap()
    {
        if (isFlying)
        {
            Vector3 flatVel = new(rigidBody.linearVelocity.x, rigidBody.linearVelocity.y, rigidBody.linearVelocity.z);
            if (flatVel.magnitude > movementSpeed * movementSpeedMultiplier * physicsMultiplier)
            {
                Vector3 newVel = movementSpeed * movementSpeedMultiplier * physicsMultiplier * flatVel.normalized;
                rigidBody.linearVelocity = new Vector3(newVel.x, newVel.y, newVel.z);
            }
        }
        else
        {
            Vector3 flatVel = new(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
            if (flatVel.magnitude > movementSpeed * movementSpeedMultiplier * physicsMultiplier)
            {
                Vector3 newVel = movementSpeed * movementSpeedMultiplier * physicsMultiplier * flatVel.normalized;
                rigidBody.linearVelocity = new Vector3(newVel.x, rigidBody.linearVelocity.y, newVel.z);
            }
        }
    }

    private void HandleDrag()
    {
        rigidBody.linearDamping = isGrounded || isFlying ? groundDrag : airDrag;

    }

    private void HandleJump()
    {
        if (Input.GetKey(KeyCode.Space) && isReadyToJump && isGrounded)
        {
            isReadyToJump = false;
            rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
            rigidBody.AddForce(jumpPower * physicsMultiplier * Vector3.up, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        isReadyToJump = true;
    }

    private void HandleDoubleSpaceFlight()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isPhasing)
        {
            if (pressedFirstTimeSpace && Time.time - lastPressedTimeSpace <= delayBetweenDoublePresses)
            {
                if (canFly && !isFlying)
                {
                    isFlying = true;
                }
                else if (isFlying)
                {
                    isFlying = false;
                }


                pressedFirstTimeSpace = false;
            }
            else
            {
                pressedFirstTimeSpace = true;
            }

            lastPressedTimeSpace = Time.time;
        }
        if (pressedFirstTimeSpace && Time.time - lastPressedTimeSpace > delayBetweenDoublePresses)
        {
            pressedFirstTimeSpace = false;
        }

    }

    private void HandleDoubleWRun()
    {
        if (canRun && Input.GetKeyDown(KeyCode.W))
        {
            if (pressedFirstTimeW && Time.time - lastPressedTimeW <= delayBetweenDoublePresses)
            {
                isRunning = true;
                pressedFirstTimeW = false;
            }
            else
            {
                pressedFirstTimeW = true;
            }

            lastPressedTimeW = Time.time;
        }
        if (canRun && pressedFirstTimeW && Time.time - lastPressedTimeW > delayBetweenDoublePresses)
        {
            pressedFirstTimeW = false;
        }

    }

    private void HandleBreakPlace()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BreakBlock();
        }

        if (Input.GetMouseButtonDown(1))
        {
            PlaceBlock();
        }

    }

    // NOTE: not implemented for now
    private void BreakBlock()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, breakPlaceRange))
        {
            Vector3Int placedBlockPos = Vector3Int.RoundToInt(hit.point - (hit.normal / 2));
            Debug.Log("Break: from " + playerCamera.transform.position + " to " + placedBlockPos + "");
        }
    }

    // NOTE: not implemented for now
    private void PlaceBlock()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, breakPlaceRange))
        {
            Vector3Int placedBlockPos = Vector3Int.RoundToInt(hit.point + (hit.normal / 2));
            Debug.Log("Place: from " + playerCamera.transform.position + " to " + placedBlockPos + "");
        }
    }

    private void HandleRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && canRun)
        {
            isRunning = true;
        }

        if (Input.GetKeyUp(KeyCode.W) && isRunning)
        {
            isRunning = false;
        }

    }

    private void HandleDebug()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            udpClient.SendChatMessage("Hello World!");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            udpClient.LogGameState();
        }

    }

    private void HandlePhase()
    {
        if (Input.GetKeyDown(KeyCode.U) && canPhase)
        {
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
        Vector3 moveVector = 10f * movementSpeed * movementSpeedMultiplier * physicsMultiplier * ((transform.forward * movement.z) + (transform.right * movement.x) + (transform.up * movement.y)).normalized;
        rigidBody.AddForce(moveVector, ForceMode.Force);
    }

    private void GravityForce()
    {
        if (!(isGrounded || isFlying))
        {
            Vector3 gravity = gravityAcceleration * physicsMultiplier * Vector3.up;
            rigidBody.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    // public methods

    public Vector3 GetPosition()
    {
        return GameObject.Find("MainUser").transform.position;
    }

    public Vector3 GetRotation()
    {
        return GameObject.Find("MainUser").transform.eulerAngles;
    }

    public Vector3 GetCamera()
    {
        return GameObject.Find("MainUser").transform.GetChild(0).gameObject.transform.eulerAngles;
    }
}