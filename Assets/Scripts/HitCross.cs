using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HitCross: Controls automatic disabling of the hitbox after a delay when enabled.
/// HitCross: Điều khiển vô hiệu hóa tự động hitbox sau khoảng thời gian khi được kích hoạt.
/// </summary>
public class HitCross : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float disableTime = 0.3f; 
    private void OnEnable()
    {
        RestartDisableCall();
    }
    private void DisableHitBox()
    {
        gameObject.SetActive(false);
    }
    public void RestartDisableCall()
    {
        CancelInvoke();
        Invoke("DisableHitBox", disableTime);
    }
}
