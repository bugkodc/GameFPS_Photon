using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This cross is activated when we hit an enemy
public class HitCross : MonoBehaviour
{
    [SerializeField] private float disableTime = 0.3f;

    //Is showed only for a little time
    private void OnEnable()
    {
        RestartDisableCall();
    }
    //Disable the cross
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


