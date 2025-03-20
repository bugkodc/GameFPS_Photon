using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToSever : MonoBehaviourPunCallbacks
{
    public float loadingTime = 2f;
    // Start is called before the first frame update
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
