using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
/// <summary>
/// Khi chạy game offline sẽ disconnect photon
/// </summary>
public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string sceneGamePlay;
    public void StartGame()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected (DisconnectCause cause)
    {
        SceneManager.LoadScene(sceneGamePlay);
    }
}
