using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Kết nối đến Photon Server ngay khi scene bắt đầu.
/// Sau khi kết nối, sẽ tự động load scene Menu.
/// </summary>
public class ConnectToSever : MonoBehaviourPunCallbacks
{
    public float loadingTime = 2f;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        Invoke("LoadMainMenu", loadingTime);
    }
    void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
