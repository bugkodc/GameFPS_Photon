using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementMobile : MonoBehaviour
{

    float verticalInput, horizontalInput;
    public CharacterController characterController;
    PlayerManager playerManager;
    public float speed = 12;
    public float gravity = -9.81f;
    public float jumpHeight = 3;
    Vector3 yVelocity;
    public Transform isGroundedGO;
    bool isGrounded;
    [SerializeField]
    float checkGroundRadius;
    [SerializeField]
    public LayerMask groundLayer;
    //float screenWidth = Screen.width;
    //int leftFingerID, rightFingerID;
    Vector3 direction;

    Vector2 moveTouchDirection;
    Vector2 moveTouchStart;
    [SerializeField] PhotonView photonView;
    public VariableJoystick variableJoystick;

    // Xoay camera bằng touch input
    public Transform playerBody; // Nhân vật cần xoay
    public float sensitivity = 100f; // Độ nhạy xoay
    private Vector2 lastTouchPosition;
    private bool isDragging = false;
    private float verticalRotation = 0f; // Lưu góc xoay dọc
    public Transform cameraTransform; // Camera để xoay lên/xuống
    public float verticalClampAngle = 60f; // Giới hạn góc nhìn lên xuống
    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        //leftFingerID = -1;
        //rightFingerID = -1;
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
            Touch touch = Input.GetTouch(0); // Lấy touch đầu tiên
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
        //Reset gravity if we are on the ground
        if (isGrounded && yVelocity.y < 0)
        {
            //We use -2 in order to avoid bugs related to 0 speed.
            yVelocity.y = -2;
        }

        //Increase yvelocity by the force of gravity every second
        yVelocity.y += gravity * Time.deltaTime;
    }
}

