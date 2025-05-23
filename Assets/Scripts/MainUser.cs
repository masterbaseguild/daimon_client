using UnityEngine;

// the main user controller
public class MainUser : MonoBehaviour
{
    [SerializeField] private MainUdpClient udpClient;
    [SerializeField] private World world;

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

    [SerializeField] private AudioClip blockSound;

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
    private int voxel = 1;
    private bool isFullVoxel = true;

    private float xRotation;
    private float yRotation;
    private float movementSpeedMultiplier;
    private Vector3 movement;

    // setup components and cursor, then enable the user controller

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rigidBody.MovePosition(spawnPoint);
    }

    // run input logic every frame
    private void Update()
    {
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
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        MovementForce();
        GravityForce();
    }

    private void HandleLoopback()
    {
        if (transform.position.y < loopbackY)
        {
            rigidBody.MovePosition(spawnPoint);
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

        if (Input.GetMouseButtonDown(2))
        {
            SelectBlock();
        }
    }

    private void BreakBlock()
    {
        Vector3 offset = new(0.5f, 0.5f, 0.5f);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, breakPlaceRange))
        {
            AudioSource.PlayClipAtPoint(blockSound, hit.point);
            Vector3Int fullBlockPos = Vector3Int.FloorToInt(hit.point - (hit.normal * 0.25f) + offset);
            if(!world.GetVoxel(fullBlockPos.x, fullBlockPos.y, fullBlockPos.z).Equals(0))
            {
                udpClient.TcpSend($"{Packet.Server.SETBLOCK}\t0\t{fullBlockPos.x}\t{fullBlockPos.y}\t{fullBlockPos.z}\t0");
            }
            else
            {
                Vector3Int placedBlockPos = Vector3Int.FloorToInt((hit.point - (hit.normal * 0.25f) + offset)*2);
                udpClient.TcpSend($"{Packet.Server.SETMINIBLOCK}\t0\t{placedBlockPos.x}\t{placedBlockPos.y}\t{placedBlockPos.z}\t0");
            }
        }
    }

    private void PlaceBlock()
    {
        Vector3 offset = new(0.5f, 0.5f, 0.5f);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, breakPlaceRange))
        {
            if(isFullVoxel)
            {
                Vector3Int placedBlockPos = Vector3Int.FloorToInt(hit.point + (hit.normal * 0.25f) + offset);
                if(
                    world.GetMiniVoxel(placedBlockPos.x*2, placedBlockPos.y*2, placedBlockPos.z*2).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2+1, placedBlockPos.y*2, placedBlockPos.z*2).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2+1, placedBlockPos.y*2+1, placedBlockPos.z*2).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2+1, placedBlockPos.y*2+1, placedBlockPos.z*2+1).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2, placedBlockPos.y*2+1, placedBlockPos.z*2).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2, placedBlockPos.y*2+1, placedBlockPos.z*2+1).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2, placedBlockPos.y*2, placedBlockPos.z*2+1).Equals(0)&&
                    world.GetMiniVoxel(placedBlockPos.x*2+1, placedBlockPos.y*2, placedBlockPos.z*2+1).Equals(0)
                )
                {
                    AudioSource.PlayClipAtPoint(blockSound, hit.point);
                    udpClient.TcpSend($"{Packet.Server.SETBLOCK}\t0\t{placedBlockPos.x}\t{placedBlockPos.y}\t{placedBlockPos.z}\t{voxel}");
                }
            }
            else
            {
                Vector3Int fullBlockPos = Vector3Int.FloorToInt(hit.point + (hit.normal * 0.25f) + offset);
                if(world.GetVoxel(fullBlockPos.x, fullBlockPos.y, fullBlockPos.z).Equals(0))
                {
                    Vector3Int placedBlockPos = Vector3Int.FloorToInt((hit.point + (hit.normal * 0.25f) + offset)*2);
                    udpClient.TcpSend($"{Packet.Server.SETMINIBLOCK}\t0\t{placedBlockPos.x}\t{placedBlockPos.y}\t{placedBlockPos.z}\t{voxel}");
                    AudioSource.PlayClipAtPoint(blockSound, hit.point);
                }
            }
        }
    }

    private void SelectBlock()
    {
        Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, breakPlaceRange))
        {
            Vector3Int fullBlockPos = Vector3Int.FloorToInt(hit.point - (hit.normal * 0.25f) + offset);
            if(!world.GetVoxel(fullBlockPos.x, fullBlockPos.y, fullBlockPos.z).Equals(0))
            {
                voxel = world.GetVoxel(fullBlockPos.x, fullBlockPos.y, fullBlockPos.z);
                isFullVoxel = true;
            }
            else
            {
                Vector3Int placedBlockPos = Vector3Int.FloorToInt((hit.point - (hit.normal * 0.25f) + offset)*2);
                voxel = world.GetMiniVoxel(placedBlockPos.x, placedBlockPos.y, placedBlockPos.z);
                isFullVoxel = false;
            }
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

        // if user presses 1, set voxel to 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            voxel = 1;
            isFullVoxel = true;
        }

        // if user presses 2, set voxel to 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            voxel = 2;
            isFullVoxel = true;
        }

        // if user presses 3, set voxel to 3
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            voxel = 3;
            isFullVoxel = true;
        }

        // if user presses 4, set voxel to 4
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            voxel = 4;
            isFullVoxel = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            voxel = 1;
            isFullVoxel = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            voxel = 2;
            isFullVoxel = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            voxel = 3;
            isFullVoxel = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            voxel = 4;
            isFullVoxel = false;
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