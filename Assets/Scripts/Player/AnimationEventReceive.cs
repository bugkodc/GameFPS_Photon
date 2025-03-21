using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceive : MonoBehaviour
{
    [SerializeField] GameObject scopeOverlay;
    [SerializeField] GameObject sniperGO;
    [SerializeField] Camera playerCamera;
    public ZombieController zombieController;
    public BossController bossController;
    public bool isZoombie;

    //Called when the animation of the zombie makes him stretch his arm to attack
    public void MakeDamage()
    {
        if (isZoombie)
        {
            zombieController.MakeDamage();
        }
        else
        {
            bossController.MakeDamage();
        }
    }
    public void ScopeOverlay()
    {
        WeaponController weapon = sniperGO.GetComponent<WeaponController>();
        if (weapon) weapon.isScoping = true;
        scopeOverlay.SetActive(true);
        sniperGO.GetComponent<MeshRenderer>().enabled = false;
        playerCamera.fieldOfView = 15;
    }
}
