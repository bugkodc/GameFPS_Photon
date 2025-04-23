using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;
    public float mouseSensitivity = 100;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private Camera _camera;

    private float mouseX, mouseY;
    private float xRotation;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (!photonView.IsMine && PhotonNetwork.InRoom)
        {
            AudioListener audioListener = gameObject.GetComponentInChildren<AudioListener>();
            Destroy(audioListener);
        }
    }
    private void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            _camera.enabled = false;
            return;
        }
        if (gameManager.CurrentLocalGameState == GameState.inGame)
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerTransform.Rotate(Vector3.up * mouseX);
        }
    }
}
