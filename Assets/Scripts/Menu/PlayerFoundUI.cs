using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// Hiển thị tên người chơi khi tìm thấy người chơi khác (vd: trong danh sách bạn, lobby, v.v).
/// </summary>
public class PlayerFoundUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    public void SetUserName(string username)
    {
        usernameText.text = username;
    }
}
