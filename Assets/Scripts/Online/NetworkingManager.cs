using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Quản lý quá trình kết nối, tạo/join room, hiển thị UI liên quan đến phòng và người chơi.
/// </summary>
public class NetworkingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GameObject lobbyPanel, mainPanel, usernamePanel,
    usernameInput, RoomIDInput, playerFoundUI, playerFoundHolder, idRoomText , StartButton;

    private string userName;
    private bool onUserNameScreen;
    private int indexPlayer;

    void Start()
    {
        // Nếu Photon đang ở trạng thái Leaving hoặc chưa kết nối, thực hiện reconnect.
        if (PhotonNetwork.NetworkClientState == ClientState.Leaving || !PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon đang trong trạng thái Leaving hoặc chưa kết nối, bắt đầu quá trình reconnect...");
            PhotonNetwork.Disconnect();
            StartCoroutine(Reconnect());
        }
    }

    IEnumerator Reconnect()
    {
        // Chờ cho đến khi Photon hoàn toàn disconnect
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Debug.Log("Đang kết nối lại với Photon...");
        PhotonNetwork.ConnectUsingSettings();
        yield return null;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Đã kết nối lại thành công với Master Server.");
        // Tại đây, bạn có thể cập nhật UI Menu, cho phép người chơi tạo phòng hoặc join phòng mới
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Đã ngắt kết nối, cause: " + cause.ToString());
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public void StartOnlineGame()
    {
        PhotonNetwork.LoadLevel("Main");
    }
    public void MainToUserName()
    {
        mainPanel.SetActive(false);
        usernamePanel.SetActive(true);
        onUserNameScreen = true;
    }
    public void LobbyToMain()
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.LeaveRoom();
        onUserNameScreen = false;
    }
    public void UserNameToMain()
    {
        mainPanel.SetActive(true);
        usernamePanel.SetActive(false);
        onUserNameScreen = false;
    }
    public void SubmitJoinRoom()
    {
        TMP_InputField usernameInputField = usernameInput.GetComponent<TMP_InputField>();
        usernameInputField.characterLimit = 10;
        photonView.Owner.NickName = usernameInputField.text;
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public void SubmitQuickMath()
    {
        TMP_InputField usernameInputField = usernameInput.GetComponent<TMP_InputField>();
        usernameInputField.characterLimit = 10;
        photonView.Owner.NickName = usernameInputField.text;
        PhotonNetwork.JoinRandomRoom();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        JoinOrCreateRoom();
    }
    public void JoinOrCreateRoom()
    {
        string roomID = RoomIDInput.GetComponent<TMP_InputField>().text;
        if (string.IsNullOrEmpty(roomID))
        {
            Debug.Log("Room ID cannot be empty.");
            return;
        }

        RoomOptions roomOptions = new RoomOptions { IsVisible = true, IsOpen = true, MaxPlayers = 6 };
        PhotonNetwork.JoinOrCreateRoom(roomID, roomOptions, TypedLobby.Default);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"OnJoinRandomFailed: {RoomIDInput.GetComponent<TMP_InputField>().text}");
        CreateRoom();
    }
    void CreateRoom()
    {
        string roomID = RoomIDInput.GetComponent<TMP_InputField>().text;
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 6
        };
        if (PhotonNetwork.CreateRoom($"Room_{roomID}", roomOptions))
            Debug.Log($"Room created with id: {roomID}");


    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Room creation failed return code: {returnCode}");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        // Kiểm tra nếu player hiện tại là Master Client
        StartButton.SetActive(PhotonNetwork.IsMasterClient);
        Debug.Log("Loading game");
        OpenLobbyScreen();
        

    }
    public void OpenLobbyScreen()
    {
        usernamePanel.SetActive(false);
        lobbyPanel.SetActive(true);
        onUserNameScreen = false;
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
        Debug.Log($"Master Client switched to: {newMasterClient.NickName}");

        // Kiểm tra xem player hiện tại có phải Master Client mới không
        StartButton.SetActive(PhotonNetwork.IsMasterClient);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerFoundUI, playerFoundHolder.transform)
        .GetComponent<PlayerFoundUI>().SetUserName(newPlayer.NickName);

    }

    void UpdatePlayersListUI()
    {
        Player[] playerList = PhotonNetwork.PlayerList;

        foreach (Transform playerFound in playerFoundHolder.transform)
        {
            Destroy(playerFound.gameObject);
        }

        for (int i = 0; i < playerList.Length; i++)
        {
            Instantiate(playerFoundUI, playerFoundHolder.transform).
            GetComponent<PlayerFoundUI>().SetUserName(playerList[i].NickName);
        }
    }
}
