using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public static RoomManager roomManager;
    SceneManager sceneManager;
    public bool isMobi = false;

    private void Awake()
    {
        if (roomManager == null)
        {
            roomManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    override public void OnEnable()
    {
        SceneManager.sceneLoaded += InstantiatePlayer;
    }
    override public void OnDisable()
    {
        SceneManager.sceneLoaded -= InstantiatePlayer;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= InstantiatePlayer;
    }

    void InstantiatePlayer(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;

        Vector3 playerSpawnPosition = new Vector3(Random.Range(-3, 3), 2, Random.Range(-3, 3));

        // Chỉ spawn nhân vật cho chính client này
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            if (isMobi)
            {
                PhotonNetwork.Instantiate("PlayerMobile", playerSpawnPosition, Quaternion.identity);
            }
            else
            {
                PhotonNetwork.Instantiate("Player", playerSpawnPosition, Quaternion.identity);
            }
        }
        else
        {
            // Nếu không kết nối Photon thì dùng Instantiate thường
            if (isMobi)
            {
                Instantiate(Resources.Load("PlayerMobile"), playerSpawnPosition, Quaternion.identity);
            }
            else
            {
                Instantiate(Resources.Load("Player"), playerSpawnPosition, Quaternion.identity);
            }
        }
    }
}
