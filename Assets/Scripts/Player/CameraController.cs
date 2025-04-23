using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CameraController: Handles camera rotation based on mouse input, allowing first-person style controls.
/// CameraController: Điều khiển quay camera dựa trên input chuột, cho phép điều khiển theo kiểu góc nhìn người thứ nhất.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Player & Camera References")]
    public Transform playerTransform; 
    [SerializeField] private Camera _camera; 

    [Header("Settings")]
    public float mouseSensitivity = 100f; 

    [Header("Game Management")]
    [SerializeField] private GameManager gameManager; 

    [Header("Rotation Variables")]
    private float mouseX, mouseY; 
    private float xRotation; 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; 
        AudioListener audioListener = gameObject.GetComponentInChildren<AudioListener>(); 
        Destroy(audioListener);
    }

    private void Update()
    {
       
        if (gameManager.CurrentLocalGameState == GameState.inGame)
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; 
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; 

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); 

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); 
            playerTransform.Rotate(Vector3.up * mouseX);
        }
    }
}
