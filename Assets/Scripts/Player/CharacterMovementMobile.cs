using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementMobile : MonoBehaviour
{
    [Header("Movement")]
    public CharacterController characterController;
    public VariableJoystick variableJoystick;
    public float speed = 6f;

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
    public float sensitivity = 2f;
    public float verticalClampAngle = 60f;
    private Vector2 lastTouchPosition;
    private float verticalRotation = 0f;
    private bool isDragging = false;

    Vector3 direction;
    private PlayerManager playerManager;
    [SerializeField] PhotonView photonView;

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);
        Vector3 moveInput = new Vector3(variableJoystick.Horizontal, 0, variableJoystick.Vertical);
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        direction = camForward * moveInput.z + camRight * moveInput.x;

        if (playerManager.isAlive)
        {
            SimulateGravity();
            characterController.Move(direction * Time.deltaTime * speed);
        }

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

    private void SimulateGravity()
    {
        if (isGrounded && yVelocity.y < 0)
        {
            yVelocity.y = -2f;
        }

        yVelocity.y += gravity * Time.deltaTime;
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
                    float touchSensitivity = sensitivity;

                    float mouseX = delta.x * touchSensitivity * Time.deltaTime;
                    float mouseY = delta.y * touchSensitivity * Time.deltaTime;

                    playerBody.Rotate(Vector3.up * mouseX);

                    // Xoay dọc - camera
                    verticalRotation -= mouseY;
                    verticalRotation = Mathf.Clamp(verticalRotation, -verticalClampAngle, verticalClampAngle);
                    cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                }
            }
        }
    }
}
