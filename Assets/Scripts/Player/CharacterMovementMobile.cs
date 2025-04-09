using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementMobile : MonoBehaviour
{

    [Header("Movement")]
    public CharacterController characterController;
    public VariableJoystick variableJoystick;
    public float speed = 12f;

    [Header("Jump & Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    private Vector3 yVelocity;
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform isGroundedGO;
    public float checkGroundRadius = 0.3f;
    public LayerMask groundLayer;

    [Header("Camera Rotation")]
    public Transform playerBody;
    public Transform cameraTransform;
    public float sensitivity = 100f;
    public float verticalClampAngle = 60f;
    private Vector2 lastTouchPosition;
    private float verticalRotation = 0f;
    private bool isDragging = false;

    Vector3 direction;
    private PlayerManager playerManager;
    [SerializeField] PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }
        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);
        // Lấy đầu vào từ joystick
        direction = transform.right * variableJoystick.Horizontal + transform.forward * variableJoystick.Vertical;

        if (playerManager.isAlive)
        {
            SimulateGravity();
            characterController.Move(direction * Time.deltaTime * speed);
        }
        yVelocity.y += gravity * Time.deltaTime;
        characterController.Move(yVelocity * Time.deltaTime);
        HandleCameraRotation();
    }
    public void Jump()
    {

        if (isGrounded)
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }
    private void HandleCameraRotation()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); 
            if (touch.position.x > Screen.width / 2)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPosition = touch.position;
                    isDragging = true;
                }
                else if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    Vector2 delta = touch.deltaPosition;
                    float mouseX = delta.x * sensitivity * Time.deltaTime;
                    float mouseY = delta.y * sensitivity * Time.deltaTime;
                    playerBody.Rotate(Vector3.up * mouseX);
                    verticalRotation -= mouseY;
                    verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);
                    // Xoay nhân vật theo trục Y
                    playerBody.localRotation = Quaternion.Euler(verticalRotation, playerBody.localRotation.eulerAngles.y, 0f);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    isDragging = false;
                }
            }
        }
    }
    void SimulateGravity()
    {
        if (isGrounded && yVelocity.y < 0)
        {
            
            yVelocity.y = -2;
        }
        yVelocity.y += gravity * Time.deltaTime;
    }
}

