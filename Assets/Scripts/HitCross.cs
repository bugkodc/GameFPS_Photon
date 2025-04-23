using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


