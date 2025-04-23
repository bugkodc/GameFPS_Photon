using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AnimationEventReceive: Receives animation events to trigger gameplay effects (e.g. deal damage, enable scope UI).
/// </summary>
public class AnimationEventReceive : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject scopeOverlay;
    [SerializeField] private GameObject sniperGO;
    [SerializeField] private Camera playerCamera;

    [Header("Target Controllers")]
    public ZombieController zombieController;
    public BossController bossController;

    [Header("Settings")]
    public bool isZoombie;
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

        if (weapon)
        {
            weapon.isScoping = true;
        }

        scopeOverlay.SetActive(true);
        sniperGO.GetComponent<MeshRenderer>().enabled = false;
        playerCamera.fieldOfView = 15;
    }
}
