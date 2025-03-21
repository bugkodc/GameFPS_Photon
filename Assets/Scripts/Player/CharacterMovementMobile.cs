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
    float screenWidth = Screen.width;
    int leftFingerID, rightFingerID;
    Vector3 direction;

    Vector2 moveTouchDirection;
    Vector2 moveTouchStart;

    public VariableJoystick variableJoystick;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        leftFingerID = -1;
        rightFingerID = -1;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);

        if (isGrounded && yVelocity.y < 0)
        {
            yVelocity.y = -2;
        }

        // Lấy đầu vào từ joystick
        direction = transform.right * variableJoystick.Horizontal + transform.forward * variableJoystick.Vertical;

        if (playerManager.isAlive)
        {
            characterController.Move(direction * Time.deltaTime * speed);
        }
        yVelocity.y += gravity * Time.deltaTime;
        characterController.Move(yVelocity * Time.deltaTime);
    }
    public void Jump()
    {

        if (isGrounded)
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }
}
