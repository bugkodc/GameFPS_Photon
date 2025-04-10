using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField]
    private GameObject lobbyPanel, mainPanel, usernamePanel,
    usernameInput, RoomIDInput, playerFoundUI, playerFoundHolder, idRoomText, StartButton;

    private bool tryingToReconnect = false;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        Debug.Log("Start - ClientState: " + PhotonNetwork.NetworkClientState);

        if (!PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.PeerCreated || PhotonNetwork.NetworkClientState == ClientState.Disconnected)
        {
            Debug.Log("Chưa connect hoặc đã disconnect, tiến hành connect...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.InRoom)
        {
            Debug.Log("Đang trong phòng, thoát phòng...");
            PhotonNetwork.LeaveRoom();
        }
        else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            Debug.Log("Đã connect MasterServer, sẵn sàng");
        }
    }

    IEnumerator Reconnect()
    {
        tryingToReconnect = true;

        while (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.Disconnecting)
        {
            yield return null;
        }

        Debug.Log("Thực hiện reconnect...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Đã bị ngắt kết nối: {cause}");

        if (!tryingToReconnect)
        {
            StartCoroutine(Reconnect());
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Đã kết nối lại Master thành công.");
        tryingToReconnect = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartOnlineGame()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel("Main");
    }

    public void MainToUserName()
    {
        mainPanel.SetActive(false);
        usernamePanel.SetActive(true);
    }

    public void LobbyToMain()
    {
        PhotonNetwork.LeaveRoom();
        lobbyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void UserNameToMain()
    {
        usernamePanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void SubmitJoinRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Chưa sẵn sàng để join lobby");
            return;
        }

        TMP_InputField input = usernameInput.GetComponent<TMP_InputField>();
        input.characterLimit = 10;
        PhotonNetwork.NickName = input.text;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinLobby();
    }

    public void SubmitQuickMath()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Chưa sẵn sàng để join room");
            return;
        }

        TMP_InputField input = usernameInput.GetComponent<TMP_InputField>();
        input.characterLimit = 10;
        PhotonNetwork.NickName = input.text;
        PhotonNetwork.JoinRandomRoom();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby thành công");
        JoinOrCreateRoom();
    }

    public void JoinOrCreateRoom()
    {
        string roomID = RoomIDInput.GetComponent<TMP_InputField>().text;
        if (string.IsNullOrEmpty(roomID))
        {
            Debug.LogWarning("Room ID không được để trống.");
            return;
        }

        RoomOptions options = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = 6 };
        PhotonNetwork.JoinOrCreateRoom(roomID, options, TypedLobby.Default);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random thất bại, tạo phòng mới...");
        CreateRoom();
    }

    void CreateRoom()
    {
        string roomID = RoomIDInput.GetComponent<TMP_InputField>().text;
        RoomOptions options = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 6
        };

        PhotonNetwork.CreateRoom("Room_" + roomID, options);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Tạo phòng thất bại: {message}");
    }

    public override void OnJoinedRoom()
    {
        StartButton.SetActive(PhotonNetwork.IsMasterClient);
        Debug.Log("Đã vào phòng.");
        OpenLobbyScreen();
    }

    public void OpenLobbyScreen()
    {
        usernamePanel.SetActive(false);
        lobbyPanel.SetActive(true);
        idRoomText.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayersListUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdatePlayersListUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersListUI();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master Client mới: {newMasterClient.NickName}");
        StartButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerFoundUI, playerFoundHolder.transform)
        .GetComponent<PlayerFoundUI>().SetUserName(newPlayer.NickName);
    }

    void UpdatePlayersListUI()
    {
        foreach (Transform child in playerFoundHolder.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Instantiate(playerFoundUI, playerFoundHolder.transform)
                .GetComponent<PlayerFoundUI>().SetUserName(player.NickName);
        }
    }
}
