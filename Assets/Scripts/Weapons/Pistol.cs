using UnityEngine;

public class Pistol : MonoBehaviour, IWeapon
{
    float totalDamage;


    public float CalculateDamage(WeaponStats weaponSO, ZombieManager enemyManager, RaycastHit hit)
    {
        float baseDamage = weaponSO.weaponDamage;
        totalDamage = baseDamage;
        if (hit.collider.gameObject.name == "HeadCollider")
            totalDamage *= 2;
        return totalDamage;
    }


}
