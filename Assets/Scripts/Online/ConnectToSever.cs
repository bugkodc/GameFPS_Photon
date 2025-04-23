using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ConnectToServer: Handles the logic for connecting to the server and loading the main menu after a delay.
/// ConnectToServer: Xử lý logic kết nối đến server và tải màn hình chính sau một khoảng thời gian trì hoãn.
/// </summary>
public class ConnectToSever : MonoBehaviour
{
    [Header("Settings")]
    public float loadingTime = 5f;  
    void Start()
    {
        Invoke("LoadMainMenu", loadingTime);
    }
    void LoadMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
