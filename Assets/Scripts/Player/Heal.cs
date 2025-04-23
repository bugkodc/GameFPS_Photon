using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Heal: Handles restoring the player’s health when they enter this trigger.
/// Heal: Xử lý việc hồi máu cho người chơi khi họ va chạm vào vùng kích hoạt này.
/// </summary>
public class Heal : MonoBehaviour
{
    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 20; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerManager playerManager = other.gameObject.GetComponent<PlayerManager>();
            if (playerManager.currentHealth < playerManager.maximumHealth)
            {
                playerManager.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}
