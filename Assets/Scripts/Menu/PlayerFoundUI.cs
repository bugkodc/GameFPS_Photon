using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerFoundUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI usernameText;
    public void SetUserName(string username)
    {
        usernameText.text = username;
    }
}
