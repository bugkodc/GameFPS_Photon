using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerMobi : MonoBehaviour
{
    public Transform playerTransform;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private Camera _camera;

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
    }
}
