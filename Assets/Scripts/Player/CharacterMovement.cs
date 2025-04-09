using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Transform isGroundedGO;
    public LayerMask groundLayer;
    [SerializeField] PhotonView photonView;
    PlayerManager playerManager;

    [Header("Speed Settings")]
    public float normalSpeed = 8f;
    public float runSpeed = 12f;
    private float speed;

    [Header("Jump & Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    [SerializeField] float checkGroundRadius = 0.3f;

    private Vector3 direction;
    private Vector3 yVelocity;
    private bool isGrounded;
    float verticalInput, horizontalInput;
    

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        speed = normalSpeed;
    }

    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }
        isGrounded = Physics.CheckSphere(isGroundedGO.position, checkGroundRadius, groundLayer);

        if (playerManager.isAlive)
        {
            SimulateGravity();

            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            direction = transform.right * horizontalInput + transform.forward * verticalInput;

            if (Input.GetButtonDown("Run"))
            {
                speed = runSpeed;
            }
            if (Input.GetButtonUp("Run"))
            {
                speed = normalSpeed;
            }

            characterController.Move(direction * Time.deltaTime * speed);

            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }

            characterController.Move(yVelocity * Time.deltaTime);
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(isGroundedGO.position, checkGroundRadius);
    }
}
