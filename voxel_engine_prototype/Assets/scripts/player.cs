using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class player : NetworkBehaviour
{
    public float moveSpeed = 8f;
    public float jumpHeight = 0.75f;
    public float sens = 1024f;
    public float groundDistance = 0.2f;
    public float gravity = -16f;
    public bool canFly = true;
    public bool isFlying = false;
    public bool isRunning = false;
    public Vector3 spawnPoint = new Vector3(120,25,120);
    public GameObject playerCamera;
    public GameObject gameUi;
    public Transform groundCheck;
    public LayerMask ground;
    public bool isGrounded;
    float xRotation;
    float yRotation;
    Rigidbody rigidBody;

    float delayBetweenPresses = 0.25f;
    bool pressedFirstTimeSpace = false;
    float lastPressedTimeSpace;
    bool pressedFirstTimeW = false;
    float lastPressedTimeW;

    public float velocity;
    private void Start()
    {
        if(!IsOwner) return;
        playerCamera = transform.GetChild(0).gameObject;
        groundCheck = transform.GetChild(2).gameObject.transform;
        rigidBody = GetComponent<Rigidbody>();
        playerCamera.SetActive(true);
        gameUi = GameObject.Find("networkmanagerui");
        gameUi.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = spawnPoint;
    }
    
    private void Update()
    {
        if(!IsOwner) return;

        //this.isGrounded = Physics.Raycast(groundCheck.position, -Vector3.up, groundDistance, ground);
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, ground);

        velocity += gravity * Time.deltaTime;
        if((isGrounded||isFlying)&&velocity<0) velocity = 0;
        if(transform.position.y<-120)
        {
            velocity = 0;
            transform.position = spawnPoint;
        }
        float flightManeuver = 0f;
        float moveSpeedBonus = 1f;
        if(isFlying&&isGrounded) isFlying = false;
        if(Input.GetKey(KeyCode.Space)&&isFlying) flightManeuver = moveSpeed;
        if(Input.GetKey(KeyCode.LeftShift)&&isFlying) flightManeuver = -moveSpeed;

        if(Input.GetKey(KeyCode.Space)&&isGrounded) velocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        checkDoublePressSpace();
        checkDoublePressW();
        if(Input.GetKeyDown(KeyCode.LeftControl)&&Input.GetKey(KeyCode.W)) isRunning = true;
        if(Input.GetKeyUp(KeyCode.W)&&isRunning) isRunning = false;
        if(isRunning)
        {
            //Camera.main.fieldOfView = 90f;
            moveSpeedBonus *= 2f;
        }
        if(Input.GetKey(KeyCode.LeftShift)&&!isFlying) moveSpeedBonus /= 2f;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized * moveSpeed * moveSpeedBonus;

        rigidBody.velocity = new Vector3(move.x, velocity+flightManeuver, move.z);

        float mouseX = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;
        float mouseY = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;

        yRotation += mouseY;
        xRotation -= mouseX;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void checkDoublePressSpace()
    {
        if(!IsOwner) return;
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
        if(!IsOwner) return;
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
}
