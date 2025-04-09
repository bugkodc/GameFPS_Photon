using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hiển thị dấu "Hit Cross" khi tấn công trúng kẻ địch.
/// Dấu này sẽ tự động tắt sau một khoảng thời gian.
/// </summary>
public class HitCross : MonoBehaviour
{
    [SerializeField] private float disableTime = 0.3f;
    private void OnEnable()
    {
        RestartDisableCall();
    }
    void DisableHitBox()
    {
        this.gameObject.SetActive(false);
    }
    public void RestartDisableCall()
    {
        CancelInvoke();
        Invoke("DisableHitBox", disableTime);
    }
}


