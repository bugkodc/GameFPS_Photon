using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// RoomManager: Quản lý singleton cho phòng và sinh người chơi.
/// RoomManager: Manages the room singleton and player instantiation.
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Header("Singleton")]
    public static RoomManager roomManager;  

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
    private void Start()
    {
        InstantiatePlayer();
    }
    private void InstantiatePlayer()
    {
        Vector3 playerSpawnPosition = new Vector3(
            Random.Range(-3f, 3f),
            2f,
            Random.Range(-3f, 3f)
        );

        Instantiate(Resources.Load("Player"), playerSpawnPosition, Quaternion.identity);
    }
}
